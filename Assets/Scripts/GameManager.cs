/*
 * GameManager.cs
 * --------------
 * Made by Lucas Jeong
 * Contains main game logic and singleton instance.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public sealed class GameManager : MonoSingleton<GameManager>
{
#region Variables

	// Game Logic
	private       int[]            gridSize = { 10, 22, 10 };
	public static bool             isGameOver;
	public static bool             isPause;
	public        CoroutineManager coroutineManager;

	// Score
	private const    int       baseScore  = 10;
	private readonly int[]     scoreValue = { 1, 2, 4, 8 };
	private static   int       comboIdx;
	public static    int       totalScore;
	public static    SaveInfo  saveData;
	public           TextAsset textAsset;

	// Test
	public static  bool             testGrid;
	public static  bool             gridRegen;
	public static  bool             testBlock;
	public static  int              testHeight;
	private static int              testFieldSize;
	public static  Block.BLOCK_TYPE testBlockType;

	// Grid / Blocks
	public static  GameGrid   grid;
	public static  BlockQueue BlockQueue { get; set; }
	public static  Block      currentBlock;
	public static  Block      shadowBlock;
	public static  bool       canSave;
	public const   float      blockSize             = 1.0f;
	private const  float      downIntervalOrigin    = 1f;
	private const  float      downIntervalSpeedBase = 1.05f;
	private static int        downIntervalSpeedExp;
	public static  float      downInterval = downIntervalOrigin;

	public enum INPUT_CONTROL
	{
		DEFAULT = 0,
		MOVE_LEFT,
		MOVE_RIGHT,
		MOVE_FORWARD,
		MOVE_BACKWARD,
		ROTATE_X,
		ROTATE_X_INV,
		ROTATE_Y,
		ROTATE_Y_INV,
		ROTATE_Z,
		ROTATE_Z_INV,
		BLOCK_DOWN,
		BLOCK_DROP,
		BLOCK_SAVE,
		PAUSE,
	}

#endregion

#region MonoFunction

	private void Start()
	{
		Init();

		if (!File.Exists(Application.dataPath + "/Resources/SaveData.json"))
		{
			saveData = new SaveInfo();
			string saveStr = JsonUtility.ToJson(saveData);
			File.WriteAllText(Application.dataPath + "/Resources/SaveData.json", saveStr);
		}

		textAsset = Resources.Load<TextAsset>("SaveData");
		ReadData();

#if UNITY_EDITOR
		grid = new GameGrid(ref gridSize, blockSize);

#else
		grid = new GameGrid(ref gridSize, blockSize);

#endif
		coroutineManager = GameObject.Find("CoroutineManager").GetComponent<CoroutineManager>();

		BlockQueue   = new BlockQueue();
		currentBlock = BlockQueue.GetAndUpdateBlock();

		CameraSystem.Instance.Init();
		RenderSystem.Instance.Init();

		grid.Mesh.Obj.transform.parent = RenderSystem.gridObj.transform;

		RenderSystem.startOffset = new Vector3(-grid.SizeX / 2f + blockSize / 2,
		                                       grid.SizeY  / 2f - blockSize / 2,
		                                       -grid.SizeZ / 2f + blockSize / 2);
		if (testGrid) RenderSystem.RenderGrid();

		RenderSystem.RenderLine();
		RenderSystem.RenderCurrentBlock();
		RenderSystem.RenderShadowBlock();

		AudioSystem.Instance.Init();
		EffectSystem.Instance.Init();
		EnvironmentSystem.Instance.Init();
		InputSystem.Instance.Init();
		UISystem.Instance.Init();
	}

	private void Init()
	{
		isGameOver = false;
		isPause    = true;
		totalScore = 0;
		comboIdx   = 0;

		downInterval         = downIntervalOrigin;
		downIntervalSpeedExp = 0;

		testHeight    = 7;
		testFieldSize = 10;
		testBlockType = Block.BLOCK_TYPE.I;

#if UNITY_EDITOR
		testGrid  = true;
		gridRegen = true;
		testBlock = true;

		gridSize[0] = testFieldSize;
		gridSize[2] = testFieldSize;

#else
		testGrid = false;
		gridRegen = false;
		testBlock = false;

#endif

		canSave = true;
	}

	private void Update()
	{
		INPUT_CONTROL control = InputSystem.Instance.InputWindows();

		if (!isPause)
		{
			InputSystem.Instance.InputMouse();

			switch (control)
			{
				case INPUT_CONTROL.DEFAULT:
					break;

				case INPUT_CONTROL.MOVE_LEFT:
					MoveBlockLeft();

					break;

				case INPUT_CONTROL.MOVE_RIGHT:
					MoveBlockRight();

					break;

				case INPUT_CONTROL.MOVE_FORWARD:
					MoveBlockForward();

					break;

				case INPUT_CONTROL.MOVE_BACKWARD:
					MoveBlockBackward();

					break;

				case INPUT_CONTROL.ROTATE_X:
					RotateBlockX();

					break;

				case INPUT_CONTROL.ROTATE_X_INV:
					RotateBlockXInv();

					break;

				case INPUT_CONTROL.ROTATE_Y:
					RotateBlockY();

					break;

				case INPUT_CONTROL.ROTATE_Y_INV:
					RotateBlockYInv();

					break;

				case INPUT_CONTROL.ROTATE_Z:
					RotateBlockZ();

					break;

				case INPUT_CONTROL.ROTATE_Z_INV:
					RotateBlockZInv();

					break;

				case INPUT_CONTROL.BLOCK_DOWN:
					MoveBlockDown();

					break;

				case INPUT_CONTROL.BLOCK_DROP:
					DropBlock();

					break;

				case INPUT_CONTROL.BLOCK_SAVE:
					SaveBlock();

					break;

				case INPUT_CONTROL.PAUSE:
					UISystem.Instance.GamePause();

					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		else
		{
			switch (control)
			{
				case INPUT_CONTROL.PAUSE:
					UISystem.Instance.PauseResume();

					break;
			}
		}
	}

#endregion

#region GameControl

	public void SaveBlock()
	{
		canSave = false;
		coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.SHIFT);
		currentBlock = BlockQueue.SaveAndUpdateBlock();
		UISystem.Instance.UpdateSaveBlockImg();
		RenderSystem.RefreshCurrentBlock();
	}

	public void Reset()
	{
		if (testGrid)
		{
			gridSize[0] = testFieldSize;
			gridSize[2] = testFieldSize;
			grid        = new GameGrid(ref gridSize, blockSize);
		}
		else
		{
			grid = new GameGrid(ref gridSize, blockSize);
		}

		isPause    = false;
		isGameOver = false;

		totalScore = 0;
		comboIdx   = 0;

		currentBlock = BlockQueue.GetAndUpdateBlock();
		BlockQueue.SaveBlockReset();
		canSave = true;

		downInterval         = downIntervalOrigin;
		downIntervalSpeedExp = 0;

		RenderSystem.Instance.Reset();
	}

	public static bool BlockFits(Block block)
	{
		return block.TilePositions().All(coord => grid.IsEmpty(coord.X, coord.Y, coord.Z));
	}

	private static bool CheckGameOver()
	{
		return !grid.IsPlaneEmpty(0);
	}

	private void PlaceBlock()
	{
		foreach (Coord coord in currentBlock.TilePositions())
		{
			grid[coord.X, coord.Y, coord.Z] = currentBlock.GetId();
		}

		List<int> cleared = grid.ClearFullRows();
		ScoreCalculation(cleared.Count);

		coroutineManager.GridEffect(cleared);

		if (cleared.Count == 4)
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.TETRIS1);
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.TETRIS2);
			coroutineManager.CameraFOVEffect();
		}
		else if (cleared.Count > 0)
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.CLEAR);
		}

		RenderSystem.RenderGrid();

		cleared.Clear();

		if (CheckGameOver())
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.GAME_OVER);

			isPause    = true;
			isGameOver = true;

			coroutineManager.UpdateScore(UISystem.SCORE_TYPE.GAME_OVER, totalScore);
			coroutineManager.PitchDownBGM(0.2f);
			coroutineManager.GameOverEffect();
			coroutineManager.StopAnimChange();
			coroutineManager.GameOverScreen();
		}
		else
		{
			canSave      = true;
			currentBlock = BlockQueue.GetAndUpdateBlock();
			UISystem.Instance.UpdateNextBlockImg();
			RenderSystem.RefreshCurrentBlock();
		}
	}

	private void ScoreCalculation(int cleared)
	{
		if (cleared == 0)
		{
			comboIdx = 0;

			return;
		}

		int addScore = baseScore * (int)Mathf.Pow(scoreValue[cleared - 1], ++comboIdx);
		totalScore += addScore;

		if (addScore >= baseScore * scoreValue[3])
		{
			downInterval = Mathf.Pow(downIntervalSpeedBase, --downIntervalSpeedExp);
			
			UISystem.Instance.SpeedUpTransition();
		}

		coroutineManager.UpdateScore(UISystem.SCORE_TYPE.PLAY, addScore);
	}

#endregion

#region BlockRotation

	public void RotateBlockX()
	{
		switch (CameraSystem.viewAngle)
		{
			case 0:
				currentBlock.RotateXClockWise();

				break;

			case 1:
				currentBlock.RotateZCounterClockWise();

				break;

			case 2:
				currentBlock.RotateXCounterClockWise();

				break;

			case 3:
				currentBlock.RotateZClockWise();

				break;
		}

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			switch (CameraSystem.viewAngle)
			{
				case 0:
					currentBlock.RotateXCounterClockWise();

					break;

				case 1:
					currentBlock.RotateZClockWise();

					break;

				case 2:
					currentBlock.RotateXClockWise();

					break;

				case 3:
					currentBlock.RotateZCounterClockWise();

					break;
			}
		}
		else
		{
			coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.size * blockSize * 0.5f);

			Quaternion rotation;

			switch (CameraSystem.viewAngle)
			{
				case 0:
					rotation = Quaternion.Euler(0f, 0f, 90f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 1:
					rotation = Quaternion.Euler(90f, 0f, 0f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 2:
					rotation = Quaternion.Euler(0f, 0f, -90f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 3:
					rotation = Quaternion.Euler(-90f, 0f, 0f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;
			}

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void RotateBlockXInv()
	{
		switch (CameraSystem.viewAngle)
		{
			case 0:
				currentBlock.RotateXCounterClockWise();

				break;

			case 1:
				currentBlock.RotateZClockWise();

				break;

			case 2:
				currentBlock.RotateXClockWise();

				break;

			case 3:
				currentBlock.RotateZCounterClockWise();

				break;
		}

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			switch (CameraSystem.viewAngle)
			{
				case 0:
					currentBlock.RotateXClockWise();

					break;

				case 1:
					currentBlock.RotateZCounterClockWise();

					break;

				case 2:
					currentBlock.RotateXCounterClockWise();

					break;

				case 3:
					currentBlock.RotateZClockWise();

					break;
			}
		}
		else
		{
			coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.size * blockSize * 0.5f);

			Quaternion rotation;

			switch (CameraSystem.viewAngle)
			{
				case 0:
					rotation = Quaternion.Euler(0f, 0f, -90f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 1:
					rotation = Quaternion.Euler(-90f, 0f, 0f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 2:
					rotation = Quaternion.Euler(0f, 0f, 90f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 3:
					rotation = Quaternion.Euler(90f, 0f, 0f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;
			}

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void RotateBlockY()
	{
		currentBlock.RotateYClockWise();

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.RotateYCounterClockWise();
		}
		else
		{
			coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.size * blockSize * 0.5f);
			Quaternion rotation = Quaternion.Euler(0f, 0f, 180f);

			switch (CameraSystem.viewAngle)
			{
				case 0:
				case 1:
				case 2:
				case 3:
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;
			}

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void RotateBlockYInv()
	{
		currentBlock.RotateYCounterClockWise();

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.RotateYClockWise();
		}
		else
		{
			coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.size * blockSize * 0.5f);
			Quaternion rotation = Quaternion.identity;

			switch (CameraSystem.viewAngle)
			{
				case 0:
				case 1:
				case 2:
				case 3:
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;
			}

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void RotateBlockZ()
	{
		switch (CameraSystem.viewAngle)
		{
			case 0:
				currentBlock.RotateZClockWise();

				break;

			case 1:
				currentBlock.RotateXClockWise();

				break;

			case 2:
				currentBlock.RotateZCounterClockWise();

				break;

			case 3:
				currentBlock.RotateXCounterClockWise();

				break;
		}

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			switch (CameraSystem.viewAngle)
			{
				case 0:
					currentBlock.RotateZCounterClockWise();

					break;

				case 1:
					currentBlock.RotateXCounterClockWise();

					break;

				case 2:
					currentBlock.RotateZClockWise();

					break;

				case 3:
					currentBlock.RotateXClockWise();

					break;
			}
		}
		else
		{
			coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.size * blockSize * 0.5f);

			Quaternion rotation;

			switch (CameraSystem.viewAngle)
			{
				case 0:
					rotation = Quaternion.Euler(-90f, 0f, 0f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 1:
					rotation = Quaternion.Euler(0f, 0f, 90f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 2:
					rotation = Quaternion.Euler(90f, 0f, 0f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 3:
					rotation = Quaternion.Euler(0f, 0f, -90f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;
			}

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void RotateBlockZInv()
	{
		switch (CameraSystem.viewAngle)
		{
			case 0:
				currentBlock.RotateZCounterClockWise();

				break;

			case 1:
				currentBlock.RotateXCounterClockWise();

				break;

			case 2:
				currentBlock.RotateZClockWise();

				break;

			case 3:
				currentBlock.RotateXClockWise();

				break;
		}

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			switch (CameraSystem.viewAngle)
			{
				case 0:
					currentBlock.RotateZClockWise();

					break;

				case 1:
					currentBlock.RotateXClockWise();

					break;

				case 2:
					currentBlock.RotateZCounterClockWise();

					break;

				case 3:
					currentBlock.RotateXCounterClockWise();

					break;
			}
		}
		else
		{
			coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.size * blockSize * 0.5f);

			Quaternion rotation;

			switch (CameraSystem.viewAngle)
			{
				case 0:
					rotation = Quaternion.Euler(90f, 0f, 0f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 1:
					rotation = Quaternion.Euler(0f, 0f, -90f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 2:
					rotation = Quaternion.Euler(-90f, 0f, 0f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;

				case 3:
					rotation = Quaternion.Euler(0f, 0f, 90f);
					EffectSystem.Instance.CreateRotationEffect(ref offset, ref rotation);

					break;
			}

			RenderSystem.RefreshCurrentBlock();
		}
	}

#endregion

#region BlockMove

	public void MoveBlockLeft()
	{
		currentBlock.Move(Coord.Left[CameraSystem.viewAngle]);

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.Move(Coord.Right[CameraSystem.viewAngle]);
		}
		else
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.MOVE);

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void MoveBlockRight()
	{
		currentBlock.Move(Coord.Right[CameraSystem.viewAngle]);

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.Move(Coord.Left[CameraSystem.viewAngle]);
		}
		else
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.MOVE);

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void MoveBlockForward()
	{
		currentBlock.Move(Coord.Forward[CameraSystem.viewAngle]);

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.Move(Coord.Backward[CameraSystem.viewAngle]);
		}
		else
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.MOVE);

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void MoveBlockBackward()
	{
		currentBlock.Move(Coord.Backward[CameraSystem.viewAngle]);

		if (!BlockFits(currentBlock))
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.Move(Coord.Forward[CameraSystem.viewAngle]);
		}
		else
		{
			coroutineManager.BurstSFX(AudioSystem.SFX_VALUE.MOVE);

			RenderSystem.RefreshCurrentBlock();
		}
	}

	public void MoveBlockDown()
	{
		currentBlock.Move(Coord.Down);

		if (BlockFits(currentBlock))
		{
			RenderSystem.RenderCurrentBlock();

			return;
		}

		coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.DROP1, AudioSystem.SFX_VALUE.DROP2);

		currentBlock.Move(Coord.Up);
		EffectSystem.DropEffect();
		PlaceBlock();
	}

	private void DropBlock()
	{
		int num = 0;

		do
		{
			currentBlock.Move(Coord.Down);
			++num;
		} while (BlockFits(currentBlock));

		if (num > 2)
		{
			coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.HARD_DROP1, AudioSystem.SFX_VALUE.HARD_DROP5);
			coroutineManager.CameraShake();
		}
		else
		{
			coroutineManager.PlayRandomSFX(AudioSystem.SFX_VALUE.DROP1, AudioSystem.SFX_VALUE.DROP2);
		}

		currentBlock.Move(Coord.Up);
		EffectSystem.DropEffect();
		PlaceBlock();
	}

#endregion

#region Save Data

	private void ReadData()
	{
		saveData = JsonUtility.FromJson<SaveInfo>(textAsset.text);
	}

	public static void AddData(SaveData info)
	{
		saveData.list.Add(info);
		SortData();
		StoreData();
	}

	public static void StoreData()
	{
		string saveStr = JsonUtility.ToJson(saveData);
		File.WriteAllText(Application.dataPath + "/Resources/SaveData.json", saveStr);
	}

	private static void SortData()
	{
		saveData.list.Sort();
	}

#endregion
}