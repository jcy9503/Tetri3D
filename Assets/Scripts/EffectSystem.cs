using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public sealed class EffectSystem : MonoSingleton<EffectSystem>
{
	private static          GameObject       effectObj;
	private static          GameGrid         grid;
	private static          List<LineMesh>   lineMeshes;
	private static          List<PrefabMesh> gridMeshes;
	private                 ParticleRender   rotationParticle;
	private const           string           vfxRotation = "Prefabs/VFX_Rotation";
	private const           string           vfxDrop     = "Prefabs/VFX_Drop";
	public static           float            lineGlowPower;
	private static readonly int              alpha      = Shader.PropertyToID("_Alpha");
	private static readonly int              clear      = Shader.PropertyToID("_Clear");
	private static readonly int              emission   = Shader.PropertyToID("_Emission");
	private static readonly int              over       = Shader.PropertyToID("_GameOver");
	private static readonly int              smoothness = Shader.PropertyToID("_Smoothness");
	private static readonly int              color      = Shader.PropertyToID("_Color");
	public static readonly  int              power      = Shader.PropertyToID("_Power");

	public EffectSystem()
	{
		Init();
	}

	protected override void Init()
	{
		rotationParticle = null;
	}

	private void Start()
	{
		grid = GameManager.grid;
		
		lineMeshes = RenderSystem.lineMeshList;
		gridMeshes = RenderSystem.gridMeshList;
		
		effectObj     = GameObject.Find("Effect");
		lineGlowPower = lineMeshes[0].Renderer.material.GetFloat(power);
	}

	public static void DropEffect()
	{
		int yMax = GameManager.currentBlock.Tile.Select(coord => coord.Y).Prepend(-1).Max();

		foreach (Coord coord in GameManager.currentBlock.Tile)
		{
			if (coord.Y != yMax) continue;

			Vector3 offset = RenderSystem.startOffset + GameManager.currentBlock.Pos.ToVector() + coord.ToVector() +
			                 new Vector3(0f, -GameManager.blockSize / 2f, 0f);
			ParticleRender ptc = new(vfxDrop, offset, effectObj, Quaternion.identity);
			Destroy(ptc.Obj, 3f);
		}
	}

	private IEnumerator GridEffect()
	{
		const float   alphaUnit = 0.01f;
		float         alphaSet  = grid.Mesh.MRenderer.material.GetFloat(alpha) + alphaUnit;
		Vector3       targetLoc = grid.Mesh.Obj.transform.position             - Vector3.up    * 5f;
		float         glowSet   = lineGlowPower                                + lineGlowPower * 0.01f;
		const float   range     = 0.15f;
		List<Vector3> listRd    = new();

		for (int i = 0; i < 24; ++i)
		{
			listRd.Add(new Vector3(Random.Range(-range, range),
			                       Random.Range(-range, range),
			                       Random.Range(-range, range)));
		}

		while ((grid.Mesh.Obj.transform.position - targetLoc).magnitude > 0.001f)
		{
			alphaSet -= 0.01f;
			glowSet  -= lineGlowPower * 0.01f;

			grid.Mesh.Obj.transform.position = Vector3.Lerp(grid.Mesh.Obj.transform.position, targetLoc, 0.02f);
			grid.Mesh.MRenderer.material.SetFloat(alpha, Mathf.Max(alphaSet, 0f));

			for (int i = 0; i < lineMeshes.Count; ++i)
			{
				lineMeshes[i].Renderer.material.SetFloat(alpha, alphaSet);
				lineMeshes[i].Renderer.SetPosition(0, lineMeshes[i].Renderer.GetPosition(0) +
				                                      listRd[i * 2]);
				lineMeshes[i].Renderer.material.SetFloat(power, glowSet);
				lineMeshes[i].Renderer.SetPosition(1, lineMeshes[i].Renderer.GetPosition(1) +
				                                      listRd[i * 2 + 1]);
			}

			yield return new WaitForSeconds(0.02f);
		}

		Destroy(grid.Mesh.Obj);

		foreach (LineMesh mesh in lineMeshes)
		{
			Destroy(mesh.Obj);
		}

		lineMeshes.Clear();
	}

	public IEnumerator GameOverEffect()
	{
		RenderSystem.ClearCurrentBlock();
		RenderSystem.ClearShadowBlock();

		const float explosionForce  = 200f;
		float       explosionRadius = grid.SizeY;
		const float torque          = 50f;

		foreach (PrefabMesh mesh in gridMeshes)
		{
			Rigidbody rb = mesh.Obj.AddComponent<Rigidbody>();

			rb.AddExplosionForce(explosionForce,
			                     new Vector3(0f, RenderSystem.startOffset.y - mesh.pos.Y -
			                                     GameManager.blockSize / 2f, 0f), explosionRadius);

			Vector3 rdVec = new(Random.Range(-torque, torque), Random.Range(-torque, torque),
			                    Random.Range(-torque, torque));
			rb.AddTorque(rdVec);
			rb.angularDrag = Random.Range(0.5f, 2f);

			mesh.renderer.material.SetFloat(over,       1f);
			mesh.renderer.material.SetFloat(smoothness, 0f);
		}

		StartCoroutine(GridEffect());

		float alphaSet = 1.01f;

		while (alphaSet > 0f)
		{
			alphaSet -= 0.01f;

			foreach (PrefabMesh mesh in gridMeshes)
			{
				mesh.renderer.material.SetFloat(alpha, alphaSet);
			}

			yield return new WaitForSeconds(0.02f);
		}

		foreach (PrefabMesh mesh in gridMeshes)
		{
			Destroy(mesh.Obj);
		}

		gridMeshes.Clear();
	}

	public IEnumerator ClearEffect(List<int> cleared)
	{
		List<PrefabMesh> clearMeshList   = new();
		const float      explosionForce  = 900f;
		float            explosionRadius = grid.SizeX + grid.SizeZ;
		const float      explosionUp     = 5f;
		const float      torque          = 100f;

		foreach (int height in cleared)
		{
			for (int i = 0; i < grid.SizeX; ++i)
			{
				for (int j = 0; j < grid.SizeZ; ++j)
				{
					Vector3 offset = new(i, -height, j);
					PrefabMesh mesh = new("Prefabs/Mesh_Block", RenderSystem.startOffset + offset, Block.MatPath[^1],
					                      new Coord(i, height, j), ShadowCastingMode.Off);
					mesh.renderer.material.SetFloat(clear,    1f);
					mesh.renderer.material.SetFloat(color,    Random.Range(0f, 1f));
					mesh.renderer.material.SetFloat(emission, 10f);

					Rigidbody rb = mesh.Obj.AddComponent<Rigidbody>();


					rb.AddForce(Physics.gravity * 40f, ForceMode.Acceleration);
					rb.AddForce(new Vector3(0f, Random.Range(-explosionUp, explosionUp), 0f),
					            ForceMode.Impulse);
					rb.AddExplosionForce(explosionForce,
					                     new Vector3(0f, RenderSystem.startOffset.y - height, 0f),
					                     explosionRadius);

					Vector3 rdVec = new(Random.Range(-torque, torque), Random.Range(-torque, torque),
					                    Random.Range(-torque, torque));
					rb.AddTorque(rdVec);
					rb.angularDrag = Random.Range(0.5f, 2f);

					clearMeshList.Add(mesh);
					mesh.Obj.transform.parent = effectObj.transform;
				}
			}
		}

		float alphaSet    = 1.02f;
		float emissionSet = 2.1f;

		while (alphaSet > 0)
		{
			alphaSet    -= 0.02f;
			emissionSet =  emissionSet > 0 ? emissionSet - 0.1f : 0f;

			foreach (PrefabMesh mesh in clearMeshList)
			{
				mesh.Obj.transform.localScale *= 1.02f;
				mesh.renderer.material.SetFloat(alpha,    alphaSet);
				mesh.renderer.material.SetFloat(emission, emissionSet);
			}

			yield return new WaitForSeconds(0.02f);
		}

		foreach (PrefabMesh mesh in clearMeshList)
		{
			Destroy(mesh.Obj);
		}
	}

	public void MoveRotationEffect()
	{
		if (rotationParticle != null)
			rotationParticle.Obj.transform.position -= Vector3.up;
	}

	public void CreateRotationEffect(ref Vector3 offset, ref Quaternion rotation)
	{
		rotationParticle = new ParticleRender(vfxRotation, offset, effectObj, rotation);

		Destroy(rotationParticle!.Obj, 0.3f);
		rotationParticle = null;
	}
}