using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static          bool   shuttingDown;
	private static          T      instance;
	private static readonly object locker = new object();
	public static T Instance
	{
		get
		{
			if (shuttingDown)
			{
				Debug.Log("[Singleton] instance " + typeof(T) + " already deleted. Returning null.");

				return null;
			}

			lock (locker)
			{
				if (instance == null)
				{
					instance = (T)FindObjectOfType(typeof(T));

					if (instance == null)
					{
						GameObject temp = new(typeof(T).ToString());
						instance = temp.AddComponent<T>();
					}

					DontDestroyOnLoad(instance);
				}
			}

			return instance;
		}
	}

	private void OnApplicationQuit()
	{
		shuttingDown = true;
	}

	private void OnDestroy()
	{
		shuttingDown = true;
	}

	public abstract void Init();
}