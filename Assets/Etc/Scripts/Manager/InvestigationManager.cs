using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InvestigationManager : MonoBehaviour
{
    private AudioSource[] audioSources;

    [SerializeField] private Transform seatTransform;  // 용의자가 앉을 위치
    [SerializeField] private SpecificNPC currentNPC;

    [SerializeField] private GameObject[] suspectPrefabs = new GameObject[3];    // Resources 폴더의 용의자 프리팹 배열
    private readonly string[] suspectNames = new string[] { "William", "Emma", "John" };    // 용의자 프리팹 이름들
    
    public AudioClip[] audioClips; // 0 - 문 여는 소리
    public AudioClip[] npcVoices;

    public Animator npcAnimator;
    public RuntimeAnimatorController[] controllers; // 0 - 대화 시, 1 - 독백 시
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

        // 실제 할때는 아래 코드들 다 지우기
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
                Debug.LogError($"프리팹을 찾을 수 없습니다: {suspectNames[i]}");
            }
        }
    }

    /// <summary>
    /// 용의자 버튼 클릭 시 호출되는 메서드입니다.
    /// </summary>
    /// <param name="index">클릭된 용의자의 인덱스</param>
    /// <remarks>
    /// 선택된 용의자의 정보를 프로필 매니저를 통해 UI에 표시하고,
    /// 용의자 선택 버튼 패널을 비활성화합니다.
    /// </remarks>
    public void ClickSuspect(int index)
    {
        if (index >= 0 && index < suspectNames.Length)
        {
            if (suspectPrefabs[index] == null)
            {
                Debug.LogError($"용의자 프리팹이 null입니다: {suspectNames[index]}");
                return;
            }

            if (seatTransform == null)
            {
                Debug.LogError("seatTransform이 할당되지 않았습니다.");
                return;
            }

            GameObject instantiatedObject = Instantiate(suspectPrefabs[index], seatTransform.position, Quaternion.identity);

            if (instantiatedObject == null)
            {
                Debug.LogError("프리팹 생성 실패");
                return;
            }

            currentNPC = instantiatedObject.GetComponent<SpecificNPC>();
            npcAnimator = currentNPC.GetComponent<Animator>();

            // summaryManager.SetSpecificNPC(currentNPC);

            if (currentNPC == null)
            {
                Debug.LogError($"SpecificNPC 컴포넌트를 찾을 수 없습니다: {instantiatedObject.name}");
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

        // currentNPC 게임 오브젝트 삭제
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
        yield return StartCoroutine(ShowText("윌리엄", "헨리는 제 연인 엠마의 소개로 알게 된 사람입니다."));

        SetNPCVocie(1);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Joy");
        yield return StartCoroutine(ShowText("윌리엄", "처음엔 그의 병원이 겉보기에는 안정적으로 잘 운영되고 있는 듯 보였죠.", SecondScene));
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
        yield return StartCoroutine(ShowText("윌리엄", "엠마의 추천이기도 해서, 저는 큰 의심 없이 투자를 결정했습니다."));
        
        SetNPCVocie(3);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Surprise");
        yield return StartCoroutine(ShowText("윌리엄", "하지만 재정을 직접 확인하던 중 충격적인 사실을 알게 되었습니다.", ThirdScene));
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
        yield return StartCoroutine(ShowText("윌리엄", "헨리는 분식회계를 통해 수익을 부풀리고, 의료사고를 은폐하고 있었습니다."));
        
        SetNPCVocie(5);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Fear");
        yield return StartCoroutine(ShowText("윌리엄", "저는 투자 철회를 요구했지만, 헨리는 오히려 저를 상대로 소송을 제기했습니다.", FourthScene));
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
        yield return StartCoroutine(ShowText("윌리엄", "그러나 그 소송은 결국 의미가 없어지고 말았죠…"));

        SetNPCVocie(7);
        audioSources[1].Play();
        npcAnimator.SetTrigger("Disgust");
        yield return StartCoroutine(ShowText("윌리엄", "소송 준비를 하던 중, 헨리가 사망했다는 소식을 듣게 되었으니까요.", LastScene));
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
            (ShowText("윌리엄", "저는 그를 죽이지 않았습니다. 제가 알고 있는 건 전부 말씀드렸습니다.", () => SetActiveTutorial(true)));
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
        conversationManager.GetNPCLine.text = ""; // 초기화

        currentNPC.PlayEmotion(fullText);

        // 글자 하나씩 출력
        foreach (char c in fullText)
        {
            conversationManager.GetNPCLine.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        // 모든 텍스트가 출력된 후 → 입력 대기
        bool waitingInput = true;
        conversationManager.GetEndWaitingMark.SetActive(true); // 입력 대기 마크 활성화
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

        conversationManager.GetEndWaitingMark.SetActive(false); // 입력 대기 마크 비활성화

        // 입력이 감지되면 다음 액션 실행
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
