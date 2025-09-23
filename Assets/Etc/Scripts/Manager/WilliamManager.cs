using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class WilliamManager : MonoBehaviour
{
    public Image screen;
    public Image []images;    
    public GameObject dialogue;    
    
    [SerializeField] private Image[] slides = new Image[2];
    [SerializeField] private Text NPCName;
    [SerializeField] private Text line;
    [SerializeField] private GameObject endWaitingMark;

    public AudioClip[] audioClips; // 오디오 클립 배열 (필요시 사용)
    public AudioClip[] susan1;
    public AudioClip[] susan2;
    public AudioClip[] cain1;
    public AudioClip[] cain2;
    public Camera[] cameras;
    public Sprite[] sprites;    // 0 - 윌리엄 사진, 1 - 기사, 2 - 고소, 3 - 헨리 시신으로 발견된 장면
    public GameObject phone;
    
    public Profile profileSO;
    public ProfileManager profileManager;

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

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
                images[i].gameObject.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();

        NPCName = dialogue.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        line = dialogue.transform.GetChild(1).GetChild(1).GetComponent<Text>();
        endWaitingMark = dialogue.transform.GetChild(1).GetChild(2).gameObject;
        slides[0] = dialogue.transform.GetChild(0).GetComponent<Image>();
        slides[1] = dialogue.transform.GetChild(1).GetComponent<Image>();

        // Slide(true, 0f);
        // SwitchCamera(0);
        methods[0]();

        var currentLocale = LocalizationSettings.SelectedLocale;
        if (currentLocale.Identifier.Code == "ja")
        {
            delay = 0.1f;
        }
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

    public IEnumerator ShowText(string NPCName = "", string fullText = "", AudioClip sound = null, Action onFinished = null)
    {
        this.NPCName.text = NPCName;
        line.text = ""; // 초기화
        audioSource.clip = sound;
        audioSource.Play();

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
        var currentLocale = LocalizationSettings.SelectedLocale;

        Slide(true, 0f);

        SetAudioClip(0);
        audioSource.Play();        

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(LookAtPhone(2f));

        SetAudioClip(1);
        audioSource.Play();
        yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 1f));

        yield return new WaitForSeconds(2f);

        if (currentLocale.Identifier.Code == "en")
        {
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "Prosecutor, did you see the news just now?", cain1[0]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("Susan", "You mean the case of Director Henry from the hospital, right?", susan1[0]));

            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "Yes. That case is going to be assigned to you.", cain1[1]));
            yield return StartCoroutine(ShowText("Cain", "I’ll organize the case files and deliver them shortly.", cain1[2]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("Susan", "Understood, I’ll be ready.", susan1[1]));

            SetAudioClip(2);
            audioSource.Play();
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 1f));

            yield return StartCoroutine(ShowText("Susan", "(The director of a well-known hospital was found murdered at a construction site.)", susan1[2]));
            yield return StartCoroutine(ShowText("Susan", "(First, I need to investigate the people around Henry.)", susan1[3]));

            yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 3f));

            yield return StartCoroutine(ShowText("", "", null, SecondScene));
        }
        else if(currentLocale.Identifier.Code == "it")
        {
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "Procuratore, ha visto il telegiornale poco fa?", cain1[0]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("Susan", "Si riferisce al caso del direttore Henry dell’ospedale, giusto?", susan1[0]));

            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "Esatto. Quel caso sarà assegnato a lei.", cain1[1]));
            yield return StartCoroutine(ShowText("Cain", "Presto organizzerò i fascicoli e glieli consegnerò.", cain1[2]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("Susan", "Va bene, mi farò trovare pronta.", susan1[1]));

            SetAudioClip(2);
            audioSource.Play();
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 1f));

            yield return StartCoroutine(ShowText("Susan", "(Il direttore di un noto ospedale è stato trovato assassinato in un cantiere.)", susan1[2]));
            yield return StartCoroutine(ShowText("Susan", "(Per prima cosa devo indagare sulle persone vicine a Henry.)", susan1[3]));

            yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 3f));

            yield return StartCoroutine(ShowText("", "", null, SecondScene));
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("ケイン", "検事、さっきのニュースをご覧になりましたか？", cain1[0]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("スーザン", "ヘンリー院長の事件のことですよね？", susan1[0]));

            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("ケイン", "はい。その事件は検事さんに配属される予定です。", cain1[1]));
            yield return StartCoroutine(ShowText("ケイン", "まもなく資料を整理してお渡しします。", cain1[2]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("スーザン", "わかりました。準備しておきます。", susan1[1]));

            SetAudioClip(2);
            audioSource.Play();
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 1f));

            yield return StartCoroutine(ShowText("スーザン", "（有名な病院の院長が工事現場で殺害された状態で発見された）", susan1[2]));
            yield return StartCoroutine(ShowText("スーザン", "（まずはヘンリーの周囲の人物から調べなければ）", susan1[3]));

            yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 3f));

            yield return StartCoroutine(ShowText("", "", null, SecondScene));
        }
        else if( currentLocale.Identifier.Code == "ko")
        {
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("케인", "검사님, 방금 뉴스 보셨습니까?", cain1[0]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("수잔", "헨리 병원장 사건 말씀하시는 거죠?", susan1[0]));

            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("케인", "예. 그 사건이 검사님께 배정될 예정입니다.", cain1[1]));
            yield return StartCoroutine(ShowText("케인", "곧 사건 자료를 정리해 전달드리겠습니다.", cain1[2]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("수잔", "네, 준비하고 있겠습니다.", susan1[1]));

            SetAudioClip(2);
            audioSource.Play();
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 1f));

            yield return StartCoroutine(ShowText("수잔", "(유명 병원의 병원장이 공사 현장에서 살해된 채로 발견되었다)", susan1[2]));
            yield return StartCoroutine(ShowText("수잔", "(우선 헨리의 주변 인물부터 조사해야 되겠어)", susan1[3]));

            yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 3f));

            yield return StartCoroutine(ShowText("", "", null, SecondScene));
        }
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
        var currentLocale = LocalizationSettings.SelectedLocale;

        if (currentLocale.Identifier.Code == "en")
        {
            StartCoroutine(FadeUtility.Instance.FadeIn(images[0], 1f));
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "This man is William, the vice president of Orion Capital.", cain2[0]));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[1], 3f));
            yield return StartCoroutine(ShowText("Cain", "About a month ago, the media reported that he had invested a huge sum in Henry’s hospital.", cain2[1]));
            yield return StartCoroutine(ShowText("Cain", "But recently, during a financial review of the hospital, evidence of accounting fraud and medical malpractice cover-ups was uncovered, and he reportedly considered withdrawing the investment.", cain2[2]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("Susan", "From Henry’s standpoint, he couldn’t return the investment. He must have tried to block it at all costs.", susan2[0]));

            StartCoroutine(FadeUtility.Instance.FadeOut(images[0], 1.5f));
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(images[1], 1.5f));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[2], 1f));
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "That’s right. In fact, Henry even filed a lawsuit against William, claiming the withdrawal was unjustified.", cain2[3]));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[3], 1f));
            yield return StartCoroutine(ShowText("Cain", "And shortly after that, Henry was murdered.", cain2[4]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("Susan", "We can’t rule out the possibility that William was involved in the case.", susan2[1]));
            yield return StartCoroutine(ShowText("Susan", "We need to dig deeper into what really happened between the two.", susan2[2]));

            StartCoroutine(FadeUtility.Instance.FadeOut(images[2], 1.5f));
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(images[3], 1.5f));

            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "You’ll be facing William directly in the interrogation room soon.", cain2[5]));
            yield return StartCoroutine(ShowText("Cain", "Come on, let’s go.", cain2[6]));
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            StartCoroutine(FadeUtility.Instance.FadeIn(images[0], 1f));
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "Quest’uomo è William, il vicepresidente di Orion Capital.", cain2[0]));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[1], 3f));
            yield return StartCoroutine(ShowText("Cain", "Un mese fa circa, i media hanno riportato che aveva investito una grossa somma nell’ospedale di Henry.", cain2[1]));
            yield return StartCoroutine(ShowText("Cain", "Ma di recente, durante una revisione finanziaria dell’ospedale, sono emerse prove di falso in bilancio e di occultamento di casi medici,\ne pare che abbia preso in considerazione il ritiro dell’investimento.", cain2[2]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("Susan", "Dal punto di vista di Henry, non poteva restituire i fondi. Avrà cercato di impedirlo a tutti i costi.", susan2[0]));

            StartCoroutine(FadeUtility.Instance.FadeOut(images[0], 1.5f));
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(images[1], 1.5f));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[2], 1f));
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "Esatto. In effetti Henry ha persino intentato una causa contro William, sostenendo che il ritiro fosse ingiustificato.", cain2[3]));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[3], 1f));
            yield return StartCoroutine(ShowText("Cain", "E subito dopo, Henry è stato assassinato.", cain2[4]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("Susan", "Non si può escludere la possibilità che William sia coinvolto nel caso.", susan2[1]));
            yield return StartCoroutine(ShowText("Susan", "Dobbiamo approfondire cosa sia realmente successo tra i due.", susan2[2]));

            StartCoroutine(FadeUtility.Instance.FadeOut(images[2], 1.5f));
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(images[3], 1.5f));

            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("Cain", "Presto affronterà William direttamente nella sala degli interrogatori.", cain2[5]));
            yield return StartCoroutine(ShowText("Cain", "Andiamo, è ora di partire.", cain2[6]));
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            StartCoroutine(FadeUtility.Instance.FadeIn(images[0], 1f));
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("ケイン", "この人物はウィリアム、オリオン・キャピタルの副社長です。", cain2[0]));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[1], 3f));
            yield return StartCoroutine(ShowText("ケイン", "1か月前、彼がヘンリーの病院に巨額の投資をしたと報じられました。", cain2[1]));
            yield return StartCoroutine(ShowText("ケイン", "しかし最近、病院の財務を精査する過程で粉飾決算や医療事故の隠蔽の疑いが発覚し、\n投資の撤回を検討していたといいます。", cain2[2]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("スーザン", "ヘンリーの立場からすれば、投資金を返すことはできなかったでしょう。何としても阻止しようとしたはずです。", susan2[0]));

            StartCoroutine(FadeUtility.Instance.FadeOut(images[0], 1.5f));
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(images[1], 1.5f));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[2], 1f));
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("ケイン", "その通りです。実際にヘンリーは、投資撤回は不当だとしてウィリアムを相手に訴訟を起こしました。", cain2[3]));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[3], 1f));
            yield return StartCoroutine(ShowText("ケイン", "そしてその直後、ヘンリーは殺害されたのです。", cain2[4]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("スーザン", "ウィリアムが事件に関与していた可能性は否定できませんね。", susan2[1]));
            yield return StartCoroutine(ShowText("スーザン", "二人の間で何があったのか、さらに深く調べる必要があります。", susan2[2]));

            StartCoroutine(FadeUtility.Instance.FadeOut(images[2], 1.5f));
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(images[3], 1.5f));

            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("ケイン", "まもなく取調室でウィリアムと直接対面することになります。", cain2[5]));
            yield return StartCoroutine(ShowText("ケイン", "さあ、行きましょう。", cain2[6]));
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            StartCoroutine(FadeUtility.Instance.FadeIn(images[0], 1f));
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("케인", "이 사람은 윌리엄, 오리온 캐피탈의 부사장입니다.", cain2[0]));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[1], 3f));
            yield return StartCoroutine(ShowText("케인", "한 달 전, 헨리의 병원에 거액을 투자했다는 사실이 언론에 보도됐습니다.", cain2[1]));
            yield return StartCoroutine(ShowText("케인", "그런데 최근 병원 재정을 검토하는 과정에서 분식회계와 의료사고 은폐 정황을 발견했고,\n투자 철회를 검토했다고 합니다.", cain2[2]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("수잔", "헨리 입장에서는 투자금을 돌려줄 수 없었을 겁니다. 어떻게든 막으려 했겠군요.", susan2[0]));

            StartCoroutine(FadeUtility.Instance.FadeOut(images[0], 1.5f));
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(images[1], 1.5f));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[2], 1f));
            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("케인", "맞습니다. 실제로 헨리는 투자 철회가 부당하다며 윌리엄을 상대로 소송을 제기했습니다.", cain2[3]));

            StartCoroutine(FadeUtility.Instance.FadeIn(images[3], 1f));
            yield return StartCoroutine(ShowText("케인", "그리고 그 직후, 헨리가 살해당한 겁니다.", cain2[4]));

            NPCName.color = Color.darkRed;
            yield return StartCoroutine(ShowText("수잔", "윌리엄이 사건에 연루됐을 가능성을 배제할 수 없겠군요.", susan2[1]));
            yield return StartCoroutine(ShowText("수잔", "두 사람 사이에 무슨 일이 있었는지 좀 더 깊이 파악해야겠습니다.", susan2[2]));

            StartCoroutine(FadeUtility.Instance.FadeOut(images[2], 1.5f));
            yield return StartCoroutine(FadeUtility.Instance.FadeOut(images[3], 1.5f));

            NPCName.color = Color.darkBlue;
            yield return StartCoroutine(ShowText("케인", "곧 심문실에서 윌리엄을 직접 대면하게 될 예정입니다.", cain2[5]));
            yield return StartCoroutine(ShowText("케인", "자, 출발하시죠.", cain2[6]));
        }

        // Data UI 활성

        NPCName.text = "";
        line.text = "";
        Slide(false, 1f);        
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Interrogation Room 1");
        // dataPanel.gameObject.SetActive(true);
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
