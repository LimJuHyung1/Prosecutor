using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LogManager : MonoBehaviour
{
    public GameObject log;
    public GameObject logPage;
    public ScrollRect []logScrollRect;    

    private int npcIndex = -1;
    private float space = 50f;    
    
    private Dictionary<ScrollRect, List<RectTransform>> npcLogPair = new Dictionary<ScrollRect, List<RectTransform>>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logPage.gameObject.SetActive(false);

        foreach (var scroll in logScrollRect)
        {
            scroll.gameObject.SetActive(false);
            var logList = new List<RectTransform>();
            npcLogPair.Add(scroll, logList);
        }
    }

    public void SetupLogPage(bool isActive)
    {
        logPage.gameObject.SetActive(isActive);
    }

    public void SetupNPCLog(int index)
    {
        for (int i = 0; i < logScrollRect.Length; i++)
        {
            logScrollRect[i].gameObject.SetActive(false);
        }

        logScrollRect[index].gameObject.SetActive(true);
    }



    public void AddNewLog(SpecificNPC npc, string question, string answer)
    {
        npcIndex = (int)npc.currentCharacter;

        var newLog = Instantiate(log, logScrollRect[npcIndex].content).GetComponent<RectTransform>();        
        npcLogPair[logScrollRect[npcIndex]].Add(newLog);

        newLog.transform.GetChild(0).GetComponent<Text>().text = "Question: " + question;
        newLog.transform.GetChild(1).GetComponent<Text>().text = "Answer: " + answer;

        float y = 0;
        for(int i = 0; i < npcLogPair[logScrollRect[npcIndex]].Count; i++)
        {
            npcLogPair[logScrollRect[npcIndex]][i].anchoredPosition = new Vector2(0, -y);
            y += npcLogPair[logScrollRect[npcIndex]][i].sizeDelta.y + space;
        }

        logScrollRect[npcIndex].content.sizeDelta = new Vector2(logScrollRect[npcIndex].content.sizeDelta.x, y);
    }

}
