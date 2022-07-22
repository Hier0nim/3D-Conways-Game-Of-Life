using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the Game scene, and executes rules on the grid.
/// </summary>
public class Rules : Board
{
    
    private float timeGapBetweenGenerations;
    private bool areBordersConnected;
    private bool hasRunCoroutineFinished;
    public bool[] ctlToggles = new bool[26];
    public bool[] saToggles = new bool[26];
    public List<int> staysAliveRule = new List<int>();
    public List<int> comesToLifeRule = new List<int>();


    /// <summary>
    /// Called when the scene instance is being loaded.
    /// </summary>
    private void Awake()
    {
        LoadVariables();
        LoadRules();
        hasRunCoroutineFinished = true;

        if (!is3D)
        {
            gridDepth = 1;
        }

        if (Manager.Initialize(isDeadCellInvisible))
        {
            Manager.GameState = GameStateEnum.Wait;
        }
    }

    private void Start()
    {
        if (Manager.GameState == GameStateEnum.Invalid) return;
        gridOfCells = new Cell[gridWidth, gridHeight, gridDepth];
        PopulateGrid();

        CameraController.SetupCamera.Invoke(gridWidth, gridHeight, gridDepth, distanceMultiplier);
    }

    /// <summary>
    /// Method called every frame.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (Manager.GameState == GameStateEnum.AcceptInput && hasRunCoroutineFinished)
            {
                Manager.GameState = GameStateEnum.Run;
                hasRunCoroutineFinished = false;
                StartCoroutine(Run()); 
            }
            else if (Manager.GameState == GameStateEnum.Run)
            {
                Manager.GameState = GameStateEnum.AcceptInput;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().buildIndex == 2)
                SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Starts a Coroutine containing a game loop.
    /// </summary>
    private IEnumerator Run()
    {
        while (Manager.GameState == GameStateEnum.Run)
        {
            if (is3D) SearchForAlive3d();
            else SearchForAlive2d();
            yield return new WaitForSeconds(timeGapBetweenGenerations);
        }
        hasRunCoroutineFinished = true;
    }

    /// <summary>
    /// Loads Variables Florm PlayerPrefs, or sets the base values.
    /// </summary>
    private void LoadVariables()
    {
        gridWidth = PlayerPrefs.GetInt("width", 25); ;
        gridHeight = PlayerPrefs.GetInt("height", 25);
        gridDepth = PlayerPrefs.GetInt("depth", 25);
        timeGapBetweenGenerations = PlayerPrefs.GetFloat("generationGap", 0.2f); ;
        distanceMultiplier = PlayerPrefs.GetInt("distanceGap", 1);
        is3D = Convert.ToBoolean(PlayerPrefs.GetInt("dimensions", 1));
        areBordersConnected = Convert.ToBoolean(PlayerPrefs.GetInt("connectBorders", 0));
        isDeadCellInvisible = Convert.ToBoolean(PlayerPrefs.GetInt("isInvisible", 1));
        colorsRepresentingAge = Convert.ToBoolean(PlayerPrefs.GetInt("colorsOfAge", 1));

        string tempString;
        for (int i = 1; i <= 26; i++)
        {
            tempString = "saToggle" + i.ToString();
            saToggles[i - 1] = Convert.ToBoolean(PlayerPrefs.GetInt(tempString, 0));

            tempString = "ctlToggle" + i.ToString();
            ctlToggles[i - 1] = Convert.ToBoolean(PlayerPrefs.GetInt(tempString, 0));
        }
    }

    /// <summary>
    /// Loads Rules from bool array to List object as int.
    /// </summary>
    private void LoadRules()
    {
        staysAliveRule.Clear();
        comesToLifeRule.Clear();
        for (int i = 0; i < 26; i++)
        {
            if (saToggles[i] == true)
            {
                staysAliveRule.Add(i + 1);
            }
            if (ctlToggles[i] == true)
            {
                comesToLifeRule.Add(i + 1);
            }
        }
    }

    /// <summary>
    /// Method which iterates through the 2d grid searching for alive cells.
    /// Uses TaskParallelLibrary for Parallel.For usage.
    /// </summary>
    public void SearchForAlive2d()
    {
        Parallel.For(0, gridWidth, w =>
        {
            for (int h = 0; h < gridHeight; h++)
            {
                if (gridOfCells[w, h, 0].IsAlive)
                {
                    if (!gridOfCells[w, h, 0].isChecked)
                    {
                        FindNeighbours2d(w, h);
                    }
                }
            }
        });
        CopyFromNextGrid();
    }

    /// <summary>
    /// Method which iterates through the 3d grid searching for alive cells. 
    /// Uses TaskParallelLibrary for Parallel.For usage.
    /// Uses TaskParallelLibrary for Parallel.For usage.
    /// </summary>
    public void SearchForAlive3d()
    {
        Parallel.For(0, gridWidth, w =>
        {
            for (int h = 0; h < gridHeight; h++)
            {
               for(int d = 0; d < gridDepth; d++)
                {
                    if (gridOfCells[w, h, d].IsAlive)
                    {
                        if (!gridOfCells[w, h, d].isChecked)
                        {
                            FindNeighbours3d(w, h, d);
                        }
                    }
                }
            }
        });
        CopyFromNextGrid();
    }

