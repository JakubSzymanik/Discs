using System.Collections.Generic;

[System.Serializable]
public class UnlockedSkinsData
{
    public List<int> indexes;

    public UnlockedSkinsData()
    {
        indexes = new List<int>();
        indexes.Add(0);
    }
}
