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

	private static void RotatorPositionClear()
	{
		rotatorTr.position = Vector3.zero;

		isShaking = false;
	}
}