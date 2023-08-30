using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuControler : MonoBehaviour {

    //inspector variables
    [SerializeField] private int GameSceneIndex;
    [SerializeField] private int ShopSceneIndex;
    [Space]

    //reference
    [SerializeField] private SceneTransitor transitor;
    [SerializeField] private Text HighScoreTxt;
    [SerializeField] private Text GoldTxt;
    [SerializeField] private SettingsControler SettingsControler;

    //fields
    private bool sceneLoading = false;

    //monobehaviur methods
    private void Start()
    {
        transitor.gameObject.SetActive(true);
        StartCoroutine(transitor.SceneLoadedAnimation());
        HighScoreTxt.text = Overlord.gameData.HighScore.ToString();
        GoldTxt.text = Overlord.gameData.Gold.ToString();
        //ustawić powiadomienie o możliwosci kupienia skina
    }

    private void Update() //for registering back key
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(SettingsControler.gameObject.activeSelf) //if settings panel is active
            {
                SettingsControler.Close();
            }
            else
            {
                Application.Quit();
            }
        }
    }

    //public methods
    public void BtnPressed(string type)
    {
        if (type == "Play")
        {
            if (Overlord.gameData.TutorialCompleted == true)
                LoadScene(SceneIndexes.GameIndex);
            else
                LoadScene(3); //3 - tutorial
        }
        else if (type == "Shop")
            LoadScene(SceneIndexes.ShopIndex);
        else if (type == "Settings")
            SettingsControler.gameObject.SetActive(true);
        else if (type == "SettingsBack")
            SettingsControler.Close();
        else if (type == "Quit")
            Application.Quit();
        else if (type == "Ranking")
            RankingAchievementManager.ShowLeaderboardUI(); ////////////
        else if (type == "SettingsTutorial")
            LoadScene(3); //3 - tutorial
    }

    //private methods
    private void LoadScene(int index)
    {
        sceneLoading = true;
        transitor.gameObject.SetActive(true);
        StartCoroutine(transitor.LoadSceneAsync(index));
    }
}
