public class CurrentGameData
{
    public int Score;
    public int[] MatchCounts;
    public int LongestCombo;
    public int gold;

    public CurrentGameData()
    {
        Score = 0;
        MatchCounts = new int[3] { 0, 0, 0 };
        LongestCombo = 0;
        gold = 0;
    }
}
