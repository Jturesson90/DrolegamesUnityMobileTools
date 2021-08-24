using System;

namespace Drolegames.SocialService
{
    public interface ILeaderboards
    {
        void ReportScore(long score, string leaderBoardId, Action<bool> callback);
        void ReportScore(long score, string leaderBoardId, string tag, Action<bool> callback);
        void ShowUI();
        void ShowUI(string leaderboardId);
    }
}