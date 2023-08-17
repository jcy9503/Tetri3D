using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class EnvironmentSystem : MonoSingleton<EnvironmentSystem>
{
	private                 GameObject      cubeMeshes;
	private                 List<Transform> cubeTrs;
	public static           Animator[]      cubeAnimators;
	private                 Renderer[]      cubeRenderers;
	public static           List<bool>      cubesFloating;
	public const            int             totalAnim = 4;
	private static readonly int             speed = Shader.PropertyToID("_Speed");
	public static readonly  int             phase = Animator.StringToHash("Phase");

	public void Init()
	{
		cubeTrs       = new List<Transform>();
		cubesFloating = new List<bool>();

		cubeMeshes    = GameObject.Find("Meshes");
		cubeAnimators = cubeMeshes.GetComponentsInChildren<Animator>();
		cubeRenderers = cubeMeshes.GetComponentsInChildren<Renderer>();

		Transform[] trs = cubeMeshes.GetComponentsInChildren<Transform>();

		foreach (Transform tr in trs)
		{
			if (tr.parent != cubeMeshes.transform) continue;

			cubeTrs.Add(tr);
		}

		foreach (Transform tr in cubeTrs)
		{
			float randFloat = Random.Range(0f, 360f);

			tr.rotation = Quaternion.Euler(0f, randFloat, 0f);

			randFloat = Random.Range(0.3f, 1.5f);

			tr.localScale = Vector3.one * randFloat;
		}

		foreach (Animator animator in cubeAnimators)
		{
			float randFloat = Random.Range(0.3f, 1f);
			int   randInt   = Mathf.Clamp(Random.Range(-2, totalAnim), 0, totalAnim - 1);

			animator.speed = randFloat;
			animator.SetInteger(phase, randInt);

			if (randInt != 0)
			{
				cubesFloating.Add(true);
				animator.gameObject.transform.rotation *= Quaternion.Euler(Random.Range(0f, 360f), 0f,
				                                                           Random.Range(0f, 360f));
			}
			else
			{
				cubesFloating.Add(false);
			}
		}

		foreach (Renderer rd in cubeRenderers)
		{
			rd.material.SetFloat(speed, Random.Range(0.15f, 0.45f));
		}

		GameManager.Instance.coroutineManager.StartAnimChange();
	}
}