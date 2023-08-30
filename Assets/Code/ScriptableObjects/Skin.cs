using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skin", menuName = "Skin")]
public class Skin : ScriptableObject
{
    public List<Color> Colors;
    public List<Sprite> Symbols;
    public Color SymbolColor;
    public int index; //-1: gold, 0: defalut
}
