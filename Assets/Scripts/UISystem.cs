using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class UISystem : MonoSingleton<UISystem>
{
	private Dictionary<string, CanvasGroup>                screenObjects;
	private Dictionary<string, Dictionary<string, Button>> buttons;
	private readonly string[] SCREEN_STR =
	{
		"MainScreen",
		"PlayScreen",
		"OptionScreen",
		"LeaderBoardScreen",
		"GameOverScreen",
	};

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

	private Dictionary<string, GameObject> playObjects;
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
	private TMP_Text playScoreTxt;

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
	private const OPTION_PANEL curPanel = OPTION_PANEL.SOUND;
	private       Slider       sliderBGM;
	private       Slider       sliderSFX;

#endregion

#region Leader Board Screen

	private readonly string[] LEADER_BOARD_BTN_STR =
	{
		"LeaderBoardHome",
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
	private TMP_Text gameOverScoreTxt;

#endregion

	public override void Init()
	{
		screenObjects = new Dictionary<string, CanvasGroup>();

		for (int i = 0; i < SCREEN_STR.Length; ++i)
		{
			screenObjects.Add(SCREEN_STR[i], GameObject.Find(SCREEN_STR[i]).GetComponent<CanvasGroup>());
		}

		buttons = new Dictionary<string, Dictionary<string, Button>>();

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

		buttons[SCREEN_STR[0]][MAIN_BTN_STR[0]].onClick.AddListener(() => StartCoroutine(GameStart()));
		//mainButtons["Option"].onClick.AddListener(Option);
		//mainButtons["LeaderBoard"].onClick.AddListener(LeaderBoard);
		buttons[SCREEN_STR[0]][MAIN_BTN_STR[3]].onClick.AddListener(() => mainQuitPanel.gameObject.SetActive(true));
		buttons[SCREEN_STR[0]][MAIN_BTN_STR[4]].onClick.AddListener(Application.Quit);
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
		buttons[SCREEN_STR[1]][PLAY_BTN_STR[1]].onClick.AddListener(PauseHome);
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

		playScoreTxt      = GameObject.Find("GameScoreTxt").GetComponent<TMP_Text>();
		playScoreTxt.text = "0";

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

		playObjects["BlockMove"].SetActive(false);
		playObjects["BlockRotate"].SetActive(false);

#endif

		screenObjects[SCREEN_STR[1]].gameObject.SetActive(false);
	}

	private void InitOptionScreen()
	{
		buttons.Add(SCREEN_STR[2], new Dictionary<string, Button>());

		for (int i = 0; i < OPTION_BTN_STR.Length; ++i)
		{
			buttons[SCREEN_STR[2]].Add(OPTION_BTN_STR[i], GameObject.Find(OPTION_BTN_STR[i]).GetComponent<Button>());
		}

		buttons[SCREEN_STR[2]][OPTION_BTN_STR[0]].onClick.AddListener(() => OptionPanel(OPTION_PANEL.SOUND));
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[1]].onClick.AddListener(() => OptionPanel(OPTION_PANEL.GRAPHIC));
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[2]].onClick.AddListener(() => OptionPanel(OPTION_PANEL.CONTROL));
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[3]].onClick.AddListener(OptionHome);
		//buttons["OptionButtons"]["ColorToggle"].onClick.AddListener();
		//buttons["OptionButtons"]["ButtonToggle"].onClick.AddListener();
		//buttons["OptionButtons"]["ButtonHelp"].onClick.AddListener();

		sliderBGM = GameObject.Find("BGMSlider").GetComponent<Slider>();
		sliderSFX = GameObject.Find("SFXSlider").GetComponent<Slider>();

		sliderBGM.minValue = 0f;
		sliderBGM.maxValue = AudioSystem.BGMVolume;

		sliderSFX.minValue = 0f;
		sliderSFX.maxValue = AudioSystem.SFXVolume;

		sliderBGM.onValueChanged.AddListener(delegate { OptionSlider(SLIDER_TYPE.BGM); });
		sliderSFX.onValueChanged.AddListener(delegate { OptionSlider(SLIDER_TYPE.SFX); });

		buttons[SCREEN_STR[2]][OPTION_BTN_STR[1]].gameObject.SetActive(false);
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[2]].gameObject.SetActive(false);
		buttons[SCREEN_STR[2]][OPTION_BTN_STR[0]].gameObject.SetActive(true);

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

		gameOverScoreTxt      = GameObject.Find("GameOverScore").GetComponent<TMP_Text>();
		gameOverScoreTxt.text = "00000000";

		screenObjects[SCREEN_STR[4]].gameObject.SetActive(false);
	}

