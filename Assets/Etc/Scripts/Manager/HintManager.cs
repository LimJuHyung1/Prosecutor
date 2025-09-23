using System;
using UnityEngine;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    public GameObject hint;
    public Image[] hintImages;

    public InvestigationManager investigationManager;

    private void Start()
    {
        SetActiveHintPage(false);
    }

    public void SetActiveHintPage(bool state)
    {
        hint.SetActive(state);
        if(state) StartCoroutine(investigationManager.PauseTimer());
        else investigationManager.StartTimer(false);
    }

    public void ShowHint(int index)
    {
        Transform hintImageTransform = hintImages[index].transform;
        
        for (int i = 0; i < hintImageTransform.childCount; i++)
        {
            hintImageTransform.GetChild(i).gameObject.SetActive(true);

            if (hintImageTransform.GetChild(i).name == "?")
            {
                hintImageTransform.GetChild(i).gameObject.SetActive(false);                
                break;
            }
        }        
    }
}
