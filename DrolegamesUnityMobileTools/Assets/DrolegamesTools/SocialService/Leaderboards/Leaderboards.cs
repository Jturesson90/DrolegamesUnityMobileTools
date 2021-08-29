namespace Drolegames.SocialService
{
    using Drolegames.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class Leaderboards : ILeaderboards
    {
        private readonly ISocialLeaderboards social;
        private readonly ISession session;
        public Leaderboards(ISocialLeaderboards social, ISession session)
        {
            this.social = social;
            this.session = session;
        }
        public void Initialize()
        {
            SavedLeaderboards.Load();
            if (session.IsLoggedIn)
            {
                SavedLeaderboards.Flush(session, social);
            }
            else
            {
                session.IsLoggedInChanged += (bool success) =>
                {
                    if (success)
                    {
                        SavedLeaderboards.Flush(session, social);
                    }
                };
            }
            // TODO, if we dont want to use the default UI, we can Loadscores here
        }


        public long GetLocalUserAllTimeHighscore(string leaderboardId, bool isMoreBetter) => SavedLeaderboards.GetLeaderboard(leaderboardId, session, social, isMoreBetter).highestScore;
        public void ShowUI() => social.ShowLeaderboardUI();
        public void ShowUI(string leaderboardId) => social.ShowLeaderboardUI(leaderboardId);
        public void ReportScore(long score, string leaderboardId, Action<bool> callback, bool isMoreBetter) => ReportScore(score, leaderboardId, string.Empty, callback, isMoreBetter);
        public void ReportScore(long score, string leaderboardId, string tag, Action<bool> callback, bool isMoreBetter)
        {
            var savedLeaderboard = SavedLeaderboards.GetLeaderboard(leaderboardId, session, social, isMoreBetter);
            if (session.IsLoggedIn)
            {
                social.ReportLeaderboardScore(score, leaderboardId, tag, (bool success) =>
                 {
                     callback?.Invoke(success);
                     if (!success)
                     {
                         savedLeaderboard.AddUnpublishedScore(score, DateTime.Now);
                     }
                 });
            }
            else
            {
                savedLeaderboard.AddUnpublishedScore(score, DateTime.Now);
            }

            savedLeaderboard.ApplyNewHighscore(score);
            SavedLeaderboards.Save();
        }

        private sealed class SavedLeaderboards
        {
            public static event Action<LeaderboardSaveData> OnLoadScoresComplete;

            static LeaderboardsdSave save;

            static readonly string SAVE_PATH = $"{Application.persistentDataPath}/leaderboards.dat";

            public SavedLeaderboards()
            {
                save = new LeaderboardsdSave();
            }
            public static LeaderboardSaveData GetLeaderboard(string id, ISession session, ISocialLeaderboards social, bool isMoreBetter)
            {
                if (save == null) return null;

                LeaderboardSaveData result = null;

                int saveDateLen = save.data.Length;

                for (int i = 0; i < saveDateLen; i++)
                {
                    if (save.data[i].id.Equals(id))
                        result = save.data[i];
                }
                if (result == null)
                {
                    result = new LeaderboardSaveData(id, isMoreBetter);
                    save.Add(result);
                    Save();
                }
                if (result.isMoreBetter != isMoreBetter)
                {
                    result.isMoreBetter = isMoreBetter;
                    Save();
                }


                if (session.IsLoggedIn && !result.LoadedScores)
                {
                    social.LoadUserLeaderboardScore(result.GetLeaderBoard(), (bool success) =>
                    {
                        if (success)
                        {
                            result.LoadedScores = true;
                            result.ApplyNewHighscore(result.GetLeaderBoard().localUserScore.value);
                            OnLoadScoresComplete?.Invoke(result);
                        }
                    });
                }

                return result;
            }

            public static void Flush(ISession session, ISocialLeaderboards social)
            {
                if (!session.IsLoggedIn) return;

                int saveDateLen = save.data.Length;
                for (int i = 0; i < saveDateLen; i++)
                {
                    LeaderboardSaveData lbsd = save.data[i];
                    lbsd.unpublishedScores = lbsd.unpublishedScores.Where(a => DateTime.FromFileTime(a.date) > DateTime.Now.AddDays(-7)).ToArray();
                    int unpublishedLen = lbsd.unpublishedScores.Length;
                    for (int j = 0; j < unpublishedLen; j++)
                    {
                        UnpublishedScores us = lbsd.unpublishedScores[j];
                        social.ReportLeaderboardScore(us.score, lbsd.id, (bool success) =>
                        {

                        });
                    }
                    lbsd.ClearUnpublishedScores();
                }
                Save();
            }

            public static void Save()
            {
                if (SAVE_PATH.Equals(string.Empty))
                {
                    Debug.Log("COULD NOT SAVE, NEEDS LOAD FIRST");
                    return;
                }
                try
                {
                    EasySerializer.SerializeObjectToFile(save, SAVE_PATH);
                }
                catch
                {
                    EasySerializer.RemoveFile(SAVE_PATH);
                }
            }

            public static void Load()
            {
                try
                {
                    if (!(EasySerializer.DeserializeObjectFromFile(SAVE_PATH) is LeaderboardsdSave savedScore))
                    {
                        savedScore = new LeaderboardsdSave();
                    }
                    save = savedScore;
                }
                catch
                {
                    save = new LeaderboardsdSave();
                    EasySerializer.RemoveFile(SAVE_PATH);
                }
            }

            [Serializable]
            public class LeaderboardsdSave
            {
                public LeaderboardSaveData[] data;
                public LeaderboardsdSave()
                {
                    data = new LeaderboardSaveData[0];
                }

                internal void Add(LeaderboardSaveData result)
                {
                    data = data.Append(result).ToArray();
                }
            }
        }
        [Serializable]
        public class LeaderboardSaveData
        {
            public bool LoadedScores { get; set; }
            public readonly string id;
            public bool isMoreBetter;
            public string tag;
            public long highestScore;
            [NonSerialized] private ILeaderboard leaderboard;
            public UnpublishedScores[] unpublishedScores = new UnpublishedScores[0];
            public LeaderboardSaveData(string id, bool isMoreBetter)
            {
                this.isMoreBetter = isMoreBetter;
                this.id = id;
                leaderboard = Social.CreateLeaderboard();
                leaderboard.id = id;
                unpublishedScores = new UnpublishedScores[0];
            }
            public ILeaderboard GetLeaderBoard()
            {
                if (leaderboard == null)
                {
                    leaderboard = Social.CreateLeaderboard();
                    leaderboard.id = id;
                }

                return leaderboard;
            }
            public void ClearUnpublishedScores()
            {
                unpublishedScores = new UnpublishedScores[0];
            }
            public void ApplyNewHighscore(long score)
            {
                if (isMoreBetter)
                {
                    if (score > highestScore)
                    {
                        highestScore = score;
                    }
                }
                else
                {
                    if (score < highestScore)
                    {
                        highestScore = score;
                    }
                }
            }

            public void AddUnpublishedScore(long score, DateTime time)
            {
                var u = new List<UnpublishedScores>(unpublishedScores)
                {
                    new UnpublishedScores(score, time.ToFileTime())
                };
                unpublishedScores = u.ToArray();
            }
        }

        [Serializable]
        public class UnpublishedScores
        {
            public long date;
            public long score;
            public UnpublishedScores(long score, long date)
            {
                this.score = score;
                this.date = date;
            }
        }

    }
}