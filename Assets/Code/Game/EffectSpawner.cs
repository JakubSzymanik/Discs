using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectSpawner : MonoBehaviour {

    //inspector values
    [SerializeField] private float spawnDelay;
    //references
    [SerializeField] private GameObject matchPointsTextPrefab;
    //fields
    [SerializeField] private List<GameObject> matchTexts;

    public void SpawnEffects(int points, Vector2 pos, Quaternion rot, Color color, bool isGold, bool isCombo)
    {
        GameObject pointsTxtObject = GetAvaibleText();
        if (pointsTxtObject == null)
        {
            pointsTxtObject = Instantiate(matchPointsTextPrefab, pos, rot);
        }
        else
        {
            pointsTxtObject.SetActive(true);
            pointsTxtObject.transform.position = pos;
            pointsTxtObject.transform.rotation = rot;
        }
        Text pointsTxt = pointsTxtObject.transform.GetChild(0).GetComponent<Text>();
        pointsTxt.text = "+" + points.ToString() +
            (isGold ? ("\n+" + (points * 10).ToString() + " gold") : "" ) + (isCombo ? "\nCOMBO!" : "");
        pointsTxt.color = color;
    }

    GameObject GetAvaibleText()
    {
        for(int i = 0; i < matchTexts.Count; i++)
        {
            if(!matchTexts[i].gameObject.activeSelf)
            {
                return matchTexts[i];
            }
        }
        return null;
    }
}
