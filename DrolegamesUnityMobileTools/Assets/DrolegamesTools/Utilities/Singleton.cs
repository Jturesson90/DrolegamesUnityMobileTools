namespace Drolegames.Utils
{
    using UnityEngine;

    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        [Header("Singleton settings")]
        [SerializeField] private bool m_DontDestroyOnLoad = false;
        public static T Current { get; private set; }
        public static bool IsInitialized => Current != null;

        protected virtual void Awake()
        {
            if (Current != null)
            {
                Debug.LogWarning("Destroying Multiple Singletons of same Type " + gameObject.name);
                DestroyImmediate(gameObject);
            }
            else
            {
                Current = (T)this;
                if (m_DontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }
        protected virtual void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }
    }
}