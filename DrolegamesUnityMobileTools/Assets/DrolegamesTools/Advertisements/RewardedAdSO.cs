namespace Drolegames.Advertisements
{
    using System;
    using UnityEngine;
    using UnityEngine.Advertisements;

    public class RewardedAdSO : AdSO, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private Action<bool> loadCallback;
        private Action<RewardFinished> showCallback;
        private string referenceId;
        public override void Load(Action<bool> callback)
        {
            loadCallback = callback;
            Advertisement.Load(AdUnitId, this);
        }

        public void Show(string referenceId, Action<RewardFinished> callback)
        {
            showCallback = callback;
            this.referenceId = referenceId;
        }

        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            if (adUnitId.Equals(AdUnitId))
                loadCallback?.Invoke(true);
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            if (adUnitId.Equals(AdUnitId))
                loadCallback?.Invoke(false);
        }

        public void OnUnityAdsShowClick(string adUnitId)
        {
            Debug.Log("OnUnityAdsShowClick");
        }

        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        {
            if (adUnitId.Equals(AdUnitId))
            {
                Advertisement.Load(adUnitId, this);
                showCallback?.Invoke(new RewardFinished((AdvertisementResult)showCompletionState, referenceId));
                showCallback = null;
            }
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            if (adUnitId.Equals(AdUnitId))
            {
                Advertisement.Load(adUnitId, this);
                showCallback?.Invoke(new RewardFinished(AdvertisementResult.UNKNOWN, referenceId));
                showCallback = null;
            }
        }

        public void OnUnityAdsShowStart(string adUnitId)
        {
            Debug.Log("OnUnityAdsShowStart");
        }
    }
}