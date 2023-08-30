using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.Events;

public sealed class UISystem : MonoSingleton<UISystem>
{
	public Dictionary<string, CanvasGroup>                  screenObjects;
	public Dictionary<string, Dictionary<string, UIButton>> buttons;

	private readonly string[] SCREEN_STR =
	{
		"MainScreen",
		"PlayScreen",
		"OptionScreen",
		"LeaderBoardScreen",
		"GameOverScreen",
	};

	public enum SCORE_TYPE
	{
		PLAY = 0,
		GAME_OVER,
	}

	public TMP_Text[] scoreTxt;

#region Main Screen

	private readonly string[] MAIN_BTN_STR =
	{
		"Start",
		"Option",
		"LeaderBoard",
		"Quit",
		"QuitYes",
		"QuitNo",
	};

	private GameObject mainQuitPanel;

#endregion

#region Play Screen

	public Dictionary<string, GameObject> playObjects;

	private readonly string[] PLAY_OBJ_STR =
	{
		"PauseScreen",
		"ControlScreen",
		"BlockMove",
		"BlockRotate",
	};

	private readonly string[] PLAY_BTN_STR =
	{
		"Pause",
		"PauseHome",
		"PauseResume",
		"BlockLeft",
		"BlockRight",
		"BlockForward",
		"BlockBackward",
		"RotateX",
		"RotateXInv",
		"RotateY",
		"RotateYInv",
		"RotateZ",
		"RotateZInv",
		"BlockSave",
	};

	private readonly string[] BLOCK_IMG_STR =
	{
		"block_null",
		"block_I",
		"block_L",
		"block_T",
		"block_O",
		"block_J",
		"block_Z",
		"block_S",
	};

	private readonly string[] SPEED_UP_TXT =
	{
		"SpeedUp_01",
		"SpeedUp_02",
		"SpeedUp_03",
	};

	private      Image               blockNextImg;
	private      Image               blockSaveImg;
	private      GameObject          pauseScreen;
	public       List<RectTransform> speedUpTxt;
	public       TMP_Text            speedUpMainTxt;
	public const float               speedUpTxtOriginX = -550f;
	public const float               speedUpTxtDestX   = 550f;
	public const float               speedUpSpeed      = 1000f;

#endregion

#region Option Screen

	private enum SLIDER_TYPE
	{
		BGM = 0,
		SFX,
		COUNT
	}

	private enum OPTION_PANEL
	{
		SOUND = 0,
		GRAPHIC,
		CONTROL,
		COUNT
	}

	private readonly string[] OPTION_BTN_STR =
	{
		"SoundTab",
		"GraphicTab",
		"ControlTab",
		"OptionHome",
		"ColorOption",
		"ButtonOption",
		"ButtonHelp",
		"ToastMsg",
	};

	private readonly string[] OPTION_PANEL_STR =
	{
		"SoundButtons",
		"GraphicButtons",
		"ControlButtons",
	};

	private static OPTION_PANEL     curPanel = OPTION_PANEL.SOUND;
	private        List<GameObject> optionButtons;
	private        Sprite[]         toggleImg;
	private        List<int>        toggleValue;
	private        Slider           sliderBGM;
	private        Slider           sliderSFX;
	public         CanvasGroup      controlOptionHelpMsg;
	public static  bool             toastShow;

#endregion

#region Leader Board Screen

	private readonly string[] LEADER_BOARD_BTN_STR =
	{
		"LeaderBoardHome",
	};

	private readonly string[] RANK_SPRITE =
	{
		"UI/Textures/Medal_1",
		"UI/Textures/Medal_2",
		"UI/Textures/Medal_3",
	};

	private const string     LEADER_BOARD_ASSET = "UI/Prefabs/Board";
	private       GameObject leaderBoardContents;

#endregion

#region Game Over Screen

	private readonly string[] GAME_OVER_BTN_STR =
	{
		"GameOverHome",
		"GameOverRetry",
	};

	private TMP_InputField scoreInput;

#endregion

