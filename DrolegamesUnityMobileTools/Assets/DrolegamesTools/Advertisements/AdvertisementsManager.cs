namespace Drolegames.Advertisements
{
    using System;
    using UnityEngine;
    using UnityEngine.Advertisements;
    using Drolegames.Utils;
    using System.Collections.Generic;

    public class AdvertisementsManager : Singleton<AdvertisementsManager>, IAdvertisementsManager
    {
        [SerializeField] AdvertisementInitializerSO initializer;
        [SerializeField] BannerAdSO bannerAd;
        [SerializeField] InterstitialAdSO interstitialAd;
        [SerializeField] RewardedAdSO rewardedAd;

        public static event Action<RewardFinished> OnRewardVideoFinished;

        private List<AdSO> allAds = new List<AdSO>();

        protected override void Awake()
        {
            base.Awake();
            allAds = new List<AdSO>();
            if (bannerAd)
                allAds.Add(bannerAd);
            if (interstitialAd)
                allAds.Add(interstitialAd);
            if (rewardedAd)
                allAds.Add(rewardedAd);

            if (!initializer)
            {
                Debug.LogError("Missing AdvertisementInitializer in AdvertisementsManager");
                return;
            }

            initializer.Initialize((bool success) =>
            {
                if (success)
                {
                    allAds.ForEach(a => a.Load((bool loadSuccess) => { }));
                }
            });
        }

        public bool ShowBanner()
        {
            if (bannerAd && bannerAd.IsLoadedAndReady())
            {
                bannerAd.Show();
                return true;
            }
            return false;
        }

        public bool HideBanner()
        {
            if (bannerAd && bannerAd.IsLoadedAndReady())
            {
                bannerAd.Hide();
                return true;
            }
            return false;
        }

        public bool ShowInterstitial()
        {
            if (interstitialAd && interstitialAd.IsLoadedAndReady())
            {
                interstitialAd.Show();
                return true;
            }
            return false;
        }

        public bool ShowRewardedVideo(string referenceId, Action<RewardFinished> callback)
        {
            if (rewardedAd && rewardedAd.IsLoadedAndReady())
            {
                rewardedAd.Show(referenceId, (RewardFinished rewardAdFinish) =>
                {
                    OnRewardVideoFinished?.Invoke(rewardAdFinish);
                    callback?.Invoke(rewardAdFinish);
                });
                return true;
            }
            return false;
        }
    }

    public struct RewardFinished
    {
        public AdvertisementResult showResult;
        public string referenceId;
        public RewardFinished(AdvertisementResult showResult, string referenceId)
        {
            this.showResult = showResult;
            this.referenceId = referenceId;
        }
    }
    public enum AdvertisementResult
    {
        SKIPPED = 0,
        COMPLETED = 1,
        UNKNOWN = 2
    }
}