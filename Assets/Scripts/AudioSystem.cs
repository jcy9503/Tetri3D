using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
		PUNISH1,
		PUNISH2,
		PUNISH3,
		PUNISH4,
		PUNISH5,
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
		"SFX/Punish1",
		"SFX/Punish2",
		"SFX/Punish3",
		"SFX/Punish4",
		"SFX/Punish5",
	};

	public static  AudioSource     audioSourceBGM;
	public static  AudioSource[]   audioSourcesSFX;
	public static  List<AudioClip> bgmSource;
	public static  List<AudioClip> sfxSource;
	public static  int             sfxIdx;
	public static  float           baseBGMVolume = 0.2f;
	private static float           bgmVolume     = baseBGMVolume;
	public static float BGMVolume
	{
		get => bgmVolume;
		set
		{
			bgmVolume             = Mathf.Clamp(value, 0f, baseBGMVolume);
			audioSourceBGM.volume = bgmVolume;
		}
	}
	private static float bgmPitch = 1f;
	public static float BGMPitch
	{
		get => bgmPitch;
		set
		{
			bgmPitch             = Mathf.Clamp(value, 0f, 1f);
			audioSourceBGM.pitch = bgmPitch;
		}
	}
	public static  bool  BGMPlaying => audioSourceBGM.isPlaying;
	public static  float baseSFXVolume = 1f;
	private static float sfxVolume     = 1f;
	public static float SFXVolume
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

	public void Init()
	{
		bgmSource = new List<AudioClip>();

		sfxIdx = -1;

		sfxSource = new List<AudioClip>();

		audioSourceBGM  = CameraSystem.mainCameraObj.AddComponent<AudioSource>();
		audioSourcesSFX = RenderSystem.gridObj.GetComponentsInChildren<AudioSource>();

		audioSourceBGM.playOnAwake = true;
		audioSourceBGM.loop        = false;
		audioSourceBGM.volume      = bgmVolume;

		foreach (string path in BGM_PATH)
		{
			bgmSource.Add(Resources.Load<AudioClip>(path));
		}

		foreach (string path in SFX_PATH)
		{
			sfxSource.Add(Resources.Load<AudioClip>(path));
		}

		GameManager.Instance.coroutineManager.PlayMainMenuBGM();
	}

	public static void RandomPlayBGM()
	{
		audioSourceBGM.volume = bgmVolume;
		audioSourceBGM.clip   = bgmSource[Random.Range(1, bgmSource.Count)];
		audioSourceBGM.Play();
	}
}