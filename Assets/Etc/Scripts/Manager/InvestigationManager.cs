using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InvestigationManager : MonoBehaviour
{
    private AudioSource[] audioSources;

    [SerializeField] private Transform seatTransform;  // �����ڰ� ���� ��ġ
    [SerializeField] private SpecificNPC currentNPC;

    [SerializeField] private GameObject[] suspectPrefabs = new GameObject[3];    // Resources ������ ������ ������ �迭
    private readonly string[] suspectNames = new string[] { "William", "Emma", "John" };    // ������ ������ �̸���
    
    public AudioClip[] audioClips; // 0 - �� ���� �Ҹ�
    public AudioClip[] npcVoices;

    public Animator npcAnimator;
    public RuntimeAnimatorController[] controllers; // 0 - ��ȭ ��, 1 - ���� ��
    public ConversationManager conversationManager;
    public Image screen;
    public Image tutorial;

    public GameObject data;
    private Button startInvestigationButton;
    private Image profilePage;
    private Image newsPage;
    private Image crimeScenePage;   
    
    public Timer timer;

    private bool[] checkData = new bool[3];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSources = GetComponents<AudioSource>();

        screen.gameObject.SetActive(true);
        SetActiveTutorial(false);
        timer.StopTimer();
        timer.gameObject.SetActive(false);
        
        profilePage = data.transform.GetChild(4).GetComponent<Image>();
        newsPage = data.transform.GetChild(5).GetComponent<Image>();
        crimeScenePage = data.transform.GetChild(6).GetComponent<Image>();
        startInvestigationButton = data.transform.GetChild(3).GetComponent<Button>();
        data.gameObject.SetActive(false);
        startInvestigationButton.gameObject.SetActive(false);

        for (int i = 0; i < checkData.Length; i++)
        {
            checkData[i] = false;
        }

        LoadPrefabsFromResources();
        ClickSuspect(0);        

        // ���� �Ҷ��� �Ʒ� �ڵ�� �� �����
        conversationManager.Slide(true);
        conversationManager.dialogue.SetActive(true);
        conversationManager.StartConversation(currentNPC);
        StartInterrogation();        
        SetNPCAnimator(0);
    }

    private void LoadPrefabsFromResources()
    {
        for (int i = 0; i < suspectNames.Length; i++)
        {
            suspectPrefabs[i] = Resources.Load<GameObject>($"{suspectNames[i]}");

            if (suspectPrefabs[i] == null)
            {
                Debug.LogError($"�������� ã�� �� �����ϴ�: {suspectNames[i]}");
            }
        }
    }

    /// <summary>
    /// ������ ��ư Ŭ�� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="index">Ŭ���� �������� �ε���</param>
    /// <remarks>
    /// ���õ� �������� ������ ������ �Ŵ����� ���� UI�� ǥ���ϰ�,
    /// ������ ���� ��ư �г��� ��Ȱ��ȭ�մϴ�.
    /// </remarks>
    public void ClickSuspect(int index)
    {
        if (index >= 0 && index < suspectNames.Length)
        {
            if (suspectPrefabs[index] == null)
            {
                Debug.LogError($"������ �������� null�Դϴ�: {suspectNames[index]}");
                return;
            }

            if (seatTransform == null)
            {
                Debug.LogError("seatTransform�� �Ҵ���� �ʾҽ��ϴ�.");
                return;
            }

            GameObject instantiatedObject = Instantiate(suspectPrefabs[index], seatTransform.position, Quaternion.identity);

            if (instantiatedObject == null)
            {
                Debug.LogError("������ ���� ����");
                return;
            }

            currentNPC = instantiatedObject.GetComponent<SpecificNPC>();
            npcAnimator = currentNPC.GetComponent<Animator>();

            // summaryManager.SetSpecificNPC(currentNPC);

            if (currentNPC == null)
            {
                Debug.LogError($"SpecificNPC ������Ʈ�� ã�� �� �����ϴ�: {instantiatedObject.name}");
                return;
            }

            currentNPC.GetConversationManager(conversationManager);
            conversationManager.GetNPCRole(currentNPC);
            // StartInterrogation();
        }
    }

    public void StartInterrogation()
    {
        timer.gameObject.SetActive(true);
        timer.StartTimer();
        conversationManager.conversationUI.SetActive(true);
        SetActiveTutorial(false);
        AudioManager.Instance.PlayInterrogationAudio();

        // conversationManager.StartConversation(currentNPC);
    }

    public void EndInterrogation()
    {
        timer.StopTimer();
        timer.gameObject.SetActive(false);

        AudioManager.Instance.PlayProfileAudio();

        conversationManager.EndConversation();
        conversationManager.SetInteractableAskField(true);

        // currentNPC ���� ������Ʈ ����
        if (currentNPC != null)
        {
            Destroy(currentNPC.gameObject);
            currentNPC = null;
        }

        // summaryManager.Summary();
    }


    public void FirstScene()
    {        
        StartCoroutine(FirstCoroutine());
    }

    private IEnumerator FirstCoroutine()
    {
        SetAudioClip(0);
        SetNPCAnimator(1);
        audioSources[0].Play();
        conversationManager.Slide(true);
        conversationManager.dialogue.SetActive(true);

        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 1f));        

        conversationManager.GetNPCName.color = Color.blue;        

        yield return new WaitForSeconds(3f);
        
        SetNPCVocie(0);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Netural");
        yield return StartCoroutine(ShowText("������", "��� �� ���� ������ �Ұ��� �˰� �� ����Դϴ�."));

        SetNPCVocie(1);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Joy");
        yield return StartCoroutine(ShowText("������", "ó���� ���� ������ �Ѻ��⿡�� ���������� �� ��ǰ� �ִ� �� ������.", SecondScene));
    }

    public void SecondScene()
    {
        StartCoroutine(SecondCoroutine());
    }

    private IEnumerator SecondCoroutine()
    {
        conversationManager.GetNPCName.color = Color.blue;
        
        SetNPCVocie(2);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Netural");
        yield return StartCoroutine(ShowText("������", "������ ��õ�̱⵵ �ؼ�, ���� ū �ǽ� ���� ���ڸ� �����߽��ϴ�."));
        
        SetNPCVocie(3);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Surprise");
        yield return StartCoroutine(ShowText("������", "������ ������ ���� Ȯ���ϴ� �� ������� ����� �˰� �Ǿ����ϴ�.", ThirdScene));
    }

    public void ThirdScene()
    {
        StartCoroutine(ThirdCoroutine());
    }

    private IEnumerator ThirdCoroutine()
    {
        conversationManager.GetNPCName.color = Color.blue;
        
        SetNPCVocie(4);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Anger");
        yield return StartCoroutine(ShowText("������", "��� �н�ȸ�踦 ���� ������ ��Ǯ����, �Ƿ��� �����ϰ� �־����ϴ�."));
        
        SetNPCVocie(5);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Fear");
        yield return StartCoroutine(ShowText("������", "���� ���� öȸ�� �䱸������, ��� ������ ���� ���� �Ҽ��� �����߽��ϴ�.", FourthScene));
    }

    public void FourthScene()
    {
        StartCoroutine(FourthCoroutine());
    }

    private IEnumerator FourthCoroutine()
    {
        conversationManager.GetNPCName.color = Color.blue;
        
        SetNPCVocie(6);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Sadness");
        yield return StartCoroutine(ShowText("������", "�׷��� �� �Ҽ��� �ᱹ �ǹ̰� �������� �����ҡ�"));

        SetNPCVocie(7);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Disgust");
        yield return StartCoroutine(ShowText("������", "�Ҽ� �غ� �ϴ� ��, ��� ����ߴٴ� �ҽ��� ��� �Ǿ����ϱ��.", LastScene));
    }

    public void LastScene()
    {
        StartCoroutine(LastCoroutine());
    }

    private IEnumerator LastCoroutine()
    {
        conversationManager.GetNPCName.color = Color.blue;
        SetNPCVocie(8);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Netural");
        yield return StartCoroutine
            (ShowText("������", "���� �׸� ������ �ʾҽ��ϴ�. ���� �˰� �ִ� �� ���� ������Ƚ��ϴ�.", () => SetActiveTutorial(true)));
        conversationManager.StartConversation(currentNPC);
        SetNPCAnimator(0);
    }




    private void SetAudioClip(int index)
    {
        audioSources[0].clip = audioClips[index];
    }

    private void SetNPCVocie(int index)
    {
        audioSources[1].clip = npcVoices[index];
    }

    private void SetNPCAnimator(int index)
    {
        Animator anim = currentNPC.GetComponent<Animator>();
        anim.runtimeAnimatorController = controllers[index];
    }

    public IEnumerator ShowText(string NPCName = "", string fullText = "", Action onFinished = null)
    {
        conversationManager.GetNPCName.text = NPCName;
        conversationManager.GetNPCLine.text = ""; // �ʱ�ȭ

        currentNPC.PlayEmotion(fullText);

        // ���� �ϳ��� ���
        foreach (char c in fullText)
        {
            conversationManager.GetNPCLine.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        // ��� �ؽ�Ʈ�� ��µ� �� �� �Է� ���
        bool waitingInput = true;
        conversationManager.GetEndWaitingMark.SetActive(true); // �Է� ��� ��ũ Ȱ��ȭ
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

            yield return null; // ���� �����ӱ��� ���
        }

        conversationManager.GetEndWaitingMark.SetActive(false); // �Է� ��� ��ũ ��Ȱ��ȭ

        // �Է��� �����Ǹ� ���� �׼� ����
        if (onFinished != null)
            onFinished();
    }




    public void SetActiveTutorial(bool isActive)
    {
        tutorial.gameObject.SetActive(isActive);
    }

    public void SetActiveData(bool state)
    {
        data.gameObject.SetActive(state);
    }

    public void SetActiveProfilePage(bool state)
    {
        profilePage.gameObject.SetActive(state);
        SetCheckData(0);
        CheckData();
    }

    public void SetActiveNewsPage(bool state)
    {
        newsPage.gameObject.SetActive(state);
        SetCheckData(1);
        CheckData();
    }

    public void SetActiveCrimeScenePage(bool state)
    {
        crimeScenePage.gameObject.SetActive(state);
        SetCheckData(2);
        CheckData();
    }

    public void SetActiveStartInvestigationButton(bool state)
    {
        startInvestigationButton.gameObject.SetActive(state);
    }

    private void SetCheckData(int index)
    {
        checkData[index] = true;
    }

    private void CheckData()
    {
        for (int i = 0; i < checkData.Length; i++)
        {
            if (checkData[i] == false)
                return;
        }

        SetActiveStartInvestigationButton(true);
    }
}
