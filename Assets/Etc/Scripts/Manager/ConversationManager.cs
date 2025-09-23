using Newtonsoft.Json.Linq;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.InputSystem; // �� �Է� �ý��� ���ӽ����̽�
using UnityEngine.UI;


public class ConversationManager : MonoBehaviour
{
    public Image screen;
    public GameObject conversationUI;    
    public GameObject dialogue;  // ��ȭ UI    
    public Timer timer;
    private Image[] slides = new Image[2];
    // public SummaryManager summaryManager;
    public HintManager hintManager;
    public InvestigationManager investigationManager;
    public LogManager logManager;
    public RecommendationManager recommendationManager;

    private Button endConversationBtn;  // ��ȭ ���� ��ư
    private InputField inputField;
    [SerializeField] private Text NPCName;
    public Text GetNPCName { get { return NPCName; } }
    [SerializeField] private Text NPCLine;
    public Text GetNPCLine { get { return NPCLine; } }
    [SerializeField] private GameObject endWaitingMark;
    public GameObject GetEndWaitingMark { get { return endWaitingMark; } }

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
    private Dictionary<int, bool> revealedSecretDict = new Dictionary<int, bool>(){
        {1, false},
        {2, false},
        {3, false},
        {4, false}
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NPCName = dialogue.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        NPCLine = dialogue.transform.GetChild(1).GetChild(1).GetComponent<Text>();
        endWaitingMark = dialogue.transform.GetChild(1).GetChild(2).gameObject;
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

        // GetNPCRole(npcParam);        // InvestigationManager에서 실행
        player.GetLookAtTarget(npcParam.GetAnchor());
        StartCoroutine(StartConversationCoroutine());
        AddListenersResponse();


        CursorManager.Instance.OnVisualization();
    }

    private IEnumerator StartConversationCoroutine()
    {
        yield return FadeUtility.Instance.SwitchCameraWithFade(screen, player, npcRole);
        // conversationUI.SetActive(true);  // 대사 다 끝나고 실행됨
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
    public void SetActiveConversationUI(bool state)
    {
        conversationUI.SetActive(state);
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
        int pressureLevel = 0;
        int revealedSecret = 0;
        string responseText = "";

        recommendationManager.SetInteractableButtons(false);

        try
        {
            // JSON 유효성 검사
            if (!answer.TrimStart().StartsWith("{"))
            {
                Debug.LogWarning("응답이 JSON 형식이 아닙니다. 원본 출력: " + answer);
                return;
            }

            JObject json = JObject.Parse(answer); // JSON �Ľ�

            emotion = json["emotion"]?.ToString();            
            pressureLevel = json["pressure_level"] != null ? (int)json["pressure_level"] : 0;
            revealedSecret = json["revealed_secret"] != null ? (int)json["revealed_secret"] : 0;
            responseText = json["response"]?.ToString();

            recommendationManager.GetResponse(responseText);
            npcRole.PlayOnomatopoeia(emotion);

            if (pressureLevel == 0)
                NPCLine.color = Color.white;
            else if (pressureLevel == 1) NPCLine.color = Color.yellow;
            else if (pressureLevel == 2 && revealedSecret != 0)
            {
                NPCLine.color = Color.red;
                if (revealedSecretDict.ContainsKey(revealedSecret) && !revealedSecretDict[revealedSecret])
                {
                    revealedSecretDict[revealedSecret] = true;
                    timer.AddTime(120);
                    timer.GetComponent<AudioSource>().Play();
                    hintManager.ShowHint(revealedSecret - 1);
                }                     
            }
            else if (pressureLevel == 2 && revealedSecret != 0) NPCLine.color = Color.white;


            /* 나중에 적용하기
            switch (pressureLevel)
            {
                case 1: // 보통 거리
                    player.ZoomCamera(2); break;
                case 2: // 가까이 줌
                    player.ZoomCamera(1); break;
                default:    // 멀리 줌
                    player.ZoomCamera(3); break;
            }
            */

            // 로컬라이제이션 할 때 주의!
            // summaryManager.GetMessages("플레이어(player) : " + tmpQuestion, 
            //     npcRole.currentCharacter.ToString() + " : " + responseText.Trim());
            logManager.AddNewLog(npcRole, tmpQuestion, responseText.Trim());

            if (string.IsNullOrWhiteSpace(responseText))
            {
                Debug.LogWarning("response 항목이 비어 있습니다.");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"JSON 파싱 중 오류 발생: {ex.Message}");
            Debug.Log("문제 발생한 응답: " + answer);
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

    public void SetActiveEndConversationButton(bool state)
    {
        endConversationBtn.gameObject.SetActive(state);
    }

    public void SetInteractableAskField(bool state)
    {
        inputField.interactable = state;
    }

    public void ChangeIsSkipping(bool state)
    {
        waitToSkip = state;

        SetActiveEndWaitingMark(!state);
    }

    public void SetActiveEndWaitingMark(bool state, bool isTalking = false)
    {
        endWaitingMark.SetActive(state);

        if (isTalking)
        {
            NPCLine.color = Color.white;
        }
    }

    public void FocusOnAskField()
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

    public InputField GetAskField()
    {
        return inputField;
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




    /// <summary>
    /// revealSecretDict의 모든 값이 true인지 확인
    /// </summary>
    /// <returns></returns>
    public bool AreAllSecretsRevealed()
    {
        foreach (var kvp in revealedSecretDict)
        {
            if (!kvp.Value)
                return false;
        }
        return true;
    }





    public IEnumerator ShowText(string NPCName = "", string fullText = "", Action onFinished = null)
    {
        this.NPCName.text = NPCName;
        NPCLine.text = ""; // 초기화

        npcRole.PlayEmotion(fullText);

        // 글자 하나씩 출력
        foreach (char c in fullText)
        {
            NPCLine.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        // 모든 텍스트가 출력된 후 → 입력 대기
        bool waitingInput = true;
        endWaitingMark.SetActive(true); // 입력 대기 마크 활성화
        while (waitingInput)
        {
            if (
                Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                Keyboard.current != null && (
                    Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.numpadEnterKey.wasPressedThisFrame
                )
            )
            {
                waitingInput = false;
            }

            yield return null; // 다음 프레임까지 대기
        }

        endWaitingMark.SetActive(false); // 입력 대기 마크 비활성화

        // 입력이 감지되면 다음 액션 실행
        if (onFinished != null)
            onFinished();
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