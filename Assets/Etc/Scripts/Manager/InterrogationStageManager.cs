using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static SpecificNPC;

public class InterrogationStageManager : MonoBehaviour
{    
    /*
    private AudioSource audioSource;

    private Dictionary<(SpecificNPC.Character, InterrogationStage), AudioClip[]> _audioClipMap;

    private string[] _2ndInterrogationStageLine_william =        
    {
        "검사님, 디지털포렌식팀에서 윌리엄 씨 차량 블랙박스 분석 결과를 넘겨왔습니다.",
        "사건 발생 전날 밤, 정확히 22시 41분에 차량이 시 외곽 폐창고 인근 도로를 지나간 흔적이 확인됐습니다.",
        "추가로, 윌리엄 씨 휴대폰에는 위치추적 앱 설치 기록이 남아 있습니다.",
        "사용자는 엠마 씨로 등록돼 있었고, 설치 시점은 사건 하루 전 저녁입니다.",
        "해당 캡처 이미지와 보고서, 이쪽에 준비해뒀습니다. 심문 전에 확인하시죠."
    };
    private string[] _2ndInterrogationStageLine_emma = 
    {
        "엠마 씨 관련해서, 두 가지 보고서가 동시에 도착했습니다.",
        "첫 번째는 디지털포렌식팀이 복원한 존 씨의 문자 메시지 기록입니다.",
        "강한 협박성 표현이 다수 포함돼 있고, 엠마 씨의 사적인 과거를 언급하는 문장도 발견됐습니다.",
        "두 번째는 병원 회계팀의 내부 진술입니다.",
        "엠마 씨가 헨리로부터 고가의 장비 및 의약품 구매 명목으로 약 4천만 원 규모의 병원 예산을 수령한 내역이 확인됐습니다.",
        "문제는, 해당 장비나 물품의 실제 사용 흔적이 없다는 점입니다.\r\n장부에는 있지만, 실물은 어디에도 없다는 거죠.",
        "메시지 복원본과 재무기록 사본은 이쪽에 준비해뒀습니다.\r\n심문 전에 눈여겨보시길 권합니다."
    };
    private string[] _2ndInterrogationStageLine_john =
    {
        "존 씨의 스마트폰 포렌식 결과가 나왔습니다.",
        "내부 사진첩에서, 사건 현장인 폐창고를 촬영한 사진이 24장이나 발견됐습니다.\r\n외부 전경은 물론이고, 내부 구조까지 상세히 담겨 있었습니다.",
        "그리고 또 하나... 존 씨가 엠마 씨에게 접근금지명령을 받은 과거 기록도 확인됐습니다.",
        "2018년 9월 3일자로 정식 발부된 문서고,\r\n지속적인 연락 시도와 사생활 침해가 이유로 명시돼 있습니다.",
        "이 모든 자료는 여기 있습니다.\r\n심문 전에 꼭 검토해보시죠."
    };
    
    public Sprite[] cueSprites; //0:William, 1:Emma, 2:John

    public GameObject InvestigatorIntro;
    private Image cue;
    private Button nextButton;

    public AudioClip[] _1stInterrogationStageSound_william;
    public AudioClip[] _1stInterrogationStageSound_emma;
    public AudioClip[] _1stInterrogationStageSound_john;

    public AudioClip[] _2ndInterrogationStageSound_william;
    public AudioClip[] _2ndInterrogationStageSound_emma;
    public AudioClip[] _2ndInterrogationStageSound_john;

    public AudioClip[] _finalInterrogationStageSound_william;
    public AudioClip[] _finalInterrogationStageSound_emma;
    public AudioClip[] _finalInterrogationStageSound_john;

    public ConversationManager conversationManager;
    Text investigatorLine;

    void Awake()
    {        
        audioSource = GetComponent<AudioSource>();

        _audioClipMap = new Dictionary<(SpecificNPC.Character, InterrogationStage), AudioClip[]>
        {
            { (SpecificNPC.Character.William, InterrogationStage.Stage1), _1stInterrogationStageSound_william },
            { (SpecificNPC.Character.William, InterrogationStage.Stage2), _2ndInterrogationStageSound_william },
            { (SpecificNPC.Character.William, InterrogationStage.StageFinal), _finalInterrogationStageSound_william },

            { (SpecificNPC.Character.Emma, InterrogationStage.Stage1), _1stInterrogationStageSound_emma },
            { (SpecificNPC.Character.Emma, InterrogationStage.Stage2), _2ndInterrogationStageSound_emma },
            { (SpecificNPC.Character.Emma, InterrogationStage.StageFinal), _finalInterrogationStageSound_emma },

            { (SpecificNPC.Character.John, InterrogationStage.Stage1), _1stInterrogationStageSound_john },
            { (SpecificNPC.Character.John, InterrogationStage.Stage2), _2ndInterrogationStageSound_john },
            { (SpecificNPC.Character.John, InterrogationStage.StageFinal), _finalInterrogationStageSound_john },
        };

        cue = InvestigatorIntro.transform.GetChild(0).GetComponent<Image>();
        nextButton = InvestigatorIntro.transform.GetChild(1).GetComponent<Button>();
        investigatorLine = conversationManager.dialogue.transform.GetChild(1).GetChild(1).GetComponent<Text>();

        SetInvestigatorIntroActive(false);
    }

    public void SetInvestigatorIntroActive(bool isActive)
    {
        InvestigatorIntro.SetActive(isActive);
        if (isActive)
        {
            nextButton.gameObject.SetActive(false);            
        }
    }

    public void SetInvestigatorIntroNextButtonActive(bool isActive)
    {
        nextButton.gameObject.SetActive(isActive);
    }

    public void PlayInvestigatorIntro(InterrogationStage stage, SpecificNPC npc)
    {
        string[] lines = null;

        conversationManager.dialogue.gameObject.SetActive(true);
        conversationManager.Slide(true);
        SetInvestigatorIntroActive(false);

        if (stage == InterrogationStage.Stage2)
        {
            cue.gameObject.SetActive(true);
            switch (npc.currentCharacter)
            {
                case SpecificNPC.Character.William:
                    lines = _2ndInterrogationStageLine_william;
                    cue.sprite = cueSprites[0]; // William의 큐 이미지
                    break;
                case SpecificNPC.Character.Emma:
                    lines = _2ndInterrogationStageLine_emma;
                    cue.sprite = cueSprites[1]; // Emma의 큐 이미지
                    break;
                case SpecificNPC.Character.John:
                    lines = _2ndInterrogationStageLine_john;
                    cue.sprite = cueSprites[2]; // John의 큐 이미지
                    break;
            }
        }

        if (_audioClipMap.TryGetValue((npc.currentCharacter, stage), out var audioClips))
        {
            if (audioClips != null && audioClips.Length > 0)
            {
                StartCoroutine(PlayClipsSequentially(audioClips, lines));
            }
        }
        else
        {
            Debug.LogError($"No audio clips found for {npc.currentCharacter} at {stage}.");
        }
    }

    private IEnumerator PlayClipsSequentially(AudioClip[] clips, string[] lines = null)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            audioSource.clip = clip;
            conversationManager.SetActiveEndWaitingMark(false, true);

            if (lines != null && i < lines.Length)
            {
                investigatorLine.text = "";
                audioSource.Play();

                yield return StartCoroutine(TypeText(lines[i], 0.05f));

                // 오디오 끝날 때까지 대기
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
            else
            {
                audioSource.Play();
                yield return new WaitWhile(() => audioSource.isPlaying);
            }

            conversationManager.SetActiveEndWaitingMark(true, true);

            yield return new WaitUntil(() =>
                (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
            );
        }

        // cue.gameObject.SetActive(false);
        SetInvestigatorIntroNextButtonActive(true);
        conversationManager.Slide(false);
        conversationManager.dialogue.gameObject.SetActive(false);
    }

    // 타이핑 효과 코루틴
    private IEnumerator TypeText(string fullText, float delay)
    {
        investigatorLine.text = "";
        foreach (char letter in fullText)
        {
            investigatorLine.text += letter;
            yield return new WaitForSeconds(delay); // 글자 간 간격
        }
    }     
    */
}
