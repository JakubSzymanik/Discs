using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public class Overlord : MonoBehaviour {

    //references
    [SerializeField] private int MenuSceneIndex;
    [SerializeField] private int GameSceneIndex;
    [SerializeField] private int ShopSceneIndex;
    [SerializeField] private SkinList skinList;

    //fields
    public static GameData gameData;
    public static Skin ChosenSkin { get; private set; }
    public static Skin GoldSkin { get; private set; }
    private SaveSystem saveSystem;

    //singleton
    public static Overlord overlord;

    private void Awake()
    {

        //singleton
        if(overlord == null)
        {
            overlord = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if(overlord != this)
        {
            Destroy(this.gameObject);
            return;
        }

        //on application start
        Application.targetFrameRate = 60; //set target framerate
        saveSystem = new SaveSystem(); 
        gameData = saveSystem.LoadGameData(); //load game data
        SceneManager.sceneLoaded += OnSceneLoaded; //add delegates to sceneLoaded

        int chosenSkin = PlayerPrefs.GetInt("ChosenSkin", 0);
        for (int i = 0; i < skinList.skins.Count; i++)
        {
            if (skinList.skins[i].index == -1)
                GoldSkin = skinList.skins[i];
            else if (skinList.skins[i].index == chosenSkin)
                ChosenSkin = skinList.skins[i];
        }

        //initialize ads
        Advertisement.Initialize("3152037", false); //true - test mode

        //initialize google play
        RankingAchievementManager.Initialize();
    }

    public void Save()
    {
        saveSystem.SaveGameData(gameData);
    }

    public void SaveShop(UnlockedSkinsData skinsData)
    {
        saveSystem.SaveGameData(gameData);
        saveSystem.SaveSkinData(skinsData);
    }

    //on scenes load
    void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) //chujowo z tymi Findami
    {
        if(scene.buildIndex == GameSceneIndex)
        {
            //set up skins
            int chosenSkin = PlayerPrefs.GetInt("ChosenSkin", 0);
            for (int i = 0; i < skinList.skins.Count; i++)
            {
                if (skinList.skins[i].index == -1)
                    GoldSkin = skinList.skins[i];
                else if (skinList.skins[i].index == chosenSkin)
                    ChosenSkin = skinList.skins[i];
            }
            GameObject.Find("GameControler").GetComponent<GameControler>().save += Save;
        }
        else if(scene.buildIndex == ShopSceneIndex)
        {
            ShopControler sc = GameObject.Find("ShopControler").GetComponent<ShopControler>();
            sc.skinsData = saveSystem.LoadSkinsData();
            sc.save += SaveShop;
        }
        else if(scene.buildIndex == 3) //3 - tutorial
        {
            GameObject.Find("TutorialControler").GetComponent<TutorialControler>().save += Save;
        }
    }


}