#region Main Functions

	private IEnumerator GameStart()
	{
		yield return StartCoroutine(FadeOutIn("MainScreen", "PlayScreen", 1f));

		StartCoroutine(GameManager.Instance.GameStart());
	}

	private IEnumerator MainOption()
	{
		yield return StartCoroutine(FadeOutIn("MainScreen", "OptionScreen", 3f));
	}

#endregion

#region Play Functions

	private void GamePause()
	{
		playObjects["ControlScreen"].SetActive(false);
		playObjects["PauseScreen"].SetActive(true);

		GameManager.Instance.GamePause();
	}

	private void PauseResume()
	{
		playObjects["PauseScreen"].SetActive(false);
		playObjects["ControlScreen"].SetActive(true);

		GameManager.Instance.GameResume();
	}

	private void PauseHome()
	{
		StartCoroutine(FadeOutIn("PlayScreen", "MainScreen", 1f));
		StartCoroutine(GameManager.Instance.GameHome());
	}

	private void UIReplayOnClick()
	{
	}

	public void ScoreUpdate(int score)
	{
		playScoreTxt.text = score.ToString("D8");
	}

#endregion

#region Option Functions

	private void OptionHome()
	{
		StartCoroutine(FadeOutIn("OptionScreen", "MainScreen", 3f));
	}

	private void OptionPanel(OPTION_PANEL panel)
	{
		if (curPanel == panel) return;

		for (int i = 0; i < 3; ++i)
		{
			buttons["OptionScreen"][OPTION_BTN_STR[i]].gameObject.SetActive(false);
		}

		buttons["OptionScreen"][OPTION_BTN_STR[(int)panel]].gameObject.SetActive(true);
	}

	private void OptionSlider(SLIDER_TYPE type)
	{
		switch (type)
		{
			case SLIDER_TYPE.BGM:
				AudioSystem.BGMVolume = sliderBGM.value;

				break;

			case SLIDER_TYPE.SFX:
				AudioSystem.SFXVolume = sliderSFX.value;

				break;
		}
	}

#endregion

#region Leader Board Functions

	private void AddBoard(string user, int score)
	{
		GameObject board = Resources.Load<GameObject>(LEADER_BOARD_ASSET);
		board.transform.parent                                    = leaderBoardContents.transform;
		board.transform.GetChild(1).GetComponent<TMP_Text>().text = user;
		board.transform.GetChild(2).GetComponent<TMP_Text>().text = score.ToString();

		bool isBetween = false;

		for (int i = 0; i < leaderBoardContents.transform.childCount; ++i)
		{
			string originScore = leaderBoardContents.transform.GetChild(i).GetChild(2).GetComponent<TMP_Text>().text;

			if (score <= int.Parse(originScore)) continue;

			board.transform.parent = leaderBoardContents.transform;
			board.transform.SetSiblingIndex(i);
			isBetween = true;

			break;
		}

		if (!isBetween)
		{
			board.transform.parent = leaderBoardContents.transform;
		}
	}

#endregion

#region Game Over Functions

	public void PrintScore(int score)
	{
		gameOverScoreTxt.text = score.ToString("D8");
	}

#endregion

#region General Functions

	public IEnumerator FadeOutIn(string fadeOut, string fadeIn, float acc)
	{
		const float alphaUnit = 0.02f;
		float       alphaSet  = 1f;

	#region Fade Out

		foreach (KeyValuePair<string, Button> button in buttons[fadeOut])
		{
			button.Value.interactable = false;
		}

		while (alphaSet >= 0f)
		{
			alphaSet                     -= alphaUnit * acc;
			screenObjects[fadeOut].alpha =  alphaSet;

			yield return new WaitForSeconds(0.01f);
		}

		screenObjects[fadeOut].alpha = 0f;

		foreach (KeyValuePair<string, Button> button in buttons[fadeOut])
		{
			button.Value.interactable = true;
		}

		screenObjects[fadeOut].gameObject.SetActive(false);

	#endregion

	#region Fade In

		screenObjects[fadeIn].gameObject.SetActive(true);
		screenObjects[fadeIn].alpha = 0f;

		foreach (KeyValuePair<string, Button> button in buttons[fadeIn])
		{
			button.Value.interactable = false;
		}

		alphaSet = 0f;

		while (alphaSet <= 1f)
		{
			alphaSet                    += alphaUnit * acc;
			screenObjects[fadeIn].alpha =  alphaSet;

			yield return new WaitForSeconds(0.01f);
		}

		screenObjects[fadeIn].alpha = 1f;

		foreach (KeyValuePair<string, Button> button in buttons[fadeIn])
		{
			button.Value.interactable = true;
		}

	#endregion
	}

#endregion
}