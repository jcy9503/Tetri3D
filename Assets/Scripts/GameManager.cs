/*
 * GameManager.cs
 * --------------
 * Made by Lucas Jeong
 * Contains main game logic and singleton instance.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class GameManager : MonoSingleton<GameManager>
{
#region Variables

	// Game Logic
	private       int[]           gridSize = { 10, 22, 10 };
	public static bool            isGameOver;
	public static bool            isPause;
	private       List<Coroutine> updateLogics;

	// Score
	private const    int   baseScore  = 100;
	private readonly int[] scoreValue = { 1, 2, 4, 8 };
	private static   int   comboIdx;
	private static   int   totalScore;

	// Test
	public static  bool             testGrid;
	public static  bool             gridRegen;
	public static  bool             testBlock;
	private static bool             loaded;
	public static  int              testHeight;
	private static int              testFieldSize;
	public         Block.BLOCK_TYPE testBlockType;

	// Grid / Blocks
	public static  GameGrid   grid;
	private static BlockQueue BlockQueue { get; set; }
	public static  Block      currentBlock;
	public static  Block      shadowBlock;
	public static  Block      saveBlock;
	public static  bool       canSave;
	public const   float      blockSize    = 1.0f;
	private const  float      downInterval = 1.0f;

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
		loaded = false;

		Init();

#if UNITY_EDITOR
		grid = new GameGrid(ref gridSize, blockSize);

#else
		grid = new GameGrid(ref gridSize, blockSize);

#endif

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

		loaded = true;
	}

	public override void Init()
	{
		isGameOver = false;
		isPause    = true;
		totalScore = 0;

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
		if (!loaded) return;

		if (isGameOver)
		{
			Terminate();
		}

		INPUT_CONTROL control = InputSystem.Instance.InputWindows();

		if (!isPause)
		{
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
					GamePause();

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
					GameResume();

					break;
			}
		}
	}

#endregion

#region GameControl

	private void SaveBlock()
	{
		canSave = false;
		AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.SHIFT);
		currentBlock = BlockQueue.SaveAndUpdateBlock(currentBlock);
		RenderSystem.Instance.RefreshCurrentBlock();
	}

	public void GameStart()
	{
		AudioSystem.Instance.AudioGameStart();

		StartCoroutine(CameraGameStart(StartLogic));
	}

	private static IEnumerator CameraGameStart(Callback func)
	{
		const float unit = 0.05f;

		for (float elapsed = 0f; elapsed < 2f; elapsed += unit)
		{
			CameraSystem.mainCamera.transform.rotation = Quaternion.Slerp(CameraSystem.mainCamera.transform.rotation,
			                                                              Quaternion.LookRotation(
				                                                               new Vector3(0f, -6.35f, 23.7f)), 0.01f);

			//yield return new WaitForSeconds(unit);
		}

		CameraSystem.mainCamera.transform.rotation = CameraSystem.initRotation;

		func();

		yield break;
	}

	private void StartLogic()
	{
		updateLogics = new List<Coroutine>
		{
			StartCoroutine(BlockDown()),
			StartCoroutine(CameraSystem.AngleCalculate()),
		};

		isPause = false;
	}

	public IEnumerator GameHome()
	{
		AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.CLICK);

		yield return StartCoroutine(CameraSystem.MainMenu());

		isPause = true;

		AudioSystem.Instance.MainMenu();

		Reset();
	}

	private void Reset()
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

		currentBlock = BlockQueue.GetAndUpdateBlock();
		BlockQueue.SaveBlockReset();
		canSave = true;

		RenderSystem.Instance.Reset();
	}

	public void GamePause()
	{
		isPause = true;

		AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.PAUSE);

		AudioSystem.Instance.PauseBGM(AudioSystem.bgmVolumeAdj);

		foreach (Coroutine coroutine in updateLogics)
		{
			StopCoroutine(coroutine);
		}
	}

	public void GameResume()
	{
		isPause = false;

		AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.RESUME);

		AudioSystem.Instance.ResumeBGM(AudioSystem.bgmVolumeAdj);

		updateLogics.Add(StartCoroutine(BlockDown()));
		updateLogics.Add(StartCoroutine(CameraSystem.AngleCalculate()));
	}

	private IEnumerator BlockDown()
	{
		while (true)
		{
			RenderSystem.RenderCurrentBlock();

			if (!grid.IsPlaneEmpty(0))
			{
				isGameOver = true;

				break;
			}

			MoveBlockDown();

			EffectSystem.Instance.MoveRotationEffect();

			yield return new WaitForSeconds(downInterval);
		}
	}

	private void Terminate()
	{
		if (updateLogics.Count == 0) return;

		foreach (Coroutine coroutine in updateLogics)
		{
			StopCoroutine(coroutine);
		}
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
		ScoreCalc(cleared.Count);

		StartCoroutine(EffectSystem.Instance.ClearEffect(cleared));

		if (cleared.Count == 4)
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.TETRIS1);
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.TETRIS2);

			StartCoroutine(CameraSystem.CameraFOVEffect());
		}
		else if (cleared.Count > 0)
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.CLEAR);
		}

		RenderSystem.RenderGrid();

		cleared.Clear();

		if (CheckGameOver())
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.GAME_OVER);

			isPause    = true;
			isGameOver = true;

			UISystem.Instance.PrintScore(totalScore);

			StartCoroutine(AudioSystem.Instance.PitchDownBGM(0.2f));
			StartCoroutine(EffectSystem.Instance.GameOverEffect());
			StartCoroutine(EnvironmentSystem.Instance.AnimStop());
			StartCoroutine(UISystem.Instance.FadeOutIn("PlayScreen", "GameOverScreen", 1f));
		}
		else
		{
			canSave      = true;
			currentBlock = BlockQueue.GetAndUpdateBlock();
			RenderSystem.Instance.RefreshCurrentBlock();
		}
	}

	private void ScoreCalc(int cleared)
	{
		if (cleared == 0)
		{
			comboIdx = 0;

			return;
		}

		totalScore += baseScore * (int)Mathf.Pow(scoreValue[cleared - 1], ++comboIdx);
		UISystem.Instance.ScoreUpdate(totalScore);
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
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

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
			AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.Pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.Size * blockSize * 0.5f);

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

			RenderSystem.Instance.RefreshCurrentBlock();
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
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

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
			AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.Pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.Size * blockSize * 0.5f);

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

			RenderSystem.Instance.RefreshCurrentBlock();
		}
	}

	public void RotateBlockY()
	{
		currentBlock.RotateYClockWise();

		if (!BlockFits(currentBlock))
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.RotateYCounterClockWise();
		}
		else
		{
			AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.Pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.Size * blockSize * 0.5f);
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

			RenderSystem.Instance.RefreshCurrentBlock();
		}
	}

	public void RotateBlockYInv()
	{
		currentBlock.RotateYCounterClockWise();

		if (!BlockFits(currentBlock))
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.RotateYClockWise();
		}
		else
		{
			AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.Pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.Size * blockSize * 0.5f);
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

			RenderSystem.Instance.RefreshCurrentBlock();
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
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

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
			AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.Pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.Size * blockSize * 0.5f);

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

			RenderSystem.Instance.RefreshCurrentBlock();
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
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

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
			AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.ROTATE1, AudioSystem.SFX_VALUE.ROTATE2);

			Vector3 offset = RenderSystem.startOffset                    + currentBlock.Pos.ToVector() +
			                 new Vector3(-0.5f, 0.5f, -0.5f) * blockSize +
			                 new Vector3(1f,    -1f,  1f)    * (currentBlock.Size * blockSize * 0.5f);

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

			RenderSystem.Instance.RefreshCurrentBlock();
		}
	}

#endregion

#region BlockMove

	public void MoveBlockLeft()
	{
		currentBlock.Move(Coord.Left[CameraSystem.viewAngle]);

		if (!BlockFits(currentBlock))
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.Move(Coord.Right[CameraSystem.viewAngle]);
		}
		else
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.MOVE);

			RenderSystem.Instance.RefreshCurrentBlock();
		}
	}

	public void MoveBlockRight()
	{
		currentBlock.Move(Coord.Right[CameraSystem.viewAngle]);

		if (!BlockFits(currentBlock))
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.Move(Coord.Left[CameraSystem.viewAngle]);
		}
		else
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.MOVE);

			RenderSystem.Instance.RefreshCurrentBlock();
		}
	}

	public void MoveBlockForward()
	{
		currentBlock.Move(Coord.Forward[CameraSystem.viewAngle]);

		if (!BlockFits(currentBlock))
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.Move(Coord.Backward[CameraSystem.viewAngle]);
		}
		else
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.MOVE);

			RenderSystem.Instance.RefreshCurrentBlock();
		}
	}

	public void MoveBlockBackward()
	{
		currentBlock.Move(Coord.Backward[CameraSystem.viewAngle]);

		if (!BlockFits(currentBlock))
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.UNAVAILABLE);

			currentBlock.Move(Coord.Forward[CameraSystem.viewAngle]);
		}
		else
		{
			AudioSystem.Instance.BurstSFX(AudioSystem.SFX_VALUE.MOVE);

			RenderSystem.Instance.RefreshCurrentBlock();
		}
	}

	private void MoveBlockDown()
	{
		currentBlock.Move(Coord.Down);

		if (BlockFits(currentBlock))
		{
			RenderSystem.RenderCurrentBlock();

			return;
		}

		AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.DROP1, AudioSystem.SFX_VALUE.DROP2);

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
			AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.HARD_DROP1, AudioSystem.SFX_VALUE.HARD_DROP5);

			StartCoroutine(CameraSystem.Instance.Shake());
		}
		else
		{
			AudioSystem.Instance.PlayRandomSFX(AudioSystem.SFX_VALUE.DROP1, AudioSystem.SFX_VALUE.DROP2);
		}

		currentBlock.Move(Coord.Up);
		EffectSystem.DropEffect();
		PlaceBlock();
	}

#endregion
}