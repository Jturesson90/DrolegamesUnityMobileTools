namespace Drolegames.SocialService
{
    using System;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;
    public interface ISocialService : ISocialAchievements, ISocialLeaderboards, ISession
    {
        void Initialize();
        void Login(Action<bool> callback);
        void Logout(Action<bool> callback);
        void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback);
        void LoadFromCloud(Action<bool> callback);
        byte[] CloudData { get; }
        bool UserCanSign { get; }
        bool CloudSaveEnabled { get; }
        string Name { get; }
        string StoreName { get; }
        string Greeting { get; }
     
    }
    public interface ISocialAchievements
    {
        bool AchievementsEnabled { get; }
        void LoadAchievements(Action<IAchievement[]> callback);
        void UnlockAchievement(string achievementId, Action<bool> callback);
        void IncrementAchievement(string achievementId, double steps, Action<bool> callback);
        void ShowAchievementsUI();
    }
    public interface ISession
    {
        bool IsLoggedIn { get; }
        RuntimePlatform Platform { get; }
        event Action<bool> IsLoggedInChanged;
    }
    public interface ISocialLeaderboards
    {
        bool LeaderboardsEnabled { get; }
    }
}
