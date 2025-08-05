using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem; // �� �Է� �ý��� ���ӽ����̽�
using TMPro;


public class ConversationManager : MonoBehaviour
{
    public Image screen;
    public GameObject conversationUI;    
    public GameObject dialogue;  // ��ȭ UI    
    private Image[] slides = new Image[2];
    public SummaryManager summaryManager;

    private Button endConversationBtn;  // ��ȭ ���� ��ư
    private InputField inputField;
    [SerializeField] private Text NPCName;
    [SerializeField] private Text NPCLine;

    public string tmpQuestion = "";
    public bool isReadyToSkip = false;  // NPC�� ���� ��� ���� �� true�� �����
    public bool IsReadyToSkip
    {
        get { return isReadyToSkip; }
        set { isReadyToSkip = value; }
    }


    private bool waitToSkip = true;
    private bool isTalking = false;
    private bool isAbleToGoNext = false;
    private bool isSkipping = false;

    private int interrogationStage = 1;
    public int InterrogationStage
    {
        get { return interrogationStage; }
        set { interrogationStage = value; }
    }

    private float duration = 1.0f;     // �̵��� �ɸ��� �ð�

    public Player player;
    [SerializeField] private SpecificNPC npcRole;
    private Coroutine displayCoroutine;

    private Queue<string> sentencesQueue = new Queue<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NPCName = dialogue.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        NPCLine = dialogue.transform.GetChild(1).GetChild(1).GetComponent<Text>();
        endConversationBtn = conversationUI.transform.GetChild(0).GetComponent<Button>();
        inputField = conversationUI.GetComponentInChildren<InputField>(true);

        slides[0] = dialogue.transform.GetChild(0).GetComponent<Image>();
        slides[1] = dialogue.transform.GetChild(1).GetComponent<Image>();

        conversationUI.SetActive(false);

