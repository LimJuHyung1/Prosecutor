using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("프리팹 참조")]
    [SerializeField] private GameObject[] suspectPrefabs = new GameObject[3];    // Resources 폴더의 용의자 프리팹 배열
    private readonly string[] suspectNames = new string[] { "William", "Emma", "John" };    // 용의자 프리팹 이름들
    [SerializeField] private Transform seatTransform;  // 용의자가 앉을 위치
    private SpecificNPC currentNPC;

    public AudioSource audioSource;
    public AudioClip audioClip; // 오디오 클립 배열

    public ProfileManager profileManager;
    public ConversationManager conversationManager;
    public SummaryManager summaryManager;
    public InterrogationStageManager interrogationStageManager;
    public Timer timer;

    public GameObject profileObject;            
    public Profile[] profiles = new Profile[3];

    public GameObject suspectButtonPanel;
    private Button[] suspectButtons = new Button[3];

    [Header("UI 참조")]
    public Image dayImage;
    private TextMeshProUGUI dayText;
    [SerializeField] private int day = 1;

    /// <summary>
    /// MonoBehaviour가 생성된 후 Update가 처음 실행되기 전에 한 번 호출됩니다.
    /// UI 컴포넌트를 초기화하고, 버튼 이벤트를 등록하며, 초기 설정을 수행합니다.
    /// </summary>
    void Start()
    {        
        dayText = dayImage.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        
        audioSource = GetComponent<AudioSource>();
        // 모든 버튼 컴포넌트를 한 번에 가져오기
        suspectButtons = suspectButtonPanel.GetComponentsInChildren<Button>();

        // 버튼 이벤트 등록
        for (int i = 0; i < suspectButtons.Length; i++)
        {
            int index = i; // 클로저를 위한 변수
            suspectButtons[i].onClick.AddListener(() => ClickSuspect(index));
        }

        timer.StopTimer();
        timer.gameObject.SetActive(false);

        // StartIntroScene();
        ShowDay();

        LoadPrefabsFromResources();        
        SetupSuspectButtons(true);
    }

    /// <summary>
    /// Resources 폴더에서 용의자 프리팹을 로드합니다.
    /// </summary>
    /// <remarks>
    /// suspectNames 배열에 정의된 이름을 기반으로 프리팹을 로드하며,
    /// 프리팹을 찾을 수 없는 경우 에러 로그를 출력합니다.
    /// </remarks>
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
    /// 현재 일차를 UI에 표시하고 페이드 효과를 적용합니다.
    /// 4일차와 7일차에는 모든 용의자 버튼을 다시 활성화합니다.
    /// </summary>
    /// <remarks>
    /// dayImage에 페이드 인/아웃 효과를 적용하고 다음 일차로 진행합니다.
    /// </remarks>
    public void ShowDay()
    {        
        dayText.text = "Day " + day;
        StartCoroutine(FadeUtility.Instance.FadeInOut(dayImage, 1f, 2f));
        
        // 4일차나 7일차일 때 모든 버튼 초기화
        if (day == 4 || day == 7)
        {
            ResetAllSuspectButtons();
        }                
    }

    public void NextDay(){
        SetupSuspectButtons(true);
        // summaryManager.HideSummary();
        day++;
        ShowDay();        
    }

    /// <summary>
    /// 모든 용의자 버튼을 초기 상태로 되돌립니다.
    /// </summary>
    private void ResetAllSuspectButtons()
    {
        foreach (Button button in suspectButtons)
        {
            // 버튼 활성화
            button.interactable = true;

            // 이미지 컴포넌트 초기화
            Image[] buttonImages = button.GetComponentsInChildren<Image>();
            foreach (Image img in buttonImages)
            {
                img.color = Color.white;
            }

            // Text 컴포넌트 초기화
            Text[] buttonTexts = button.GetComponentsInChildren<Text>();
            foreach (Text txt in buttonTexts)
            {
                txt.color = Color.black;
            }

            // TextMeshProUGUI 컴포넌트 초기화
            TextMeshProUGUI[] tmpTexts = button.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI tmp in tmpTexts)
            {
                tmp.color = Color.white;
            }
        }
    }

    /// <summary>
    /// 용의자 버튼 패널의 활성화 상태를 설정합니다.
    /// </summary>
    /// <param name="isActive">패널의 활성화 여부를 지정하는 불리언 값</param>
    private void SetupSuspectButtons(bool isActive)
    {
        suspectButtonPanel.SetActive(isActive);
    }

    private void SetupProfile(bool isActive){
        profileObject.SetActive(isActive);
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

            if(day < 4){
                instantiatedObject.GetComponent<SpecificNPC>().interrogationStage = SpecificNPC.InterrogationStage.Stage1;
                SetupProfile(true);                
            }
            else if(day < 7){
                instantiatedObject.GetComponent<SpecificNPC>().interrogationStage = SpecificNPC.InterrogationStage.Stage2;
                interrogationStageManager.SetInvestigatorIntroActive(true);
                interrogationStageManager.PlayInvestigatorIntro(SpecificNPC.InterrogationStage.Stage2, instantiatedObject.GetComponent<SpecificNPC>());
                // 수사관 인트로 만들고 끝나면 StartInterrogation();
            }
            else
            {
                instantiatedObject.GetComponent<SpecificNPC>().interrogationStage = SpecificNPC.InterrogationStage.StageFinal;
            }
            
            if (instantiatedObject == null)
            {
                Debug.LogError("프리팹 생성 실패");
                return;
            }
            
            currentNPC = instantiatedObject.GetComponent<SpecificNPC>();

            summaryManager.SetSpecificNPC(currentNPC);            

            if (currentNPC == null)
            {
                Debug.LogError($"SpecificNPC 컴포넌트를 찾을 수 없습니다: {instantiatedObject.name}");
                return;
            }

            currentNPC.GetConversationManager(conversationManager);
            SetupSuspectButtons(false);                
            profileManager.UpdateProfileUI(profiles[index]);

            // 버튼 비활성화 및 시각적 피드백
            suspectButtons[index].interactable = false;
            
            // 버튼과 모든 하위 오브젝트의 Image 컴포넌트 색상 변경
            Image[] buttonImages = suspectButtons[index].GetComponentsInChildren<Image>();
            foreach (Image img in buttonImages)
            {
                img.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            }
            
            // 버튼과 모든 하위 오브젝트의 Text 컴포넌트 색상 변경
            Text[] buttonTexts = suspectButtons[index].GetComponentsInChildren<Text>();
            foreach (Text txt in buttonTexts)
            {
                txt.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            // 버튼과 모든 하위 오브젝트의 TextMeshProUGUI 컴포넌트 색상 변경
            TextMeshProUGUI[] tmpTexts = suspectButtons[index].GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI tmp in tmpTexts)
            {
                tmp.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }
    }

    public void StartInterrogation(){        
        timer.gameObject.SetActive(true);
        timer.StartTimer();

        AudioManager.Instance.PlayInterrogationAudio();

        conversationManager.StartConversation(currentNPC);
        SetupProfile(false);                
    }

    public void EndInterrogation(){
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

        NextDay();
        // summaryManager.Summary();
    }

    public void StartIntroScene()
    {
        audioSource.Play();
    }
}
