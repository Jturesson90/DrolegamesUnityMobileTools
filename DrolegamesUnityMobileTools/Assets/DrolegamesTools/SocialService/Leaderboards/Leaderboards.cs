namespace Drolegames.SocialService
{
    using System;
    using System.Collections;
    using UnityEngine;
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
        public void ReportScore(long score, string leaderboardId, Action<bool> callback)
        {
            social.ReportLeaderboardScore(score, leaderboardId, callback);
        }

        public void ReportScore(long score, string leaderboardId, string tag, Action<bool> callback)
        {
            social.ReportLeaderboardScore(score, leaderboardId, tag, callback);
        }

        public void ShowUI()
        {
            this.social.ShowLeaderboardUI();
        }

        public void ShowUI(string leaderboardId)
        {
            this.social.ShowLeaderboardUI(leaderboardId);
        }
    }
}