	public void Init()
	{
		screenObjects = new Dictionary<string, CanvasGroup>();

		for (int i = 0; i < SCREEN_STR.Length; ++i)
		{
			screenObjects.Add(SCREEN_STR[i], GameObject.Find(SCREEN_STR[i]).GetComponent<CanvasGroup>());
		}

		buttons = new Dictionary<string, Dictionary<string, UIButton>>();

		scoreTxt = new[]
		{
			GameObject.Find("GameScoreTxt").GetComponent<TMP_Text>(),
			GameObject.Find("GameOverScore").GetComponent<TMP_Text>()
		};

		InitMainScreen();
		InitPlayScreen();
		InitOptionScreen();
		InitLeaderBoardScreen();
		InitGameOverScreen();
	}

	private void InitMainScreen()
	{
		mainQuitPanel = GameObject.Find("QuitPanel");

		buttons.Add(SCREEN_STR[0], new Dictionary<string, UIButton>());
		UnityAction[] callbackFuncs =
		{
			GameManager.Instance.coroutineManager.OnClickMainMenuGameStart,
			GameManager.Instance.coroutineManager.OnClickMainMenuOption,
			GameManager.Instance.coroutineManager.OnClickMainMenuLeaderBoard,
			() =>
			{
				GameManager.Instance.coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.CLICK);
				mainQuitPanel.gameObject.SetActive(true);
			},
			GameTerminate,
			() =>
			{
				GameManager.Instance.coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.CLICK);
				mainQuitPanel.gameObject.SetActive(false);
			},
		};

		if (MAIN_BTN_STR.Length != callbackFuncs.Length)
		{
			Debug.LogError("InitMainScreen: Please allocate proper amount of functions to button.");
		}

		for (int i = 0; i < MAIN_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[0]].Add(MAIN_BTN_STR[i], new UIButton(MAIN_BTN_STR[i], callbackFuncs[i]));
		}

		mainQuitPanel.SetActive(false);
		screenObjects[SCREEN_STR[0]].gameObject.SetActive(true);
	}

	private void InitPlayScreen()
	{
		playObjects = new Dictionary<string, GameObject>();

		for (int i = 0; i < PLAY_OBJ_STR.Length; ++i)
		{
			playObjects.Add(PLAY_OBJ_STR[i], GameObject.Find(PLAY_OBJ_STR[i]));
		}

		buttons.Add(SCREEN_STR[1], new Dictionary<string, UIButton>());
		UnityAction[] callbackFuncs =
		{
			GamePause,
			GameManager.Instance.coroutineManager.OnClickPauseHome,
			PauseResume,
			GameManager.Instance.MoveBlockLeft,
			GameManager.Instance.MoveBlockRight,
			GameManager.Instance.MoveBlockForward,
			GameManager.Instance.MoveBlockBackward,
			GameManager.Instance.RotateBlockX,
			GameManager.Instance.RotateBlockXInv,
			GameManager.Instance.RotateBlockY,
			GameManager.Instance.RotateBlockYInv,
			GameManager.Instance.RotateBlockZ,
			GameManager.Instance.RotateBlockZInv,
			GameManager.Instance.SaveBlock,
		};

		if (PLAY_BTN_STR.Length != callbackFuncs.Length)
		{
			Debug.LogError("InitPlayScreen: Please allocate proper amount of functions to button.");
		}

		for (int i = 0; i < PLAY_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[1]].Add(PLAY_BTN_STR[i], new UIButton(PLAY_BTN_STR[i], callbackFuncs[i]));
		}

		blockNextImg = GameObject.Find("BlockNextImg").GetComponent<Image>();
		blockNextImg.sprite =
			Resources.Load<Sprite>("UI/Textures/" + BLOCK_IMG_STR[GameManager.BlockQueue.GetNextBlockId()]);

		blockSaveImg        = GameObject.Find("BlockSaveImg").GetComponent<Image>();
		blockSaveImg.sprite = Resources.Load<Sprite>("UI/Textures/" + BLOCK_IMG_STR[0]);

		scoreTxt[0].text = "0";

		speedUpTxt = new List<RectTransform>();

		for (int i = 0; i < SPEED_UP_TXT.Length; ++i)
		{
			speedUpTxt.Add(GameObject.Find(SPEED_UP_TXT[i]).GetComponent<RectTransform>());
		}

		speedUpMainTxt = speedUpTxt[SPEED_UP_TXT.Length - 1].GetComponent<TMP_Text>();

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

		playObjects["BlockMove"].SetActive(false);
		playObjects["BlockRotate"].SetActive(false);

