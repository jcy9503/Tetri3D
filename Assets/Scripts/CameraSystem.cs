using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraSystem : MonoBehaviour
{
	public static    GameObject mainCameraObj;
	private          Camera     mainCamera;
	private          Transform  rotatorTr;
	private          Quaternion mementoRotation;
	private readonly Quaternion initRotation        = Quaternion.Euler(15f, 0f, 0f);
	private const    float      rotationConstraintX = 55f;
	private const    float      speed               = 2000f;
	private const    float      rotationSpeed       = 1f;
	private const    float      shakeAmount         = 0.5f;
	private const    float      shakeTime           = 0.2f;
	public           bool       isShaking;
	private          int        viewAngle;
	private          bool       checkDir;
	private          bool       dir;
	private bool Dir
	{
		get => dir;
		set
		{
			if (value != dir)
				checkDir = true;
			dir = value;
		}
	}

	public CameraSystem()
	{
		Init();
	}

	private void Init()
	{
		mainCameraObj                     = GameObject.Find("Main Camera");
		mainCameraObj!.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
		mainCamera                        = mainCameraObj.GetComponent<CameraSystem>();
		rotatorTr                         = GameObject.Find("Rotator").GetComponent<Transform>();
		mementoRotation                   = Quaternion.identity;
		Dir                               = false;
		checkDir                          = false;
		viewAngle                         = 0;
		isShaking                         = false;
	}

	public void RotateHorizontal(bool direction)
	{
		Quaternion target = rotatorTr.rotation;

		target *= Quaternion.AngleAxis(direction ? rotationSpeed : -rotationSpeed, Vector3.right);

		float angleX = target.eulerAngles.x;
		angleX = angleX > 180 ? angleX - 360 : angleX;
		target.eulerAngles = new Vector3(Mathf.Clamp(angleX, -rotationConstraintX,
		                                             rotationConstraintX),
		                                 target.eulerAngles.y, 0f);
		rotatorTr.rotation = Quaternion.RotateTowards(rotatorTr.rotation, target,
		                                              speed * Time.deltaTime);
	}

	public void RotateVertical(bool direction)
	{
		Quaternion rotation = rotatorTr.rotation;
		Quaternion target   = rotation;

		target             *= Quaternion.AngleAxis(direction ? rotationSpeed : -rotationSpeed, Vector3.up);
		target.eulerAngles =  new Vector3(target.eulerAngles.x, target.eulerAngles.y, 0f);

		rotation = Quaternion.RotateTowards(rotation, target,
		                                    speed * Time.deltaTime);
		rotatorTr.rotation = rotation;
	}

	public IEnumerator AngleCalculate()
	{
		while (true)
		{
			if (gameOver) break;

			viewAngle = rotatorTr.rotation.eulerAngles.y switch
			{
				<= 45f or > 315f   => 0,
				<= 135f and > 45f  => 1,
				<= 225f and > 135f => 2,
				_                  => 3
			};

			yield return new WaitForSeconds(defaultKeyInterval);
		}
	}

	private IEnumerator Shake()
	{
		isShaking = true;
		float timer = 0;

		while (timer <= shakeTime)
		{
			rotatorTr.position =  (Vector3)Random.insideUnitCircle * shakeAmount;
			timer              += Time.deltaTime;

			yield return null;
		}

		RotatorPositionClear();
	}

	public IEnumerator GameStart()
	{
		float pastTime = 0f;

		while (pastTime < 2f)
		{
			mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation,
			                                                 Quaternion.LookRotation(new Vector3(0f, -6.35f, 23.7f)),
			                                                 0.05f);

			yield return new WaitForSeconds(0.01f);

			pastTime += 0.01f;
		}

		mainCamera.transform.rotation = initRotation;
	}

	public IEnumerator MainMenu()
	{
		float pastTime = 0f;

		while (pastTime < 2f)
		{
			mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation,
			                                                 Quaternion.LookRotation(Vector3.right),
			                                                 0.05f);

			yield return new WaitForSeconds(0.01f);

			pastTime += 0.01f;
		}

		mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
	}

	private void RotatorPositionClear()
	{
		rotatorTr.position = Vector3.zero;

		isShaking = false;
	}
}