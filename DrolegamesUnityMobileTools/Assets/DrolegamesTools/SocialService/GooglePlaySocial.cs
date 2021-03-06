#if UNITY_ANDROID

namespace Drolegames.SocialService
{
    using System;
    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class GooglePlaySocial : ISocialService
    {
        public RuntimePlatform Platform => RuntimePlatform.Android;

        public bool UserCanSign => true;

        public bool IsLoggedIn => PlayGamesActive.localUser.authenticated;

        public string Name => PlayGamesActive.localUser.userName;
        private readonly string greeting;
        public string Greeting => string.Format(greeting, Name);
        public string StoreName { get; private set; }

        public byte[] CloudData { get; private set; }

        private readonly string cloudFileName;
        private PlayGamesPlatform PlayGamesActive => (PlayGamesPlatform)Social.Active;

        public bool CloudSaveEnabled { get; private set; }

        public bool AchievementsEnabled { get; private set; }
        public bool LeaderboardsEnabled { get; private set; }

        public string LocalUserId => PlayGamesActive.localUser.id;

        private readonly bool debugMode = false;
        public GooglePlaySocial(SocialAndroidSettingsSO settings)
        {
            LeaderboardsEnabled = settings.leaderboards;
            AchievementsEnabled = settings.achievements;
            CloudSaveEnabled = settings.cloudSave;
            greeting = settings.greeting;
            cloudFileName = settings.cloudFileName;
            StoreName = settings.storeName;
            debugMode = settings.debugLog;
        }

        public void Initialize()
        {
            var config = new PlayGamesClientConfiguration
                .Builder();
            if (CloudSaveEnabled)
            {
                config = config.EnableSavedGames();
            }

            PlayGamesPlatform.InitializeInstance(config.Build());
            PlayGamesPlatform.DebugLogEnabled = debugMode;
            PlayGamesPlatform.Activate();
        }
        public void LoadFromCloud(Action<bool> callback)
        {
            if (loadingFromCloud || !IsLoggedIn)
            {
                Debug.LogWarning("GooglePlaySocial loading or is not LoggedIn");
                LoadComplete(false);
                return;
            }
            loadingFromCloud = true;
            loadCallback = callback;

            PlayGamesActive.SavedGame.OpenWithAutomaticConflictResolution(cloudFileName,
                            DataSource.ReadCacheOrNetwork,
                            ConflictResolutionStrategy.UseLongestPlaytime,
                            SavedGameOpened);
        }


        public void Login(Action<bool> callback)
        {
            if (IsLoggedIn)
            {
                callback?.Invoke(false);
                return;
            }
            PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, success =>
            {
                callback?.Invoke(success == SignInStatus.Success);
                IsLoggedInChanged?.Invoke(IsLoggedIn);
            });
        }
        public event Action<bool> IsLoggedInChanged;
        public void Logout(Action<bool> callback)
        {
            PlayGamesPlatform.Instance.SignOut();
            callback?.Invoke(true);
            IsLoggedInChanged?.Invoke(IsLoggedIn);
        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            if (savingToCloud)
            {
                SaveComplete(false);
            };
            saveCallback = callback;
            savingToCloud = true;

            PlayGamesActive.SavedGame.OpenWithAutomaticConflictResolution(cloudFileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime,
              (SavedGameRequestStatus status, ISavedGameMetadata game) =>
              {
                  if (status != SavedGameRequestStatus.Success)
                  {
                      Debug.LogWarning("GooglePlaySocial OpenWithAutomaticConflictResolution Failed");
                      SaveComplete(false);
                      return;
                  }
                  SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
                       .WithUpdatedPlayedTime(playedTime)
                       .WithUpdatedDescription("Saved Game at " + DateTime.Now);

                  SavedGameMetadataUpdate updatedMetadata = builder.Build();

                  PlayGamesActive.SavedGame
                    .CommitUpdate(game, updatedMetadata, data,
                         (SavedGameRequestStatus committedStatus, ISavedGameMetadata committedGame) =>
                         {
                             Debug.LogWarning("GooglePlaySocial SavedGameRequestStatus " + committedStatus.ToString() + " " + committedGame.Description + " commitedGane: " + committedGame.Filename);
                             SaveComplete(committedStatus == SavedGameRequestStatus.Success);
                         }
                    );
              }
              );


        }

        Action<bool> saveCallback;
        bool savingToCloud = false;
        private void SaveComplete(bool success)
        {
            savingToCloud = false;
            if (saveCallback != null)
            {
                saveCallback.Invoke(success);
            }
            saveCallback = null;
        }

        Action<bool> loadCallback;
        bool loadingFromCloud = false;



        private void LoadComplete(bool success)
        {
            loadingFromCloud = false;
            if (loadCallback != null)
            {
                loadCallback.Invoke(success);
            }
            loadCallback = null;
        }


        private void SavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                PlayGamesActive.SavedGame.ReadBinaryData(game, SavedGameLoaded);
            }
            else
            {
                LoadComplete(false);
            }
        }

        private void SavedGameLoaded(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                CloudData = data;
                LoadComplete(true);
            }
            else
            {
                LoadComplete(false);
            }
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            PlayGamesActive.UnlockAchievement(achievementId, callback);
        }

        public void IncrementAchievement(string achievementId, double steps, double stepsRatio, Action<bool> callback)
        {
            PlayGamesActive.IncrementAchievement(achievementId, (int)steps, callback);
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            PlayGamesActive.LoadAchievements(callback);
        }

        public void ShowAchievementsUI()
        {
            PlayGamesActive.ShowAchievementsUI();
        }

        public void ShowLeaderboardUI()
        {
            PlayGamesActive.ShowLeaderboardUI();
        }

        public void ShowLeaderboardUI(string leaderboardId)
        {
            PlayGamesActive.ShowLeaderboardUI(leaderboardId, (UIStatus status) =>
            {
                if (status < 0)
                {
                    Debug.LogError("ShowLeaderboardUI " + status);
                }
            });
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, Action<bool> callback)
        {
            PlayGamesActive.ReportScore(score, leaderboardId, callback);
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, string tag, Action<bool> callback)
        {
            PlayGamesActive.ReportScore(score, leaderboardId, tag, callback);
        }

        public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
        {
            leaderboard.SetUserFilter(new string[] { PlayGamesActive.localUser.id });
            leaderboard.LoadScores(callback);
        }
    }
}
#endif