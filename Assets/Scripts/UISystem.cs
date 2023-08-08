using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISystem : MonoBehaviour
{
	private Dictionary<string, (GameObject obj, CanvasGroup canvas)> screenObjects;
	private readonly string[] SCREEN_STR =
	{
		new("MainScreen"),
		new("PlayScreen"),
		new("OptionScreen"),
		new("LeaderBoardScreen"),
		new("GameOverScreen"),
	};

#region Main Screen

	private Dictionary<string, Button> mainButtons;
	private readonly string[] MAIN_BTN_STR =
	{
		new("Start"),
		new("Option"),
		new("LeaderBoard"),
		new("Quit"),
		new("QuitYes"),
		new("QuitNo"),
	};
	private GameObject mainQuitPanel;

#endregion

#region Play Screen

	private Dictionary<string, GameObject> playObjects;
	private Dictionary<string, Button>     playButtons;
	private readonly string[] PLAY_OBJ_STR =
	{
		new("PauseScreen"),
		new("ControlScreen"),
		new("BlockMove"),
		new("BlockRotate"),
	};
	private readonly string[] PLAY_BTN_STR =
	{
		new("Pause"),
		new("PauseHome"),
		new("PauseResume"),
		new("BlockLeft"),
		new("BlockRight"),
		new("BlockForward"),
		new("BlockBackward"),
		new("RotateX"),
		new("RotateXInv"),
		new("RotateY"),
		new("RotateYInv"),
		new("RotateZ"),
		new("RotateZInv"),
	};
	private TMP_Text   playScoreTxt;

#endregion

#region Option Screen

	private Dictionary<string, Button> optionButtons;
	private readonly string[] OPTION_BTN_STR =
	{
		new("OptionHome"),
		new("SoundTab"),
		new("GraphicTab"),
		new("ControlTab"),
		new("ColorToggle"),
		new("ButtonToggle"),
		new("ButtonHelp"),
	};
	private List<GameObject> optionPanels;
	private Slider           sliderBGM;
	private Slider           sliderSFX;

#endregion

#region Leader Board Screen

#endregion

#region Game Over Screen

	private Button   retryBtn;
	private Button   gameOverHomeBtn;
	private TMP_Text gameOverScoreText;

#endregion

	public UISystem()
	{
		InitUI();
	}

	private void InitUI()
	{
		Queue<GameObject> screenObjs = new();

		for (int i = 0; i < SCREEN_STR.Length; ++i)
		{
			screenObjs.Enqueue(GameObject.Find(SCREEN_STR[i]));
		}

		screenObjects = new Dictionary<string, (GameObject obj, CanvasGroup canvas)>();

		for (int i = 0; i < SCREEN_STR.Length; ++i)
		{
			screenObjects.Add(SCREEN_STR[i], (screenObjs.Peek(),
			                                  screenObjs.Dequeue().GetComponent<CanvasGroup>()));
		}

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

		mainButtons = new Dictionary<string, Button>();

		for (int i = 0; i < MAIN_BTN_STR.Length; ++i)
		{
			mainButtons.Add(MAIN_BTN_STR[i], mainObjs.Dequeue().GetComponent<Button>());
		}

		mainButtons["Start"].onClick.AddListener(GameStart);
		//mainButtons["Option"].onClick.AddListener(Option);
		//mainButtons["LeaderBoard"].onClick.AddListener(LeaderBoard);
		mainButtons["Quit"].onClick.AddListener(() => mainQuitPanel.gameObject.SetActive(true));
		mainButtons["QuitYes"].onClick.AddListener(Application.Quit);
		mainButtons["QuitNo"].onClick.AddListener(() => mainQuitPanel.gameObject.SetActive(false));

		mainQuitPanel.SetActive(false);
		screenObjects["MainScreen"].obj.SetActive(true);
	}

	private void InitPlayScreen()
	{
		playObjects = new Dictionary<string, GameObject>();

		for (int i = 0; i < PLAY_OBJ_STR.Length; ++i)
		{
			playObjects.Add(PLAY_OBJ_STR[i], GameObject.Find(PLAY_OBJ_STR[i]));
		}

		playButtons = new Dictionary<string, Button>();

		for (int i = 0; i < PLAY_BTN_STR.Length; ++i)
		{
			playButtons.Add(PLAY_BTN_STR[i], GameObject.Find(PLAY_BTN_STR[i]).GetComponent<Button>());
		}
		
		playButtons["Pause"].onClick.AddListener(GamePause);
		playButtons["PauseHome"].onClick.AddListener(PauseHome);
		playButtons["PauseResume"].onClick.AddListener(GameResume);
		playButtons["BlockLeft"].onClick.AddListener(GameManager.Instance.MoveBlockLeft);
		playButtons["BlockRight"].onClick.AddListener(GameManager.Instance.MoveBlockRight);
		playButtons["BlockForward"].onClick.AddListener(GameManager.Instance.MoveBlockForward);
		playButtons["BlockBackward"].onClick.AddListener(GameManager.Instance.MoveBlockBackward);
		playButtons["RotateX"].onClick.AddListener(GameManager.Instance.RotateBlockX);
		playButtons["RotateXInv"].onClick.AddListener(GameManager.Instance.RotateBlockXInv);
		playButtons["RotateY"].onClick.AddListener(GameManager.Instance.RotateBlockY);
		playButtons["RotateYInv"].onClick.AddListener(GameManager.Instance.RotateBlockYInv);
		playButtons["RotateZ"].onClick.AddListener(GameManager.Instance.RotateBlockZ);
		playButtons["RotateZInv"].onClick.AddListener(GameManager.Instance.RotateBlockZInv);

		playScoreTxt      = GameObject.Find("GameScoreText").GetComponent<TMP_Text>();
		playScoreTxt.text = "0";

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

		playObjects["BlockMove"].SetActive(false);
		playObjects["BlockRotate"].SetActive(false);

#endif

		screenObjects["PlayScreen"].obj.SetActive(false);
	}

	private void InitOptionScreen()
	{
		optionPanels = new List<GameObject>
		{
			GameObject.Find("SoundButtons"),
			GameObject.Find("GraphicButtons"),
			GameObject.Find("ControlButtons")
		};

		optionButtons = new List<Button>();

		for (int i = 0; i < (int)OPTION_BTN.COUNT; ++i)
		{
			optionButtons.Add(GameObject.Find(OPTION_BTN_STR[i]).GetComponent<Button>());
		}

		optionButtons[(int)OPTION_BTN.HOME].onClick.AddListener(OptionHome);
		optionButtons[(int)OPTION_BTN.TAB_SOUND].onClick.AddListener(() => OptionTab(OPTION_TAB.SOUND));
		optionButtons[(int)OPTION_BTN.TAB_GRAPHIC].onClick.AddListener(() => OptionTab(OPTION_TAB.GRAPHIC));
		optionButtons[(int)OPTION_BTN.TAB_CONTROL].onClick.AddListener(() => OptionTab(OPTION_TAB.CONTROL));
		//optionButtons[(int)OPTION_BTN.TOGGLE_COLOR_OPT].onClick.AddListener();
		//optionButtons[(int)OPTION_BTN.TOGGLE_BUTTON_OPT].onClick.AddListener();
		//optionButtons[(int)OPTION_BTN.BUTTON_OPT_HELP].onClick.AddListener();

		sliderBGM = GameObject.Find("BGMSlider").GetComponent<Slider>();
		sliderSFX = GameObject.Find("SFXSlider").GetComponent<Slider>();

		sliderBGM.onValueChanged.AddListener();
		sliderSFX.onValueChanged.AddListener();

		optionPanels[(int)OPTION_BTN.TAB_SOUND].gameObject.SetActive(true);
		for (int i = (int)OPTION_TAB.GRAPHIC; i < (int)OPTION_TAB.COUNT; ++i)
			optionPanels[i].gameObject.SetActive(false);

		screens[(int)SCREEN.OPTION].SetActive(false);
	}

	private void InitLeaderBoardScreen()
	{
		screens.Add(GameObject.Find("LeaderBoard"));
		screenCanvases.Add(screens[(int)SCREEN.LEADER_BOARD].GetComponent<CanvasGroup>());

		//leaderBoard.onClick.AddListener(() => MoveScreen(curMain, curLeader));
		cloneG     = GameObject.Find("Boards").GetComponent<Image>();
		leaderBack = GameObject.Find("Leader_Back").GetComponent<Button>();
		//leaderBack.onClick.AddListener(() => MoveScreen(curLeader, curMain));

		screens[(int)SCREEN.LEADER_BOARD].SetActive(false);
	}

	private void InitGameOverScreen()
	{
		screens.Add(GameObject.Find("GameOverScreen"));
		screenCanvases.Add(screens[(int)SCREEN.GAME_OVER].GetComponent<CanvasGroup>());

		retryBtn        = GameObject.Find("Retry_Button").GetComponent<Button>();
		gameOverHomeBtn = GameObject.Find("Home_Button").GetComponent<Button>();

		gameOverScoreText = GameObject.Find("GameOver_Score").GetComponent<TMP_Text>();

		retryBtn.onClick.AddListener(InitPlayScreen);

		screens[(int)SCREEN.GAME_OVER].SetActive(false);
	}

	private void OptionHome()
	{
		StartCoroutine(FadeOutIn());
	}

	private void OptionTab(OPTION_TAB tab)
	{
		for (int i = 0; i < (int)OPTION_TAB.COUNT; ++i)
			optionPanels[i].gameObject.SetActive(false);
		optionPanels[(int)tab].gameObject.SetActive(true);
	}

	private static IEnumerator FadeOutIn(GameObject fadeOut, GameObject fadeIn, float acc)
	{
		const float alphaUnit = 0.02f;
		float       alphaSet  = 1f;

		while (alphaSet >= 0f)
		{
			alphaSet      -= alphaUnit * acc;
			fadeOut.alpha =  alphaSet;

			yield return new WaitForSeconds(0.01f);
		}

		fadeOut.alpha = 0f;
		fadeIn.gameObject.SetActive(true);
		fadeIn.alpha = 0f;

		alphaSet = 0f;

		while (alphaSet <= 1f)
		{
			alphaSet     += alphaUnit * acc;
			fadeIn.alpha =  alphaSet;

			yield return new WaitForSeconds(0.01f);
		}
	}

	private void OpenSoundTab()
	{
		soundPanel.gameObject.SetActive(true);
		graphicPanel.gameObject.SetActive(false);
		controlPanel.gameObject.SetActive(false);
		optionTitle.text = optionTitles[0];
	}

	private void OpenGraphicTab()
	{
		soundPanel.gameObject.SetActive(false);
		graphicPanel.gameObject.SetActive(true);
		controlPanel.gameObject.SetActive(false);
		optionTitle.text = optionTitles[1];
	}

	private void OpenControlTab()
	{
		soundPanel.gameObject.SetActive(false);
		graphicPanel.gameObject.SetActive(false);
		controlPanel.gameObject.SetActive(true);
		optionTitle.text = optionTitles[2];
	}

	private static void CloneGrades(Image boards)
	{
		Vector3 pos = new(0, -10f);

		for (int i = 0; i < 7; i++)
		{
			Instantiate(boards, pos, Quaternion.identity);
		}
	}

	private static void ImageOnOff(Behaviour blink)
	{
		blink.enabled = !blink.enabled;
	}

	private void GameStart()
	{
		startBtn.interactable = false;
		StartCoroutine(FadeOutIn(screenMain, screenPlay, 1f));
		startBtn.interactable = true;
		screenMain.gameObject.SetActive(false);

		StartCoroutine(GameManager.Instance.GameStart());
	}

	private void GamePause()
	{
		playControlScreen.SetActive(false);
		playPauseScreen.SetActive(true);

		GameManager.Instance.GamePause();
	}

	private void GameResume()
	{
		playPauseScreen.SetActive(false);
		playControlScreen.SetActive(true);

		GameManager.Instance.GameResume();
	}

	private void PauseHome()
	{
		pauseHomeBtn.interactable = false;
		StartCoroutine(FadeOutIn(screenPlay, screenMain, 1f));
		pauseHomeBtn.interactable = true;
		playPauseScreen.SetActive(false);
		playControlScreen.SetActive(true);
		screenPlay.gameObject.SetActive(false);

		StartCoroutine(GameManager.Instance.GameHome());
	}

	private void UIReplayOnClick()
	{
	}
}