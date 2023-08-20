using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public sealed class UISystem : MonoSingleton<UISystem>
{
	public Dictionary<string, CanvasGroup>                screenObjects;
	public Dictionary<string, Dictionary<string, Button>> buttons;
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
	};
	private GameObject pauseScreen;

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
	};
	private readonly string[] OPTION_PANEL_STR =
	{
		"SoundButtons",
		"GraphicButtons",
		"ControlButtons",
	};
	private static OPTION_PANEL     curPanel = OPTION_PANEL.SOUND;
	private        List<GameObject> optionButtons;
	private        Slider           sliderBGM;
	private        Slider           sliderSFX;

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

		buttons = new Dictionary<string, Dictionary<string, Button>>();

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

		Queue<GameObject> mainObjs = new();

		for (int i = 0; i < MAIN_BTN_STR.Length; ++i)
		{
			mainObjs.Enqueue(GameObject.Find(MAIN_BTN_STR[i]));
		}

		buttons.Add(SCREEN_STR[0], new Dictionary<string, Button>());

		for (int i = 0; i < MAIN_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[0]].Add(MAIN_BTN_STR[i], mainObjs.Dequeue().GetComponent<Button>());
		}

		buttons[SCREEN_STR[0]][MAIN_BTN_STR[0]].onClick
		                                       .AddListener(GameManager.Instance.coroutineManager
		                                                               .OnClickMainMenuGameStart);
		buttons[SCREEN_STR[0]][MAIN_BTN_STR[1]].onClick
		                                       .AddListener(GameManager.Instance.coroutineManager
		                                                               .OnClickMainMenuOption);
		buttons[SCREEN_STR[0]][MAIN_BTN_STR[2]].onClick
		                                       .AddListener(GameManager.Instance.coroutineManager
		                                                               .OnClickMainMenuLeaderBoard);
		buttons[SCREEN_STR[0]][MAIN_BTN_STR[3]].onClick.AddListener(() => mainQuitPanel.gameObject.SetActive(true));

#if UNITY_EDITOR
		buttons[SCREEN_STR[0]][MAIN_BTN_STR[4]].onClick
		                                       .AddListener(GameTerminate);
#else
		buttons[SCREEN_STR[0]][MAIN_BTN_STR[4]].onClick.AddListener(Application.Quit);
#endif

		buttons[SCREEN_STR[0]][MAIN_BTN_STR[5]].onClick.AddListener(() => mainQuitPanel.gameObject.SetActive(false));

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

		buttons.Add(SCREEN_STR[1], new Dictionary<string, Button>());

		for (int i = 0; i < PLAY_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[1]].Add(PLAY_BTN_STR[i], GameObject.Find(PLAY_BTN_STR[i]).GetComponent<Button>());
		}

		buttons[SCREEN_STR[1]][PLAY_BTN_STR[0]].onClick.AddListener(GamePause);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[1]].onClick
		                                       .AddListener(GameManager.Instance.coroutineManager.OnClickPauseHome);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[2]].onClick.AddListener(PauseResume);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[3]].onClick.AddListener(GameManager.Instance.MoveBlockLeft);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[4]].onClick.AddListener(GameManager.Instance.MoveBlockRight);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[5]].onClick.AddListener(GameManager.Instance.MoveBlockForward);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[6]].onClick.AddListener(GameManager.Instance.MoveBlockBackward);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[7]].onClick.AddListener(GameManager.Instance.RotateBlockX);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[8]].onClick.AddListener(GameManager.Instance.RotateBlockXInv);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[9]].onClick.AddListener(GameManager.Instance.RotateBlockY);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[10]].onClick.AddListener(GameManager.Instance.RotateBlockYInv);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[11]].onClick.AddListener(GameManager.Instance.RotateBlockZ);
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[12]].onClick.AddListener(GameManager.Instance.RotateBlockZInv);

		scoreTxt[0].text = "0";

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

		playObjects["BlockMove"].SetActive(false);
		playObjects["BlockRotate"].SetActive(false);

