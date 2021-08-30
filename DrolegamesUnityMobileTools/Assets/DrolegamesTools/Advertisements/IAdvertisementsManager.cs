namespace Drolegames.Advertisements
{
    using System;
    public interface IAdvertisementsManager
    {
        bool ShowBanner();
        bool HideBanner();

        bool ShowInterstitial();
        bool ShowRewardedVideo(string referenceId, Action<RewardFinished> callback);
    }
}