using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AudioSystem : MonoSingleton<AudioSystem>
{
	public enum SFX_VALUE
	{
		BOOL = 0,
		CLICK,
		CLOSE,
		DROP1,
		DROP2,
		GAME_OVER,
		SHIFT,
		ITEM,
		ROTATE1,
		ROTATE2,
		SWITCH,
		UNAVAILABLE,
		HARD_DROP1,
		HARD_DROP2,
		HARD_DROP3,
		HARD_DROP4,
		HARD_DROP5,
		MOVE,
		PAUSE,
		CLEAR,
		TETRIS1,
		TETRIS2,
		RESUME,
	}

	private readonly string[] BGM_PATH =
	{
		"BGM/BGM01",
		"BGM/BGM02",
		"BGM/BGM03",
		"BGM/BGM04",
		"BGM/BGM05",
		"BGM/BGM06",
		"BGM/BGM07",
		"BGM/BGM08",
		"BGM/BGM09",
		"BGM/BGM10",
		"BGM/BGM11",
		"BGM/BGM12",
	};
	private readonly string[] SFX_PATH =
	{
		"SFX/Bool",
		"SFX/Click",
		"SFX/Close",
		"SFX/Drop1",
		"SFX/Drop2",
		"SFX/GameOver",
		"SFX/Shift",
		"SFX/Item",
		"SFX/Rotate1",
		"SFX/Rotate2",
		"SFX/Switch",
		"SFX/Unavailable",
		"SFX/HardDrop01",
		"SFX/HardDrop02",
		"SFX/HardDrop03",
		"SFX/HardDrop04",
		"SFX/HardDrop05",
		"SFX/Move",
		"SFX/Pause",
		"SFX/Clear",
		"SFX/Tetris1",
		"SFX/Tetris2",
		"SFX/Resume",
	};
	public static AudioSource     audioSourceBGM;
	public static AudioSource[]   audioSourcesSFX;
	private static List<AudioClip> bgmSource;
	private static List<AudioClip> sfxSource;
	private static int             sfxIdx;
	private static float           bgmVolume = 0.2f;
	public float BGMVolume
	{
		get => bgmVolume;
		set
		{
			bgmVolume             = Mathf.Clamp(value, 0f, 0.4f);
			audioSourceBGM.volume = bgmVolume;
		}
	}
	public const float bgmVolumeAdj = 3f;
	private      float sfxVolume    = 1f;
	public float SFXVolume
	{
		get => sfxVolume;
		set
		{
			sfxVolume = Mathf.Clamp(value, 0f, 1f);

			for (int i = 0; i < audioSourcesSFX.Length; i++)
			{
				audioSourcesSFX[i].volume = sfxVolume;
			}
		}
	}
	private const float     audioInterval = 2f;
	private       Coroutine mainBGM;

	public AudioSystem()
	{
		Init();
	}

	protected override void Init()
	{
		audioSourceBGM             = CameraSystem.mainCameraObj.AddComponent<AudioSource>();
		audioSourceBGM.playOnAwake = true;
		audioSourceBGM.loop        = false;
		audioSourceBGM.volume      = bgmVolume;
		bgmSource                  = new List<AudioClip>();

		foreach (string path in BGM_PATH)
		{
			bgmSource.Add(Resources.Load<AudioClip>(path));
		}

		mainBGM = StartCoroutine(PlayMainBGM());

		audioSourcesSFX = RenderSystem.gridObj.GetComponentsInChildren<AudioSource>();
		sfxIdx          = -1;

		sfxSource = new List<AudioClip>();

		foreach (string path in SFX_PATH)
		{
			sfxSource.Add(Resources.Load<AudioClip>(path));
		}
	}

	private IEnumerator PlayMainBGM()
	{
		audioSourceBGM.clip   = bgmSource[0];
		audioSourceBGM.volume = bgmVolume;
		audioSourceBGM.pitch  = 1f;
		audioSourceBGM.Play();

		while (true)
		{
			if (GameManager.isGameOver) break;
			if (!audioSourceBGM.isPlaying)
				audioSourceBGM.Play();

			yield return new WaitForSeconds(audioInterval);
		}
	}

	private IEnumerator RewindGameBGM()
	{
		while (true)
		{
			if (GameManager.isGameOver) break;

			if (GameManager.isPause) continue;

			if (!audioSourceBGM.isPlaying)
				RandomPlayBGM();

			yield return new WaitForSeconds(audioInterval);
		}
	}

	private void RandomPlayBGM()
	{
		audioSourceBGM.volume = bgmVolume;
		audioSourceBGM.clip   = bgmSource[Random.Range(1, bgmSource.Count)];
		audioSourceBGM.Play();
	}

	public IEnumerator PitchDownBGM(float acc)
	{
		const float volDown   = 0.01f;
		const float pitchDown = 0.01f;

		while (audioSourceBGM.volume > 0f)
		{
			audioSourceBGM.volume -= volDown   * acc;
			audioSourceBGM.pitch  -= pitchDown * acc;

			yield return new WaitForSeconds(0.03f);
		}

		audioSourceBGM.Stop();
		audioSourceBGM.volume = bgmVolume;
		audioSourceBGM.pitch  = 1f;
	}

	private IEnumerator FadeOutBGM(float acc)
	{
		const float volDown = 0.01f;

		while (audioSourceBGM.volume > 0f)
		{
			audioSourceBGM.volume -= acc * volDown;

			yield return new WaitForSeconds(0.03f);
		}

		audioSourceBGM.volume = 0f;
		audioSourceBGM.Pause();
	}

	private IEnumerator FadeInBGM(float acc)
	{
		const float volUp = 0.01f;

		audioSourceBGM.Play();

		while (audioSourceBGM.volume < bgmVolume)
		{
			audioSourceBGM.volume += volUp * acc;

			yield return new WaitForSeconds(0.03f);
		}

		audioSourceBGM.volume = bgmVolume;
	}

	public void PauseBGM(float acc)
	{
		StartCoroutine(FadeOutBGM(acc));
	}

	public void ResumeBGM(float acc)
	{
		StartCoroutine(FadeInBGM(acc));
	}

	private IEnumerator PlaySFX(SFX_VALUE value)
	{
		sfxIdx                         = Mathf.Clamp(sfxIdx + 1, 0, audioSourcesSFX.Length - 1);
		audioSourcesSFX[sfxIdx].volume = sfxVolume;
		audioSourcesSFX[sfxIdx].PlayOneShot(sfxSource[(int)value]);

		yield return new WaitForSeconds(1f);

		--sfxIdx;
	}

	public void PlayRandomSFX(SFX_VALUE start, SFX_VALUE end)
	{
		int rand = Random.Range((int)start, (int)end + 1);

		StartCoroutine(PlaySFX((SFX_VALUE)rand));
	}

	private IEnumerator PlayGameBGM()
	{
		StartCoroutine(PlaySFX(SFX_VALUE.CLICK));

		yield return StartCoroutine(FadeOutBGM(bgmVolumeAdj));

		StopCoroutine(mainBGM);
		mainBGM = StartCoroutine(RewindGameBGM());
	}

	public void GameStart()
	{
		StartCoroutine(PlayGameBGM());
	}

	public void MainMenu()
	{
		mainBGM = StartCoroutine(PlayMainBGM());
	}

	public void BurstSFX(SFX_VALUE value)
	{
		StartCoroutine(PlaySFX(value));
	}
}