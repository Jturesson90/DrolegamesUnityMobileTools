namespace Drolegames.Advertisements
{
    using System;
    using UnityEngine;
    using UnityEngine.Advertisements;

    [CreateAssetMenu(fileName = "InterstitialAd", menuName = "Drolegames/Advertisements/Interstitial")]
    public class InterstitialAdSO : AdSO, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private Action<bool> loadCallback;
        public override void Load(Action<bool> callback)
        {
            loadCallback = callback;
            Advertisement.Load(AdUnitId, this);
        }
        public void Show()
        {
            if (Advertisement.IsReady(AdUnitId))
                Advertisement.Show(AdUnitId, this);
        }

        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            if (adUnitId.Equals(AdUnitId))
            {
                loadCallback?.Invoke(true);
            }
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            if (adUnitId.Equals(AdUnitId))
            {
                loadCallback?.Invoke(false);
            }
        }

        public void OnUnityAdsShowClick(string adUnitId)
        {

        }

        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        {

        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {

        }

        public void OnUnityAdsShowStart(string adUnitId)
        {

        }
    }
}