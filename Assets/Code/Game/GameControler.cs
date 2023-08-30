using System.Collections;
using UnityEngine;
using UniRx;
using UnityEngine.Advertisements;

public class GameControler : MonoBehaviour {

    //references
    [SerializeField] private GameInputManager inputManager;
    [SerializeField] private Rings rings;
    [SerializeField] private Timer timer;
    [SerializeField] private EffectSpawner effects;
    [SerializeField] private GameUI ui;
    [SerializeField] private SceneTransitor sceneTransitor;
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private GameObject GameStartAnimation;
    [SerializeField] private Animator GameEndAnimation;
    [SerializeField] private PostgameAdPanel adPanel;

    //private variables
    private bool receiveInput; //should game input be received
    private bool pausedInput; //'receiveInput' state before pausing
    private bool isPaused; //is game paused
    private bool isFinished;
    private bool isLoading;
    private bool combo;
    private int comboCount;
    private CurrentGameData currentData;
    private float ringsDefaultSpeed;
    private bool inversedSteering;

    //public variables
    public delegate void Save();
    public Save save;

    private void Start()
    {
        //subskrypcje streamow
        inputManager.GameInputStream
            .Where(_ => !isLoading) //podczas ładowania nie zbieraj inputu
            .Subscribe(input => 
            {
                if (input == InputType.Escape)
                {
                    if (isFinished)
                        ExitToMenu();
                    else
                        Pause();
                }
                else
                    HandleInput(input);
            }).AddTo(this);

        ui.UiButtonStream
            .Subscribe(HandleButtonPress)
            .AddTo(this);

        rings.MatchesStream
            .Subscribe(ManagePointsAndGold)
            .AddTo(this);

        rings.LoseStream
            .Subscribe(_ => StartCoroutine(FinishGame()))
            .AddTo(this);

        timer.GameTimerStream
            .Subscribe(_ => InsertOut())
            .AddTo(this);

        //start gry
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        rings.CreateRings();
        ui.SetUp(Overlord.gameData.HighScore, Overlord.gameData.Gold);

        //wait for load animation to finish
        yield return StartCoroutine(sceneTransitor.SceneLoadedAnimation());

        //wait for game starting animation to finish
        uiAnimator.Play("GameStart");
        do
        {
            yield return null;
        } while (uiAnimator.GetCurrentAnimatorStateInfo(0).IsName("GameStart"));
        Destroy(GameStartAnimation);

        //inverse steering?
        inversedSteering = PlayerPrefs.GetInt("InverseControls", 0) == 1 ? true : false;

        //check if tutorial should be played

        Time.timeScale = 1;
        currentData = new CurrentGameData();
        isPaused = false;
        isFinished = false;
        receiveInput = true;
        isLoading = false;
        ringsDefaultSpeed = rings.animationSpeed;
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
            ui.Pause(true);
        }
        else
        {
            receiveInput = pausedInput;
            Time.timeScale = 1;
            ui.Pause(false);
        }
    }

    private IEnumerator FinishGame()
    {
        isFinished = true;
        receiveInput = false;
        timer.enabled = false;
        //zapisz gre
        //odpal reklamy
        //start lose animation
        GameEndAnimation.gameObject.SetActive(true);
        //wait for lose animation finish
        do
        {
            yield return null;
        } while (GameEndAnimation.GetCurrentAnimatorStateInfo(0).IsName("GameEnd"));
        uiAnimator.Play("EndMenu");


        Time.timeScale = 0;
        bool isHighScore;
        if (Overlord.gameData.HighScore < currentData.Score)
        {
            Overlord.gameData.HighScore = currentData.Score;
            isHighScore = true;
        }
        else
        {
            isHighScore = false;
        }

        //save data
        save.Invoke();

        //leaderboard info
        RankingAchievementManager.AddScoreToLeaderboard(GPGSIds.leaderboard_high_score, Overlord.gameData.HighScore);
        //achievement info
        if (currentData.Score == 0)   /////////////////////////
        {
            RankingAchievementManager.UnlockAchievement(GPGSIds.achievement_null);
        }
        if (currentData.MatchCounts[2] >= 5)
        {
            RankingAchievementManager.UnlockAchievement(GPGSIds.achievement_5x5);
        }
        if (currentData.Score >= 25)
        {
            RankingAchievementManager.UnlockAchievement(GPGSIds.achievement_25_points);
            if (currentData.Score >= 50)
            {
                RankingAchievementManager.UnlockAchievement(GPGSIds.achievement_50_points);
                if (currentData.Score >= 75)
                {
                    RankingAchievementManager.UnlockAchievement(GPGSIds.achievement_75_points);
                    if (currentData.Score >= 100)
                    {
                        RankingAchievementManager.UnlockAchievement(GPGSIds.achievement_100_points);
                        if (currentData.Score >= 150)
                        {
                            RankingAchievementManager.UnlockAchievement(GPGSIds.achievement_150_points);
                        }
                    }
                }
            }
        }

        if (currentData.gold > 50 && Random.Range(0,2) % 2 == 0 )
        {
            //show ad panel, and wait for completion
            adPanel.gameObject.SetActive(true);
            adPanel.Initialize(currentData.gold);
            while (adPanel.gameObject.activeSelf)
            {
                yield return null;
            }
        }
        else
        {
            //advertisements
            Overlord.gameData.GamesSinceAd++;
            if (Overlord.gameData.GamesSinceAd >= 6)
            {
                Advertisement.Show();
                Overlord.gameData.GamesSinceAd = 0;
            }
        }

        //activate end menu
        ui.ActivateEndMenu(currentData, Overlord.gameData.HighScore, isHighScore); //enable end menu
    }

    private void RestartGame()
    {
        //nie zapisywać, spierdalać

        isLoading = true;
        sceneTransitor.gameObject.SetActive(true);
        StartCoroutine(sceneTransitor.LoadSceneAsync(SceneIndexes.GameIndex)); //1 - game scene index
    }

    private void ExitToMenu()
    {
        //nic sie nie dzieje, spierdalac

        isLoading = true;
        sceneTransitor.gameObject.SetActive(true);
        StartCoroutine(sceneTransitor.LoadSceneAsync(SceneIndexes.MenuIndex));
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
            if(matchDatas[i].isGold)
            {
                Overlord.gameData.Gold += (matchDatas[i].matchedCount - 2) * 10;
                currentData.gold += (matchDatas[i].matchedCount - 2) * 10;
            }
            currentData.Score += matchDatas[i].matchedCount - 2;
            currentData.MatchCounts[matchDatas[i].matchedCount - 3]++;
            comboCount++;
        }
        UpdateUI();
    }

    //UI methods
    private void UpdateUI()
    {
        ui.UpdateUI(currentData.Score, Overlord.gameData.Gold);
    }

    //Game Input handling methods / coroutines
    private void HandleButtonPress(ButtonType button)
    {
        switch(button)
        {
            case ButtonType.Pause:
            case ButtonType.Resume:
                Pause();
                break;
            case ButtonType.Menu:
                ExitToMenu();
                break;
            case ButtonType.Restart:
                RestartGame();
                break;
        }
    }

    private void HandleInput(InputType input)
    {
        if(!receiveInput)
        {
            //co jak ktos dotknie przed zakonczeniem poprzedniej animacji
            return;
        }

        //handle end of combo
        combo = false;
        if (comboCount > currentData.LongestCombo)
            currentData.LongestCombo = comboCount;
        comboCount = 0;

        //handle input
        switch(input)
        {
            case InputType.SwipeLeft:
                rings.animationSpeed = ringsDefaultSpeed;
                StartCoroutine(Move(!inversedSteering));
                break;
            case InputType.SwipeRight:
                rings.animationSpeed = ringsDefaultSpeed;
                StartCoroutine(Move(inversedSteering));
                break;
            case InputType.SwipeUp:
                StartCoroutine(InsertIn());
                break;
            case InputType.SuperSwipeLeft:
                rings.animationSpeed = ringsDefaultSpeed * 1.5f;
                StartCoroutine(Move(true));
                break;
            case InputType.SuperSwipeRight:
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
