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
        private Dictionary<string, IAchievement> unlockedAchievements = new Dictionary<string, IAchievement>();

        private readonly ISocialAchievements social;
        private readonly ISession session;
        public Achievements(ISocialAchievements social, ISession session)
        {
            this.social = social;
            this.session = session;
            _pendingAchievements = new PendingAchievements();
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
            int index = _pendingAchievements.pending.FindIndex(c => c.id.Equals(pendingAchievement.id) && c.hasIncrement == pendingAchievement.hasIncrement && c.steps.Equals(pendingAchievement.steps));
            if (index >= 0)
                _pendingAchievements.pending.RemoveAt(index);
        }
        private void AddPendingAchievement(DroleAchievement pendingAchievement)
        {
            if (!_pendingAchievements.pending.Any(c => c.id.Equals(pendingAchievement.id) && c.hasIncrement == pendingAchievement.hasIncrement && c.steps.Equals(pendingAchievement.steps)))
            {
                _pendingAchievements.pending.Add(pendingAchievement);
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
                unlockedAchievements = achievements
                .Where(a => a.completed)
                .ToDictionary(a => a.id);

                // Remove all old pending achievements that the appstore says is deleted
                for (int i = _pendingAchievements.pending.Count - 1; i >= 0; i--)
                {
                    if (unlockedAchievements.ContainsKey(_pendingAchievements.pending[i].id))
                    {
                        _pendingAchievements.pending.RemoveAt(i);
                    }
                }
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
    }
}
