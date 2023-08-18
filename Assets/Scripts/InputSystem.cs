using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class InputSystem : MonoSingleton<InputSystem>
{
	private enum KEY_VALUE
	{
		LEFT = 0,
		RIGHT,
		UP,
		DOWN,
		ROTATE_X,
		ROTATE_X_INV,
		ROTATE_Y,
		ROTATE_Y_INV,
		ROTATE_Z,
		ROTATE_Z_INV,
		SPACE,
		LEFT_ALT,
		ESC
	}

	private         Vector2     clickPos;
	private         List<bool>  keyUsing;
	private         List<float> keyIntervals;
	public const    float       defaultKeyInterval = 0.2f;
	public delegate void        LogicFunc();

	public void Init()
	{
		clickPos = Vector2.zero;

		keyUsing = new List<bool>
		{
			false, false, false, false, false, false,
			false, false, false, false, false, false,
			false,
		};
		keyIntervals = new List<float>()
		{
			defaultKeyInterval, defaultKeyInterval, defaultKeyInterval,
			defaultKeyInterval, defaultKeyInterval, defaultKeyInterval,
			defaultKeyInterval, defaultKeyInterval, defaultKeyInterval,
			defaultKeyInterval, defaultKeyInterval, 0.1f,
			2f,
		};
	}

	public GameManager.INPUT_CONTROL InputWindows()
	{
	#region ScreenControl

		if (Input.GetKey(KeyCode.UpArrow) && !CameraSystem.isShaking)
		{
			CameraSystem.RotateHorizontal(false);
		}

		if (Input.GetKey(KeyCode.DownArrow) && !CameraSystem.isShaking)
		{
			CameraSystem.RotateHorizontal(true);
		}

		if (Input.GetKey(KeyCode.LeftArrow) && !CameraSystem.isShaking)
		{
			CameraSystem.RotateVertical(false);
		}

		if (Input.GetKey(KeyCode.RightArrow) && !CameraSystem.isShaking)
		{
			CameraSystem.RotateVertical(true);
		}

	#endregion

	#region BlockControl

		if (Input.GetKey(KeyCode.A) && !keyUsing[(int)KEY_VALUE.LEFT])
		{
			keyUsing[(int)KEY_VALUE.LEFT] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.LEFT));

			return GameManager.INPUT_CONTROL.MOVE_LEFT;
		}

		if (Input.GetKey(KeyCode.D) && !keyUsing[(int)KEY_VALUE.RIGHT])
		{
			keyUsing[(int)KEY_VALUE.RIGHT] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.RIGHT));

			return GameManager.INPUT_CONTROL.MOVE_RIGHT;
		}

		if (Input.GetKey(KeyCode.W) && !keyUsing[(int)KEY_VALUE.UP])
		{
			keyUsing[(int)KEY_VALUE.UP] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.UP));

			return GameManager.INPUT_CONTROL.MOVE_FORWARD;
		}

		if (Input.GetKey(KeyCode.S) && !keyUsing[(int)KEY_VALUE.DOWN])
		{
			keyUsing[(int)KEY_VALUE.DOWN] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.DOWN));

			return GameManager.INPUT_CONTROL.MOVE_BACKWARD;
		}

		if ((Input.GetKey(KeyCode.O) || Input.GetKey(KeyCode.Keypad7)) && !keyUsing[(int)KEY_VALUE.ROTATE_X])
		{
			keyUsing[(int)KEY_VALUE.ROTATE_X] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.ROTATE_X));

			return GameManager.INPUT_CONTROL.ROTATE_X_INV;
		}

		if ((Input.GetKey(KeyCode.P) || Input.GetKey(KeyCode.Keypad8)) && !keyUsing[(int)KEY_VALUE.ROTATE_X_INV])
		{
			keyUsing[(int)KEY_VALUE.ROTATE_X_INV] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.ROTATE_X_INV));

			return GameManager.INPUT_CONTROL.ROTATE_X;
		}

		if ((Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.Keypad4)) && !keyUsing[(int)KEY_VALUE.ROTATE_Y])
		{
			keyUsing[(int)KEY_VALUE.ROTATE_Y] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.ROTATE_Y));

			return GameManager.INPUT_CONTROL.ROTATE_Y;
		}

		if ((Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.Keypad5)) && !keyUsing[(int)KEY_VALUE.ROTATE_Y_INV])
		{
			keyUsing[(int)KEY_VALUE.ROTATE_Y_INV] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.ROTATE_Y_INV));

			return GameManager.INPUT_CONTROL.ROTATE_Y_INV;
		}

		if ((Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Keypad1)) && !keyUsing[(int)KEY_VALUE.ROTATE_Z])
		{
			keyUsing[(int)KEY_VALUE.ROTATE_Z] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.ROTATE_Z));

			return GameManager.INPUT_CONTROL.ROTATE_Z_INV;
		}

		if ((Input.GetKey(KeyCode.Comma) || Input.GetKey(KeyCode.Keypad2)) &&
		    !keyUsing[(int)KEY_VALUE.ROTATE_Z_INV])
		{
			keyUsing[(int)KEY_VALUE.ROTATE_Z_INV] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.ROTATE_Z_INV));

			return GameManager.INPUT_CONTROL.ROTATE_Z;
		}

		if (Input.GetKey(KeyCode.Space) && !keyUsing[(int)KEY_VALUE.SPACE])
		{
			keyUsing[(int)KEY_VALUE.SPACE] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.SPACE));

			return GameManager.INPUT_CONTROL.BLOCK_DROP;
		}

		if (Input.GetKey(KeyCode.LeftAlt) && !keyUsing[(int)KEY_VALUE.LEFT_ALT])
		{
			keyUsing[(int)KEY_VALUE.LEFT_ALT] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.LEFT_ALT));

			return GameManager.INPUT_CONTROL.BLOCK_DOWN;
		}

		if (Input.GetKey(KeyCode.LeftShift) && GameManager.canSave)
		{
			return GameManager.INPUT_CONTROL.BLOCK_SAVE;
		}

	#endregion

	#region Game Control

		if (Input.GetKey(KeyCode.Escape) && !keyUsing[(int)KEY_VALUE.ESC])
		{
			keyUsing[(int)KEY_VALUE.ESC] = true;
			StartCoroutine(KeyRewind((int)KEY_VALUE.ESC));

			return GameManager.INPUT_CONTROL.PAUSE;
		}

	#endregion

		return GameManager.INPUT_CONTROL.DEFAULT;
	}

	public void InputMouse()
	{
		if (Input.GetMouseButtonDown(0) && !CameraSystem.isShaking)
		{
			CameraSystem.mementoRotation = CameraSystem.rotatorTr.rotation;
			clickPos                     = Vector2.zero;
		}

		if (Input.GetMouseButton(0) && !CameraSystem.isShaking)
		{
			Vector2 angle = new(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

			clickPos         += angle;
			CameraSystem.Dir =  Mathf.Abs(clickPos.x) > Mathf.Abs(clickPos.y);

			if (CameraSystem.checkDir)
			{
				CameraSystem.rotatorTr.rotation = CameraSystem.mementoRotation;
				CameraSystem.checkDir           = false;
				clickPos                        = Vector2.zero;
			}

			Quaternion target = CameraSystem.rotatorTr.rotation;

			if (CameraSystem.Dir)
			{
				target *= Quaternion.AngleAxis(angle.x, Vector3.right);
			}
			else
			{
				target *= Quaternion.AngleAxis(angle.y, Vector3.up);
			}

			float angleX = target.eulerAngles.x;
			angleX = angleX > 180 ? angleX - 360 : angleX;
			target.eulerAngles = new Vector3(Mathf.Clamp(angleX, -CameraSystem.rotationConstraintX,
			                                             CameraSystem.rotationConstraintX),
			                                 target.eulerAngles.y, 0f);
			CameraSystem.rotatorTr.rotation = Quaternion.RotateTowards(CameraSystem.rotatorTr.rotation, target,
			                                                           CameraSystem.cameraSpeed * Time.deltaTime);
		}

		if (Input.GetMouseButtonUp(0) && !CameraSystem.isShaking)
		{
			CameraSystem.mementoRotation = CameraSystem.rotatorTr.rotation;
		}
	}

	private IEnumerator KeyRewind(int id)
	{
		yield return new WaitForSeconds(keyIntervals[id]);

		keyUsing[id] = false;
	}
}