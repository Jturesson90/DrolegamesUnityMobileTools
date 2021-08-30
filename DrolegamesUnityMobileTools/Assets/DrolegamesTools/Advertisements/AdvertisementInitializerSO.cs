namespace Drolegames.Advertisements
{
    using System;
    using UnityEngine;
    using UnityEngine.Advertisements;
    [CreateAssetMenu(fileName = "", menuName = "Drolegames/Advertisements/Initializer")]
    public class AdvertisementInitializerSO : ScriptableObject, IUnityAdsInitializationListener
    {
        [SerializeField] bool m_TestMode = false;
        [SerializeField] public bool m_EnablePerPlacementLoad = false;
        [SerializeField] public string m_iOSGameId = string.Empty;
        [SerializeField] public string m_AndroidGameId = string.Empty;

        private string gameId = string.Empty;

        Action<bool> callback;
        public void Initialize(Action<bool> callback)
        {
            this.callback = callback;
#if UNITY_IOS
            gameId = iOSGameId;
#else
            gameId = m_AndroidGameId;
#endif
            Advertisement.Initialize(gameId, m_TestMode, m_EnablePerPlacementLoad, this);
        }

        public void OnInitializationComplete()
        {
            callback?.Invoke(true);
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            callback?.Invoke(false);
            Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }
    }
}