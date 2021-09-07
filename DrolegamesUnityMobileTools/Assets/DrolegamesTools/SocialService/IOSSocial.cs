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

        public bool IsLoggedIn => Social.localUser.authenticated;

        public string Name => Social.localUser.userName;

        public string StoreName { get; private set; }
        private readonly string greeting;
        public string Greeting => string.Format(greeting, Name);

        public byte[] CloudData { get; private set; }

        public bool CloudSaveEnabled { get; private set; }

        public bool AchievementsEnabled { get; private set; }

        public bool LeaderboardsEnabled { get; private set; }

        public string LocalUserId => Social.localUser.id;

        public IOSSocial(SocialIOSSettingsSO settings)
        {
            LeaderboardsEnabled = settings.leaderboards;
            AchievementsEnabled = settings.achievements;
            CloudSaveEnabled = settings.cloudSave;
            greeting = settings.greeting;
            StoreName = settings.storeName;
        }

        Dictionary<string, IAchievement> achievementById;
        public void Initialize()
        {
            GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
            achievementById = new Dictionary<string, IAchievement>();
        }


        public event Action<bool> IsLoggedInChanged;

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            // Loads all known achievements. Not everyone
            Social.LoadAchievements(a =>
            {
                achievementById = a.ToDictionary(b => b.id);
                callback?.Invoke(a);
            });
        }

        public void LoadFromCloud(Action<bool> callback)
        {
            callback?.Invoke(false);
        }

        public void Login(Action<bool> callback)
        {
            if (IsLoggedIn)
            {
                callback?.Invoke(false);
                return;
            }
            Social.localUser.Authenticate((bool success) =>
            {
                callback?.Invoke(success);
                IsLoggedInChanged?.Invoke(IsLoggedIn);
            });
        }

        public void Logout(Action<bool> callback)
        {
            /* Can't logout*/
            callback?.Invoke(false);
        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            // Not implemented
            callback?.Invoke(false);
        }

        public void ShowAchievementsUI()
        {
            Social.ShowAchievementsUI();
        }

        public void IncrementAchievement(string achievementId, double steps, double stepsToComplete, Action<bool> callback)
        {
            double stepsAspect = 100.0 / stepsToComplete;
            double percentCompleted = steps * stepsAspect;

            IAchievement achievement;
            if (achievementById.ContainsKey(achievementId))
            {
                achievement = achievementById[achievementId];
            }
            else
            {
                achievement = Social.CreateAchievement();
                achievement.id = achievementId;
                achievementById.Add(achievement.id, achievement);
            }
            achievement.percentCompleted += percentCompleted;
            achievement.ReportProgress(callback);
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            Social.ReportProgress(achievementId, 100d, callback);
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