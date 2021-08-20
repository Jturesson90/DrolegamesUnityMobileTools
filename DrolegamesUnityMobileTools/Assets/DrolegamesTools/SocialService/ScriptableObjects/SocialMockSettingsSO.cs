namespace Drolegames.SocialService
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "", menuName = "Drolegames/SocialSettings/Mock")]
    public class SocialMockSettingsSO : SocialSettingsSO
    {
        [Header("Mock settings")]
        public string userName = "Mock";
        [Min(0)]
        public float loginDelay = 1.5f;
    }
}