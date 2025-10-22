using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class optionsScreen : MonoBehaviour
{
    public TMP_Text resoultionLabel;
    public Toggle fullscreenTog, vsyncTog;
    private int selectedResoultion;
    public List<ResItem> resoultions = new List<ResItem>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fullscreenTog.isOn = Screen.fullScreen;
        if (QualitySettings.vSyncCount == 0)
        {
            vsyncTog.isOn = false;
        }
        else
        {
            vsyncTog.isOn = true;
        }

        bool foundRes = false;
        for (int i = 0; i < resoultions.Count; i++)
        {
            if (Screen.width == resoultions[i].horizontial && Screen.height == resoultions[i].vertical)
            {
                foundRes = true; 

                selectedResoultion = i;

                UpdateResLabel();
            }

            if(!foundRes)
            {
                ResItem newRes = new ResItem();
                newRes.horizontial = Screen.width;
                newRes.vertical = Screen.height;

                resoultions.Add(newRes);
                selectedResoultion = resoultions.Count - 1;
                UpdateResLabel();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResLeft()
    {
        selectedResoultion--;
        if (selectedResoultion < 0)
        {
            selectedResoultion = 0;
        }
        UpdateResLabel();

    }

    public void ResRight()
    {
        selectedResoultion++;
        if (selectedResoultion > resoultions.Count - 1)
        {
            selectedResoultion = resoultions.Count - 1;
        }
        UpdateResLabel();
    }

    public void UpdateResLabel()
    {
        resoultionLabel.text = resoultions[selectedResoultion].horizontial.ToString() + " x " + resoultions[selectedResoultion].vertical.ToString();
    }

    public void ApplyGraphics()
    {
        //Screen.fullScreen = fullscreenTog.isOn;

        if (vsyncTog.isOn)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }

        Screen.SetResolution(resoultions[selectedResoultion].horizontial, resoultions[selectedResoultion].vertical, fullscreenTog.isOn);
    }
}
[System.Serializable]
public class ResItem
{
    public int horizontial, vertical;
}
