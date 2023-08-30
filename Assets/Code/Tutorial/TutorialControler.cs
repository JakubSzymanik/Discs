using System.Collections;
using UnityEngine;
using UniRx;
using UnityEngine.Advertisements;

public class TutorialControler : MonoBehaviour {

    //references
    [SerializeField] private GameInputManager inputManager;
    [SerializeField] private Rings rings;
    [SerializeField] private EffectSpawner effects;
    [SerializeField] private SceneTransitor sceneTransitor;
    [SerializeField] private Animator tutorialUIAnimator;

    //private variables
    private bool receiveInput; //should game input be received
    private bool pausedInput; //'receiveInput' state before pausing
    private bool isPaused; //is game paused
    private bool isFinished;
    private bool isLoading;
    private bool combo;
    private int comboCount;
    private float ringsDefaultSpeed;
    private bool inversedSteering;

    private bool receiveSwipe = false;
    private bool receiveTap = false;

    //public variables
    public delegate void Save();
    public Save save;

    //state machine lookalike
    private int stage = 0;
    private int rotateCount = 0;

    private void Start()
    {
        //subskrypcje streamow
        inputManager.GameInputStream
            .Where(_ => !isLoading) //podczas ładowania nie zbieraj inputu
            .Subscribe(input => 
            {
                HandleInput(input);
            }).AddTo(this);

        rings.MatchesStream
            .Subscribe(ManagePointsAndGold)
            .AddTo(this);

        //start gry
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        rings.CreateRings();

        //wait for load animation to finish
        yield return StartCoroutine(sceneTransitor.SceneLoadedAnimation());

        //inverse steering?
        inversedSteering = PlayerPrefs.GetInt("InverseControls", 0) == 1 ? true : false;

        Time.timeScale = 1;
        isPaused = false;
        isFinished = false;
        receiveInput = true;
        isLoading = false;
        ringsDefaultSpeed = rings.animationSpeed;
        StartCoroutine(Game());
    }

    IEnumerator Game()
    {
        receiveTap = true;
        while(stage < 1)
        {
            yield return null;
        }
        tutorialUIAnimator.SetTrigger("Transition");
        while (stage < 2)
        {
            yield return null;
        }
        tutorialUIAnimator.SetTrigger("Transition");
        while (stage < 3)
        {
            yield return null;
        }
        tutorialUIAnimator.SetTrigger("Transition");
        while (stage < 4)
        {
            yield return null;
        }
        tutorialUIAnimator.SetTrigger("Transition");
        while (stage < 5)
        {
            yield return null;
        }
        Overlord.gameData.TutorialCompleted = true;
        save.Invoke();
        sceneTransitor.gameObject.SetActive(true);
        StartCoroutine(sceneTransitor.LoadSceneAsync(1));
    }

    //Game control methods
    private void Pause()
    {
        isPaused = !isPaused;
        if(isPaused)
        {
            pausedInput = receiveInput;
            receiveInput = false;
            Time.timeScale = 0;
        }
        else
        {
            receiveInput = pausedInput;
            Time.timeScale = 1;
        }
    }

    private IEnumerator FinishGame()
    {
        isFinished = true;
        receiveInput = false;

        Time.timeScale = 0;
        
        //tu załadować scene gry, i zapisać, że tutorial skończony


        yield break;
    }

    //Point control methods
    private void ManagePointsAndGold(MatchData[] matchDatas)
    {
        for(int i = 0; i < matchDatas.Length; i++)
        {
            //trigger effects
            effects
                .SpawnEffects(
                matchDatas[i].matchedCount - 2, 
                matchDatas[i].midPoint,
                matchDatas[i].midRotation,
                matchDatas[i].matchColor,
                matchDatas[i].isGold,
                combo);
            combo = true;
            //update game and match data
            comboCount++;
        }
    }

    //Game Input handling methods / coroutines

    private void HandleInput(InputType input)
    {
        if(!receiveInput)
        {
            //co jak ktos dotknie przed zakonczeniem poprzedniej animacji
            return;
        }

        //handle end of combo
        combo = false;
        comboCount = 0;

        //handle input
        switch(input)
        {
            case InputType.SwipeLeft:
                if (stage == 2)
                    stage++;
                else if (stage == 3)
                {
                    rotateCount++;
                    if (rotateCount >= 2)
                        stage++;
                }
                rings.animationSpeed = ringsDefaultSpeed;
                StartCoroutine(Move(!inversedSteering));
                break;
            case InputType.SwipeRight:
                if (stage == 2)
                    stage++;
                else if(stage == 3)
                {
                    rotateCount++;
                    if (rotateCount >= 2)
                        stage++;
                }
                rings.animationSpeed = ringsDefaultSpeed;
                StartCoroutine(Move(inversedSteering));
                break;
            case InputType.SwipeUp:
                 if (stage == 1)
                    stage++;
                StartCoroutine(InsertIn());
                break;
            case InputType.Tap:
                if (stage == 0)
                {
                    stage++;
                    break;
                }
                else if (stage == 4)
                {
                    stage++;
                    break;
                }
                break;
            case InputType.SuperSwipeLeft:
                if (stage == 3)
                {
                    rotateCount++;
                    if (rotateCount >= 2)
                        stage++;
                }
                rings.animationSpeed = ringsDefaultSpeed * 1.5f;
                StartCoroutine(Move(true));
                break;
            case InputType.SuperSwipeRight:
                if (stage == 3)
                {
                    rotateCount++;
                    if (rotateCount >= 2)
                        stage++;
                }
                rings.animationSpeed = ringsDefaultSpeed * 1.5f;
                StartCoroutine(Move(false));
                break;
        }
    }

    private IEnumerator Move(bool left)
    {
        receiveInput = false;
        yield return StartCoroutine( rings.Rotate(left) );
        receiveInput = true;
    }

    private IEnumerator InsertIn()
    {
        receiveInput = false;
        yield return StartCoroutine( rings.InsertIn() );
        receiveInput = true;
    }

    private void InsertOut()
    {
        rings.InsertOut();
    }
}