#endif

		playObjects["PauseScreen"].SetActive(false);
		screenObjects[SCREEN_STR[1]].gameObject.SetActive(false);
	}

	private void InitOptionScreen()
	{
		buttons.Add(SCREEN_STR[2], new Dictionary<string, Button>());

		for (int i = 0; i < OPTION_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[2]].Add(OPTION_BTN_STR[i], GameObject.Find(OPTION_BTN_STR[i]).GetComponent<Button>());
		}

		optionButtons = new List<GameObject>();

		for (int i = 0; i < OPTION_PANEL_STR.Length; ++i)
		{
			optionButtons.Add(GameObject.Find(OPTION_PANEL_STR[i]));
		}

		buttons[SCREEN_STR[2]][OPTION_BTN_STR[0]].onClick.AddListener(() => OptionPanel(OPTION_PANEL.SOUND));
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[1]].onClick.AddListener(() => OptionPanel(OPTION_PANEL.GRAPHIC));
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[2]].onClick.AddListener(() => OptionPanel(OPTION_PANEL.CONTROL));
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[3]].onClick
		                                         .AddListener(GameManager.Instance.coroutineManager.OnClickOptionHome);
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[4]].onClick.AddListener(OptionColor);
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[5]].onClick.AddListener(OptionButton);
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[6]].onClick.AddListener(OptionHelp);

		sliderBGM = GameObject.Find("BGMSlider").GetComponent<Slider>();
		sliderSFX = GameObject.Find("SFXSlider").GetComponent<Slider>();

		sliderBGM.minValue = 0f;
		sliderBGM.value    = sliderBGM.maxValue = AudioSystem.BGMVolume;

		sliderSFX.minValue = 0f;
		sliderSFX.value    = sliderSFX.maxValue = AudioSystem.SFXVolume;

		sliderBGM.onValueChanged.AddListener(delegate { OptionSlider(SLIDER_TYPE.BGM); });
		sliderSFX.onValueChanged.AddListener(delegate { OptionSlider(SLIDER_TYPE.SFX); });

		optionButtons[0].SetActive(true);
		optionButtons[1].SetActive(false);
		optionButtons[2].SetActive(false);

		screenObjects[SCREEN_STR[2]].gameObject.SetActive(false);
	}

	private void InitLeaderBoardScreen()
	{
		buttons.Add(SCREEN_STR[3], new Dictionary<string, Button>());

		for (int i = 0; i < LEADER_BOARD_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[3]].Add(LEADER_BOARD_BTN_STR[i],
			                           GameObject.Find(LEADER_BOARD_BTN_STR[i]).GetComponent<Button>());
		}

		buttons[SCREEN_STR[3]][LEADER_BOARD_BTN_STR[0]].onClick
		                                               .AddListener(GameManager.Instance.coroutineManager
			                                                           .OnClickLeaderBoardHome);

		leaderBoardContents = GameObject.Find("LeaderBoardContents");

		screenObjects[SCREEN_STR[3]].gameObject.SetActive(false);
	}

	private void InitGameOverScreen()
	{
		buttons.Add(SCREEN_STR[4], new Dictionary<string, Button>());

		for (int i = 0; i < GAME_OVER_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[4]].Add(GAME_OVER_BTN_STR[i],
			                           GameObject.Find(GAME_OVER_BTN_STR[i]).GetComponent<Button>());
		}

		buttons[SCREEN_STR[4]][GAME_OVER_BTN_STR[0]].onClick
		                                            .AddListener(GameManager.Instance.coroutineManager
		                                                                    .OnClickGameOverHome);
		buttons[SCREEN_STR[4]][GAME_OVER_BTN_STR[1]].onClick
		                                            .AddListener(GameManager.Instance.coroutineManager
		                                                                    .OnClickGameOverRetry);

		scoreTxt[1].text = "0";

		scoreInput = GameObject.Find("ScoreInput").GetComponent<TMP_InputField>();

		screenObjects[SCREEN_STR[4]].gameObject.SetActive(false);
	}

#region Main Menu Functions

	private static void GameTerminate()
	{
		GameManager.StoreData();

#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

#endregion

#region Play Functions

	private void GamePause()
	{
		playObjects["ControlScreen"].SetActive(false);
		playObjects["PauseScreen"].SetActive(true);

		GameManager.Instance.coroutineManager.GamePause();
	}

	private void PauseResume()
	{
		playObjects["PauseScreen"].SetActive(false);
		playObjects["ControlScreen"].SetActive(true);

		GameManager.Instance.coroutineManager.GameResume();
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
	}

	private void OptionButton()
	{
	}

	private void OptionHelp()
	{
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
			instance.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(RANK_SPRITE[rank]);
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