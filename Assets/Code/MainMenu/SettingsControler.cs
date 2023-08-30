using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsControler : MonoBehaviour {

    //references
    [SerializeField] private Toggle inverseToggle;
    [SerializeField] private Toggle muteToggle;

    public void OnEnable()
    {
        bool invCtrl = PlayerPrefs.GetInt("InverseControls", 0) == 1 ? true : false;
        bool muteSound = PlayerPrefs.GetInt("MuteSound", 0) == 1 ? true : false;
        inverseToggle.isOn = invCtrl;
        muteToggle.isOn = muteSound;
    }

    public void Close()
    {
        PlayerPrefs.SetInt("InverseControls", inverseToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("MuteSound", muteToggle.isOn ? 1 : 0);
        this.gameObject.SetActive(false);
    }
}
