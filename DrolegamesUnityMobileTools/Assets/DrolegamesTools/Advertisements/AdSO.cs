namespace Drolegames.Advertisements
{
    using System;
    using UnityEngine;
    using UnityEngine.Advertisements;

    public abstract class AdSO : ScriptableObject
    {
        [SerializeField] string _androidAdUnitId = "Interstitial_Android";
        [SerializeField] string _iOsAdUnitId = "Interstitial_iOS";
        protected string AdUnitId
        {
            get
            {
#if UNITY_IOS
return _iOsAdUnitId;
#else
                return _androidAdUnitId;
#endif
            }
        }
        public abstract void Load(Action<bool> callback = null);
        public bool IsLoadedAndReady() => Advertisement.IsReady(AdUnitId);
    }
}