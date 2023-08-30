using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public static class RankingAchievementManager
{
    //signing in
    public static void Initialize()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(_ => { });
        //SignIn();
    }

    static void SignIn()
    {
        Social.localUser.Authenticate(success => { });
    }

    //achievements
    public static void UnlockAchievement(string id)
    {
        Social.ReportProgress(id, 100, success => { });
    }

    public static void ShowAchievementsUI()
    {
        Social.ShowAchievementsUI();
    }

    //leaderboards
    public static void AddScoreToLeaderboard(string leaderboardId, long score)
    {
        Social.ReportScore(score, leaderboardId, success => { });
    }

    public static void ShowLeaderboardUI()
    {
        Social.ShowLeaderboardUI();
    }
}
