using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basic : Block {

    public int type { get; private set; }

    public void SetUp(int size, int type)
    {
        base.SetUp(size);
        this.type = type;
        PieSprite.color = Overlord.ChosenSkin.Colors[type];
        Symbol.sprite = Overlord.ChosenSkin.Symbols[type];
        Symbol.color = Overlord.ChosenSkin.SymbolColor;
    }
}
