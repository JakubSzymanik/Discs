using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingsGenerator : MonoBehaviour {

	public void CreateRings(int fill, ref Block[,] blocks, Transform parent)
    {
        int height = blocks.GetLength(0);
        int width = blocks.GetLength(1);
        int startSymbol = RandomBlock.Get;
        for (int i = 0; i < fill; i++)
        {
            for(int j = 0; j < width; j++)
            {
                int Symbol = startSymbol;
                startSymbol += Random.Range(1,3);
                startSymbol = startSymbol > 3 ? startSymbol - 5 : startSymbol;
                Block block = BlockCreator.Creator.Spawn(Symbol, i, parent);
                block.transform.localRotation = Quaternion.Euler(0, 0, j * -30);
                blocks[i, j] = block;
            }
        }
    }

    //private methods
    private bool IsInRange(int x, int y)
    {
        return x >= 0 && x < 3 && y >= 0 && y < 12;
    }

    private int Loop(int x)
    {
        return x < 0 ? 12 + x : x > 11 ? x - 12 : x;
    }
}
