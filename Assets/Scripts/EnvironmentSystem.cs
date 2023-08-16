using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class EnvironmentSystem : MonoSingleton<EnvironmentSystem>
{
	private                 GameObject      cubeMeshes;
	private                 List<Transform> cubeTrs;
	private                 Animator[]      cubeAnimators;
	private                 Renderer[]      cubeRenderers;
	private                 List<bool>      cubesFloating;
	private const           int             totalAnim = 4;
	public                  Coroutine       animFunc;
	private static readonly int             speed = Shader.PropertyToID("_Speed");
	private static readonly int             phase = Animator.StringToHash("Phase");

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

		animFunc = StartCoroutine(AnimChange());
	}

	private IEnumerator AnimChange()
	{
		while (true)
		{
			if (GameManager.isGameOver) break;

			int randObj = Random.Range(0, cubeAnimators.Length);
			int randInt = Random.Range(0, totalAnim);

			if (!cubesFloating[randObj])
			{
				cubeAnimators[randObj].SetInteger(phase, randInt);
			}

			yield return new WaitForSeconds(1.0f);
		}
	}

	public IEnumerator AnimStop()
	{
		const float slowDown = 0.01f;

		while (cubeAnimators[0].speed > 0f)
		{
			foreach (Animator anim in cubeAnimators)
			{
				anim.speed = Mathf.Clamp(anim.speed - slowDown, 0f, 1f);
			}

			yield return new WaitForSeconds(0.1f);
		}

		foreach (Animator anim in cubeAnimators)
		{
			anim.speed = 0f;
		}
	}

	private IEnumerator AnimStart()
	{
		const float speedUp = 0.01f;

		while (cubeAnimators[0].speed < 1f)
		{
			foreach (Animator anim in cubeAnimators)
			{
				anim.speed = Mathf.Clamp(anim.speed + speedUp, 0f, 1f);
			}

			yield return new WaitForSeconds(0.1f);
		}

		foreach (Animator anim in cubeAnimators)
		{
			anim.speed = 1f;
		}
	}

	private IEnumerator AnimRestart()
	{
		yield return StartCoroutine(AnimStart());

		StartCoroutine(AnimChange());
	}
}