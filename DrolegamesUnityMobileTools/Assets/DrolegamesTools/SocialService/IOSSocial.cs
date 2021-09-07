#if UNITY_IOS
namespace Drolegames.SocialService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    using UnityEngine.SocialPlatforms.GameCenter;


    public class IOSSocial : ISocialService
    {
        public RuntimePlatform Platform => RuntimePlatform.IPhonePlayer;

        public bool UserCanSign => false;

        public bool IsLoggedIn => Social.Active.localUser.authenticated;

        public string Name => Social.Active.localUser.userName;

        public string StoreName { get; private set; }
        private readonly string greeting;
        public string Greeting => string.Format(greeting, Name);

        public byte[] CloudData { get; private set; }

        public bool CloudSaveEnabled { get; private set; }

        public bool AchievementsEnabled { get; private set; }

        public bool LeaderboardsEnabled { get; private set; }

        public string LocalUserId => Social.Active.localUser.id;

        public IOSSocial(SocialIOSSettingsSO settings)
        {
            LeaderboardsEnabled = settings.leaderboards;
            AchievementsEnabled = settings.achievements;
            CloudSaveEnabled = settings.cloudSave;
            greeting = settings.greeting;
            StoreName = settings.storeName;
        }

        public void Initialize()
        {
#if UNITY_IOS || UNITY_EDITOR
            GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#endif
        }
        Dictionary<string, IAchievement> achievementById;

        public event Action<bool> IsLoggedInChanged;

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            Social.Active.LoadAchievements(a =>
            {
                achievementById = a.ToDictionary(b => b.id);
                callback(a);
            });
        }

        public void LoadFromCloud(Action<bool> callback)
        {
            callback(false);
        }

        public void Login(Action<bool> callback)
        {
            if (IsLoggedIn)
            {
                callback?.Invoke(false);
                return;
            }
            Social.Active.localUser.Authenticate((bool success) =>
            {
                callback?.Invoke(success);
                IsLoggedInChanged?.Invoke(IsLoggedIn);
            });
        }

        public void Logout(Action<bool> callback)
        {
            /* Can't logout*/
            callback(false);
        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            // Not implemented
            callback(false);
        }

        public void ShowAchievementsUI()
        {
            Social.Active.ShowAchievementsUI();
        }

        public void IncrementAchievement(string achievementId, double steps, double stepsToComplete, Action<bool> callback)
        {
            double stepsAspect = 100.0 / stepsToComplete;
            double percentCompleted = steps * stepsAspect;
            if (achievementById.ContainsKey(achievementId))
            {
                var achievement = achievementById[achievementId];
                achievement.percentCompleted += percentCompleted;
                percentCompleted = achievement.percentCompleted;
            }
            Social.ReportProgress(achievementId, percentCompleted, callback);
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            Social.Active.ReportProgress(achievementId, 100d, callback);
        }

        public void ShowLeaderboardUI()
        {
            Social.ShowLeaderboardUI();
        }

        public void ShowLeaderboardUI(string leaderboardId)
        {
            ShowLeaderboardUI();
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, Action<bool> callback)
        {
            // Appstore wants a hundreth of a second, while we calculate in thousandth. 
            score /= 10;
            Social.ReportScore(score, leaderboardId, callback);
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, string tag, Action<bool> callback)
        {
            ReportLeaderboardScore(score, leaderboardId, callback);
        }

        public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
        {
            leaderboard.SetUserFilter(new string[] { Social.localUser.id });
            leaderboard.LoadScores(callback);
        }
    }
}
#endif