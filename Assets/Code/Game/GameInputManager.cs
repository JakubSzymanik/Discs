using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI; //test

public class GameInputManager : MonoBehaviour {

    public float swipeTreshold;
    public float swipeSideAngle;

    public IObservable<InputType> GameInputStream { get { return GameInputSubject; } }
    private Subject<InputType> GameInputSubject = new Subject<InputType>();

    private void Update()
    {
        SimulateSwipes();
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartCoroutine(ProcessTouch());
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GameInputSubject.OnNext(InputType.Escape);
        }
    }

    private IEnumerator ProcessTouch() //przerobic na wylapywanie odleglosci swipea
    {
        Vector2 startPos = Input.GetTouch(0).position;
        float temp = swipeTreshold * swipeTreshold;
        while( (Input.GetTouch(0).position - startPos).sqrMagnitude < temp )
        {
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                GameInputSubject.OnNext(InputType.Tap);
                yield break;
            }
            yield return null;
        }

        bool inputRegistered = false;
        Vector2 line; 
        while(Input.GetTouch(0).phase != TouchPhase.Ended)
        {
            line = Input.GetTouch(0).position - startPos;
            if (line.sqrMagnitude < temp)
            {
                yield return null;
                continue;
            }
            if(Vector2.Angle(Vector2.right, line) < swipeSideAngle)
            {
                if (line.sqrMagnitude > 5 * temp)
                    GameInputSubject.OnNext(InputType.SuperSwipeRight);
                else
                    GameInputSubject.OnNext(InputType.SwipeRight);
                inputRegistered = true;
            }
            else if(Vector2.Angle(Vector2.left, line) < swipeSideAngle)
            {
                if (line.sqrMagnitude > 5 * temp)
                    GameInputSubject.OnNext(InputType.SuperSwipeLeft);
                else
                    GameInputSubject.OnNext(InputType.SwipeLeft);
                inputRegistered = true;
            }
            else if(line.y > 0)
            {
                GameInputSubject.OnNext(InputType.SwipeUp);
                inputRegistered = true;
            }
            yield return null;
        }
    }

    private void SimulateSwipes() //do testow z klawiatura
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GameInputSubject.OnNext(InputType.SwipeLeft);
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GameInputSubject.OnNext(InputType.SwipeRight);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            GameInputSubject.OnNext(InputType.SwipeUp);
        }
    }
}
