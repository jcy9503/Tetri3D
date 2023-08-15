using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class CameraSystem : MonoSingleton<CameraSystem>
{
	public static          GameObject mainCameraObj;
	public static          Camera     mainCamera;
	public static          Transform  rotatorTr;
	public static          Quaternion mementoRotation;
	public static readonly Quaternion initRotation        = Quaternion.Euler(15f, 0f, 0f);
	public const           float      rotationConstraintX = 55f;
	public const           float      cameraSpeed         = 2000f;
	private const          float      rotationSpeed       = 1f;
	private const          float      shakeAmount         = 0.5f;
	private const          float      shakeTime           = 0.2f;
	public static          bool       isShaking;
	public static          int        viewAngle;
	public static          bool       checkDir;
	private static         bool       dir;
	public static bool Dir
	{
		get => dir;
		set
		{
			if (value != dir)
				checkDir = true;
			dir = value;
		}
	}
	private static readonly int speed = Shader.PropertyToID("_Speed");

	public override void Init()
	{
		mementoRotation = Quaternion.identity;
		Dir             = false;
		checkDir        = false;
		viewAngle       = 0;
		isShaking       = false;

		mainCameraObj                     = GameObject.Find("Main Camera");
		mainCameraObj!.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

		mainCamera = mainCameraObj.GetComponent<Camera>();
		rotatorTr  = GameObject.Find("Rotator").GetComponent<Transform>();
	}

	public static void RotateHorizontal(bool direction)
	{
		Quaternion target = rotatorTr.rotation;

		target *= Quaternion.AngleAxis(direction ? rotationSpeed : -rotationSpeed, Vector3.right);

		float angleX = target.eulerAngles.x;
		angleX = angleX > 180 ? angleX - 360 : angleX;
		target.eulerAngles = new Vector3(Mathf.Clamp(angleX, -rotationConstraintX,
		                                             rotationConstraintX),
		                                 target.eulerAngles.y, 0f);
		rotatorTr.rotation = Quaternion.RotateTowards(rotatorTr.rotation, target,
		                                              cameraSpeed * Time.deltaTime);
	}

	public static void RotateVertical(bool direction)
	{
		Quaternion rotation = rotatorTr.rotation;
		Quaternion target   = rotation;

		target             *= Quaternion.AngleAxis(direction ? rotationSpeed : -rotationSpeed, Vector3.up);
		target.eulerAngles =  new Vector3(target.eulerAngles.x, target.eulerAngles.y, 0f);

		rotation = Quaternion.RotateTowards(rotation, target,
		                                    cameraSpeed * Time.deltaTime);
		rotatorTr.rotation = rotation;
	}

	public static IEnumerator AngleCalculate()
	{
		while (true)
		{
			if (GameManager.isGameOver) break;

			viewAngle = rotatorTr.rotation.eulerAngles.y switch
			{
				<= 45f or > 315f   => 0,
				<= 135f and > 45f  => 1,
				<= 225f and > 135f => 2,
				_                  => 3
			};

			yield return new WaitForSeconds(InputSystem.defaultKeyInterval);
		}
	}

	public IEnumerator Shake()
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

	public static IEnumerator MainMenu()
	{
		float elapsedTime = 0f;

		while (elapsedTime < 2f)
		{
			mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation,
			                                                 Quaternion.LookRotation(Vector3.right),
			                                                 0.001f);

			yield return new WaitForSeconds(0.05f);

			elapsedTime += 0.05f;
		}

		mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
	}

	private static void RotatorPositionClear()
	{
		rotatorTr.position = Vector3.zero;

		isShaking = false;
	}

	public static IEnumerator CameraFOVEffect()
	{
		const float target      = 120f;
		float       originFOV   = mainCamera.fieldOfView;
		Material    material    = GameManager.grid.Mesh.MRenderer.material;
		float       originSpeed = material.GetFloat(speed);

		GameManager.isPause = true;
		material.SetFloat(speed, 10f);

		while (mainCamera.fieldOfView < target - 1f)
		{
			mainCamera.fieldOfView           =  Mathf.Lerp(mainCamera.fieldOfView, target, 0.1f);
			AudioSystem.audioSourceBGM.pitch -= 0.02f;

			yield return new WaitForSeconds(0.01f);
		}

		while (mainCamera.fieldOfView > originFOV + 1f)
		{
			mainCamera.fieldOfView           =  Mathf.Lerp(mainCamera.fieldOfView, originFOV, 0.2f);
			AudioSystem.audioSourceBGM.pitch += 0.02f;

			yield return new WaitForSeconds(0.01f);
		}

		GameManager.isPause = false;

		mainCamera.fieldOfView = originFOV;
		GameManager.grid.Mesh.MRenderer.material.SetFloat(speed, originSpeed);
		AudioSystem.audioSourceBGM.pitch = 1f;
	}
}