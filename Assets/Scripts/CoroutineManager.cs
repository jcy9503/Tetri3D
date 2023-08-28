using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CoroutineManager : MonoBehaviour
{
#region Variable

	private delegate        void      Callback();
	private const           float     logicDownInterval = 1f;
	private const           float     audioTimeUnit     = 0.03f;
	private const           float     audioBGMInterval  = 3f;
	private const           float     audioSFXDestroy   = 1f;
	private                 Coroutine mainBGM;
	private const           float     cameraSpeed       = 0.5f;
	private const           float     cameraShakeAmount = 0.5f;
	private const           float     cameraShakeTime   = 0.2f;
	private                 Coroutine animFunc;
	private static readonly int       speed = Shader.PropertyToID("_Speed");

#endregion

#region Public Coroutine Call Methods

#region Logic

	public void GameStart()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);
		StartCoroutine(AudioPlayGameBGM());
		StartCoroutine(CameraGameStart(StartLogic));
	}

	public void GamePause()
	{
		GameManager.isPause = true;

		BurstSFX(AudioSystem.SFX_VALUE.PAUSE);
		PauseBGM(audioBGMInterval);
	}

	public void GameResume()
	{
		GameManager.isPause = false;

		BurstSFX(AudioSystem.SFX_VALUE.RESUME);
		ResumeBGM(audioBGMInterval);
		StartLogic();
	}

#endregion

#region Audio

	public void PlayMainMenuBGM()
	{
		if (mainBGM == null)
			mainBGM = StartCoroutine(AudioPlayMainMenuBGM());
		else
		{
			StopCoroutine(mainBGM);
			mainBGM = StartCoroutine(AudioPlayMainMenuBGM());
		}
	}

	public void PitchDownBGM(float acc)
	{
		StartCoroutine(AudioGameOverBGM(acc));
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

	public void CameraShake()
	{
		CameraSystem.isShaking = true;

		StartCoroutine(CameraShaking(() => CameraSystem.isShaking = false));
	}

	public void CameraFOVEffect()
	{
		GameManager.isPause = true;

		StartCoroutine(CameraFOVAudioEffect(() => GameManager.isPause = false));
	}

#endregion

#region UI

	public void OnClickPauseHome()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		StartCoroutine(UIGameHome("PlayScreen"));

		PlayMainMenuBGM();
	}

	public void OnClickMainMenuGameStart()
	{
		GameManager.isGameOver = false;
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		UISystem.Instance.playObjects["PauseScreen"].SetActive(false);

		StartCoroutine(UIFadeOutIn("MainScreen", "PlayScreen", 1f));
		GameStart();
	}

	public void OnClickMainMenuOption()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		StartCoroutine(UIFadeOutIn("MainScreen", "OptionScreen", 3f));
	}

	public void OnClickMainMenuLeaderBoard()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		StartCoroutine(UIFadeOutIn("MainScreen", "LeaderBoardScreen", 3f));
		UISystem.Instance.BoardReset();
	}

	public void OnClickOptionHome()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		StartCoroutine(UIFadeOutIn("OptionScreen", "MainScreen", 3f));
	}

	public void OnClickLeaderBoardHome()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		StartCoroutine(UIFadeOutIn("LeaderBoardScreen", "MainScreen", 3f));
	}

	public void GameOverScreen()
	{
		StartCoroutine(UIFadeOutIn("PlayScreen", "GameOverScreen", 1f));
	}

	public void OnClickGameOverHome()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		UISystem.Instance.GameOverSave();

		StartCoroutine(UIGameHome("GameOverScreen"));
	}

	public void OnClickGameOverRetry()
	{
		BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		UISystem.Instance.GameOverSave();

		StartCoroutine(UIFadeOutIn("GameOverScreen", "PlayScreen", 10f));

		GameManager.isGameOver = false;
		GameManager.Instance.Reset();

		StartLogic();

		StartCoroutine(AudioRepeatGameBGM());
	}

	public void UpdateScore(UISystem.SCORE_TYPE type, int addScore)
	{
		StartCoroutine(UIScoreAnimation(type, addScore));
	}

