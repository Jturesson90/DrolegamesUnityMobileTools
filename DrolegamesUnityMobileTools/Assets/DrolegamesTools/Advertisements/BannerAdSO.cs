namespace Drolegames.Advertisements
{
    using System;
    using UnityEngine;
    using UnityEngine.Advertisements;

    [CreateAssetMenu(fileName = "BannerAd", menuName = "Drolegames/Advertisements/Banner")]
    public class BannerAdSO : AdSO
    {
        [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;
        public override void Load(Action<bool> callback)
        {
            Advertisement.Banner.SetPosition(_bannerPosition);
            BannerLoadOptions options = new BannerLoadOptions
            {
                loadCallback = () => callback?.Invoke(true),
                errorCallback = (string message) => callback?.Invoke(true),
            };
            Advertisement.Banner.Load(AdUnitId, options);
        }

        public void Show()
        {
            // Set up options to notify the SDK of show events:
            BannerOptions options = new BannerOptions
            {
                clickCallback = () => { },
                hideCallback = () => { },
                showCallback = () => { }
            };

            // Show the loaded Banner Ad Unit:
            Advertisement.Banner.Show(AdUnitId, options);
        }
        public void Hide()
        {
            Advertisement.Banner.Hide();
        }
    }
}