using Newtonsoft.Json.Linq;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum CourtRole
{
    Player,
    Judge,
    Lawyer
}

public class CourtManager : MonoBehaviour
{
    public CourtRole subjectRole;
    public CourtRole targetRole;

    public CourtCameraManager courtCameraManager;


    public Button[] roleButtons;    // 0 : Judge, 1 : Lawyer
    private Button currentSelectedButton; // 현재 선택된 버튼 추적
    public Image screen;
    public GameObject conversationUI;
    public GameObject dialogue;  // ��ȭ UI    
    private Image[] slides = new Image[2];

    private Button endConversationBtn;  // ��ȭ ���� ��ư
    private InputField inputField;
    public InputField InputField
    {
        get { return inputField; }
    }
    [SerializeField] private Text nameText;
    [SerializeField] private Text lineText;
    public Text LineText
    {
        get { return lineText; }
    }
    [SerializeField] private GameObject endWaitingMark;


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

    private int totalConvictionScore = 0;
    private float duration = 1.0f;     // �̵��� �ɸ��� �ð�

    public GameObject defendant;    // 윌리엄, 엠마, 존 중 한 사람
    public GameObject player;
    public Judge judge;       
    public Lawyer lawyer;
    private GameObject target; // 줌 인/아웃 대상


    private Coroutine displayCoroutine;

    private Queue<string> sentencesQueue = new Queue<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nameText = dialogue.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        lineText = dialogue.transform.GetChild(1).GetChild(1).GetComponent<Text>();
        endWaitingMark = dialogue.transform.GetChild(1).GetChild(2).gameObject;
        endConversationBtn = conversationUI.transform.GetChild(0).GetComponent<Button>();
        inputField = conversationUI.GetComponentInChildren<InputField>(true);

        if (roleButtons[1] != null)
        {
            roleButtons[1].onClick.Invoke();
            ClickRoleButton(1);
        }            

        slides[0] = dialogue.transform.GetChild(0).GetComponent<Image>();
        slides[1] = dialogue.transform.GetChild(1).GetComponent<Image>();

        conversationUI.SetActive(false);

        inputField.onSubmit.RemoveAllListeners();
        inputField.onSubmit.AddListener((string _) =>
        {
            StartCoroutine(WaitAndLogInput());
        });

