using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class EffectSystem : MonoSingleton<EffectSystem>
{
	public static  GameObject       effectObj;
	public static  List<LineMesh>   lineMeshes;
	public static  List<PrefabMesh> gridMeshes;
	private        ParticleRender   rotationParticle;
	private const  string           vfxRotation = "Prefabs/VFX_Rotation";
	private const  string           vfxDrop     = "Prefabs/VFX_Drop";
	public static  float            lineGlowPower;
	private static VolumeProfile    postProcessVolume;
	private static ColorAdjustments volumeColor;
	private const  float            saturationOrigin = 20f;
	public static readonly int alpha      = Shader.PropertyToID("_Alpha");
	public static readonly int clear      = Shader.PropertyToID("_Clear");
	public static readonly int emission   = Shader.PropertyToID("_Emission");
	public static readonly int over       = Shader.PropertyToID("_GameOver");
	public static readonly int smoothness = Shader.PropertyToID("_Smoothness");
	public static readonly int color      = Shader.PropertyToID("_Color");
	public static readonly int power      = Shader.PropertyToID("_Power");

	public void Init()
	{
		rotationParticle = null;

		lineMeshes = RenderSystem.lineMeshList;
		gridMeshes = RenderSystem.gridMeshList;

		effectObj     = GameObject.Find("Effect");

		postProcessVolume = Resources.Load<VolumeProfile>("GlobalVolumeProfile");

		if (!postProcessVolume.TryGet(out volumeColor))
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}

	public void Reset()
	{
		lineGlowPower = lineMeshes[0].Renderer.material.GetFloat(power);
	}

	public static void DropEffect()
	{
		int yMax = GameManager.currentBlock.Tile.Select(coord => coord.Y).Prepend(-1).Max();

		foreach (Coord coord in GameManager.currentBlock.Tile)
		{
			if (coord.Y != yMax) continue;

			Vector3 offset = RenderSystem.startOffset + GameManager.currentBlock.pos.ToVector() + coord.ToVector() +
			                 new Vector3(0f, -GameManager.blockSize / 2f, 0f);
			ParticleRender ptc = new(vfxDrop, offset, effectObj, Quaternion.identity);
			Destroy(ptc.Obj, 3f);
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

	public static void SaturationChange()
	{
		volumeColor.saturation.value = volumeColor.saturation.value > 0f ? -100f : saturationOrigin;
	}

	public static int SaturationValue()
	{
		return volumeColor.saturation.value > 0f ? 0 : 1;
	}
}