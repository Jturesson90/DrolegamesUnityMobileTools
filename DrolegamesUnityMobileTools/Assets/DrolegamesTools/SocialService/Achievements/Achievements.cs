//#define DEBUG_ACHIEVEMENTS
namespace Drolegames.SocialService
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class Achievements : IAchievements
    {
        private PendingAchievements _pendingAchievements;
        private Dictionary<string, IAchievement> unlockedAchievements;

        private readonly ISocialAchievements social;
        private readonly ISession session;
        public Achievements(ISocialAchievements social, ISession session)
        {
            this.social = social;
            this.session = session;
            _pendingAchievements = new PendingAchievements();
            unlockedAchievements = new Dictionary<string, IAchievement>();
        }

        private void Session_IsLoggedInChanged(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                LoadAchievements();
            }
        }

        public void Initialize()
        {
            LoadFromDisk();
            session.IsLoggedInChanged += Session_IsLoggedInChanged;
            if (session.IsLoggedIn)
            {
                LoadAchievements();
            }
        }
        public void Save()
        {
            SaveToDisk();
        }
        ~Achievements()
        {
            SaveToDisk();
        }

        private void FlushAchievements()
        {
            if (!session.IsLoggedIn) return;

            foreach (var pending in _pendingAchievements.pending)
            {
                if (pending.hasIncrement)
                {
                    Increment(pending.id, pending.steps, pending.stepsToComplete);
                }
                else
                {
                    Unlock(pending.id);
                }
            }
        }
        public void Unlock(string id, Action<bool> callback = null)
        {
            if (!social.AchievementsEnabled) return;
            var s = new DroleAchievement(id);
            if (!session.IsLoggedIn)
            {
                AddPendingAchievement(s);
                return;
            }

            social.UnlockAchievement(id, (bool success) =>
            {
                if (success)
                    RemovePendingAchievement(s);
                else
                    AddPendingAchievement(s);

                callback?.Invoke(success);
            });
        }
        public void Increment(string id, double steps, double stepsRatio, Action<bool> callback = null)
        {
            if (!social.AchievementsEnabled) return;
            var s = new DroleAchievement(id, steps, stepsRatio);
            if (!session.IsLoggedIn)
            {
                AddPendingAchievement(s);
                return;
            }
            social.IncrementAchievement(id, s.steps, s.stepsToComplete, (bool success) =>
            {
                if (success)
                    RemovePendingAchievement(s);
                else
                    AddPendingAchievement(s);

                callback?.Invoke(success);
            });

        }

        private void RemovePendingAchievement(DroleAchievement pendingAchievement)
        {
            if (_pendingAchievements != null)
            {
                _pendingAchievements.RemoveAchievement(pendingAchievement);
            }
        }

        private void AddPendingAchievement(DroleAchievement pendingAchievement)
        {
            if (_pendingAchievements != null)
            {
                _pendingAchievements.AddAchievement(pendingAchievement);
            }
        }

        public void ShowUI()
        {
            if (!social.AchievementsEnabled) return;
            social.ShowAchievementsUI();
        }

        private void LoadAchievements()
        {
            unlockedAchievements.Clear();
            social.LoadAchievements(achievements =>
            {
                unlockedAchievements = new Dictionary<string, IAchievement>();
                foreach (var achievement in achievements)
                {
                    if (achievement.completed)
                    {
                        unlockedAchievements.Add(achievement.id, achievement);
                    }
                }
                if (_pendingAchievements != null)
                    _pendingAchievements.RemoveAllWithId(unlockedAchievements.Keys.ToArray());

                FlushAchievements();
            });
            SaveToDisk();
        }

        const string saveKey = "pend";
        private void SaveToDisk()
        {
            var json = _pendingAchievements.ToString();
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }
        private void LoadFromDisk()
        {
            var s = PlayerPrefs.GetString(saveKey, string.Empty);
            if (s == null || s.Trim().Length == 0)
            {
                _pendingAchievements = new PendingAchievements();
            }
            else
            {
                _pendingAchievements = PendingAchievements.FromString(s);
            }
        }
    }
    [Serializable]
    public class PendingAchievements
    {
        public List<DroleAchievement> pending;
        public static PendingAchievements FromString(string s) => JsonUtility.FromJson<PendingAchievements>(s);
        public override string ToString() => JsonUtility.ToJson(this, false);
        public PendingAchievements()
        {
            pending = new List<DroleAchievement>();
        }

        public void RemoveAllWithId(string[] ids)
        {
            pending.RemoveAll((item) => ids.Contains(item.id));
        }

        internal void RemoveAchievement(DroleAchievement pendingAchievement)
        {
            if (!pending.Contains(pendingAchievement))
            {
                pending.Remove(pendingAchievement);
            }
        }

        internal void AddAchievement(DroleAchievement pendingAchievement)
        {
            if (!pending.Contains(pendingAchievement))
            {
                pending.Add(pendingAchievement);
            }
        }
    }
}
