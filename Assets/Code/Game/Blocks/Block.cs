using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Block : MonoBehaviour {

    [SerializeField] private float ringWidth; //width of a single ring
    [SerializeField] private float animationSpeed; //speed of all animations
    [SerializeField] private float matchAnimationSpeed;
    [SerializeField] private float insertSpeedMod;
    [SerializeField] private float outsertSpeedMod;
    [SerializeField] private GameObject breakdownPrefab;
     
    public bool isAnimating = false; //is any animation playing?

    private int size; //size of the block

    //Reference
    protected SpriteRenderer PieSprite;
    protected SpriteMask CircleMask;
    protected SpriteRenderer Symbol;
    protected SortingGroup sortingGroup;

    protected void SetUp(int size)
    {
        this.size = size;
        CircleMask.transform.localScale = Vector3.one * GetMaskScale(size);
        PieSprite.transform.localScale = Vector3.one * GetPieScale(size);
        Symbol.transform.position = Vector2.up * (GetPieScale(size) + GetMaskScale(size)) / 2;
    }

    public IEnumerator Move(int steps)
    {
        DisableGlobalMask();
        isAnimating = true;

        size += steps;
        float targetMaskScale = GetMaskScale(size);
        float targetPieScale = GetPieScale(size);

        while (CircleMask.transform.localScale.x != targetMaskScale)
        {
            CircleMask.transform.localScale =
                Vector3.one * Mathf.MoveTowards(CircleMask.transform.localScale.x, targetMaskScale, animationSpeed * Time.deltaTime);
            PieSprite.transform.localScale =
                Vector3.one * Mathf.MoveTowards(PieSprite.transform.localScale.x, targetPieScale, animationSpeed * Time.deltaTime);
            Symbol.transform.localPosition = Vector2.up * (CircleMask.transform.localScale.y + PieSprite.transform.localScale.y) / 2;
            yield return null;
        }

        isAnimating = false;
        EnableGlobalMask();
    }

    public IEnumerator InsertIn()
    {
        DisableGlobalMask();
        float targetMidScale = 0.66f; //scale at wich other blocks should start moving
        transform.localScale = Vector3.zero;
        while (transform.localScale.x < targetMidScale)
        {
            transform.localScale =
                Vector3.one * Mathf.MoveTowards(transform.localScale.x, targetMidScale, Time.deltaTime * animationSpeed);
            yield return null;
        }
        StartCoroutine(InsertInFinish());
    }

    private IEnumerator InsertInFinish() //koncowka animacji wkładania
    {
        float targetScale = 1;
        while (transform.localScale.x < targetScale)
        {
            transform.localScale =
                Vector3.one * Mathf.MoveTowards(transform.localScale.x, targetScale, Time.deltaTime * animationSpeed * insertSpeedMod);
            yield return null;
        }
        EnableGlobalMask();
    }

    public IEnumerator InsertOut()
    {
        DisableGlobalMask();
        float pos = 5;
        transform.position = transform.up * pos;
        while(transform.localPosition != Vector3.zero)
        {
            pos = Mathf.MoveTowards(pos, 0, animationSpeed * Time.deltaTime * outsertSpeedMod);
            transform.position = transform.up * pos;
            yield return null;
        }
        EnableGlobalMask();
    }

    public IEnumerator Match(int strength)
    {
        isAnimating = true;

        //animation; now color change and symbol fade
        if (this.GetType() == typeof(Basic))
        {
            //color change animation
            while (Symbol.color.a > 0)
            {
                //PieSprite.color += colorDelta * Time.deltaTime * matchAnimationSpeed;
                Symbol.color -= Color.white * Time.deltaTime * matchAnimationSpeed; //musi szybciej znikac *2
                yield return null;
            }
            //spawn effects
            GameObject effect = BreakdownEffectPool.activePool.GetEffect();
            effect.transform.position = Symbol.transform.position;
            effect.transform.rotation = transform.rotation;
            effect.SetActive(true);
            var main = effect.GetComponent<ParticleSystem>().main;
            main.startColor = PieSprite.color;
        }
        else
        {
            //color change animation
            Color colorDelta = Symbol.color - PieSprite.color;
            while (PieSprite.color.maxColorComponent < Symbol.color.maxColorComponent)
            {
                PieSprite.color += colorDelta * matchAnimationSpeed * Time.deltaTime;
                yield return null;
            }
            //spawn effects
            var main = Instantiate(breakdownPrefab, Symbol.transform.position, transform.rotation)
                .GetComponent<ParticleSystem>().main;
            main.startColor = PieSprite.color;
        }

        Destroy(this.gameObject);
        isAnimating = false;
    }

    //private methods
    private void Awake()
    {
        PieSprite = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        CircleMask = this.transform.GetChild(1).GetComponent<SpriteMask>();
        Symbol = this.transform.GetChild(2).GetComponent<SpriteRenderer>();
        sortingGroup = this.GetComponent<SortingGroup>();
    }

    private float GetMaskScale(float size)
    {
        return 1 + size * (ringWidth - 0.003f); //ten dodatek żeby było nie było małej przerwy
    }

    private float GetPieScale(float size)
    {
        return GetMaskScale(size) + ringWidth;
    }

    private void DisableGlobalMask()
    {
        CircleMask.enabled = true;
        sortingGroup.enabled = true;
    }

    private void EnableGlobalMask()
    {
        PieSprite.sortingOrder = (2 - size) * 2;
        Symbol.sortingOrder = (2 - size) * 2 + 1;
        CircleMask.enabled = false;
        sortingGroup.enabled = false;
    }
}
