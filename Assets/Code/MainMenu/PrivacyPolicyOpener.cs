using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrivacyPolicyOpener : MonoBehaviour {

    [SerializeField] private string PolicyURL;

    public void OpenPP()
    {
        Application.OpenURL(PolicyURL);
    }
}
