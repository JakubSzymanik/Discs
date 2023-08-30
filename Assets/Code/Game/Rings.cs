using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class Rings : MonoBehaviour {

    //inpsector variables
    [SerializeField] public float animationSpeed;

    //streams
    public IObservable<MatchData[]> MatchesStream { get { return MatchesSubject; } }
    private Subject<MatchData[]> MatchesSubject = new Subject<MatchData[]>();
    public IObservable<Unit> LoseStream { get { return LoseSubject; } }
    private Subject<Unit> LoseSubject = new Subject<Unit>();

    //reference
    [SerializeField] private InsertBlock insertBlock;
    [SerializeField] private RingsGenerator generator;

    //fields
    private Block[,] board;
    private int chosenIndex = 0; //board index currently on top
    private bool matchesFound; //used for match coroutine

    //public methods
    public void CreateRings()
    {
        board = new Block[3, 12];
        generator.CreateRings(1, ref board, this.transform);
        matchesFound = false;
    }

    //public coroutines
    public IEnumerator Rotate(bool left)
    {
        chosenIndex = left ? Loop(chosenIndex + 1) : Loop(chosenIndex - 1);
        Quaternion targetRot = transform.rotation * (left ? Quaternion.Euler(0, 0, 30) : Quaternion.Euler(0, 0, -30));
        while(transform.rotation != targetRot) //mozna uzyc lepszej metody do sprawdzenia
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, animationSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;
    }

    public IEnumerator InsertIn()
    {
        //check if you can insert
        bool isFull = true;
        for(int i = 0; i < 3; i++)
        {
            if (board[i, chosenIndex] == null)
                isFull = false;
        }
        if(isFull)
        {
            yield break;
        }

        //spawn block to insert
        Block inserted = insertBlock.Insert(); //tu dostajemy gotowy klocek do wstawienia
        if (inserted == null)
            yield break;
        inserted.transform.SetParent(this.transform);
        yield return StartCoroutine(inserted.InsertIn());
        
        //after inserted block reached ring 0
        //move other blocks
        yield return StartCoroutine(MoveUp(inserted));

        //Handle matches and falling
        yield return StartCoroutine(Match());
        yield return StartCoroutine(Fall());
        while(matchesFound)
        {
            yield return StartCoroutine(Match());
            yield return StartCoroutine(Fall());
        }

        //check if lost
        SendLoseInfo();
    }

    public void InsertOut()
    {
        //check if lost - można by sprawdzać po spadnieciu klocka
        if (SendLoseInfo())
            return;

        //chosing place to insert
        List<int> indexes = new List<int>();
        int height = 0;
        bool found = false;
        for(int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                if (j == chosenIndex)
                    continue;
                if(board[i,j] == null)
                {
                    found = true;
                    height = i;
                    indexes.Add(j);
                }
            }

            if (found)
                break;
        }
        if(!found)
        {
            for(int i = 0; i < 3; i++)
            {
                if(board[i,chosenIndex] == null)
                {
                    found = true;
                    height = i;
                    indexes.Add(chosenIndex);
                }
                if (found)
                    break;
            }
        }
        int rolledIndex = indexes[Random.Range(0, indexes.Count)];

        //deciding what to insert
        List<int> excluded = new List<int>();
        Block block;
            //check blocks around chosen point
        if (height > 0 && board[height - 1,rolledIndex] != null)
        {
            block = board[height - 1, rolledIndex]; ;
            excluded.Add(block.GetType() == typeof(Gold) ? 0 : (block as Basic).type + 1);
        }
        if( ( block = board[height, Loop(rolledIndex - 1)] ) != null )
        {
            excluded.Add(block.GetType() == typeof(Gold) ? 0 : (block as Basic).type + 1);
        }
        if ((block = board[height, Loop(rolledIndex + 1)]) != null)
        {
            excluded.Add(block.GetType() == typeof(Gold) ? 0 : (block as Basic).type + 1);
        }
            //roll symbol excluding symbols around chosen point
        int inSymbol = RandomExclude(5, excluded.ToArray()) - 1;

        //insert the block!
        block = BlockCreator.Creator.Spawn(inSymbol, height, this.transform);
        block.transform.localRotation = Quaternion.Euler(0, 0, rolledIndex * -30);
        StartCoroutine(block.InsertOut());
        board[height, rolledIndex] = block;
    }

    //private animation coroutines
    IEnumerator Match()
    {
        List<List<Point>> matches = new List<List<Point>>();
        //check if there are matches, and save all points to list
        HandleMatches(ref matches);
        if (matches.Count <= 0)
        {
            matchesFound = false;
            yield break;
        }
        matchesFound = true;

        //send info about matches to points stream
        List<MatchData> matchData = new List<MatchData>();
        for (int i = 0; i < matches.Count; i++)
        {
            //create match data
            MatchData temp = new MatchData();
            temp.matchedCount = matches[i].Count;
            temp.isGold = board[matches[i][0].x, matches[i][0].y].GetType() == typeof(Gold);
            GetMidPoint(matches[i], ref temp.midPoint, ref temp.midRotation);
            temp.matchColor = board[matches[i][0].x, matches[i][0].y].GetType() != typeof(Gold) ?
                board[matches[i][0].x, matches[i][0].y].transform.GetChild(0).GetComponent<SpriteRenderer>().color :
                Overlord.GoldSkin.SymbolColor;
            matchData.Add(temp);
        }
        MatchesSubject.OnNext(matchData.ToArray());

        //launch animations and wait for completion
        yield return StartCoroutine(MatchAnimation(matches));
    }

    IEnumerator MatchAnimation(List<List<Point>> matches)
    {
        //start animations in all matched blocks, and save their references to list
        List<Block> matchedBlocks = new List<Block>();
        for (int i = 0; i < matches.Count; i++)
        {
            int strength = matches[i].Count;
            for (int j = 0; j < matches[i].Count; j++)
            {
                int x = matches[i][j].x;
                int y = matches[i][j].y;
                matchedBlocks.Add(board[x, y]);
                if(!board[x,y].isAnimating)
                    StartCoroutine(board[x, y].Match(strength));
            }
        }

        //wait for animations to complete
        bool areAnimating = true;
        while (areAnimating)
        {
            areAnimating = false;
            for (int i = 0; i < matchedBlocks.Count; i++)
            {
                if (matchedBlocks[i] != null && matchedBlocks[i].isAnimating)
                    areAnimating = true;
            }
            yield return null;
        }
    }

    IEnumerator MoveUp(Block inserted)
    {
        List<Block> movedUp = new List<Block>();
        for (int i = 2; i >= 0; i--)
        {
            if (board[i, chosenIndex] == null && i - 1 >= 0 && board[i - 1, chosenIndex] != null)
            {
                board[i, chosenIndex] = board[i - 1, chosenIndex];
                board[i - 1, chosenIndex] = null;
                StartCoroutine(board[i, chosenIndex].Move(1));
                movedUp.Add(board[i, chosenIndex]);
            }
        }
        board[0, chosenIndex] = inserted;
        //wait for animations to finish
        bool areAnimating = true;
        while (areAnimating)
        {
            areAnimating = false;
            for (int i = 0; i < movedUp.Count; i++)
            {
                if (movedUp[i].isAnimating)
                    areAnimating = true;
            }
            yield return null;
        }
    }

    IEnumerator Fall()
    {
        List<Block> falling = new List<Block>();
        for(int i = 1; i < 3; i++)
        {
            for(int j = 0; j < 12; j++)
            {
                if (board[i, j] == null)
                    continue;
                //if theres one empty space below the block
                if(board[i-1,j] == null)
                {
                    falling.Add(board[i, j]);
                    //check if there are two empty spaces
                    if(i - 2 >= 0 && board[i-2,j] == null)
                    {
                        //when there are
                        board[i - 2, j] = board[i, j];
                        StartCoroutine( board[i - 2, j].Move(-2) );
                    }
                    else 
                    {
                        //if not handle one empty space
                        board[i - 1, j] = board[i, j];
                        StartCoroutine(board[i - 1, j].Move(-1));
                    }
                    board[i, j] = null;
                }
            }
        }

        //wait for animations to complete
        bool areAnimating = true;
        while (areAnimating)
        {
            areAnimating = false;
            for (int i = 0; i < falling.Count; i++)
            {
                if (falling[i].isAnimating)
                    areAnimating = true;
            }
            yield return null;
        }
    }

    //private methods
    bool SendLoseInfo()
    {
        bool isLost = true;
        for(int i = 0; i < 3; i++)
            for(int j = 0; j < 12; j++)
            {
                if (board[i, j] == null)
                    isLost = false;
            }
        if (isLost)
            LoseSubject.OnNext(new Unit());
        return isLost;
    }

    void HandleMatches(ref List<List<Point>> matches) //troche chujówka ale niech zostanie
    {
        //transform board to ints
        int[,] boardInt = new int[3, 12];
        for(int i = 0; i < 3; i++)
            for(int j = 0; j < 12; j++)
            {
                if (board[i, j] == null)
                {
                    boardInt[i, j] = -2;
                }
                else if(board[i,j].GetType() == typeof(Gold))
                {
                    boardInt[i, j] = -1;
                }
                else
                {
                    var temp = (Basic)board[i, j];
                    boardInt[i, j] = temp.type;
                }
            }

        //find horizontal matches
        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 12; j++)
            {
                //for each block: 
                if (boardInt[i,j] == -2 || board[i, j].isAnimating ||  Contains(matches, new Point(i, j))) //if empty or already included in 'matches' - skip
                    continue;

                List<Point> currentMatch = new List<Point>();
                currentMatch.Add(new Point(i, j));

                int val = boardInt[i, j];
                for (int k = 1; boardInt[i, Loop(j + k)] == val; k++) //check points to right
                {
                    currentMatch.Add(new Point(i, Loop(j + k)));
                }
                for (int k = -1; boardInt[i, Loop(j + k)] == val; k--) //check points to right
                {
                    currentMatch.Add(new Point(i, Loop(j + k)));
                }

                if (currentMatch.Count >= 3)
                    matches.Add(currentMatch);
            }

        }
        
        //find vertical matches
        for(int i = 0; i < 12; i ++)
        {
            if(boardInt[2,i] != -2 && boardInt[0,i] == boardInt[1,i] && boardInt[1,i] == boardInt[2,i])
            {
                matches.Add(new List<Point>() { new Point(0, i), new Point(1, i), new Point(2, i) });
            }
        }
    }

    bool Contains(List<List<Point>> matches, Point point)
    {
        for(int i = 0; i < matches.Count; i++)
        {
            for(int j = 0; j < matches[i].Count; j++)
            {
                if (matches[i][j] == point)
                    return true;
            }
        }
        return false;
    }

    int Loop(int index)
    {
        if (index < 0)
            return 12 + index;
        else if (index > 11)
            return index - 12;
        else
            return index;
    }

    int RandomExclude(int range, int[] exclude) //losowa liczba od 0 do range bez liczb w exluce
    {
        int[] arr = new int[range - exclude.Length];
        for (int i = 0, j = 0; i < arr.Length; i++, j++)
        {
            while (Contains(exclude, j))
            {
                j++;
            }
            arr[i] = j;
        }
        return arr[Random.Range(0, arr.Length)];
    }

    bool Contains(int[] arr, int num) //czy arr zawiera num?
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == num)
                return true;
        }
        return false;
    }

    public void GetMidPoint(List<Point> points, ref Vector2 midPoint, ref Quaternion midRotation)
    {
        List<int> baseArr = new List<int>();
        for(int i = 0; i < points.Count; i++)
        {
            baseArr.Add(points[i].y);
        }

        List<int> resultLeft = new List<int>();
        List<int> resultRight = new List<int>();
        bool right = true;
        resultRight.Add(baseArr.Min());
        baseArr.Remove(baseArr.Min());
        while (baseArr.Count > 0)
        {
            if (baseArr.Min() > resultRight.Max() + 1)
                right = false;
            
            if(right)
            {
                resultRight.Add(baseArr.Min());
                baseArr.Remove(baseArr.Min());
            }
            else
            {
                resultLeft.Add(baseArr.Min());
                baseArr.Remove(baseArr.Min());
            }
        }

        baseArr.AddRange(resultLeft);
        baseArr.AddRange(resultRight);

        int midIndex = baseArr.Count / 2;
        int height = points[0].x;
        if(baseArr.Count % 2 == 0)
        {
            Transform midTrans1 = board[height, baseArr[midIndex]].transform.GetChild(2);
            Transform midTrans2 = board[height, baseArr[midIndex - 1]].transform.GetChild(2);
            midPoint = (midTrans1.position + midTrans2.position) / 2;
            midRotation = Quaternion.Lerp(midTrans1.rotation, midTrans2.rotation, 0.5f);
        }
        else
        {
            Transform midTrans = board[height, baseArr[midIndex]].transform.GetChild(2);
            midPoint = midTrans.position;
            midRotation = midTrans.rotation;
        }
    }
}
