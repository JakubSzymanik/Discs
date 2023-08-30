using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsertBlock : MonoBehaviour {

    //inspector variables
    [SerializeField] private float animationSpeed;

    //reference
    [SerializeField] private SpriteRenderer chosenBlock;
    [SerializeField] private SpriteRenderer chosenBlockSymbol;

    //fields
    private int blockType; //-1: gold    0 - 3: standard block types
    private bool isInserting;

    //MonoBehavious   TEST
    public void Start()
    {
        SetUp();
    }

    //public methods
    public void SetUp()
    {
        blockType = RandomBlock.Get;
        chosenBlock.color = blockType >= 0 ? Overlord.ChosenSkin.Colors[blockType] : Overlord.GoldSkin.Colors[0];
        chosenBlockSymbol.color = blockType >= 0 ? Overlord.ChosenSkin.SymbolColor : Overlord.GoldSkin.SymbolColor;
        chosenBlockSymbol.sprite = blockType >= 0 ? Overlord.ChosenSkin.Symbols[blockType] : Overlord.GoldSkin.Symbols[0];
    }

    public Block Insert()
    {
        if (isInserting)
            return null;
        Block block = BlockCreator.Creator.Spawn(blockType, 0, null);
        StartCoroutine(InsertAnimation());
        return block;
    }

    //private + animation methods
    private IEnumerator InsertAnimation()
    {
        isInserting = true;
        float scale = chosenBlock.transform.localScale.y;
        while(chosenBlock.transform.localScale.y > 0)
        {
            chosenBlock.transform.localScale =
                Vector3.one * Mathf.MoveTowards(chosenBlock.transform.localScale.y, 0, animationSpeed * Time.deltaTime);
            yield return null;
        }

        SetUp();

        while (chosenBlock.transform.localScale.y < scale)
        {
            chosenBlock.transform.localScale =
                Vector3.one * Mathf.MoveTowards(chosenBlock.transform.localScale.y, scale, animationSpeed * Time.deltaTime);
            yield return null;
        }
        isInserting = false;
    }
}
