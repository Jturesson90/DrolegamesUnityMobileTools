using System;

namespace Drolegames.SocialService
{
    public interface ILeaderboards
    {
        void ReportScore(long score, string leaderboardId, Action<bool> callback);
        void ReportScore(long score, string leaderboardId, string tag, Action<bool> callback);
        void ShowUI();
        void ShowUI(string leaderboardId);
        long GetLeaderboardUserScore(string leaderboardId);
    }
}