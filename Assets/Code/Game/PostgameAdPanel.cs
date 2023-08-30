using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class PostgameAdPanel : MonoBehaviour {

    [SerializeField] private Text goldTxt;
    [SerializeField] private Image timerImg;
    int goldToGain;

    float time = 3;
    float timer = 0;
    //monobehaviur
    private void Update()
    {
        //countdown and timer image update
        timer += Time.unscaledDeltaTime;
        timerImg.fillAmount = 1 - timer / time;
        if(timer >= time)
        {
            this.gameObject.SetActive(false);
        }
    }

    //public methods
    public void Initialize(int goldToGain)
    {
        this.goldToGain = goldToGain;
        goldTxt.text = goldToGain.ToString();
    }

	public void WatchForGoldPressed()
    {
        ShowRewardedAd();
    }

    //private ad code (from unity guide, dont change)
    private void ShowRewardedAd()
    {
        if (Advertisement.IsReady("rewardedVideo"))
        {
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show("rewardedVideo", options);
        }
    }

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                //tu moze jeszcze jakies efekty jebnac
                Overlord.gameData.Gold += goldToGain;
                this.gameObject.SetActive(false);
                break;
            case ShowResult.Skipped:
            case ShowResult.Failed:
                this.gameObject.SetActive(false);
                break;
        }
    }
}