    /// <summary>
    /// Finds nieghbours in 2d grid
    /// </summary>
    /// <param name="w">Width coordinate of cell.</param>
    /// <param name="h">Height coordinate of cell.</param>
    private void FindNeighbours2d(int w, int h)
    {
        for (int i = -1; i < 2; i++)
            for (int j = -1; j < 2; j++)
            {
                if (CheckIfInBorders(w + i, gridWidth) && CheckIfInBorders(h + j, gridHeight))
                    CheckAround2d(w + i, h + j);
                else if(areBordersConnected == true)
                {
                    CheckAround2d(WrapValue(w + i, gridWidth), WrapValue(h + j, gridHeight));
                }
            }
    }

    /// <summary>
    /// Finds nieghbours in 3d grid
    /// </summary>
    /// <param name="w">Width coordinate of cell.</param>
    /// <param name="h">Height coordinate of cell.</param>
    /// <param name="d">Depth coordinate of cell.</param>
    private void FindNeighbours3d(int w, int h, int d)
    {
        for (int i = -1; i < 2; i++)
            for (int j = -1; j < 2; j++)
                for (int k = -1; k < 2; k++)
                {
                    if (CheckIfInBorders(w + i, gridWidth) && CheckIfInBorders(h + j, gridHeight) && CheckIfInBorders(d + k, gridDepth))
                        CheckAround3d(w + i, h + j, d + k);
                    else if (areBordersConnected == true)
                    {
                        CheckAround3d(WrapValue(w + i, gridWidth), WrapValue(h + j, gridHeight), WrapValue(d + k, gridHeight));
                    }
                }
    }

    /// <summary>
    /// Applies rules to specified cell in 2 dimensions.
    /// </summary>
    /// <param name="w">Width coordinate of cell.</param>
    /// <param name="h">Height coordinate of cell.</param>
    private void CheckAround2d(int w, int h)
    {
        if (!gridOfCells[w, h, 0].isChecked)
        {
            gridOfCells[w, h, 0].isChecked = true;

            int neighborSum = 0;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    if (areBordersConnected == true)
                    {
                        int ww = WrapValue(w + i, gridWidth);
                        int hw = WrapValue(h + j, gridHeight);
                        neighborSum += Convert.ToInt32(gridOfCells[ww, hw, 0].IsAlive);
                    }
                    else
                    {
                        if (CheckIfInBorders(w + i, gridWidth) && CheckIfInBorders(h + j, gridHeight))
                            neighborSum += Convert.ToInt32(gridOfCells[w + i, h + j, 0].IsAlive);
                    }
                }
            neighborSum -= Convert.ToInt32(gridOfCells[w, h, 0].IsAlive);
            gridOfCells[w, h, 0].isNextAlive = gridOfCells[w, h, 0].IsAlive;
            if (gridOfCells[w, h, 0].isNextAlive == true && !staysAliveRule.Contains(neighborSum))
            {
                gridOfCells[w, h, 0].isNextAlive = false;
            }
            if (comesToLifeRule.Contains(neighborSum))
                gridOfCells[w, h, 0].isNextAlive = true;

        }
    }

    /// <summary>
    /// Applies rules to specified cell in 3 dimensions.
    /// </summary>
    /// <param name="w">Width coordinate of cell.</param>
    /// <param name="h">Height coordinate of cell.</param>
    /// <param name="d">Depth coordinate of cell.</param>
    private void CheckAround3d(int w, int h, int d)
    {
        if (!gridOfCells[w, h, d].isChecked)
        {
            gridOfCells[w, h, d].isChecked = true;

            int neighborSum = 0;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                    for (int k = -1; k < 2; k++)
                    {
                        if (areBordersConnected == true)
                        {
                            int ww = WrapValue(w + i, gridWidth);
                            int hw = WrapValue(h + j, gridHeight);
                            int dw = WrapValue(d + k, gridDepth);
                            neighborSum += Convert.ToInt32(gridOfCells[ww, hw, dw].IsAlive);
                        }
                        else
                        {
                            if (CheckIfInBorders(w + i, gridWidth) && CheckIfInBorders(h + j, gridHeight) && CheckIfInBorders(d + k, gridDepth))
                                neighborSum += Convert.ToInt32(gridOfCells[w + i, h + j, d + k].IsAlive);
                        }
                    }
            neighborSum -= Convert.ToInt32(gridOfCells[w, h, d].IsAlive);
            gridOfCells[w, h, d].isNextAlive = gridOfCells[w, h, d].IsAlive;
            if (gridOfCells[w, h, d].isNextAlive == true && !staysAliveRule.Contains(neighborSum))
            {
                gridOfCells[w, h, d].isNextAlive = false;
            }
            if (comesToLifeRule.Contains(neighborSum))
                gridOfCells[w, h, d].isNextAlive = true;

        }
    }

    /// <summary>
    /// Checks if value fits between borders.
    /// </summary>
    /// <param name="v">Cell coordinate.</param>
    /// <param name="vMax">Maximum possible cell coordinate.</param>
    /// <returns></returns>
    private bool CheckIfInBorders(int v, int vMax)
    {
        if (v == -1) return false;
        if (v == vMax) return false;
        return true;
    }

    /// <summary>
    /// If value does not fit in borders, returns the other end, 
    /// joining two end together.
    /// </summary>
    /// <param name="v">Cell coordinate.</param>
    /// <param name="vMax">Maximum possible cell coordinate.</param>
    /// <returns></returns>
    private int WrapValue(int v, int vMax)
    {
        if (v == -1) return vMax - 1;
        if (v == vMax) return 0;
        return v;
    }

}
