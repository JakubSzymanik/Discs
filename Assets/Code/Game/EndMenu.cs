using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndMenu : MonoBehaviour {

    [SerializeField] private Text Match3;
    [SerializeField] private Text Match4;
    [SerializeField] private Text Match5;
    [SerializeField] private Text LongestCombo;
    [SerializeField] private Text HighScore;
    [Space]
    [SerializeField] private GameObject ScorePanel;
    [SerializeField] private Text ScoreBasic;
    [SerializeField] private GameObject ScorePanelRekord;
    [SerializeField] private Text ScoreRekord;

    public void SetUp(CurrentGameData currentData, int highScore, bool isHighScore)
    {
        Match3.text = currentData.MatchCounts[0].ToString();
        Match4.text = currentData.MatchCounts[1].ToString();
        Match5.text = currentData.MatchCounts[2].ToString();
        LongestCombo.text = currentData.LongestCombo.ToString();

        if(isHighScore)
        {
            ScorePanelRekord.SetActive(true);
            ScoreRekord.text = currentData.Score.ToString();
        }
        else
        {
            HighScore.text = highScore.ToString();
            ScorePanel.SetActive(true);
            ScoreBasic.text = currentData.Score.ToString();
        }
    }
}