        inputField.onSubmit.RemoveAllListeners();
        inputField.onSubmit.AddListener((string _) =>
        {
            StartCoroutine(WaitAndLogInput());
        });
    }

    private IEnumerator WaitAndLogInput()
    {
        yield return null; // �� ������ ��ٷ��� IME Ŀ�� ������

        string finalText = inputField.text;
        if (!string.IsNullOrWhiteSpace(finalText))
        {
            Debug.Log("�Էµ� ����: " + finalText.Trim());
        }

        // �Է� �ʵ� ���� (���� ����)
        inputField.text = "";
        inputField.ActivateInputField(); // �ٽ� �Է� �����ϵ��� ��Ŀ��
    }

    /// <summary>
    /// Ű �Է� ���� �� ��ȭ ��ŵ ��� Ȱ��ȭ
    /// </summary>
    void OnEnable()
    {
        StartCoroutine(WaitForSkipInput());
    }

    IEnumerator WaitForSkipInput()
    {
        while (true)
        {
            if (isReadyToSkip && (
                Mouse.current.leftButton.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                isSkipping = true;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Player ��ũ��Ʈ���� ���� ��ȭ�ϴ� NPC�� �����ϴ� �޼���
    /// </summary>    
    public void GetNPCRole(SpecificNPC npcParam)
    {
        npcRole = npcParam;
    }

    /// <summary>
    /// �������� NPC ���� (��ȭ�� ����� �� ȣ���)
    /// </summary>
    public void RemoveNPCRole()
    {
        npcRole = null;
    }


    /// <summary>
    /// InputField���� ����Ǵ� �̺�Ʈ ������ ���
    /// </summary>
    public void AddListenersResponse()
    {
        if (npcRole != null)
        {
            OnEndEditAskField(npcRole.GetResponse);
            OnEndEditAskField(SetNullInputField);            
        }
    }

    
    /// <param name="action"></param>
    public void OnEndEditAskField(UnityAction action)
    {
        inputField.onEndEdit.AddListener((string text) =>
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                tmpQuestion = text.Trim();                
                action();
            }
        });
    }


    /// <summary>
    /// input field�� ���� ĭ�� �������� ����
    /// </summary>
    public void SetNullInputField()
    {
        SetInteractableAskField(false);
        StartCoroutine(DelayClearInput());
    }

    private IEnumerator DelayClearInput()
    {
        yield return null; // 1 ������ ��ٸ�
        inputField.text = "";
    }





    /// <summary>
    /// ��ȭ�� �����ϴ� �޼��� (���콺 Ŭ�� �� ����)
    /// </summary>
    public void StartConversation(SpecificNPC npcParam)
    {
        isTalking = true;    
        NPCName.text = "";
        NPCLine.text = "";

        if(npcParam.currentCharacter == SpecificNPC.Character.William)
            NPCName.color = Color.blue;
        else if(npcParam.currentCharacter == SpecificNPC.Character.John)
            NPCName.color = Color.purple;
        else if(npcParam.currentCharacter == SpecificNPC.Character.Emma)
            NPCName.color = Color.yellow;

        GetNPCRole(npcParam);
        player.GetLookAtTarget(npcParam.GetAnchor());
        StartCoroutine(StartConversationCoroutine());
        AddListenersResponse();
        Slide(true);

        dialogue.SetActive(true);
        conversationUI.SetActive(true);

        CursorManager.Instance.OnVisualization();
    }

    private IEnumerator StartConversationCoroutine()
    {
        yield return FadeUtility.Instance.SwitchCameraWithFade(screen, player, npcRole);
        dialogue.SetActive(true);
        conversationUI.SetActive(true);
        // SetAudio();
    }

    /// <summary>
    /// ��ȭ�� �����ϴ� �޼��� (NPC�� ��ȭ ���� �� ����)
    /// </summary>
    public void EndConversation()
    {
        dialogue.SetActive(false);
        conversationUI.SetActive(false);        
        EndConversationCoroutine();
        Slide(false);
    }

    private void EndConversationCoroutine()
    {        
        SetNullInputField();
        RemoveOnEndEditListener();
        SetBlankAnswerText();        

        CursorManager.Instance.OnVisualization();
        isTalking = false;
        RemoveNPCRole();
    }

    /// <summary>
    /// ��ȭ UI�� Ȱ��ȭ/��Ȱ��ȭ
    /// </summary>
    public void SetConversationUI(bool b)
    {
        conversationUI.SetActive(b);
    }


    public void RemoveOnEndEditListener()
    {
        inputField.onEndEdit.RemoveAllListeners();
    }

    /// <summary>
    /// NPC �亯 �ؽ�Ʈ�� �������� ����
    /// </summary>
    public void SetBlankAnswerText()
    {
        NPCLine.text = "";
    }

    /// <summary>
    /// NPC�� �亯�� �޾� ȭ�鿡 ��� (���� ������ ������ ��� ť�� ����)
    /// </summary>
    public void ShowAnswer(string answer)
    {
        if (npcRole == null)
        {
            Debug.LogError("NPC�� �����ϴ�!");
            return;
        }

        string emotion = "";
        string responseText = "";
        string truth_status = "";

        try
        {
            JObject json = JObject.Parse(answer); // JSON �Ľ�

            emotion = json["emotion"]?.ToString();
            responseText = json["response"]?.ToString();
            truth_status = json["truth_status"]?.ToString();

            npcRole.PlayOnomatopoeia(emotion);

            if (truth_status == "True")
            {
                NPCLine.color = Color.white;
                player.ZoomCamera(true);
            }
            else if (truth_status == "Lie")
            {
                NPCLine.color = Color.red;
                player.ZoomCamera(false);
            }
            else
            {
                NPCLine.color = Color.white;
                Debug.LogError("truth_status is not True or Lie");
            }
            
            // 로컬라이제이션 할 때 주의!
            summaryManager.GetMessages("플레이어(player) : " + tmpQuestion, 
                npcRole.currentCharacter.ToString() + " : " + responseText.Trim());

            if (string.IsNullOrWhiteSpace(responseText))
            {
                Debug.LogWarning("response �׸��� ����ֽ��ϴ�.");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"JSON �Ľ� �� ���� �߻�: {ex.Message}");
            return;
        }

        sentencesQueue.Clear();

        // '.', '?', '!' �������� ���� �����ϵ� "..."�� ���� ó��        
        string[] sentences = Regex.Split(responseText, @"(?<!(\.{2,}))(?<=[.!?])\s+");

        foreach (string part in sentences)
        {
            if (!string.IsNullOrWhiteSpace(part))
            {
                string trimmedSentence = part.Trim();
                sentencesQueue.Enqueue(trimmedSentence);
            }
        }

        if (displayCoroutine == null)
        {
            displayCoroutine = StartCoroutine(DisplaySentences());
        }
    }

    /// <summary>
    /// ��縦 �� ���ھ� ����ϴ� �ڷ�ƾ
    /// </summary>
    public IEnumerator ShowLine(Text t, string answer, float second = 0.1f, bool isTyping = true)
    {
        t.text = ""; // �ؽ�Ʈ �ʱ�ȭ
        Coroutine dialogSoundCoroutine = null;

        dialogSoundCoroutine = StartCoroutine(PlayDialogSound());

        yield return new WaitUntil(() => waitToSkip);
        for (int i = 0; i < answer.Length; i++)
        {
            if (isSkipping) // ���콺 ���� Ŭ�� or ����Ű ����
            {
                isSkipping = false;
                waitToSkip = false;
                t.text = answer; // ��ü �ؽ�Ʈ ��� ǥ��
                break; // ������ �ߴ��ϰ� ��ü �ؽ�Ʈ�� ǥ���ϵ��� �̵�
            }

            t.text += answer[i]; // �� ���ھ� �߰�
            yield return new WaitForSeconds(second); // 0.02�� ���
        }

        isReadyToSkip = false;
        ChangeIsSkipping(false);

        // �ڷ�ƾ�� ����Ǿ��� ��쿡�� ���� ó��
        if (dialogSoundCoroutine != null)
        {
            StopCoroutine(dialogSoundCoroutine); // PlayDialogSound �ڷ�ƾ ����
            // SoundManager.Instance.StopTextSound();
        }

        IsAbleToGoNextTrue();
    }

    /// <summary>
    /// NPC�� �亯�� �ѹ��� �� ȭ�鿡 ���
    /// </summary>
    private IEnumerator DisplaySentences()
    {
        if (!isTalking) yield break;

        switch (npcRole.name)
        {
            case "William(Clone)":
                NPCName.text = "윌리엄";
                break;
            case "Emma(Clone)":
                NPCName.text = "엠마";
                break;
            case "John(Clone)":
                NPCName.text = "존";
                break;
        }

        while (sentencesQueue.Count > 0)
        {
            string sentence = sentencesQueue.Dequeue();
            isAbleToGoNext = false;
            IsReadyToSkip = true;

            ChatMessage message = new ChatMessage { Content = sentence };

            yield return StartCoroutine(ShowLine(GetNPCAnswer(), sentence));

            yield return new WaitUntil(() => isAbleToGoNext);

            if (sentencesQueue.Count > 0)
            {
                yield return new WaitUntil(() =>
                    Mouse.current.leftButton.wasPressedThisFrame ||
                    Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.numpadEnterKey.wasPressedThisFrame);

                ChangeIsSkipping(true);
            }
        }

        IsReadyToSkip = false;
        SetActiveEndConversationButton(true);
        SetInteractableAskField(true);
        ChangeIsSkipping(true);
        FocusOnAskField();
        displayCoroutine = null;
    }
    public void SetActiveEndConversationButton(bool b)
    {
        endConversationBtn.gameObject.SetActive(b);
    }

    public void SetInteractableAskField(bool b)
    {
        inputField.interactable = b;
    }

    public void ChangeIsSkipping(bool b)
    {
        waitToSkip = b;
    }

    void FocusOnAskField()
    {
        inputField.Select();
        inputField.ActivateInputField(); // InputField Ȱ��ȭ
    }

    // NPC �亯 ��ȯ
    public Text GetNPCAnswer()
    {
        return NPCLine;
    }

    /// <summary>
    /// ���� �������� �Ѿ �� �ֵ��� ���� ����
    /// </summary>
    public void IsAbleToGoNextTrue()
    {
        isAbleToGoNext = true;
    }

    IEnumerator PlayDialogSound()
    {
        while (true) // ���� ������ ����Ͽ� �ݺ� ����
        {
            // SoundManager.Instance.PlayTextSound();
            yield return new WaitForSeconds(0.1f);
        }
    }



    // InputField �ؽ�Ʈ ���� ��ȯ
    public int GetAskFieldTextLength()
    {
        return inputField.text.Length;
    }

    // InputField �ؽ�Ʈ ��ȯ
    public string GetAskFieldText()
    {
        return inputField.text;
    }

    /// <summary>
    /// ��ȭ ������ ���� ��ȯ
    /// </summary>
    public bool GetIsTalking()
    {
        return isTalking;
    }





    public void Slide(bool isTalking, float fadeDuration = 1f)
    {
        SlideMove(0, isTalking, fadeDuration);
        SlideMove(1, isTalking, fadeDuration);
    }


    private void SlideMove(int index, bool isOn, float fadeDuration)
    {
        if (index < 0 || index >= slides.Length)
        {
            Debug.LogError("Slide index out of range.");
        }

        RectTransform tmp = slides[index].GetComponent<RectTransform>();
        float y = isOn ? 0f : (index == 0 ? tmp.rect.height : -tmp.rect.height);  // ���⿡ ���� ��ġ ���
        Vector2 targetPosition = new Vector2(tmp.anchoredPosition.x, y);

        StartCoroutine(SmoothMove(tmp, tmp.anchoredPosition, targetPosition));
    }

    private IEnumerator SmoothMove(RectTransform rect, Vector2 startPos, Vector2 endPos)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rect.anchoredPosition = endPos;
    }
}