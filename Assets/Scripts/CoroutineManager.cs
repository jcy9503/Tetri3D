using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
#region Variable

	public delegate void Callback();

	private const float logicDownInterval = 1f;

	private const float audioTimeUnit    = 0.03f;
	private const float audioBGMInterval = 3f;
	private const float audioSFXDestroy  = 1f;

	private Coroutine mainBGM;

	private const float cameraLerpAmount  = 0.01f;
	private const float cameraShakeAmount = 0.5f;
	private const float cameraShakeTime   = 0.2f;

	private static readonly int speed = Shader.PropertyToID("_Speed");

#endregion

#region Public Coroutine Call Methods

#region Logic

	public void GameStart()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);
		StartCoroutine(AudioPlayGameBGM());
		StartCoroutine(CameraGameStart(StartLogic));
	}

#endregion

#region Audio

	public void PlayMainBGM()
	{
		mainBGM = StartCoroutine(AudioPlayMainBGM());
	}

	public void PitchDownBGM(float acc)
	{
		StartCoroutine(AudioPitchDownBGM(acc));
	}

	public void PauseBGM(float acc)
	{
		StartCoroutine(AudioFadeOutBGM(acc));
	}

	public void ResumeBGM(float acc)
	{
		StartCoroutine(AudioFadeInBGM(acc));
	}

	public void PlayRandomSFX(AudioSystem.SFX_VALUE start, AudioSystem.SFX_VALUE end)
	{
		int rand = Random.Range((int)start, (int)end + 1);

		StartCoroutine(AudioPlaySFX((AudioSystem.SFX_VALUE)rand));
	}

	public void BurstSFX(AudioSystem.SFX_VALUE value)
	{
		StartCoroutine(AudioPlaySFX(value));
	}

#endregion

#region Camera

	public void CameraShake(Callback func)
	{
		CameraSystem.isShaking = true;

		StartCoroutine(CameraShaking(func));
	}

	public void CameraFOVEffect()
	{
		GameManager.isPause = true;

		StartCoroutine(CameraFOVAudioEffect());
	}

#endregion

#endregion

#region Coroutines & Private Methods

	private void StartLogic()
	{
		StartCoroutine(LogicBlockDown());
		StartCoroutine(CameraAngleCalculate());

		GameManager.isPause = false;
	}

#region Logic

	private static IEnumerator LogicBlockDown()
	{
		while (!GameManager.isPause)
		{
			RenderSystem.RenderCurrentBlock();

			if (!GameManager.grid.IsPlaneEmpty(0))
			{
				GameManager.isGameOver = true;
				GameManager.isPause    = true;

				break;
			}

			GameManager.Instance.MoveBlockDown();
			EffectSystem.Instance.MoveRotationEffect();

			yield return new WaitForSeconds(logicDownInterval);
		}
	}

#endregion

#region Audio

	private static IEnumerator AudioPlayMainBGM()
	{
		AudioSystem.audioSourceBGM.clip = AudioSystem.bgmSource[0];
		AudioSystem.BGMVolume           = AudioSystem.BGMVolume;
		AudioSystem.BGMPitch            = 1f;
		AudioSystem.audioSourceBGM.Play();

		while (true)
		{
			if (GameManager.isGameOver) break;

			if (!AudioSystem.BGMPlaying)
				AudioSystem.audioSourceBGM.Play();

			yield return new WaitForSeconds(audioBGMInterval);
		}
	}

	private static IEnumerator AudioPitchDownBGM(float acc)
	{
		const float volDown   = 0.01f;
		const float pitchDown = 0.01f;

		while (AudioSystem.BGMVolume > 0f)
		{
			AudioSystem.BGMVolume -= volDown   * acc;
			AudioSystem.BGMPitch  -= pitchDown * acc;

			yield return new WaitForSeconds(audioTimeUnit);
		}

		AudioSystem.audioSourceBGM.Stop();
		AudioSystem.BGMVolume = AudioSystem.baseBGMVolume;
		AudioSystem.BGMPitch  = 1f;
	}

	private static IEnumerator AudioRepeatGameBGM()
	{
		while (true)
		{
			if (GameManager.isGameOver) break;

			if (GameManager.isPause) continue;

			if (!AudioSystem.BGMPlaying)
				AudioSystem.RandomPlayBGM();

			yield return new WaitForSeconds(audioBGMInterval);
		}
	}

	private static IEnumerator AudioFadeOutBGM(float acc)
	{
		const float volDown = 0.01f;

		while (AudioSystem.BGMVolume > 0f)
		{
			AudioSystem.BGMVolume -= volDown * acc;

			yield return new WaitForSeconds(audioTimeUnit);
		}

		AudioSystem.BGMVolume = 0f;
		AudioSystem.audioSourceBGM.Pause();
	}

	private static IEnumerator AudioFadeInBGM(float acc)
	{
		const float volUp = 0.01f;

		AudioSystem.audioSourceBGM.Play();

		while (AudioSystem.BGMVolume < AudioSystem.baseBGMVolume)
		{
			AudioSystem.BGMVolume += volUp * acc;

			yield return new WaitForSeconds(audioTimeUnit);
		}

		AudioSystem.BGMVolume = AudioSystem.baseBGMVolume;
	}

	private static IEnumerator AudioPlaySFX(AudioSystem.SFX_VALUE value)
	{
		AudioSystem.sfxIdx = Mathf.Clamp(AudioSystem.sfxIdx + 1, 0, AudioSystem.audioSourcesSFX.Length - 1);
		AudioSystem.audioSourcesSFX[AudioSystem.sfxIdx].volume = AudioSystem.SFXVolume;
		AudioSystem.audioSourcesSFX[AudioSystem.sfxIdx].PlayOneShot(AudioSystem.sfxSource[(int)value]);

		yield return new WaitForSeconds(audioSFXDestroy);

		--AudioSystem.sfxIdx;
	}

	private IEnumerator AudioPlayGameBGM()
	{
		yield return StartCoroutine(AudioFadeOutBGM(audioBGMInterval));

		StopCoroutine(mainBGM);
		mainBGM = StartCoroutine(AudioRepeatGameBGM());
	}

