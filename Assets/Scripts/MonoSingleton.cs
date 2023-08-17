using System;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static          T      instance;
	private static readonly object locker = new();
	public static T Instance
	{
		get
		{
			lock (locker)
			{
				if (instance == null)
				{
					GameObject obj = GameObject.Find(typeof(T).Name);

					if (obj)
					{
						instance = obj.GetComponent<T>();
					}
					else
					{
						obj      = new GameObject(typeof(T).Name);
						instance = obj.AddComponent<T>();
					}
					
					DontDestroyOnLoad(obj);
				}
			}
			
			return instance;
		}
	}
	protected delegate void Callback();

	private void OnApplicationQuit()
	{
		if (instance != null) Destroy(instance.gameObject);
		instance = null;
	}
}