#endif

		playObjects["PauseScreen"].SetActive(false);
		screenObjects[SCREEN_STR[1]].gameObject.SetActive(false);
	}

	private void InitOptionScreen()
	{
		optionButtons = new List<GameObject>();

		for (int i = 0; i < OPTION_PANEL_STR.Length; ++i)
		{
			optionButtons.Add(GameObject.Find(OPTION_PANEL_STR[i]));
		}

		buttons.Add(SCREEN_STR[2], new Dictionary<string, UIButton>());
		UnityAction[] callbackFuncs =
		{
			() => OptionPanel(OPTION_PANEL.SOUND),
			() => OptionPanel(OPTION_PANEL.GRAPHIC),
			() => OptionPanel(OPTION_PANEL.CONTROL),
			GameManager.Instance.coroutineManager.OnClickOptionHome,
			OptionColor,
			OptionButton,
			OptionHelp,
			OptionHelpToastHide,
		};

		if (OPTION_BTN_STR.Length != callbackFuncs.Length)
		{
			Debug.LogError("InitOptionScreen: Please allocate proper amount of functions to button.");
		}

		for (int i = 0; i < OPTION_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[2]].Add(OPTION_BTN_STR[i], new UIButton(OPTION_BTN_STR[i], callbackFuncs[i]));
		}

		sliderBGM = GameObject.Find("BGMSlider").GetComponent<Slider>();
		sliderSFX = GameObject.Find("SFXSlider").GetComponent<Slider>();

		sliderBGM.minValue = 0f;
		sliderBGM.value    = sliderBGM.maxValue = AudioSystem.BGMVolume;

		sliderSFX.minValue = 0f;
		sliderSFX.value    = sliderSFX.maxValue = AudioSystem.SFXVolume;

		sliderBGM.onValueChanged.AddListener(delegate { OptionSlider(SLIDER_TYPE.BGM); });
		sliderSFX.onValueChanged.AddListener(delegate { OptionSlider(SLIDER_TYPE.SFX); });

		toggleImg = new[]
		{
			Resources.Load<Sprite>("UI/Textures/toggle_off"),
			Resources.Load<Sprite>("UI/Textures/toggle_on"),
		};

		toggleValue = new List<int>
		{
			EffectSystem.SaturationValue(), // Color Option
			1,                              // Control Option
		};

		controlOptionHelpMsg = GameObject.Find("ToastMsg").GetComponent<CanvasGroup>();

		optionButtons[0].SetActive(true);
		optionButtons[1].SetActive(false);
		optionButtons[2].SetActive(false);

		controlOptionHelpMsg.gameObject.SetActive(false);

		screenObjects[SCREEN_STR[2]].gameObject.SetActive(false);
	}

	private void InitLeaderBoardScreen()
	{
		buttons.Add(SCREEN_STR[3], new Dictionary<string, UIButton>());
		UnityAction[] callbackFuncs =
		{
			GameManager.Instance.coroutineManager.OnClickLeaderBoardHome,
		};

		if (LEADER_BOARD_BTN_STR.Length != callbackFuncs.Length)
		{
			Debug.LogError("InitLeaderBoardScreen: Please allocate proper amount of functions to button.");
		}

		for (int i = 0; i < LEADER_BOARD_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[3]]
				.Add(LEADER_BOARD_BTN_STR[i], new UIButton(LEADER_BOARD_BTN_STR[i], callbackFuncs[i]));
		}

		leaderBoardContents = GameObject.Find("LeaderBoardContents");

		screenObjects[SCREEN_STR[3]].gameObject.SetActive(false);
	}

	private void InitGameOverScreen()
	{
		buttons.Add(SCREEN_STR[4], new Dictionary<string, UIButton>());
		UnityAction[] callbackFuncs =
		{
			GameManager.Instance.coroutineManager.OnClickGameOverHome,
			GameManager.Instance.coroutineManager.OnClickGameOverRetry,
		};

		for (int i = 0; i < GAME_OVER_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[4]].Add(GAME_OVER_BTN_STR[i], new UIButton(GAME_OVER_BTN_STR[i], callbackFuncs[i]));
		}

		scoreTxt[1].text = "0";

		scoreInput = GameObject.Find("ScoreInput").GetComponent<TMP_InputField>();

		screenObjects[SCREEN_STR[4]].gameObject.SetActive(false);
	}

#region Main Menu Functions

	private static void GameTerminate()
	{
		GameManager.Instance.coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.CLICK);
		
		GameManager.StoreData();