#region Effect

	public void GridEffect(List<int> cleared)
	{
		StartCoroutine(EffectClear(cleared));
	}

	public void GameOverEffect()
	{
		StartCoroutine(EffectGameOver());
	}

#endregion

#region Environment

	public void StartAnimChange()
	{
		animFunc = StartCoroutine(EnvAnimChange());
	}

	public void StopAnimChange()
	{
		StopCoroutine(animFunc);
	}

#endregion

#endregion

#endregion

#region Coroutines & Private Methods

#region Logic

	private void StartLogic()
	{
		GameManager.isPause = false;

		StartCoroutine(LogicBlockDown());
		StartCoroutine(CameraAngleCalculate());
	}

	private static IEnumerator LogicBlockDown()
	{
		while (!GameManager.isPause)
		{
			RenderSystem.RenderCurrentBlock();

			yield return new WaitForSeconds(logicDownInterval);

			GameManager.Instance.MoveBlockDown();
			EffectSystem.Instance.MoveRotationEffect();
		}
	}

#endregion

#region Audio

	private static IEnumerator AudioPlayMainMenuBGM()
	{
		AudioSystem.audioSourceBGM.clip = AudioSystem.bgmSource[0];
		AudioSystem.BGMVolume           = AudioSystem.baseBGMVolume;
		AudioSystem.BGMPitch            = 1f;
		AudioSystem.audioSourceBGM.Play();

		while (true)
		{
			if (!GameManager.isPause) break;

			if (!AudioSystem.BGMPlaying)
				AudioSystem.audioSourceBGM.Play();

			yield return new WaitForSeconds(audioBGMInterval);
		}
	}

	private static IEnumerator AudioGameOverBGM(float acc)
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

			if (GameManager.isPause) yield return null;

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
		AudioSystem.BGMVolume = AudioSystem.baseBGMVolume;
	}

	private static IEnumerator AudioFadeInBGM(float acc)
	{
		const float volUp = 0.01f;

		AudioSystem.BGMVolume = 0f;
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
		float  elapsedTime = 0f;
		Camera cam         = CameraSystem.mainCamera;

		do
		{
			elapsedTime += Time.deltaTime;

			cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation,
			                                          Quaternion.LookRotation(new Vector3(0f, -6.35f, 23.7f)),
			                                          elapsedTime * cameraSpeed);

			yield return null;
		} while (Quaternion.Angle(cam.transform.rotation,
		                          Quaternion.LookRotation(new Vector3(0f, -6.35f, 23.7f))) > 1f);

		cam.transform.rotation = CameraSystem.initRotation;

		func.Invoke();
	}

	private static IEnumerator CameraMainMenu()
	{
		float     elapsedTime = 0f;
		Camera    cam         = CameraSystem.mainCamera;
		Transform rotator     = CameraSystem.rotatorTr;

		do
		{
			elapsedTime += Time.deltaTime;

			cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation,
			                                          Quaternion.LookRotation(Vector3.right),
			                                          elapsedTime * cameraSpeed);
			rotator.rotation = Quaternion.Slerp(rotator.rotation,
			                                    Quaternion.Euler(Vector3.zero),
			                                    elapsedTime * cameraSpeed);

			yield return null;
		} while (Quaternion.Angle(cam.transform.rotation,
		                          Quaternion.LookRotation(Vector3.right)) > 1f);

		cam.transform.rotation = Quaternion.LookRotation(Vector3.right);
		rotator.rotation       = Quaternion.Euler(Vector3.zero);
	}

	private static IEnumerator CameraAngleCalculate()
	{
		while (!GameManager.isPause)
		{
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

	private static IEnumerator CameraFOVAudioEffect(Callback func)
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

		func.Invoke();
	}

	private static void CameraFOVEffectClear(float originSpeed)
	{
		GameManager.isPause = false;
		GameManager.grid.Mesh.MRenderer.material.SetFloat(speed, originSpeed);
		AudioSystem.audioSourceBGM.pitch = 1f;
	}

#endregion

#region UI

	public static IEnumerator UIFadeOutIn(string fadeOut, string fadeIn, float acc)
	{
		const float alphaUnit = 0.02f;
		float       alphaSet  = 1f;

	#region Fade Out

		foreach (KeyValuePair<string,UIButton> button in UISystem.Instance.buttons[fadeOut])
		{
			button.Value.btn.interactable = false;
		}

		while (alphaSet >= 0f)
		{
			alphaSet                                       -= alphaUnit * acc;
			UISystem.Instance.screenObjects[fadeOut].alpha =  alphaSet;

			yield return new WaitForSeconds(0.01f);
		}

		UISystem.Instance.screenObjects[fadeOut].alpha = 0f;

	#endregion

	#region Fade In

		UISystem.Instance.screenObjects[fadeIn].gameObject.SetActive(true);
		UISystem.Instance.screenObjects[fadeIn].alpha = 0f;

		foreach (KeyValuePair<string, UIButton> button in UISystem.Instance.buttons[fadeIn])
		{
			button.Value.btn.interactable = false;
		}

		alphaSet = 0f;

		while (alphaSet <= 1f)
		{
			alphaSet                                      += alphaUnit * acc;
			UISystem.Instance.screenObjects[fadeIn].alpha =  alphaSet;

			yield return new WaitForSeconds(0.01f);
		}

		UISystem.Instance.screenObjects[fadeIn].alpha = 1f;

	#endregion

	#region After

		foreach (KeyValuePair<string, UIButton> button in UISystem.Instance.buttons[fadeOut])
		{
			button.Value.btn.interactable = true;
		}

		UISystem.Instance.screenObjects[fadeOut].gameObject.SetActive(false);

		foreach (KeyValuePair<string, UIButton> button in UISystem.Instance.buttons[fadeIn])
		{
			button.Value.btn.interactable = true;
		}

	#endregion
	}

	private IEnumerator UIGameHome(string curScreen)
	{
		StartCoroutine(UIFadeOutIn(curScreen, "MainScreen", 1f));
		StartCoroutine(AudioFadeOutBGM(3f));

		yield return StartCoroutine(CameraMainMenu());

		GameManager.isPause    = true;
		GameManager.isGameOver = true;

		PlayMainMenuBGM();
	}

	private static IEnumerator UIScoreAnimation(UISystem.SCORE_TYPE type, int addScore)
	{
		int scoreTp = type == UISystem.SCORE_TYPE.PLAY
			? GameManager.totalScore - addScore
			: 0;
		int save  = 0;
		int step  = 1;
		int digit = GameManager.totalScore.ToString().Length;

		const float interval  = 0.005f;
		int         loopCount = type == UISystem.SCORE_TYPE.PLAY ? 2 : 5;
		int         score     = GameManager.totalScore;
		TMP_Text    tmpScore  = UISystem.Instance.scoreTxt[(int)type];

		tmpScore.text = 0.ToString($"D{digit.ToString()}");

		yield return new WaitForSeconds(type == UISystem.SCORE_TYPE.GAME_OVER ? 1.3f : 0f);

		while (scoreTp < score)
		{
			int loop = loopCount;

			while (loop-- >= 0)
			{
				int tp = scoreTp;

				while (step * 10 > tp + save)
				{
					yield return new WaitForSeconds(interval);

					tmpScore.text =  (tp + save).ToString($"D{digit.ToString()}");
					tp            += step;
				}
			}

			while (score % (step * 10) > scoreTp + save)
			{
				yield return new WaitForSeconds(interval);

				tmpScore.text =  (scoreTp + save).ToString($"D{digit.ToString()}");
				scoreTp       += step;
			}

			yield return new WaitForSeconds(interval);

			save          += scoreTp;
			tmpScore.text =  save.ToString($"D{digit.ToString()}");
			step          *= 10;
			scoreTp       =  step;
		}

		if (type == UISystem.SCORE_TYPE.PLAY) yield break;

		float orgSize = tmpScore.fontSize;

		for (int i = 0; i < 200; ++i)
		{
			tmpScore.characterSpacing += 0.0001f * i;
			tmpScore.fontSize         =  orgSize + 0.04f * i;

			yield return new WaitForSeconds(0.001f);
		}
	}

#endregion

#region Effect

	private static IEnumerator EffectGridDestruction()
	{
		const float   alphaUnit = 0.01f;
		float         alphaSet  = GameManager.grid.Mesh.MRenderer.material.GetFloat(EffectSystem.alpha) + alphaUnit;
		Vector3       targetLoc = GameManager.grid.Mesh.Obj.transform.position - Vector3.up * 5f;
		float         glowSet   = EffectSystem.lineGlowPower + EffectSystem.lineGlowPower * 0.01f;
		const float   range     = 0.15f;
		List<Vector3> listRd    = new();

		for (int i = 0; i < 24; ++i)
		{
			listRd.Add(new Vector3(Random.Range(-range, range),
			                       Random.Range(-range, range),
			                       Random.Range(-range, range)));
		}

		float elapsedTime = 0f;

		while ((GameManager.grid.Mesh.Obj.transform.position - targetLoc).magnitude > 0.001f)
		{
			alphaSet    -= 0.01f;
			glowSet     -= EffectSystem.lineGlowPower * 0.01f;
			elapsedTime += Time.deltaTime;

			GameManager.grid.Mesh.Obj.transform.position =
				Vector3.Lerp(GameManager.grid.Mesh.Obj.transform.position, targetLoc, elapsedTime);
			GameManager.grid.Mesh.MRenderer.material.SetFloat(EffectSystem.alpha, Mathf.Max(alphaSet, 0f));

			for (int i = 0; i < EffectSystem.lineMeshes.Count; ++i)
			{
				EffectSystem.lineMeshes[i].Renderer.material.SetFloat(EffectSystem.alpha, alphaSet);
				EffectSystem.lineMeshes[i].Renderer.SetPosition(0, EffectSystem.lineMeshes[i].Renderer.GetPosition(0) +
				                                                   listRd[i * 2]);
				EffectSystem.lineMeshes[i].Renderer.material.SetFloat(EffectSystem.power, glowSet);
				EffectSystem.lineMeshes[i].Renderer.SetPosition(1, EffectSystem.lineMeshes[i].Renderer.GetPosition(1) +
				                                                   listRd[i * 2 + 1]);
			}

			yield return new WaitForSeconds(0.02f);
		}

		Destroy(GameManager.grid.Mesh.Obj);

		foreach (LineMesh mesh in EffectSystem.lineMeshes)
		{
			Destroy(mesh.Obj);
		}

		EffectSystem.lineMeshes.Clear();
	}

	private IEnumerator EffectGameOver()
	{
		RenderSystem.ClearCurrentBlock();
		RenderSystem.ClearShadowBlock();

		const float explosionForce  = 200f;
		float       explosionRadius = GameManager.grid.SizeY;
		const float torque          = 50f;

		foreach (PrefabMesh mesh in EffectSystem.gridMeshes)
		{
			Rigidbody rb = mesh.Obj.AddComponent<Rigidbody>();

			rb.AddExplosionForce(explosionForce,
			                     new Vector3(0f, RenderSystem.startOffset.y - mesh.pos.Y -
			                                     GameManager.blockSize / 2f, 0f), explosionRadius);

			Vector3 rdVec = new(Random.Range(-torque, torque), Random.Range(-torque, torque),
			                    Random.Range(-torque, torque));
			rb.AddTorque(rdVec);
			rb.angularDrag = Random.Range(0.5f, 2f);

			mesh.renderer.material.SetFloat(EffectSystem.over,       1f);
			mesh.renderer.material.SetFloat(EffectSystem.smoothness, 0f);
		}

		yield return StartCoroutine(EffectGridDestruction());

		float alphaSet = 1.01f;

		while (alphaSet > 0f)
		{
			alphaSet -= 0.01f;

			foreach (PrefabMesh mesh in EffectSystem.gridMeshes)
			{
				mesh.renderer.material.SetFloat(EffectSystem.alpha, alphaSet);
			}

			yield return new WaitForSeconds(0.02f);
		}

		foreach (PrefabMesh mesh in EffectSystem.gridMeshes)
		{
			Destroy(mesh.Obj);
		}

		EffectSystem.gridMeshes.Clear();
	}

	private static IEnumerator EffectClear(List<int> cleared)
	{
		List<PrefabMesh> clearMeshList   = new();
		const float      explosionForce  = 900f;
		float            explosionRadius = GameManager.grid.SizeX + GameManager.grid.SizeZ;
		const float      explosionUp     = 5f;
		const float      torque          = 100f;

		foreach (int height in cleared)
		{
			for (int i = 0; i < GameManager.grid.SizeX; ++i)
			{
				for (int j = 0; j < GameManager.grid.SizeZ; ++j)
				{
					Vector3 offset = new(i, -height, j);
					PrefabMesh mesh = new("Prefabs/Mesh_Block", RenderSystem.startOffset + offset, Block.MAT_PATH[^1],
					                      new Coord(i, height, j), ShadowCastingMode.Off);
					mesh.renderer.material.SetFloat(EffectSystem.clear,    1f);
					mesh.renderer.material.SetFloat(EffectSystem.color,    Random.Range(0f, 1f));
					mesh.renderer.material.SetFloat(EffectSystem.emission, 10f);

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
					mesh.Obj.transform.parent = EffectSystem.effectObj.transform;
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
				mesh.renderer.material.SetFloat(EffectSystem.alpha,    alphaSet);
				mesh.renderer.material.SetFloat(EffectSystem.emission, emissionSet);
			}

			yield return new WaitForSeconds(0.02f);
		}

		foreach (PrefabMesh mesh in clearMeshList)
		{
			Destroy(mesh.Obj);
		}
	}

#endregion

#region Environment

	private static IEnumerator EnvAnimChange()
	{
		while (true)
		{
			if (GameManager.isGameOver) break;

			int randObj = Random.Range(0, EnvironmentSystem.cubeAnimators.Length);
			int randInt = Random.Range(0, EnvironmentSystem.totalAnim);

			if (!EnvironmentSystem.cubesFloating[randObj])
			{
				EnvironmentSystem.cubeAnimators[randObj].SetInteger(EnvironmentSystem.phase, randInt);
			}

			yield return new WaitForSeconds(1f);
		}
	}

	private static IEnumerator EnvAnimStart()
	{
		const float speedUp = 0.01f;

		while (EnvironmentSystem.cubeAnimators[0].speed < 1f)
		{
			foreach (Animator anim in EnvironmentSystem.cubeAnimators)
			{
				anim.speed = Mathf.Clamp(anim.speed + speedUp, 0f, 1f);
			}

			yield return new WaitForSeconds(0.1f);
		}

		foreach (Animator anim in EnvironmentSystem.cubeAnimators)
		{
			anim.speed = 1f;
		}
	}

	private IEnumerator EnvAnimStop()
	{
		const float slowDown = 0.01f;

		while (EnvironmentSystem.cubeAnimators[0].speed > 0f)
		{
			foreach (Animator anim in EnvironmentSystem.cubeAnimators)
			{
				anim.speed = Mathf.Clamp(anim.speed - slowDown, 0f, 1f);
			}

			yield return new WaitForSeconds(0.1f);
		}

		foreach (Animator anim in EnvironmentSystem.cubeAnimators)
		{
			anim.speed = 0f;
		}
	}

	private IEnumerator EnvAnimRestart()
	{
		yield return StartCoroutine(EnvAnimStart());

		StartCoroutine(EnvAnimChange());
	}

#endregion

#endregion
}