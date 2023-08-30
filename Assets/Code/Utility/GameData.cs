[System.Serializable]
public class GameData
{
    public int HighScore;
    public int Gold;
    public int GamesSinceAd;
    public bool TutorialCompleted;

    public GameData()
    {
        HighScore = 0;
        Gold = 0;
        GamesSinceAd = 0;
        TutorialCompleted = false;
    }
}
