using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakdownEffectPool : MonoBehaviour {

    //static
    public static BreakdownEffectPool activePool;

    //reference
    [SerializeField] private GameObject breakDownEffect;
    [SerializeField] private List<GameObject> breakdownEffects;

    //methods
    private void Awake()
    {
        //singleton
        if(activePool == null)
        {
            activePool = this;
        }
        else if(activePool != this)
        {
            Destroy(this.gameObject);
        }
    }

    public GameObject GetEffect()
    {
        for(int i = 0; i < breakdownEffects.Count; i++)
        {
            if(!breakdownEffects[i].activeSelf)
            {
                return breakdownEffects[i];
            }
        }
        return Instantiate(breakDownEffect, transform.position, Quaternion.identity);
    }
}
