namespace Drolegames.SocialService
{
    using System;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class NoSocial : ISocialService
    {
        public byte[] CloudData => new byte[0];

        public bool UserCanSign => false;

        public bool CloudSaveEnabled => false;

        public string Name => string.Empty;

        public string StoreName => string.Empty;

        public string Greeting => string.Empty;

        public bool AchievementsEnabled => false;

        public bool LeaderboardsEnabled => false;

        public bool IsLoggedIn => false;

        public RuntimePlatform Platform => RuntimePlatform.WindowsEditor;

        public string LocalUserId => string.Empty;

        public event Action<bool> IsLoggedInChanged;

        public void IncrementAchievement(string achievementId, double steps, double maxSteps, Action<bool> callback)
        {
        }

        public void Initialize()
        {
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
        }

        public void LoadFromCloud(Action<bool> callback)
        {

        }

        public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
        {

        }

        public void Login(Action<bool> callback)
        {

        }

        public void Logout(Action<bool> callback)
        {

        }

        public void ReportLeaderboardScore(long score, string leaderboardId, Action<bool> callback)
        {

        }

        public void ReportLeaderboardScore(long score, string leaderboardId, string tag, Action<bool> callback)
        {

        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {

        }

        public void ShowAchievementsUI()
        {

        }

        public void ShowLeaderboardUI()
        {

        }

        public void ShowLeaderboardUI(string leaderboardId)
        {

        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {

        }
    }
}