        StartConversation();
        courtCameraManager.StartInputCheckRoutine();
    }

    private void Update()
    {
        if (courtCameraManager == null) return;

        if (inputField != null)
        {
            if (!inputField.interactable)
            {
                // 입력 불가 → 카메라 연출 시작
                courtCameraManager.StartIdleCameraMovement();
            }
            else
            {
                // 입력 가능 → 카메라 연출 중지
                courtCameraManager.StopIdleCameraMovement();
            }
        }
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
    /// InputField���� ����Ǵ� �̺�Ʈ ������ ���
    /// </summary>
    public void AddListenersResponse()
    {
        if (lawyer != null)
        {
            OnEndEditAskField(ShowPlayerAnswer);
            // OnEndEditAskField(lawyer.GetResponse);
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
    public void StartConversation()
    {
        isTalking = true;
        nameText.text = "";
        lineText.text = "";
        nameText.color = lawyer.GetCurrentHairColor();
        
        // player.GetLookAtTarget(lawyer.GetAnchor());
        // StartCoroutine(StartConversationCoroutine());
        StartConversationCoroutine();
        AddListenersResponse();
        Slide(true);

        dialogue.SetActive(true);
        conversationUI.SetActive(true);

        CursorManager.Instance.OnVisualization();
    }

    private void StartConversationCoroutine()
    {
        // yield return FadeUtility.Instance.SwitchCameraWithFade(screen, player, lawyer);
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
        lineText.text = "";
    }




    public void ShowPlayerAnswer()
    {
        string[] sentences = Regex.Split(GetAskFieldText(), @"(?<!(\.{2,}))(?<=[.!?])\s+");

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
            displayCoroutine = StartCoroutine(DisplaySentences(CourtRole.Player, targetRole));
        }

        // 여기서 다음 캐릭터 대사 트리거
        if (targetRole == CourtRole.Lawyer)
        {
            lawyer.GetResponse(tmpQuestion, false);
            judge.GetResponse(tmpQuestion, true);
        }
        else if (targetRole == CourtRole.Judge)
        {
            judge.GetResponse(tmpQuestion, false);
            lawyer.GetResponse(tmpQuestion, true);
        }        
    }

    public void ShowJudgeAnswer(string answer, bool noResponse)
    {
        string emotion = "";
        CourtRole tmpRole;
        int convictionScore;
        string responseText;

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
            Enum.TryParse(json["target"]?.ToString(), true, out tmpRole);
            convictionScore = json["convictionScore"]?.Value<int>() ?? 0;
            totalConvictionScore += convictionScore;
            responseText = json["response"]?.ToString();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"JSON 파싱 중 오류 발생: {ex.Message}");
            Debug.Log("문제 발생한 응답: " + answer);
            return;
        }

        if (!noResponse)
        {
            judge.PlayOnomatopoeia(emotion);

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
                displayCoroutine = StartCoroutine(DisplaySentences(CourtRole.Judge, tmpRole));
            }
        }
    }

    public void ShowLawyerAnswer(string answer)
    {
        string emotion = "";
        CourtRole tmpRole;
        string responseText;

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
            Enum.TryParse(json["target"]?.ToString(), true, out tmpRole);
            responseText = json["response"]?.ToString();


            // response가 없을 때도 기본값으로 채워서 끊기지 않도록
            if (string.IsNullOrWhiteSpace(responseText))
            {
                Debug.LogWarning("response 항목이 비어 있습니다. 기본 메시지로 대체합니다.");
                responseText = "(대답 없음)";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"JSON 파싱 중 오류 발생: {ex.Message}");
            Debug.Log("문제 발생한 응답: " + answer);
            return;
        }

        lawyer.GetComponent<Animator>().SetTrigger(emotion);
        lawyer.PlayOnomatopoeia(emotion);

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
            displayCoroutine = StartCoroutine(DisplaySentences(CourtRole.Lawyer, tmpRole));
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
    private IEnumerator DisplaySentences(CourtRole subjectRole, CourtRole targetRole)
    {
        if (!isTalking) yield break;

        switch (subjectRole.ToString())
        {
            case "Player":
                courtCameraManager.ZoomIn(0);
                nameText.color = Color.darkRed;
                nameText.text = "수잔";
                break;
            case "Judge":
                courtCameraManager.ZoomIn(1);
                nameText.color = Color.gray;
                nameText.text = "판사";
                break;
            case "Lawyer":
                courtCameraManager.ZoomIn(2);
                nameText.color = Color.cyan;
                nameText.text = "변호사";
                break;
        }

        while (sentencesQueue.Count > 0)
        {
            string sentence = sentencesQueue.Dequeue();
            isAbleToGoNext = false;
            IsReadyToSkip = true;

            yield return StartCoroutine(ShowLine(GetLineText(), sentence));

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
        ChangeIsSkipping(true);
        displayCoroutine = null;

        if (targetRole == CourtRole.Player)
        {
            SetActiveEndConversationButton(true);
            SetInteractableAskField(true);
            FocusOnAskField();
            courtCameraManager.StopInputCheckRoutine();
        }
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
            lineText.color = Color.white;
        }
    }

    void FocusOnAskField()
    {
        inputField.Select();
        inputField.ActivateInputField(); // InputField Ȱ��ȭ
    }

    // NPC �亯 ��ȯ
    public Text GetLineText()
    {
        return lineText;
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



    
    public void SetSubjectRole(int index)
    {
        if(index == 0) subjectRole = CourtRole.Player;
        else if(index == 1) subjectRole = CourtRole.Judge;
        else if(index == 2) subjectRole = CourtRole.Lawyer;
        else
        {
            Debug.LogError("SetSubjectRole: index out of range");
            return;
        }        
    }

    public void SetTargetRole(int index)
    {
        if (index == 0) targetRole = CourtRole.Player;
        else if (index == 1) targetRole = CourtRole.Judge;
        else if (index == 2) targetRole = CourtRole.Lawyer;
        else
        {
            Debug.LogError("SetSubjectRole: index out of range");
            return;
        }    
    }



    public void ClickRoleButton(int index)
    {
        for (int i = 0; i < roleButtons.Length; i++)
        {
            if (i == index)
            {
                // 버튼 색상 변경
                SetButtonSelectedColor(roleButtons[i], Color.blue);

                // 선택 상태로 만들기
                roleButtons[i].Select();
                currentSelectedButton = roleButtons[i];
            }
            else
            {
                // 다른 버튼은 해제 색상
                SetButtonSelectedColor(roleButtons[i], Color.white);
            }
        }
    }

    private void SetButtonSelectedColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;     // 일반 상태 색
        colors.selectedColor = color;   // 선택 상태 색
        button.colors = colors;
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
