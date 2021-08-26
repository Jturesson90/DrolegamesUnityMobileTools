namespace Drolegames.SocialService
{
    using Drolegames.Utils;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class Leaderboards : ILeaderboards
    {
        private ISocialLeaderboards social;
        private ISession session;
        public Leaderboards(ISocialLeaderboards social, ISession session)
        {
            this.social = social;
            this.session = session;
        }
        public void Initialize()
        {
            // TODO, if we dont want to use the default UI, we can Loadscores here
        }
        public long GetLeaderboardUserScore(string leaderboardId) => SavedLeaderboards.GetLeaderboard(leaderboardId, session, social).GetUserScore();
        public void ShowUI() => social.ShowLeaderboardUI();
        public void ShowUI(string leaderboardId) => social.ShowLeaderboardUI(leaderboardId);
        public void ReportScore(long score, string leaderboardId, Action<bool> callback) => ReportScore(score, leaderboardId, string.Empty, callback);
        public void ReportScore(long score, string leaderboardId, string tag, Action<bool> callback)
        {
            var savedLeaderboard = SavedLeaderboards.GetLeaderboard(leaderboardId, session, social);

            if (savedLeaderboard.Leaderboard.localUserScore != null && savedLeaderboard.Leaderboard.localUserScore.value > score) return;
            long sc = Math.Max(score, savedLeaderboard.GetUserScore());
            if (session.IsLoggedIn)
            {
                social.ReportLeaderboardScore(sc, leaderboardId, tag, (bool success) =>
                {
                    callback?.Invoke(success);
                });
            }
            savedLeaderboard.ApplyNewScore(score);
        }

        private sealed class SavedLeaderboards
        {
            private long score;
            readonly string SAVE_PATH;
            public ILeaderboard Leaderboard { get; private set; }
            private static Dictionary<string, SavedLeaderboards> savedLeaderboardsById = new Dictionary<string, SavedLeaderboards>();
            public static event Action<SavedLeaderboards> onLoadScoresComplete;

            private SavedLeaderboards(string id)
            {
                Leaderboard = Social.CreateLeaderboard();
                Leaderboard.id = id;
                SAVE_PATH = $"{Application.persistentDataPath}/ld_{id}.dat";
            }
            public static SavedLeaderboards GetLeaderboard(string id, ISession session, ISocialLeaderboards social)
            {
                if (savedLeaderboardsById.ContainsKey(id))
                {
                    return savedLeaderboardsById[id];
                }
                var savedLeaderboard = new SavedLeaderboards(id);
                if (session.IsLoggedIn)
                {
                    social.LoadUserLeaderboardScore(savedLeaderboard.Leaderboard, (bool success) =>
                    {
                        if (success)
                        {
                            savedLeaderboard.ApplyNewScore(savedLeaderboard.Leaderboard.localUserScore.value);
                            onLoadScoresComplete?.Invoke(savedLeaderboard);
                        }
                    });
                }
                savedLeaderboard.Load();
                savedLeaderboardsById.Add(id, savedLeaderboard);

                return savedLeaderboard;
            }

            public long GetUserScore()
            {
                return score;
            }
            public void ApplyNewScore(long score)
            {
                if (score > this.score)
                {
                    this.score = score;
                    Save();
                }
            }

            public void Save()
            {
                if (!(EasySerializer.DeserializeObjectFromFile(SAVE_PATH) is HighscoreSaveData savedScore))
                {
                    savedScore = new HighscoreSaveData { score = score };
                }
                savedScore.score = score > savedScore.score ? score : savedScore.score;
                EasySerializer.SerializeObjectToFile(savedScore, SAVE_PATH);
            }

            public void Load()
            {
                if (!(EasySerializer.DeserializeObjectFromFile(SAVE_PATH) is HighscoreSaveData savedScore))
                {
                    savedScore = new HighscoreSaveData();
                }
                ApplyNewScore(savedScore.score);
            }

            [Serializable]
            private class HighscoreSaveData
            {
                public long score;
            }
        }

    }
}