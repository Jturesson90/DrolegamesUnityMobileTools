namespace Drolegames.Advertisements
{
    using System;
    using UnityEngine;
    using UnityEngine.Advertisements;
    using Drolegames.Utils;

    public class AdvertisementsManager : Singleton<AdvertisementsManager>, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        public bool testMode = false;
        public bool enablePerPlacementLoad = false;
        public string iOSGameId = string.Empty;
        public string androidGameId = string.Empty;
        public string mockGameId = string.Empty;
        public string rewardVideoID = string.Empty;

        private string gameId = string.Empty;
        private bool _adsIsReady = false;

        public bool AdsIsReady
        {
            get => _adsIsReady;
            set
            {
                if (_adsIsReady != value)
                {
                    _adsIsReady = value;
                    OnAdsIsReadyChanged?.Invoke(this, value);
                }
            }
        }
        public static event EventHandler<bool> OnAdsIsReadyChanged;
        public static event EventHandler<AdsFinishedEventArgs> OnRewardVideoSuccess;

        private string currentId;
        private void Start()
        {
#if UNITY_IOS
            gameId = iOSGameId;
#elif UNITY_ANDROID
            gameId = androidGameId;
#else
            gameId = mockGameId;
#endif
            Initialize();
        }
        public void Initialize()
        {
            Advertisement.Initialize(gameId, testMode, enablePerPlacementLoad, this);
        }

        public bool GetIsReady() => Advertisement.IsReady(rewardVideoID);
        public bool ShowRewardVideo(string id)
        {
            if (!GetIsReady()) return false;
            Advertisement.Show(rewardVideoID, this);
            currentId = id;
            return true;
        }

        #region IUnityAdsInitializationListener
        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
            Advertisement.Load(gameId, this);
        }
        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogWarning($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }
        #endregion

        #region IUnityAdsLoadListener
        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId.Equals(rewardVideoID))
            {
                AdsIsReady = true;
            }
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogWarning($"Unity Ads Load Failed: {error.ToString()} - {message}");
        }
        #endregion

        #region IUnityAdsShowListener
        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogWarning($"Unity Ads Show Failed: {error.ToString()} - {message}");
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            if (placementId.Equals(rewardVideoID))
            {
                AdsIsReady = false;
            }
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            if (placementId.Equals(rewardVideoID))
            {
                AdsIsReady = false;
            }
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (rewardVideoID.Equals(placementId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                OnRewardVideoSuccess?.Invoke(this, new AdsFinishedEventArgs((AdvertisementResult)showCompletionState, currentId));
            }
            Advertisement.Load(gameId, this);
        }
        #endregion



    }
    public class AdsFinishedEventArgs : EventArgs
    {
        public AdvertisementResult showResult;
        public string id;
        public AdsFinishedEventArgs(AdvertisementResult showResult, string id)
        {
            this.showResult = showResult;
            this.id = id;
        }
    }
    public enum AdvertisementResult
    {
        SKIPPED = 0,
        COMPLETED = 1,
        UNKNOWN = 2
    }
}