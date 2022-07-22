using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Scripts;
using System.Threading.Tasks;


/// <summary>
/// Stores the grid of cells, its values.
/// Handles cells initialization and updates.
/// </summary>
public class Board : Cell
{
    protected int gridWidth;
    protected int gridHeight;
    protected int gridDepth;
    protected Cell[,,] gridOfCells;
    protected bool is3D;
    protected bool colorsRepresentingAge;
    protected int distanceMultiplier;
    protected bool isDeadCellInvisible;
    private static readonly Vector3 CellScale = Vector3.one * 0.8f;
    
    /// <summary>
    /// Copies bool value from isNextalive to iAlive Cell field and unloads unused assets.
    /// </summary>
    protected void CopyFromNextGrid()
    {
        for (int x = 0; x < gridHeight; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    gridOfCells[x, y, z].IsAlive = gridOfCells[x, y, z].isNextAlive;
                    gridOfCells[x, y, z].isChecked = false;
                }
            }
        }
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// Initialises the grid with randomised cells. 
    /// </summary>
    protected void PopulateGrid()
    {
        var offset = new Vector3Int
        {
            x = gridWidth - Mathf.FloorToInt(0.5f * (gridWidth - 1) + 1.0f),
            y = gridHeight - Mathf.FloorToInt(0.5f * (gridHeight - 1) + 1.0f),
            z = gridDepth - Mathf.FloorToInt(0.5f * (gridDepth - 1) + 1.0f)
        };

        System.Random rnd = new System.Random();
        for (var w = 0; w <  gridWidth; w++)
        {
            for (var h = 0; h < gridHeight; h++)
            {
                for (var d = 0; d < gridDepth; d++)
                {

                    gridOfCells[w, h, d] = Instantiate(Manager.CellPrefab, transform).GetComponent<Cell>();
                    var cellTransform = gridOfCells[w, h, d].transform;
                    cellTransform.SetPositionAndRotation(new Vector3(w - offset.x, h - offset.y, d - offset.z) * (is3D ? distanceMultiplier : 1.0f), Quaternion.identity);
                    cellTransform.localScale = CellScale;

                    int random = rnd.Next(1, 100);
                    if (random % 5  == 0)
                    {
                        gridOfCells[w, h, d].Initialize(true, 0, colorsRepresentingAge);
                    }
                    else
                    {
                        gridOfCells[w, h, d].Initialize(false, -1, colorsRepresentingAge);
                    }
                }
            }
        }

        Manager.GameState = GameStateEnum.AcceptInput;
    }
}
