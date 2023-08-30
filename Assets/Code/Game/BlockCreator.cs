using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCreator : MonoBehaviour {

    //reference
    [SerializeField] private GameObject basicBlock;
    [SerializeField] private GameObject goldBlock;

    //singleton
	public static BlockCreator Creator { get;  private set; }

    private void Awake()
    {
        if(Creator ==  null)
        {
            Creator = this;
        }
        else if(Creator != this)
        {
            Destroy(this.gameObject);
        }
    }
    //singleton

    public Block Spawn(int blockType, int size, Transform parent)
    {
        Block block =
            Instantiate((blockType >= 0 ? basicBlock : goldBlock), transform.position, Quaternion.identity).GetComponent<Block>();
        if (parent != null)
            block.transform.SetParent(parent);

        if (block.GetType() == typeof(Basic)) //Set up block according to type
        {
            ((Basic)block).SetUp(size, blockType);
        }
        else
        {
            ((Gold)block).SetUp(size);
        }
        return block;
    }
}
