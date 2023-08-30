using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameUI : MonoBehaviour {

    //references
    [SerializeField] private Text HighScoreTxt;
    [SerializeField] private Text ScoreTxt;
    [SerializeField] private Text GoldTxt;
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private EndMenu EndMenu;

    //stream
    public IObservable<ButtonType> UiButtonStream { get { return UiButtonSubject; } }
    public Subject<ButtonType> UiButtonSubject = new Subject<ButtonType>();

    public void SetUp(int highScore, int gold)
    {
        GoldTxt.text = gold.ToString();
        HighScoreTxt.text = highScore.ToString();
        ScoreTxt.text = "0";
    }

    public void UpdateUI(int score, int gold)
    {
        ScoreTxt.text = score.ToString();
        GoldTxt.text = gold.ToString();
        //play animations?
    }

    public void Pause(bool pause)
    {
        PausePanel.SetActive(pause);
    }

    public void BtnPressed(string type)
    {
        switch(type)
        {
            case "Pause":
                UiButtonSubject.OnNext(ButtonType.Pause);
                break;
            case "Resume":
                UiButtonSubject.OnNext(ButtonType.Resume);
                break;
            case "Menu":
                UiButtonSubject.OnNext(ButtonType.Menu);
                break;
            case "Restart":
                UiButtonSubject.OnNext(ButtonType.Restart);
                break;
        }

    }

    public void ActivateEndMenu(CurrentGameData currentData, int highScore, bool isHighScore)
    {
        EndMenu.gameObject.SetActive(true);
        EndMenu.SetUp(currentData, highScore, isHighScore);
    }
}
