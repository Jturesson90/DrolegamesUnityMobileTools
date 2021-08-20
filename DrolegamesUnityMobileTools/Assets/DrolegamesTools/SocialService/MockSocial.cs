namespace Drolegames.SocialService
{
    using Drolegames.IO;
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class MockSocial : ISocialService
    {
        public RuntimePlatform Platform => RuntimePlatform.WindowsEditor;

        public bool UserCanSign => true;

        public bool IsLoggedIn { get; private set; }

        public string Name => IsLoggedIn ? userName : string.Empty;
        private readonly string greeting;
        public string Greeting => string.Format(greeting, Name);
        public string StoreName { get; private set; }

        public byte[] CloudData { get; private set; }

        public bool CloudSaveEnabled { get; private set; }

        private readonly string cloudFileName;
        private readonly string userName;
        
        private readonly float loginDelay;
        public MockSocial(SocialMockSettingsSO settings)
        {
            CloudSaveEnabled = settings.cloudSaveEnabled;
            cloudFileName = settings.cloudFileName;
            userName = settings.userName;
            greeting = settings.greeting;
            StoreName = settings.storeName;
            loginDelay = settings.loginDelay;
        }

        public void Initialize()
        {
            IsLoggedIn = false;
        }

        public void Login(Action<bool> callback)
        {
            UseDelay(loginDelay, () => callback?.Invoke(IsLoggedIn = true));
        }

        public void Logout(Action<bool> callback)
        {
            IsLoggedIn = false;
            callback?.Invoke(true);
        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            bool success = false;
            if (!CloudSaveEnabled)
            {
                callback?.Invoke(success);
                return;
            }
            CloudData = data;
            try
            {
                FileManager.WriteToFile($"{cloudFileName}.txt", System.Text.ASCIIEncoding.Default.GetString(data));
                success = true;
            }
            catch
            {
                success = false;
            }
            UseDelay(1.5f, () => callback?.Invoke(success));
        }

        public void LoadFromCloud(Action<bool> callback)
        {
            bool success = false;
            if (!CloudSaveEnabled)
            {
                callback?.Invoke(success);
                return;
            }
            try
            {
                FileManager.LoadFromFile($"{cloudFileName}.txt", out string json);
                CloudData = System.Text.ASCIIEncoding.Default.GetBytes(json);
                success = true;
            }
            catch
            {
                success = false;
            }
            UseDelay(loginDelay, () => callback?.Invoke(success));
        }

        void UseDelay(float time, Action callback)
        {
            Task.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromSeconds(time));
                callback?.Invoke();
            });
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            callback(true);
        }

        public void IncrementAchievement(string achievementId, double steps, Action<bool> callback)
        {
            callback(true);
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            callback(new IAchievement[0]);
        }

        public void ShowAchievementsUI()
        {
            Debug.Log("MockSocial ShowAchievementsUI");
        }
    }
}
