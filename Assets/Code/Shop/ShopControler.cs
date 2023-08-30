using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.Advertisements;

public class ShopControler : MonoBehaviour {

    //inspector variables
    [SerializeField] private int skinCost = 1500;

    //referene
    [SerializeField] private Image SelectedImg;
    [SerializeField] private List<SelectableSkin> Selectables;
    [SerializeField] private Text GoldTxt;
    [SerializeField] private SceneTransitor transitor;
    [SerializeField] private GameObject goldRain;
    [SerializeField] private RectTransform watchAdRect;

    //fields
    [HideInInspector] public UnlockedSkinsData skinsData;
    private int chosenSkin;
    bool adPlaying = false;

    //delegates
    public delegate void Save(UnlockedSkinsData skinsData);
    public Save save;

    //monobehaviour methods
    private void Start()
    {
        StartCoroutine(transitor.SceneLoadedAnimation());

        //subscribe streams
        SelectableSkin.SelectablePressStream
            .Subscribe(HandleSkinPress).AddTo(this);

        //setup
        chosenSkin = PlayerPrefs.GetInt("ChosenSkin", 0);
        SetupSkins();
        GoldTxt.text = Overlord.gameData.Gold.ToString();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !adPlaying)
        {
            ExitShop();
        }
    }

    //handling buttons
    public void BtnPressed(string button)
    {
        switch(button)
        {
            case "Back":
                ExitShop();
                break;
            case "WatchAd":
                WatchAd();
                break;
            case "Buy0":
                break;
            case "Buy1":
                break;
            case "Buy2":
                break;
        }
    }

    private void ExitShop()
    {
        save.Invoke(skinsData);
        PlayerPrefs.SetInt("ChosenSkin", chosenSkin);
        transitor.gameObject.SetActive(true);
        StartCoroutine(transitor.LoadSceneAsync(0)); //0 - menu scene index
    }

    //ad button
    private void WatchAd()
    {
        ShowRewardedAd();
    }

    //private ad code (from unity guide, dont change)
    private void ShowRewardedAd()
    {
        if (Advertisement.IsReady("rewardedVideo"))
        {
            adPlaying = true;
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show("rewardedVideo", options);
        }
    }

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Overlord.gameData.Gold += 100;  //update golda
                GoldTxt.text = Overlord.gameData.Gold.ToString();   //update gold textu
                save.Invoke(skinsData); //zapisz golda
                //efekty
                Instantiate(goldRain, watchAdRect.position, Quaternion.identity);
                adPlaying = false;
                break;
            case ShowResult.Skipped:
            case ShowResult.Failed:
                adPlaying = false;
                break;
        }
    }

    //skin choice control
    private void SetupSkins()
    {
        for(int i = 0; i < Selectables.Count; i++)
        {
            SelectableSkin skin = Selectables[i];
            int index = skin.index;
            skin.SetUp(Contains(ref skinsData.indexes, index));
            if (index == chosenSkin)
                SetSelectedImg(skin.GetComponent<RectTransform>());
        }
    }

    private void HandleSkinPress(SelectableSkin skin)
    {
        if(skin.isUnlocked)
        {
            chosenSkin = skin.index;
            RectTransform skinTr = skin.GetComponent<RectTransform>();
            SetSelectedImg(skinTr);
        }
        else
        {
            if ((Overlord.gameData.Gold >= skinCost && skin.index != 1) 
                || ((Overlord.gameData.Gold >= 250 && skin.index == 1))) //ten pierwszy skin za 250
            {
                Overlord.gameData.Gold -= skin.index != 1 ? skinCost : 250;
                skin.Unlock();
                skinsData.indexes.Add(skin.index);
                save.Invoke(skinsData);
                chosenSkin = skin.index;
                GoldTxt.text = Overlord.gameData.Gold.ToString();
                SetSelectedImg(skin.GetComponent<RectTransform>());
                //odpalic efekty
            }
            else
            {
                //co jak ktos nie ma hajsu na skina
            }
        }
    }

    private void SetSelectedImg(RectTransform tr)
    {
        SelectedImg.rectTransform.position = tr.position;
        SelectedImg.rectTransform.SetParent(tr);
        SelectedImg.rectTransform.SetSiblingIndex(0);
    }

    //utility
    private bool Contains(ref List<int> indexes, int index)
    {
        for(int i = 0; i < indexes.Count; i++)
        {
            if (indexes[i] == index)
                return true;
        }
        return false;
    }
}
