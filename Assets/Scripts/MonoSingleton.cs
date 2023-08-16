using System;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static          bool   shuttingDown;
	private static          T      instance;
	private static readonly object locker = new();
	public static T Instance
	{
		get
		{
			if (instance == null && !shuttingDown)
			{
				instance = FindObjectOfType(typeof(T)) as T;

				if (!instance)
				{
					GameObject obj = new(nameof(T));
					instance = obj.AddComponent<T>();
					DontDestroyOnLoad(obj);
				}
			}

			return instance;
			// if (shuttingDown)
			// {
			// 	Debug.Log("[Singleton] instance " + typeof(T) + " already deleted. Returning null.");
			//
			// 	return null;
			// }
			//
			// lock (locker)
			// {
			// 	if (instance == null)
			// 	{
			// 		instance = (T)FindObjectOfType(typeof(T));
			//
			// 		if (instance == null)
			// 		{
			// 			GameObject temp = new(typeof(T).ToString());
			// 			instance = temp.AddComponent<T>();
			// 		}
			//
			// 		DontDestroyOnLoad(instance);
			// 	}
			// }
			//
			// return instance;
		}
	}

	protected delegate void Callback();

	private void Awake()
	{
		if (instance != null)
			instance = this as T;
	}

	private void OnApplicationQuit()
	{
		shuttingDown = true;
		if (instance != null) Destroy(instance.gameObject);
		instance = null;
	}
}