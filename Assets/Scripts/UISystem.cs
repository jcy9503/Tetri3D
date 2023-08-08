using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class UISystem : MonoSingleton<UISystem>
{
	private Dictionary<string, CanvasGroup> screenObjects;
	private Dictionary<string, Dictionary<string, Button>>           buttons;

	private readonly string[] SCREEN_STR =
	{
		new("MainScreen"),
		new("PlayScreen"),
		new("OptionScreen"),
		new("LeaderBoardScreen"),
		new("GameOverScreen"),
	};

#region Main Screen

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

	private TMP_Text playScoreTxt;

#endregion

#region Option Screen

	private enum SLIDER_TYPE
	{
		BGM,
		SFX,
		COUNT
	}

	private readonly string[] OPTION_BTN_STR =
	{
		new("SoundTab"),
		new("GraphicTab"),
		new("ControlTab"),
		new("OptionHome"),
		new("ColorToggle"),
		new("ButtonToggle"),
		new("ButtonHelp"),
	};

	private Slider sliderBGM;
	private Slider sliderSFX;

#endregion

#region Leader Board Screen

#endregion

#region Game Over Screen

#endregion

	public UISystem()
	{
		Init();
	}

	protected override void Init()
	{
		Queue<GameObject> screenObjs = new();

		for (int i = 0; i < SCREEN_STR.Length; ++i)
		{
			screenObjs.Enqueue(GameObject.Find(SCREEN_STR[i]));
		}

		screenObjects = new Dictionary<string, CanvasGroup>();

		for (int i = 0; i < SCREEN_STR.Length; ++i)
		{
			screenObjects.Add(SCREEN_STR[i], (screenObjs.Dequeue().GetComponent<CanvasGroup>()));
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

		buttons.Add("MainButtons", new Dictionary<string, Button>());

		for (int i = 0; i < MAIN_BTN_STR.Length; ++i)
		{
			buttons["MainButtons"].Add(MAIN_BTN_STR[i], mainObjs.Dequeue().GetComponent<Button>());
		}

		buttons["MainButtons"]["Start"].onClick.AddListener(() => StartCoroutine(GameStart()));
		//mainButtons["Option"].onClick.AddListener(Option);
		//mainButtons["LeaderBoard"].onClick.AddListener(LeaderBoard);
		buttons["MainButtons"]["Quit"].onClick.AddListener(() => mainQuitPanel.gameObject.SetActive(true));
		buttons["MainButtons"]["QuitYes"].onClick.AddListener(Application.Quit);
		buttons["MainButtons"]["QuitNo"].onClick.AddListener(() => mainQuitPanel.gameObject.SetActive(false));

		mainQuitPanel.SetActive(false);
		screenObjects["MainScreen"].gameObject.SetActive(true);
	}

	private void InitPlayScreen()
	{
		playObjects = new Dictionary<string, GameObject>();

		for (int i = 0; i < PLAY_OBJ_STR.Length; ++i)
		{
			playObjects.Add(PLAY_OBJ_STR[i], GameObject.Find(PLAY_OBJ_STR[i]));
		}

		buttons.Add("PlayButtons", new Dictionary<string, Button>());

		for (int i = 0; i < PLAY_BTN_STR.Length; ++i)
		{
			buttons["PlayButtons"].Add(PLAY_BTN_STR[i], GameObject.Find(PLAY_BTN_STR[i]).GetComponent<Button>());
		}

		buttons["PlayButtons"]["PauseHome"].onClick.AddListener(PauseHome);
		buttons["PlayButtons"]["Pause"].onClick.AddListener(GamePause);
		buttons["PlayButtons"]["PauseResume"].onClick.AddListener(GameResume);
		buttons["PlayButtons"]["BlockLeft"].onClick.AddListener(GameManager.Instance.MoveBlockLeft);
		buttons["PlayButtons"]["BlockRight"].onClick.AddListener(GameManager.Instance.MoveBlockRight);
		buttons["PlayButtons"]["BlockForward"].onClick.AddListener(GameManager.Instance.MoveBlockForward);
		buttons["PlayButtons"]["BlockBackward"].onClick.AddListener(GameManager.Instance.MoveBlockBackward);
		buttons["PlayButtons"]["RotateX"].onClick.AddListener(GameManager.Instance.RotateBlockX);
		buttons["PlayButtons"]["RotateXInv"].onClick.AddListener(GameManager.Instance.RotateBlockXInv);
		buttons["PlayButtons"]["RotateY"].onClick.AddListener(GameManager.Instance.RotateBlockY);
		buttons["PlayButtons"]["RotateYInv"].onClick.AddListener(GameManager.Instance.RotateBlockYInv);
		buttons["PlayButtons"]["RotateZ"].onClick.AddListener(GameManager.Instance.RotateBlockZ);
		buttons["PlayButtons"]["RotateZInv"].onClick.AddListener(GameManager.Instance.RotateBlockZInv);

		playScoreTxt      = GameObject.Find("GameScoreText").GetComponent<TMP_Text>();
		playScoreTxt.text = "0";

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

		playObjects["BlockMove"].SetActive(false);
		playObjects["BlockRotate"].SetActive(false);

#endif

		screenObjects["PlayScreen"].gameObject.SetActive(false);
	}

	private void InitOptionScreen()
	{
		buttons.Add("OptionButtons", new Dictionary<string, Button>());

		for (int i = 0; i < OPTION_BTN_STR.Length; ++i)
		{
			buttons["OptionButtons"].Add(OPTION_BTN_STR[i], GameObject.Find(OPTION_BTN_STR[i]).GetComponent<Button>());
		}

		buttons["OptionButtons"]["SoundTab"].onClick.AddListener(() => OptionPanel("SoundTab"));
		buttons["OptionButtons"]["GraphicTab"].onClick.AddListener(() => OptionPanel("GraphicTab"));
		buttons["OptionButtons"]["ControlTab"].onClick.AddListener(() => OptionPanel("ControlTab"));
		buttons["OptionButtons"]["OptionHome"].onClick.AddListener(OptionHome);
		//buttons["OptionButtons"]["ColorToggle"].onClick.AddListener();
		//buttons["OptionButtons"]["ButtonToggle"].onClick.AddListener();
		//buttons["OptionButtons"]["ButtonHelp"].onClick.AddListener();

		sliderBGM = GameObject.Find("BGMSlider").GetComponent<Slider>();
		sliderSFX = GameObject.Find("SFXSlider").GetComponent<Slider>();

		sliderBGM.minValue = 0f;
		sliderBGM.maxValue = AudioSystem.Instance.BGMVolume;

		sliderSFX.minValue = 0f;
		sliderSFX.maxValue = AudioSystem.Instance.SFXVolume;

		sliderBGM.onValueChanged.AddListener(delegate { OptionSlider(SLIDER_TYPE.BGM); });
		sliderSFX.onValueChanged.AddListener(delegate { OptionSlider(SLIDER_TYPE.SFX); });

		buttons["OptionButtons"]["GraphicTab"].gameObject.SetActive(false);
		buttons["OptionButtons"]["ControlTab"].gameObject.SetActive(false);
		buttons["OptionButtons"]["SoundTab"].gameObject.SetActive(true);

		screenObjects["OptionScreen"].gameObject.SetActive(false);
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

#region Main Functions

	private IEnumerator GameStart()
	{
		yield return StartCoroutine(FadeOutIn(screenObjects["MainScreen"],
		                                      screenObjects["PlayScreen"], 1f));

		StartCoroutine(GameManager.Instance.GameStart());
	}

#endregion

#region Play Functions

	private void GamePause()
	{
		playControlScreen.SetActive(false);
		playPauseScreen.SetActive(true);

		GameManager.instance.GamePause();
	}

	private void GameResume()
	{
		playPauseScreen.SetActive(false);
		playControlScreen.SetActive(true);

		GameManager.instance.GameResume();
	}

	private void PauseHome()
	{
		pauseHomeBtn.interactable = false;
		StartCoroutine(FadeOutIn(screenPlay, screenMain, 1f));
		pauseHomeBtn.interactable = true;
		playPauseScreen.SetActive(false);
		playControlScreen.SetActive(true);
		screenPlay.gameObject.SetActive(false);

		StartCoroutine(GameManager.instance.GameHome());
	}

	private void UIReplayOnClick()
	{
	}

#endregion

#region Option Functions

	private void OptionHome()
	{
		StartCoroutine(FadeOutIn(screenObjects["OptionScreen"].canvas,
		                         screenObjects["MainScreen"].canvas, 0.5f));
	}

	private void OptionPanel(string tab)
	{
		foreach (KeyValuePair<string, Button> panel in optionPanels)
		{
			panel.Value.gameObject.SetActive(false);
		}

		optionPanels[tab].gameObject.SetActive(true);
	}

	private void OptionSlider(SLIDER_TYPE type)
	{
		switch (type)
		{
			case SLIDER_TYPE.BGM:
				AudioSystem.Instance.BGMVolume = sliderBGM.value;

				break;

			case SLIDER_TYPE.SFX:
				AudioSystem.Instance.SFXVolume = sliderSFX.value;

				break;
		}
	}

#endregion

#region General Functions

	private IEnumerator FadeOutIn((string, CanvasGroup) fadeOut, (string, CanvasGroup) fadeIn, float acc)
	{
		const float alphaUnit = 0.02f;
		float       alphaSet  = 1f;

	#region Fade Out

		foreach (KeyValuePair<string, Button> button in buttons[fadeOut.Item1])
		{
			button.Value.interactable = false;
		}

		while (alphaSet >= 0f)
		{
			alphaSet            -= alphaUnit * acc;
			fadeOut.Item2.alpha =  alphaSet;

			yield return new WaitForSeconds(0.01f);
		}

		fadeOut.Item2.alpha = 0f;

		foreach (KeyValuePair<string, Button> button in buttons[fadeOut.Item1])
		{
			button.Value.interactable = true;
		}

		fadeOut.Item2.gameObject.SetActive(false);

	#endregion

	#region Fade In

		fadeIn.Item2.gameObject.SetActive(true);
		fadeIn.Item2.alpha = 0f;

		foreach (KeyValuePair<string, Button> button in buttons[fadeIn.Item1])
		{
			button.Value.interactable = false;
		}

		alphaSet = 0f;

		while (alphaSet <= 1f)
		{
			alphaSet           += alphaUnit * acc;
			fadeIn.Item2.alpha =  alphaSet;

			yield return new WaitForSeconds(0.01f);
		}

		fadeIn.Item2.alpha = 1f;

		foreach (KeyValuePair<string, Button> button in buttons[fadeIn.Item1])
		{
			button.Value.interactable = true;
		}

	#endregion
	}

#endregion
}