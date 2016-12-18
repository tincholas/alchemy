using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class ShapesManager : MonoBehaviour
{
    public Text DebugText, ScoreText;
    public bool ShowDebugInfo = false;
    //candy graphics taken from http://opengameart.org/content/candy-pack-1
    public List<LevelInfo> levels = new List<LevelInfo>();
    public ShapesArray shapes;
    int currentLevelIndex = 0;
    private int score;

    public Vector2 topLeft;// = new Vector2(-4.8f+.6f, -4.8f+.6f);
    public readonly Vector2 CandySize = new Vector2(1.25f, 1.25f);

    private GameState state = GameState.None;
    private GameObject hitGo = null;
    private Vector2[] SpawnPositions;
    public GameObject[] CandyPrefabs;
    public GameObject[] ExplosionPrefabs;
    //public GameObject[] BonusPrefabs;

    private IEnumerator CheckPotentialMatchesCoroutine;
    private IEnumerator AnimatePotentialMatchesCoroutine;
    /// <summary>
    /// References to where the corner cells are (which are automatically placed by the UI system)
    /// </summary>
    public GameObject topLeftCell, bottomRightCell;
    IEnumerable<GameObject> potentialMatches;

    public SoundManager soundManager;
    //Canvas canvas;
    void Awake()
    {
        DebugText.enabled = ShowDebugInfo;
    }

    // Use this for initialization
    void Start()
    {
        loadLevelInfo();
        topLeft = -3.5f * CandySize;
        InitializeTypesOnPrefabShapesAndBonuses();

        InitializeCandyAndSpawnPositions();

        StartCheckForPotentialMatches();

    }

    private void loadLevelInfo()
    {
        //level 1
        LevelInfo l = new LevelInfo();
        l.firstComplete = 8;
        l.lastSpawnable = 5;
        l.indexOfFirstElement = 0;
        l.levelCombos.Add(new comboInfo(0,1,8));
        l.levelCombos.Add(new comboInfo(2, 3, 6));
        l.levelCombos.Add(new comboInfo(3, 4, 7));
        l.levelCombos.Add(new comboInfo(2, 5, 9));
        l.levelCombos.Add(new comboInfo(6, 7, 10));
        levels.Add(l);
        LevelInfo l2=new LevelInfo();
        l2.firstComplete = 14;
        l2.lastSpawnable = 9;
        l2.indexOfFirstElement = 11;
        l2.levelCombos.Add(new comboInfo(0, 1, 10));
        l2.levelCombos.Add(new comboInfo(2, 10, 14));
        l2.levelCombos.Add(new comboInfo(3, 10, 11));
        l2.levelCombos.Add(new comboInfo(4, 5, 12));
        l2.levelCombos.Add(new comboInfo(6, 12, 15));
        l2.levelCombos.Add(new comboInfo(7, 8, 13));
        l2.levelCombos.Add(new comboInfo(5, 13, 16));
        l2.levelCombos.Add(new comboInfo(9, 11, 17));
        l2.levelCombos.Add(new comboInfo(0, 3, 11));
        levels.Add(l2);
        LevelInfo l3 = new LevelInfo();
        l3.firstComplete = 11;
        l3.lastSpawnable = 7;
        l3.indexOfFirstElement = 29;
        l3.levelCombos.Add(new comboInfo(0, 1, 11));
        l3.levelCombos.Add(new comboInfo(2, 3, 12));
        l3.levelCombos.Add(new comboInfo(2, 4, 12));
        l3.levelCombos.Add(new comboInfo(3, 5, 8));
        l3.levelCombos.Add(new comboInfo(3, 6, 8));
        l3.levelCombos.Add(new comboInfo(4, 6, 13));
        l3.levelCombos.Add(new comboInfo(4, 5, 13));
        l3.levelCombos.Add(new comboInfo(1, 7, 9));
        l3.levelCombos.Add(new comboInfo(6, 8, 10));
        l3.levelCombos.Add(new comboInfo(5, 8, 10));
        l3.levelCombos.Add(new comboInfo(2, 8, 14));
        l3.levelCombos.Add(new comboInfo(8, 9, 15));
        l3.levelCombos.Add(new comboInfo(3, 9, 15));
        l3.levelCombos.Add(new comboInfo(9, 10, 15));
        levels.Add(l3);
    }

    /// <summary>
    /// Initialize shapes
    /// </summary>
    private void InitializeTypesOnPrefabShapesAndBonuses()
    {
        //just assign the name of the prefab
        //foreach (var item in CandyPrefabs)

        ///Tincho - check if index is set somewhere else
        /*for (int i=0;i<CandyPrefabs.Length;i++)
        {
            CandyPrefabs[i].GetComponent<Shape>().Type = i;

        }*/

        ////assign the name of the respective "normal" candy as the type of the Bonus
        //foreach (var item in BonusPrefabs)
        //{
        //    //item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
        //    item.GetComponent<Shape>().Type = CandyPrefabs.
        //       Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
        //}
    }

    public void InitializeCandyAndSpawnPositionsFromPremadeLevel()
    {
        InitializeVariables();

        var premadeLevel = DebugUtilities.FillShapesArrayFromResourcesData();

        if (shapes != null)
            DestroyAllCandy();

        shapes = new ShapesArray();
        SpawnPositions = new Vector2[Constants.Columns];

        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {

                GameObject newCandy = null;

                newCandy = GetSpecificCandyOrBonusForPremadeLevel(premadeLevel[row, column]);

                InstantiateAndPlaceNewCandy(row, column, newCandy);

            }
        }

        SetupSpawnPositions();
    }


    public void InitializeCandyAndSpawnPositions()
    {
        InitializeVariables();

        if (shapes != null)
            DestroyAllCandy();

        shapes = new ShapesArray();
        SpawnPositions = new Vector2[Constants.Columns];

        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {

                GameObject newCandy = GetRandomCandy();

                //check if two previous horizontal are of the same type
                while (column >= 2 && shapes[row, column - 1].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row, column - 2].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                //check if two previous vertical are of the same type
                while (row >= 2 && shapes[row - 1, column].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row - 2, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                InstantiateAndPlaceNewCandy(row, column, newCandy);

            }
        }

        SetupSpawnPositions();
    }



    private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy)
    {
        Debug.Log(CandySize);
        float delta = Screen.height / 8;
        Debug.Log(delta);
        float x1 = topLeftCell.transform.position.x;
        float x2 = bottomRightCell.transform.position.x;
        Debug.Log("the calculated cell size from topleft-bottomright is" +(x1-x2)/8);
        GameObject go = Instantiate(newCandy,
            topLeft + new Vector2(column * CandySize.x, row * CandySize.y), Quaternion.identity)
            as GameObject;

        //assign the specific properties
        go.GetComponent<Shape>().Assign(newCandy.GetComponent<Shape>().Type, row, column);
        shapes[row, column] = go;
    }

    private void SetupSpawnPositions()
    {
        //Canvas.ForceUpdateCanvases();

        //Debug.Log("top left is " + RectTransformUtility.WorldToScreenPoint(null, topLeftCell.GetComponent<RectTransform>().position) + " and bottom right is " + RectTransformUtility.WorldToScreenPoint(null, bottomRightCell.GetComponent<RectTransform>().position));
        //Debug.Log("another way is " + topLeftCell.transform.position);

        //create the spawn positions for the new shapes (will pop from the 'ceiling')
        //Canvas.ForceUpdateCanvases();
        //float delta = Mathf.Min(Screen.height,Screen.width) / Constants.Columns;
        //topLeft = new Vector2(-Mathf.Min(Screen.height, Screen.width)/100, -Mathf.Min(Screen.height, Screen.width)/100);
        //CandySize = new Vector2(delta/100,delta/100);
        //topLeft = new Vector2(-Mathf.Min(Screen.height, Screen.width)/10, -Mathf.Min(Screen.height, Screen.width)/10)+CandySize/2;
        
        Debug.Log("topleft is " + topLeft);
        Debug.Log("candysize is " + CandySize);
        for (int column = 0; column < Constants.Columns; column++)
        {
            SpawnPositions[column] = topLeft
                + new Vector2(column * CandySize.x, Constants.Rows * CandySize.y);
        }
    }




    /// <summary>
    /// Destroy all candy gameobjects
    /// </summary>
    private void DestroyAllCandy()
    {
        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {
                Destroy(shapes[row, column]);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (ShowDebugInfo)
            DebugText.text = DebugUtilities.GetArrayContents(shapes);

        if (state == GameState.None)
        {
            //user has clicked or touched
            if (Input.GetMouseButtonDown(0))
            {
                //get the hit position
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null) //we have a hit!!!
                {
                    hitGo = hit.collider.gameObject;
                    state = GameState.SelectionStarted;
                }
                
            }
        }
        else if (state == GameState.SelectionStarted)
        {
            //user dragged
            if (Input.GetMouseButton(0))
            {
                

                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                //we have a hit
                if (hit.collider != null && hitGo != hit.collider.gameObject)
                {

                    //user did a hit, no need to show him hints 
                    StopCheckForPotentialMatches();

                    //if the two shapes are diagonally aligned (different row and column), just return
                    if (!Utilities.AreVerticalOrHorizontalNeighbors(hitGo.GetComponent<Shape>(),
                        hit.collider.gameObject.GetComponent<Shape>()))
                    {
                        state = GameState.None;
                    }
                    else
                    {
                        state = GameState.Animating;
                        FixSortingLayer(hitGo, hit.collider.gameObject);
                        StartCoroutine(FindMatchesAndCollapse(hit));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Modifies sorting layers for better appearance when dragging/animating
    /// </summary>
    /// <param name="hitGo"></param>
    /// <param name="hitGo2"></param>
    private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        SpriteRenderer sp1 = hitGo.GetComponent<SpriteRenderer>();
        SpriteRenderer sp2 = hitGo2.GetComponent<SpriteRenderer>();
        if (sp1.sortingOrder <= sp2.sortingOrder)
        {
            sp1.sortingOrder = 1;
            sp2.sortingOrder = 0;
        }
    }

    public int getCombo(GameObject GOa, GameObject GOb)
    {
        int a, b;
        a = GOa.GetComponent<Shape>().Type;
        b = GOb.GetComponent<Shape>().Type;
        if (a > b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
        for (int i = 0; i < levels[currentLevelIndex].levelCombos.Count; i++)
        {
            if (a == levels[currentLevelIndex].levelCombos[i].firstItem && b == levels[currentLevelIndex].levelCombos[i].secondItem)
            {
                return levels[currentLevelIndex].levelCombos[i].result;
            }
        }
        
        return -1;

    }


    private IEnumerator FindMatchesAndCollapse(RaycastHit2D hit2)
    {

        Vector3 t = hitGo.transform.position ;
        
        //get the second item that was part of the swipe
        var hitGo2 = hit2.collider.gameObject;
        //shapes.Swap(hitGo, hitGo2);

        //move the swapped ones
        hitGo.transform.positionTo(Constants.AnimationDuration, hitGo2.transform.position);
        //hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
        yield return new WaitForSeconds(Constants.AnimationDuration);
        Debug.Log("pos=" + t);

        int combo = getCombo(hitGo, hitGo2);
        if (combo < 0)
        {

            Debug.Log("moving from pos="+hitGo.transform.position+ "to pos=" + t);
            hitGo.transform.positionTo(Constants.AnimationDuration, t);
           // hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
            yield return new WaitForSeconds(Constants.AnimationDuration);
            Debug.Log("combo not recognized, type1 is"+hitGo.GetComponent<Shape>().Type+" type2 is "+ hitGo2.GetComponent<Shape>().Type);

        }
        else
        {
            GameObject newCandy = Instantiate(CandyPrefabs[levels[currentLevelIndex].indexOfFirstElement + combo],hitGo2.transform.position, Quaternion.identity)
                    as GameObject;
            newCandy.GetComponent<Shape>().Row = hitGo2.GetComponent<Shape>().Row;
            newCandy.GetComponent<Shape>().Column = hitGo2.GetComponent<Shape>().Column;
            newCandy.GetComponent<Shape>().Type = combo;
            shapes.Remove(hitGo);
            RemoveFromScene(hitGo);

            ///If this is a last item, remove it from the board and score
            if (combo >= levels[currentLevelIndex].firstComplete)
            {
                shapes.Remove(hitGo2);
                //RemoveFromScene(newCandy);
                newCandy.GetComponent<Shape>().eject();
                score += 5;
            }
            else
            shapes.setShape(newCandy);
            //shapes.Remove(hitGo2);
            RemoveFromScene(hitGo2);
            //hitGo2.GetComponent<Sprite>().texture = CandyPrefabs[combo].GetComponent<Sprite>().texture;
            //shapes.changeTo(newCandy, combo);



            //var collapsedCandyInfo = shapes.Collapse(hitGo.GetComponent<Shape>().Column);
            var collapsedCandyInfo = shapes.Collapse();
            
            var newCandyInfo = CreateNewCandyInSpecificColumns();

            int maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

            MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
            MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);



            //will wait for both of the above animations
            yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);
            var matches=shapes.getMatches();
            foreach (GameObject go in matches)
            {
                shapes.Remove(go);
                RemoveFromScene(go);

            }
            //var collapsedCandyInfo = shapes.Collapse(hitGo.GetComponent<Shape>().Column);
             collapsedCandyInfo = shapes.Collapse();

             newCandyInfo = CreateNewCandyInSpecificColumns();

             maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

            MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
            MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);
        }
        /*
        //get the matches via the helper methods
        var hitGomatchesInfo = shapes.GetMatches(hitGo);
        var hitGo2matchesInfo = shapes.GetMatches(hitGo2);

        var totalMatches = hitGomatchesInfo.MatchedCandy
            .Union(hitGo2matchesInfo.MatchedCandy).Distinct();

        //if user's swap didn't create at least a 3-match, undo their swap
        if (totalMatches.Count() < Constants.MinimumMatches)
        {
            hitGo.transform.positionTo(Constants.AnimationDuration, hitGo2.transform.position);
            hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
            yield return new WaitForSeconds(Constants.AnimationDuration);

            shapes.UndoSwap();
        }

        //if more than 3 matches and no Bonus is contained in the line, we will award a new Bonus
        bool addBonus = totalMatches.Count() >= Constants.MinimumMatchesForBonus &&
            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGomatchesInfo.BonusesContained) &&
            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGo2matchesInfo.BonusesContained);

        Shape hitGoCache = null;
        if (addBonus)
        {
            //get the game object that was of the same type
            var sameTypeGo = hitGomatchesInfo.MatchedCandy.Count() > 0 ? hitGo : hitGo2;
            hitGoCache = sameTypeGo.GetComponent<Shape>();
        }

        int timesRun = 1;
        while (totalMatches.Count() >= Constants.MinimumMatches)
        {
            //increase score
            IncreaseScore((totalMatches.Count() - 2) * Constants.Match3Score);

            if (timesRun >= 2)
                IncreaseScore(Constants.SubsequentMatchScore);

            soundManager.PlayCrincle();

            foreach (var item in totalMatches)
            {
                shapes.Remove(item);
                RemoveFromScene(item);
            }

  
        
            addBonus = false;
            
        //get the columns that we had a collapse
        var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

            //the order the 2 methods below get called is important!!!
            //collapse the ones gone
            var collapsedCandyInfo = shapes.Collapse(columns);
            //create new ones
            var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

            int maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

            MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
            MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);



            //will wait for both of the above animations
            yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

            //search if there are matches with the new/collapsed items
            totalMatches = shapes.GetMatches(collapsedCandyInfo.AlteredCandy).
                Union(shapes.GetMatches(newCandyInfo.AlteredCandy)).Distinct();



            timesRun++;
        }
        */
        state = GameState.None;
        StartCheckForPotentialMatches();
        
    }





    /// <summary>
    /// Spawns new candy in columns that have missing ones
    /// </summary>
    /// <param name="columnsWithMissingCandy"></param>
    /// <returns>Info about new candies created</returns>
    private AlteredCandyInfo CreateNewCandyInSpecificColumns(IEnumerable<int> columnsWithMissingCandy)
    {
        AlteredCandyInfo newCandyInfo = new AlteredCandyInfo();

        //find how many null values the column has
        foreach (int column in columnsWithMissingCandy)
        {
            var emptyItems = shapes.GetEmptyItemsOnColumn(column);
            foreach (var item in emptyItems)
            {
                var go = GetRandomCandy();
                GameObject newCandy = Instantiate(go, SpawnPositions[column], Quaternion.identity)
                    as GameObject;

                newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

                if (Constants.Rows - item.Row > newCandyInfo.MaxDistance)
                    newCandyInfo.MaxDistance = Constants.Rows - item.Row;

                shapes[item.Row, item.Column] = newCandy;
                newCandyInfo.AddCandy(newCandy);
            }
        }
        return newCandyInfo;
    }

    /// <summary>
    /// Spawns new candy in columns that have missing ones
    /// </summary>
    /// <param name="columnsWithMissingCandy"></param>
    /// <returns>Info about new candies created</returns>
    private AlteredCandyInfo CreateNewCandyInSpecificColumns()
    {
        AlteredCandyInfo newCandyInfo = new AlteredCandyInfo();

        //find how many null values the column has
        for (int column=0;column<Constants.Columns;column++)
        {
            var emptyItems = shapes.GetEmptyItemsOnColumn(column);
            foreach (var item in emptyItems)
            {
                var go = GetRandomCandy();
                GameObject newCandy = Instantiate(go, SpawnPositions[column], Quaternion.identity)
                    as GameObject;

                newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

                if (Constants.Rows - item.Row > newCandyInfo.MaxDistance)
                    newCandyInfo.MaxDistance = Constants.Rows - item.Row;

                shapes[item.Row, item.Column] = newCandy;
                newCandyInfo.AddCandy(newCandy);
            }
        }
        return newCandyInfo;
    }

    /// <summary>
    /// Spawns new candy in columns that have missing ones
    /// </summary>
    /// <param name="columnsWithMissingCandy"></param>
    /// <returns>Info about new candies created</returns>
    private AlteredCandyInfo CreateNewCandyInSpecificColumns(int column)
    {
        AlteredCandyInfo newCandyInfo = new AlteredCandyInfo();

        
            var emptyItems = shapes.GetEmptyItemsOnColumn(column);
            foreach (var item in emptyItems)
            {
                var go = GetRandomCandy();
                GameObject newCandy = Instantiate(go, SpawnPositions[column], Quaternion.identity)
                    as GameObject;

                newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

                if (Constants.Rows - item.Row > newCandyInfo.MaxDistance)
                    newCandyInfo.MaxDistance = Constants.Rows - item.Row;

                shapes[item.Row, item.Column] = newCandy;
                newCandyInfo.AddCandy(newCandy);
            }
        
        return newCandyInfo;
    }

    /// <summary>
    /// Animates gameobjects to their new position
    /// </summary>
    /// <param name="movedGameObjects"></param>
    private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects, int distance)
    {
        foreach (var item in movedGameObjects)
        {
            item.transform.positionTo(Constants.MoveAnimationMinDuration * distance, topLeft +
                new Vector2(item.GetComponent<Shape>().Column * CandySize.x, item.GetComponent<Shape>().Row * CandySize.y));
        }
    }

    /// <summary>
    /// Destroys the item from the scene and instantiates a new explosion gameobject
    /// </summary>
    /// <param name="item"></param>
    private void RemoveFromScene(GameObject item)
    {
        GameObject explosion = GetRandomExplosion();
        var newExplosion = Instantiate(explosion, item.transform.position, Quaternion.identity) as GameObject;
        Destroy(newExplosion, Constants.ExplosionDuration);
        Destroy(item);
    }

    /// <summary>
    /// Get a random candy from the first item in this level to the last spawnable item in this level
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomCandy()
    {
        Debug.Log("current level is " + currentLevelIndex);
        Debug.Log(" and index of first element is " + levels[currentLevelIndex].indexOfFirstElement);
        int firstIndex = levels[currentLevelIndex].indexOfFirstElement;
        int secondIndex = levels[currentLevelIndex].indexOfFirstElement + levels[currentLevelIndex].lastSpawnable;
        Debug.Log("" + firstIndex + " " + secondIndex);
        int indexofPrefabToReturn = UnityEngine.Random.Range(firstIndex, secondIndex + 1);
        CandyPrefabs[indexofPrefabToReturn].GetComponent<Shape>().Type= indexofPrefabToReturn - firstIndex;
        return CandyPrefabs[indexofPrefabToReturn];
    }

    private void InitializeVariables()
    {
        score = 0;
        ShowScore();
    }

    private void IncreaseScore(int amount)
    {
        score += amount;
        ShowScore();
    }

    private void ShowScore()
    {
        ScoreText.text = "Score: " + score.ToString();
    }

    /// <summary>
    /// Get a random explosion
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomExplosion()
    {
        return ExplosionPrefabs[UnityEngine.Random.Range(0, ExplosionPrefabs.Length)];
    }

   
    /// <summary>
    /// Starts the coroutines, keeping a reference to stop later
    /// </summary>
    private void StartCheckForPotentialMatches()
    {
        StopCheckForPotentialMatches();
        //get a reference to stop it later
        CheckPotentialMatchesCoroutine = CheckPotentialMatches();
        StartCoroutine(CheckPotentialMatchesCoroutine);
    }

    /// <summary>
    /// Stops the coroutines
    /// </summary>
    private void StopCheckForPotentialMatches()
    {
        if (AnimatePotentialMatchesCoroutine != null)
            StopCoroutine(AnimatePotentialMatchesCoroutine);
        if (CheckPotentialMatchesCoroutine != null)
            StopCoroutine(CheckPotentialMatchesCoroutine);
        ResetOpacityOnPotentialMatches();
    }

    /// <summary>
    /// Resets the opacity on potential matches (probably user dragged something?)
    /// </summary>
    private void ResetOpacityOnPotentialMatches()
    {
        if (potentialMatches != null)
            foreach (var item in potentialMatches)
            {
                if (item == null) break;

                Color c = item.GetComponent<SpriteRenderer>().color;
                c.a = 1.0f;
                item.GetComponent<SpriteRenderer>().color = c;
            }
    }
    List<GameObject> GetPotentialMatches()
    {
        List<GameObject> hints = new List<GameObject>();
        for (int i = 0; i < Constants.Columns-1; i++)
            for (int j = 0; j < Constants.Rows-1; j++)
            {
                GameObject go=null, toRight = null, toBottom = null, toBottomRight = null,toBottomLeft=null;
                go = shapes[i, j];
                if (i+1<Constants.Columns)
                    toRight = shapes[i+1, j];
                if (j + 1 < Constants.Rows)
                    toBottom = shapes[i, j+1];
                if (i + 1 < Constants.Columns && j+1<Constants.Rows)
                    toBottomRight = shapes[i + 1, j + 1];
                if (i - 1 >=0 && j + 1 < Constants.Rows)
                    toBottomRight = shapes[i - 1, j + 1];
                if (toRight != null && getCombo(go, toRight) >= 0)
                {
                    hints.Add(go);
                    hints.Add(toRight);
                    return hints;
                }
                else if (toBottom != null && getCombo(go, toBottom) >= 0)
                {
                    hints.Add(go);
                    hints.Add(toBottom);
                    return hints;
                }
                else if (toBottomRight != null && getCombo(go, toBottomRight) >= 0)
                {
                    hints.Add(go);
                    hints.Add(toBottomRight);
                    return hints;
                }
                else if (toBottomLeft != null && getCombo(go, toBottomLeft) >= 0)
                {
                    hints.Add(go);
                    hints.Add(toBottomLeft);
                    return hints;
                }

            }
        for (int i = 0; i < Constants.Columns; i++)
            for (int j = 0; j < Constants.Rows; j++)
            {
                RemoveFromScene(shapes[i, j]);
            }
        InitializeCandyAndSpawnPositions();
        return hints;
    }
    /// <summary>
    /// Finds potential matches
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckPotentialMatches()
    {
        yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
        potentialMatches = GetPotentialMatches();
        if (potentialMatches != null)
        {
            while (true)
            {

                AnimatePotentialMatchesCoroutine = Utilities.AnimatePotentialMatches(potentialMatches);
                StartCoroutine(AnimatePotentialMatchesCoroutine);
                yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
            }
        }
    }

    /// <summary>
    /// Gets a specific candy or Bonus based on the premade level information.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private GameObject GetSpecificCandyOrBonusForPremadeLevel(string info)
    {
        var tokens = info.Split('_');

        if (tokens.Count() == 1)
        {
            foreach (var item in CandyPrefabs)
            {
                //if (item.GetComponent<Shape>().Type.Contains(tokens[0].Trim()))
                    return item;
            }

        }
        

        throw new System.Exception("Wrong type, check your premade level");
    }



}
