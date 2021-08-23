namespace Drolegames.SocialService
{
    using System;
    using UnityEngine;
    using Drolegames.Utils;
    using Drolegames.IO;

    public class SocialManager : Singleton<SocialManager>
    {
        [Header("Platform settings")]
        [SerializeField] private SocialAndroidSettingsSO m_androidSettings = default;
        [SerializeField] private SocialIOSSettingsSO m_iosSettings = default;
        [SerializeField] private SocialMockSettingsSO m_mockSettings = default;

        private ISocialService socialService;
        public event EventHandler<SocialManagerArgs> LoggedInChanged;
        public event EventHandler<bool> LoggingInPendingChanged;
        public event EventHandler<bool> OnUploadChanged;
        public event EventHandler<OnUploadCompleteArgs> OnUploadComplete;

        private bool _loggingInPending = false;
        public bool LoggingInPending
        {
            get => _loggingInPending;
            private set
            {
                if (_loggingInPending != value)
                {
                    _loggingInPending = value;
                    LoggingInPendingChanged?.Invoke(this, _loggingInPending);
                }
            }
        }
        private bool _uploadPending = false;
        public bool UploadPending
        {
            get => _uploadPending;
            private set
            {
                if (_uploadPending != value)
                {
                    _uploadPending = value;
                    OnUploadChanged?.Invoke(this, _uploadPending);
                }
            }
        }
        public string Name => socialService.Name;
        public string Greeting => socialService.Greeting;
        public bool IsLoggedIn => socialService.IsLoggedIn;
        public bool UserCanSign => socialService.UserCanSign;
        public string StoreName => socialService.StoreName;
        public bool CloudSaveEnabled => socialService.CloudSaveEnabled;
        public RuntimePlatform Platform => socialService.Platform;

        public IAchievements Achievements { get; private set; }
        private Achievements achievements;

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            socialService = new MockSocial(m_mockSettings);
#elif UNITY_ANDROID
            socialService = new GooglePlaySocial(m_androidSettings);
#elif UNITY_IOS
            socialService = new IOSSocial(m_iosSettings);
#endif
            socialService.Initialize();

            achievements = new Achievements(socialService, socialService);
            if (socialService.AchievementsEnabled)
            {
                achievements.Initialize();
            }
            Achievements = achievements;
        }

        private void Start()
        {
            Login();
        }
        protected override void OnDestroy()
        {
            if (socialService.AchievementsEnabled)
            {
                achievements.Save();
            }
            base.OnDestroy();
        }
        public void Login()
        {
            LoggingInPending = true;
            socialService.Login((bool success) =>
            {
                if (success && CloudSaveEnabled)
                    LoadFromCloud();

                LoggingInPending = false;
                LoggedInChanged?.Invoke(this, new SocialManagerArgs()
                {
                    IsLoggedIn = socialService.IsLoggedIn,
                    Platform = socialService.Platform,
                    Name = socialService.Name
                });
            }
            );
        }
        public void SaveGame(bool manual = false)
        {
            if (GameDataManager.IsInitialized && CloudSaveEnabled)
            {
                UploadPending = true;
                TimeSpan timePlayed;
                try
                {
                    timePlayed = TimeSpan.FromSeconds(GameDataManager.Current.GameData.totalPlayingTime);
                }
                catch
                {
                    timePlayed = TimeSpan.FromSeconds(3600);
                }

                socialService.SaveGame(GameDataManager.Current.GameData.ToBytes(), timePlayed, (bool success) =>
                {
                    UploadPending = false;
                    if (success)
                    {
                        OnUploadComplete?.Invoke(this, new OnUploadCompleteArgs() { Manual = manual });
                    }
                    else
                    {
                        Debug.LogWarning("SocialManager SaveGame FAIL");
                    }
                });
            }
        }
        public void LoadFromCloud()
        {
            if (!CloudSaveEnabled) return;
            Debug.LogWarning("SocialManager LoadFromCloud");
            socialService.LoadFromCloud((bool success) =>
            {
                Debug.LogWarning("SocialManager LoadFromCloud success? " + success);
                if (success)
                {
                    if (socialService != null)
                    { }
                    else
                    {
                        Debug.LogError("SocialManager LoadFromCloud socialService is null wtf?");
                    }
                }
            });
        }
        public void LogOut()
        {
            LoggingInPending = true;
            socialService.Logout((bool success) =>
            {
                LoggingInPending = false;
                LoggedInChanged?.Invoke(this, new SocialManagerArgs()
                {
                    IsLoggedIn = false,
                    Platform = socialService.Platform,
                    Name = socialService.Name
                });
            });
        }
    }

    public class SocialManagerArgs : EventArgs
    {
        public bool IsLoggedIn { get; set; }
        public RuntimePlatform Platform { get; set; }
        public string Name { get; internal set; }
    }

    public class OnUploadCompleteArgs : EventArgs
    {
        public bool Manual { get; set; }
    }
}