#endregion

#region Camera

	private static IEnumerator CameraGameStart(Callback func)
	{
		const float unit        = 0.05f;
		float       elapsedTime = 0f;

		do
		{
			elapsedTime += unit;

			CameraSystem.mainCamera.transform.rotation = Quaternion.Slerp(CameraSystem.mainCamera.transform.rotation,
			                                                              Quaternion.LookRotation(
				                                                               new Vector3(0f, -6.35f, 23.7f)), 0.01f);

			yield return new WaitForSeconds(unit);
		} while (elapsedTime < 2f);

		func.Invoke();
	}

	public static IEnumerator CameraMainMenu()
	{
		const float unit        = 0.05f;
		float       elapsedTime = 0f;

		while (elapsedTime < 2f)
		{
			CameraSystem.mainCamera.transform.rotation = Quaternion.Slerp(CameraSystem.mainCamera.transform.rotation,
			                                                              Quaternion.LookRotation(Vector3.right),
			                                                              cameraLerpAmount);

			yield return new WaitForSeconds(unit);

			elapsedTime += unit;
		}

		CameraSystem.mainCamera.transform.rotation = Quaternion.LookRotation(Vector3.right);
	}

	private static IEnumerator CameraAngleCalculate()
	{
		while (!GameManager.isPause)
		{
			if (GameManager.isGameOver) break;

			CameraSystem.viewAngle = CameraSystem.rotatorTr.rotation.eulerAngles.y switch
			{
				<= 45f or > 315f   => 0,
				<= 135f and > 45f  => 1,
				<= 225f and > 135f => 2,
				_                  => 3
			};

			yield return new WaitForSeconds(InputSystem.defaultKeyInterval);
		}
	}

	private static IEnumerator CameraShaking(Callback func)
	{
		float timer = 0;

		while (timer <= cameraShakeTime)
		{
			CameraSystem.rotatorTr.position =  (Vector3)Random.insideUnitCircle * cameraShakeAmount;
			timer                           += Time.deltaTime;

			yield return null;
		}

		func.Invoke();
	}

	private static IEnumerator CameraFOVAudioEffect()
	{
		const float target      = 120f;
		float       originFOV   = CameraSystem.mainCamera.fieldOfView;
		Material    material    = GameManager.grid.Mesh.MRenderer.material;
		float       originSpeed = material.GetFloat(speed);

		material.SetFloat(speed, 10f);

		while (CameraSystem.mainCamera.fieldOfView < target - 1f)
		{
			CameraSystem.mainCamera.fieldOfView =  Mathf.Lerp(CameraSystem.mainCamera.fieldOfView, target, 0.1f);
			AudioSystem.audioSourceBGM.pitch    -= 0.02f;

			yield return new WaitForSeconds(0.01f);
		}

		while (CameraSystem.mainCamera.fieldOfView > originFOV + 1f)
		{
			CameraSystem.mainCamera.fieldOfView =  Mathf.Lerp(CameraSystem.mainCamera.fieldOfView, originFOV, 0.2f);
			AudioSystem.audioSourceBGM.pitch    += 0.02f;

			yield return new WaitForSeconds(0.01f);
		}

		CameraSystem.mainCamera.fieldOfView = originFOV;

		CameraFOVEffectClear(originSpeed);
	}

	private static void CameraFOVEffectClear(float originSpeed)
	{
		GameManager.isPause = false;
		GameManager.grid.Mesh.MRenderer.material.SetFloat(speed, originSpeed);
		AudioSystem.audioSourceBGM.pitch = 1f;
	}

#endregion

#endregion
}