namespace Drolegames.SocialService
{
    using UnityEngine;

    public class SocialSettingsSO : ScriptableObject
    {
        [Header("Social services Settings")]
        public bool cloudSave = false;
        public bool leaderboards = false;
        public bool achievements = false;

        [Header("Store settings")]
        [Tooltip("{0} will be the name of the logged in user")]
        public string greeting = "Welcome {0}";
        public string cloudFileName = "cloud_save1";
        public string storeName = "Store name";
    }
}