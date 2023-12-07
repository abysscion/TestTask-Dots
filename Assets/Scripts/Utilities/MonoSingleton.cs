using UnityEngine;

namespace Utilities
{
	public abstract class MonoSingleton<T> : MonoBehaviour where T : Component
	{
		[SerializeField] private bool autoInitializeOnStart = true;
		[SerializeField] private bool dontDestroyOnLoad = true;

		private static T _instance;

		public static T Instance => _instance;
		public static bool Exists => _instance != null;

		public abstract void Initialize();

		protected virtual void Start()
		{
			if (autoInitializeOnStart)
				Initialize();
		}

		protected virtual void Awake()
		{
			if (_instance == null)
			{
				if (this is T)
				{
					_instance = this as T;
					if (dontDestroyOnLoad && Application.isPlaying)
						DontDestroyOnLoad(gameObject);
				}
			}
			else if (Application.isPlaying)
			{
				Debug.LogWarning($"[Singleton] Instance {typeof(T)} already exists. Destroying {name}...");
				DestroyImmediate(gameObject);
			}
		}
	}
}