using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SelectableSkin : MonoBehaviour {

    //reference
    [SerializeField] private List<Image> images;
    [SerializeField] private List<Image> symbols;
    [SerializeField] private Skin skin;
    [SerializeField] private GameObject lockImg;

    //fields
    public bool isUnlocked { get; private set; }
    public int index { get { return skin.index; } }

    //stream
    public static IObservable<SelectableSkin> SelectablePressStream { get { return SelectablePressSubject; } }
    private static Subject<SelectableSkin> SelectablePressSubject = new Subject<SelectableSkin>();

    public void SetUp(bool isUnlocked)
    {
        this.isUnlocked = isUnlocked;
        for(int i = 0; i < 4; i++)
        {
            images[i].color = skin.Colors[i];
            symbols[i].color = skin.SymbolColor;
            symbols[i].sprite = skin.Symbols[i];
        }
        lockImg.SetActive(!isUnlocked);
    }

    public void Unlock()
    {
        isUnlocked = true;
        lockImg.SetActive(false);
    }

    public void ButtonPressed()
    {
        SelectablePressSubject.OnNext(this);
    }
}
