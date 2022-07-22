using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the settings in Settings Menu and stores them in PlayerPrefs.
/// </summary>
public class Settings : MonoBehaviour
{
    public int width;
    public int height;
    public int depth;
    public int distanceGap;
    public float generationTimeGap;
    public bool is3D;
    public bool connectBorders;
    public bool isDeadCellInvisible;
    public bool colorsRepresentingAge;

    public TextMeshProUGUI valueWidthText;
    public TextMeshProUGUI valueHeightText;
    public TextMeshProUGUI valueDepthText;
    public TextMeshProUGUI valueDistanceText;
    public TextMeshProUGUI valueGenerationGapText;

    public Slider SliderWidth;
    public Slider SliderHeight;
    public Slider SliderDepth;
    public Slider SliderDistance;
    public Slider SliderTime;

    public Toggle is3dToggle;
    public Toggle connectedBordersToggle;
    public Toggle invisibleDeadToggle;
    public Toggle ColorsToggle;

    public Toggle[] saTogglesObjects = new Toggle[26];
    public Toggle[] ctlTogglesObjects = new Toggle[26];

    /// <summary>
    /// Reads previous data from PlayerPrefs, or sets the deafult one.
    /// Sets the Objects value in Settings Menu
    /// </summary>
    private void Start()
    {
        width = PlayerPrefs.GetInt("width", 25); ;
        height = PlayerPrefs.GetInt("height", 25);
        depth = PlayerPrefs.GetInt("depth", 25);
        generationTimeGap = PlayerPrefs.GetFloat("generationGap", 1); ;
        distanceGap = PlayerPrefs.GetInt("distanceGap", 1);
        is3D = Convert.ToBoolean(PlayerPrefs.GetInt("dimensions", 1));
        connectBorders = Convert.ToBoolean(PlayerPrefs.GetInt("connectBorders", 0));
        isDeadCellInvisible = Convert.ToBoolean(PlayerPrefs.GetInt("deadInvisible", 0));
        colorsRepresentingAge = Convert.ToBoolean(PlayerPrefs.GetInt("colorsOfAge", 0));

        string tempString;
        for(int i = 1; i <= 26; i++)
        {
            tempString = "saToggle" + i.ToString();
            StaysAliveToggle.saToggles[i-1] = Convert.ToBoolean(PlayerPrefs.GetInt(tempString, 0));
            saTogglesObjects[i - 1].isOn = Convert.ToBoolean(PlayerPrefs.GetInt(tempString, 0));

            tempString = "ctlToggle" + i.ToString();
            ComesToLifeToggle.ctlToggles[i - 1] = Convert.ToBoolean(PlayerPrefs.GetInt(tempString, 0));
            ctlTogglesObjects[i - 1].isOn = Convert.ToBoolean(PlayerPrefs.GetInt(tempString, 0));
        }

        valueWidthText.text = width.ToString();
        valueHeightText.text = height.ToString();
        valueDepthText.text = depth.ToString();
        valueDistanceText.text = distanceGap.ToString();
        valueGenerationGapText.text = generationTimeGap.ToString();
        SliderWidth.value = width;
        SliderHeight.value = height;
        SliderDepth.value = depth;
        SliderDistance.value = distanceGap;
        SliderTime.value = generationTimeGap * 100;
        is3dToggle.isOn = is3D;
        connectedBordersToggle.isOn = connectBorders;
        invisibleDeadToggle.isOn = isDeadCellInvisible;
        ColorsToggle.isOn = colorsRepresentingAge;
    }

    /// <summary>
    /// Updates the values next to slider.
    /// </summary>
    private void Update()
    {
        valueWidthText.text = width.ToString();
        valueHeightText.text = height.ToString();
        valueDepthText.text = depth.ToString();
        valueDistanceText.text = distanceGap.ToString();
        valueGenerationGapText.text = generationTimeGap.ToString();
    }

    public void SetWidth(float width)
    {
       this.width = (int)Math.Round(width);
    }
    public void SetHeight(float height)
    {
        this.height = (int)Math.Round(height);
    }
    public void SetDepth(float depth)
    {
        this.depth = (int)Math.Round(depth);
    }
    public void SetDistanceGap(float gap)
    {
        distanceGap = (int)Math.Round(gap);
    }
    public void SetGenerationGap(float gap)
    {
        generationTimeGap = gap/100;
    }
    public void SetDimensions(bool _is3d)
    {
        is3D = _is3d;
    }
    public void SetConnectedBorders(bool connectedBorders)
    {
        connectBorders = connectedBorders;
    }
    public void SetDeadInvisibility(bool isInvisible)
    {
        isDeadCellInvisible = isInvisible;
    }

    public void SetColours(bool colors)
    {
        colorsRepresentingAge = colors;
    }

    /// <summary>
    /// Saves the data to PlayerPrefs and changes scene to Main Menu.
    /// </summary>
    public void BackButton()
    {
        PlayerPrefs.SetInt("width", width);
        PlayerPrefs.SetInt("height", height);
        PlayerPrefs.SetInt("depth", depth);
        PlayerPrefs.SetInt("distanceGap", distanceGap);
        PlayerPrefs.SetFloat("generationGap", generationTimeGap);
        PlayerPrefs.SetInt("dimensions", Convert.ToInt32(is3D));
        PlayerPrefs.SetInt("connectBorders", Convert.ToInt32(connectBorders));
        PlayerPrefs.SetInt("isInvisible", Convert.ToInt32(isDeadCellInvisible));
        PlayerPrefs.SetInt("colorsOfAge", Convert.ToInt32(colorsRepresentingAge));
        string tempString;
        for (int i = 1; i <= 26; i++)
        {
            tempString = "saToggle" + i.ToString();
            PlayerPrefs.SetInt(tempString, Convert.ToInt32(StaysAliveToggle.saToggles[i-1]));

            tempString = "ctlToggle" + i.ToString();
            PlayerPrefs.SetInt(tempString, Convert.ToInt32(ComesToLifeToggle.ctlToggles[i-1]));
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }

}
