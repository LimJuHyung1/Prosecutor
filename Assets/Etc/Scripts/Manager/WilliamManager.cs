using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WilliamManager : MonoBehaviour
{
    public Image screen;
    public Image image1;
    public Image image2;
    public GameObject dialogue;    
    
    [SerializeField] private Image[] slides = new Image[2];
    [SerializeField] private Text NPCName;
    [SerializeField] private Text line;
    [SerializeField] private GameObject endWaitingMark;

    public AudioClip[] audioClips; // 오디오 클립 배열 (필요시 사용)
    public Camera[] cameras;
    public Sprite[] sprites;    // 0 - 윌리엄 사진, 1 - 기사, 2 - 고소, 3 - 헨리 시신으로 발견된 장면
    public GameObject phone;
    
    public Profile profileSO;
    public ProfileManager profileManager;

    public GameObject dataPanel;
    public GameObject profilePage;
    public Image newsPage;
    public Image crimeScenePage;


    private float delay = 0.05f;     // 글자당 출력 속도 (초)
    private Action[] methods;
    private AudioSource audioSource;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SwitchCamera(-1);

        // 메소드들을 배열에 등록
        methods = new Action[]
        {
            FirstScene,
            SecondScene
        };

        image1.gameObject.SetActive(false);
        image2.gameObject.SetActive(false);

        audioSource = GetComponent<AudioSource>();

        NPCName = dialogue.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        line = dialogue.transform.GetChild(1).GetChild(1).GetComponent<Text>();
        endWaitingMark = dialogue.transform.GetChild(1).GetChild(2).gameObject;
        slides[0] = dialogue.transform.GetChild(0).GetComponent<Image>();
        slides[1] = dialogue.transform.GetChild(1).GetComponent<Image>();

        dataPanel.gameObject.SetActive(false);
        // Slide(true, 0f);
        // SwitchCamera(0);
        methods[0]();
    }

    private void SwitchCamera(int index)
    {
        if (line != null)
            line.text = "";

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        if (index != -1)
            cameras[index].gameObject.SetActive(true);
    }

    private void SetAudioClip(int index)
    {
        audioSource.clip = audioClips[index];
    }

    public IEnumerator ShowText(string NPCName = "", string fullText = "", Action onFinished = null)
    {
        this.NPCName.text = NPCName;
        line.text = ""; // 초기화

        // 글자 하나씩 출력
        foreach (char c in fullText)
        {
            line.text += c;
            yield return new WaitForSeconds(delay);
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








    public void FirstScene()
    {
        SwitchCamera(0);
        Slide(true);

        StartCoroutine(FirstCamera());
    }

    private IEnumerator FirstCamera()
    {
        yield return new WaitForSeconds(2f);

        // yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 3f));
        Slide(true, 0f);

        SetAudioClip(0);
        audioSource.Play();        

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(LookAtPhone(2f));

        SetAudioClip(1);
        audioSource.Play();
        yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 1f));

        yield return new WaitForSeconds(2f);

        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("케인", "검사님, 방금 뉴스 보셨습니까?"));

        NPCName.color = Color.darkRed;
        yield return StartCoroutine(ShowText("수잔", "헨리 병원장 사건 말씀하시는 거죠?"));

        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("케인", "예. 그 사건이 검사님께 배정될 예정입니다."));
        yield return StartCoroutine(ShowText("케인", "곧 사건 자료를 정리해 전달드리겠습니다."));

        NPCName.color = Color.darkRed;
        yield return StartCoroutine(ShowText("수잔", "네, 준비하고 있겠습니다."));

        SetAudioClip(2);
        audioSource.Play();
        yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 1f));

        yield return StartCoroutine(ShowText("수잔", "(유명 병원의 병원장이 공사 현장에서 살해된 채로 발견되었다)"));
        yield return StartCoroutine(ShowText("수잔", "(우선 헨리의 주변 인물부터 조사해야 되겠어)"));

        yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 3f));

        yield return StartCoroutine(ShowText("", "", SecondScene));
    }

    // cameras[0]이 phone을 향해 회전하는 코루틴
    private IEnumerator LookAtPhone(float duration = 2f)
    {
        Camera cam = cameras[0];
        if (cam == null || phone == null)
            yield break;

        // 시작 회전값과 목표 회전값 저장
        Quaternion startRot = cam.transform.rotation;
        Vector3 dir = (phone.transform.position - cam.transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 부드럽게 회전
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // 최종적으로 phone을 정확히 바라보도록 설정
        cam.transform.rotation = targetRot;
    }


    public void SecondScene()
    {
        // SwitchCamera(0);

        StartCoroutine(SecondCamera());
    }

    private IEnumerator SecondCamera()
    {
        // 윌리엄 관련 UI 활성화

        // yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 3f));

        image1.sprite = sprites[0];
        StartCoroutine(FadeUtility.Instance.FadeIn(image1, 1f));
        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("케인", "이 사람은 윌리엄, 오리온 캐피탈의 부사장입니다."));

        image2.sprite = sprites[1];
        StartCoroutine(FadeUtility.Instance.FadeIn(image2, 3f));
        yield return StartCoroutine(ShowText("케인", "한 달 전, 헨리의 병원에 거액을 투자했다는 사실이 언론에 보도됐습니다."));
        yield return StartCoroutine(ShowText("케인", "그런데 최근 병원 재정을 검토하는 과정에서 분식회계와 의료사고 은폐 정황을 발견했고,\n투자 철회를 검토했다고 합니다."));

        NPCName.color = Color.darkRed;
        yield return StartCoroutine(ShowText("수잔", "헨리 입장에서는 투자금을 돌려줄 수 없었을 겁니다. 어떻게든 막으려 했겠군요."));

        StartCoroutine(FadeUtility.Instance.FadeOut(image1, 1.5f));
        yield return StartCoroutine(FadeUtility.Instance.FadeOut(image2, 1.5f));
        
        image1.sprite = sprites[2];
        image2.sprite = sprites[3];
        StartCoroutine(FadeUtility.Instance.FadeIn(image1, 1f));
        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("케인", "맞습니다. 실제로 헨리는 투자 철회가 부당하다며 윌리엄을 상대로 소송을 제기했습니다."));

        StartCoroutine(FadeUtility.Instance.FadeIn(image2, 1f));
        yield return StartCoroutine(ShowText("케인", "그리고 그 직후, 헨리가 살해당한 겁니다."));

        NPCName.color = Color.darkRed;
        yield return StartCoroutine(ShowText("수잔", "윌리엄이 사건에 연루됐을 가능성을 배제할 수 없겠군요."));
        yield return StartCoroutine(ShowText("수잔", "두 사람 사이에 무슨 일이 있었는지 좀 더 깊이 파악해야겠습니다."));

        StartCoroutine(FadeUtility.Instance.FadeOut(image1, 1.5f));
        yield return StartCoroutine(FadeUtility.Instance.FadeOut(image2, 1.5f));

        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("케인", "곧 윌리엄을 조사실에서 직접 대면하게 될 예정입니다."));
        yield return StartCoroutine(ShowText("케인", "조사실에 윌리엄 관련 자료 및 사건 현장 자료가 준비되어 있습니다."));
        yield return StartCoroutine(ShowText("케인", "자, 출발하시죠."));

        // Data UI 활성

        NPCName.text = "";
        line.text = "";
        Slide(false, 1f);        
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Interrogation Room 1");
        // dataPanel.gameObject.SetActive(true);
    }





    public void ProfileButton()
    {
        profileManager.UpdateProfileUI(profileSO);
        profilePage.gameObject.SetActive(true);
    }

    public void NewsButton()
    {
        newsPage.gameObject.SetActive(true);
    }

    public void CrimeSceneButton()
    {
        crimeScenePage.gameObject.SetActive(true);
    }

    public void InvestigateWilliam()
    {
        // SceneManager.LoadScene();
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
        float y = isOn ? 0f : (index == 0 ? tmp.rect.height : -tmp.rect.height);
        Vector2 targetPosition = new Vector2(tmp.anchoredPosition.x, y);

        StartCoroutine(SmoothMove(tmp, tmp.anchoredPosition, targetPosition));
    }

    private IEnumerator SmoothMove(RectTransform rect, Vector2 startPos, Vector2 endPos)
    {
        float elapsed = 0f;

        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 1.0f);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rect.anchoredPosition = endPos;
    }
}