#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

#endregion

#region Play Functions

	public void GamePause()
	{
		playObjects["PauseScreen"].SetActive(true);

		GameManager.Instance.coroutineManager.GamePause();
	}

	public void PauseResume()
	{
		playObjects["PauseScreen"].SetActive(false);

		GameManager.Instance.coroutineManager.GameResume();
	}

	public void UpdateNextBlockImg()
	{
		blockNextImg.sprite =
			Resources.Load<Sprite>("UI/Textures/" + BLOCK_IMG_STR[GameManager.BlockQueue.GetNextBlockId()]);
	}

	public void UpdateSaveBlockImg()
	{
		blockSaveImg.sprite =
			Resources.Load<Sprite>("UI/Textures/" + BLOCK_IMG_STR[GameManager.BlockQueue.GetSaveBlockId()]);
	}

	public static void SpeedUpTransition()
	{
		GameManager.Instance.coroutineManager.SpeedUpTransition();
	}

#endregion

#region Option Functions

	private void OptionPanel(OPTION_PANEL panel)
	{
		GameManager.Instance.coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		if (curPanel == panel) return;

		for (int i = 0; i < 3; ++i)
		{
			optionButtons[i].SetActive(false);
		}

		optionButtons[(int)panel].SetActive(true);
		curPanel = panel;
	}

	private void OptionSlider(SLIDER_TYPE type)
	{
		switch (type)
		{
			case SLIDER_TYPE.BGM:
				AudioSystem.baseBGMVolume = AudioSystem.BGMVolume = sliderBGM.value;

				break;

			case SLIDER_TYPE.SFX:
				AudioSystem.baseSFXVolume = AudioSystem.SFXVolume = sliderSFX.value;

				break;
		}
	}

	private void OptionColor()
	{
		GameManager.Instance.coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.CLICK);
		toggleValue[0]                                    = (toggleValue[0] + 1) % 2;
		buttons["OptionScreen"]["ColorOption"].img.sprite = toggleImg[toggleValue[0]];
		EffectSystem.SaturationChange();
	}

	private void OptionButton()
	{
		GameManager.Instance.coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.CLICK);
		toggleValue[1]                                     = (toggleValue[1] + 1) % 2;
		buttons["OptionScreen"]["ButtonOption"].img.sprite = toggleImg[toggleValue[1]];
	}

	private static void OptionHelp()
	{
		GameManager.Instance.coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.CLICK);
		OptionHelpToastReset();
		GameManager.Instance.coroutineManager.OnClickOptionHelp();
	}

	private void OptionHelpToastHide()
	{
		GameManager.Instance.coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.SHIFT);
		buttons["OptionScreen"]["ToastMsg"].btn.interactable = toastShow = false;
	}

	public static void OptionHelpToastReset()
	{
		toastShow = true;
	}

#endregion

#region Leader Board Functions

	public void BoardReset()
	{
		for (int i = 0; i < leaderBoardContents.transform.childCount; ++i)
		{
			Destroy(leaderBoardContents.transform.GetChild(i).gameObject);
		}

		for (int i = 0; i < GameManager.saveData.list.Count; ++i)
		{
			AddBoard(GameManager.saveData.list[i].name, GameManager.saveData.list[i].score, i);
		}
	}

	private void AddBoard(string user, int score, int rank)
	{
		GameObject board    = Resources.Load<GameObject>(LEADER_BOARD_ASSET);
		GameObject instance = Instantiate(board, leaderBoardContents.transform, false);

		if (!instance) return;

		if (rank >= 3) instance.transform.GetChild(0).gameObject.SetActive(false);
		else
		{
			instance.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(RANK_SPRITE[rank]);
			instance.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite =
				Resources.Load<Sprite>(RANK_SPRITE[rank]);
		}

		instance.transform.GetChild(1).GetComponent<TMP_Text>().text = user;
		instance.transform.GetChild(2).GetComponent<TMP_Text>().text = score.ToString();
	}

#endregion

#region Game Over Functions

	public void GameOverSave()
	{
		scoreInput.text = scoreInput.text.Length switch
		{
			0   => "AAA",
			> 3 => scoreInput.text[..3],
			_   => scoreInput.text
		};

		SaveData save = new(GameManager.totalScore, scoreInput.text.ToUpper());
		GameManager.AddData(save);
	}

#endregion
}