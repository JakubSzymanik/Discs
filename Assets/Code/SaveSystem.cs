using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem
{
    private string Path;
    private string DataSaveName = "/GameData.dat";
    private string UnlockedSkinsSaveName = "/UnlockedSkins.dat";

    public SaveSystem()
    {
        Path = Application.persistentDataPath;
    }

    public void SaveGameData(GameData data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Path + DataSaveName);
        bf.Serialize(file, data);
        file.Close();
    }

    public void SaveSkinData(UnlockedSkinsData skinsData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Path + UnlockedSkinsSaveName);
        bf.Serialize(file, skinsData);
        file.Close();
    }

    public GameData LoadGameData()
    {
        GameData data = new GameData();
        string FileName = Path + DataSaveName;
        if (File.Exists(FileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(FileName, FileMode.Open);
            data = (GameData)bf.Deserialize(file);
            file.Close();
        }
        return data;
    }

    public UnlockedSkinsData LoadSkinsData()
    {
        UnlockedSkinsData data = new UnlockedSkinsData();
        string FileName = Path + UnlockedSkinsSaveName;
        if (File.Exists(FileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(FileName, FileMode.Open);
            data = (UnlockedSkinsData)bf.Deserialize(file);
            file.Close();
        }
        return data;
    }
}
