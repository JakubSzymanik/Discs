using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : Block {

	public new void SetUp(int size)
    {
        base.SetUp(size);
        PieSprite.color = Overlord.GoldSkin.Colors[0];
        Symbol.sprite = Overlord.GoldSkin.Symbols[0];
        Symbol.color = Overlord.GoldSkin.SymbolColor;
    }
}
