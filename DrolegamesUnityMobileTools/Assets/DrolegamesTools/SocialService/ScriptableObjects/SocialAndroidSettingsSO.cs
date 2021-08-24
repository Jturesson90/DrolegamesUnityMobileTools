namespace Drolegames.SocialService
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "", menuName = "Drolegames/SocialSettings/Android")]
    public class SocialAndroidSettingsSO : SocialSettingsSO
    {
        [Header("Android Settings")]
        public bool debugLog = false;
    }
}