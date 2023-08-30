using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skin List", menuName = "Skin List")]
public class SkinList : ScriptableObject
{
    public List<Skin> skins;
}
