using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class Timer : MonoBehaviour {

    //inspector variables
    [SerializeField] float outsertBaseTime;
    [SerializeField] AnimationCurve outsertTimeCurve;

    //references
    [SerializeField] Image image;

    //streams
    public IObservable<Unit> GameTimerStream { get { return GameTimerSubject; } }
    Subject<Unit> GameTimerSubject = new Subject<Unit>();

    //fields
    float outsertTime;
    public bool isPaused;
    float outsertClock;

    private void Start()
    {
        outsertTime = outsertBaseTime;
    }

    private void Update()
    {
        if (isPaused)
            return;
        UpdateGameTimer();
    }

    private void UpdateGameTimer()
    {
        outsertClock += Time.deltaTime;
        if(outsertClock >= outsertBaseTime)
        {
            GameTimerSubject.OnNext(new Unit());
            outsertClock = 0;
            outsertTime = outsertBaseTime / outsertTimeCurve.Evaluate(Time.timeSinceLevelLoad);
            //image.fillClockwise = !image.fillClockwise;
        }

        //image.fillAmount = image.fillClockwise ? outsertClock / outsertBaseTime : 1 - outsertClock / outsertBaseTime;
    }
}
