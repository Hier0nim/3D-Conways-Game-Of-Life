using System;
using Assets.Scripts;
using UnityEngine;

/// <summary>
/// Handles the actions on cell, and stores its values.
/// </summary>
public class Cell : MonoBehaviour
{
    private Renderer myObject;
    private bool isAlive;
    Color color;
    public bool isNextAlive;
    public bool isChecked;
    private bool changesColorsDepndingOnAge;
    private int age;

    /// <summary>
    /// Gets isAlive Value or sets it with selecting appropriate material and its color.
    /// </summary>
    public bool IsAlive
    {
        get { return isAlive; }
        set
        {
            isAlive = value;
            if (value == true)
            {
                myObject.sharedMaterial = Manager.CellMaterials[Convert.ToInt32(isAlive)];
                if (changesColorsDepndingOnAge == true)
                {
                    age++;
                    if (age > 0)
                    {
                        if (color.r < 1f) color.r += 0.25f;
                        else if (color.g > 0) color.g -= 0.25f;
                    }
                    myObject.material.color = color;
                }
            }
            else
            {
                myObject.sharedMaterial = Manager.CellMaterials[Convert.ToInt32(isAlive)];
                if (changesColorsDepndingOnAge == true)
                {
                    age = -1;
                    color = Color.green;
                }
            }
        }
    }

    /// <summary>
    /// Initialises the renderer and other critical cell data.
    /// </summary>
    /// <param name="isAlive">Initialized cell status.</param>
    /// <param name="age">Initialised cell age.</param>
    /// <param name="ifColors">Intialised cell color.</param>
    public void Initialize(bool isAlive, int age, bool ifColors)
    {
        myObject = GetComponent<Renderer>();
        this.isAlive = isAlive;
        isNextAlive = isAlive;
        this.age = age;
        color = Color.green;
        this.changesColorsDepndingOnAge = ifColors;
        myObject.sharedMaterial = Manager.CellMaterials[Convert.ToInt32(this.isAlive)];
    }

    /// <summary>
    /// Changes the state of cell when clicked od it.
    /// </summary>
    private void OnMouseDown()
    {
        if (Manager.GameState == GameStateEnum.AcceptInput)
        {
            IsAlive = !IsAlive;
        }
    }

 
}

