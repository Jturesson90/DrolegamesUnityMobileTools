namespace Drolegames.SocialService
{
    using UnityEngine;

    public class SocialSettingsSO : ScriptableObject
    {
        public bool cloudSaveEnabled = false;
        [Tooltip("{0} will be the name of the logged in user")]
        public string greeting = "Welcome {0}";
        public string cloudFileName = "cloud_save1";
        public string storeName = "Store name";
    }
}