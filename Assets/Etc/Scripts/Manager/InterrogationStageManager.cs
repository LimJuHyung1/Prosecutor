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
        "�˻��, ������������������ ������ �� ���� ���ڽ� �м� ����� �ѰܿԽ��ϴ�.",
        "��� �߻� ���� ��, ��Ȯ�� 22�� 41�п� ������ �� �ܰ� ��â�� �α� ���θ� ������ ������ Ȯ�εƽ��ϴ�.",
        "�߰���, ������ �� �޴������� ��ġ���� �� ��ġ ����� ���� �ֽ��ϴ�.",
        "����ڴ� ���� ���� ��ϵ� �־���, ��ġ ������ ��� �Ϸ� �� �����Դϴ�.",
        "�ش� ĸó �̹����� ����, ���ʿ� �غ��ص׽��ϴ�. �ɹ� ���� Ȯ���Ͻ���."
    };
    private string[] _2ndInterrogationStageLine_emma = 
    {
        "���� �� �����ؼ�, �� ���� ������ ���ÿ� �����߽��ϴ�.",
        "ù ��°�� ���������������� ������ �� ���� ���� �޽��� ����Դϴ�.",
        "���� ���ڼ� ǥ���� �ټ� ���Ե� �ְ�, ���� ���� ������ ���Ÿ� ����ϴ� ���嵵 �߰ߵƽ��ϴ�.",
        "�� ��°�� ���� ȸ������ ���� �����Դϴ�.",
        "���� ���� ��κ��� ���� ��� �� �Ǿ�ǰ ���� ������� �� 4õ�� �� �Ը��� ���� ������ ������ ������ Ȯ�εƽ��ϴ�.",
        "������, �ش� ��� ��ǰ�� ���� ��� ������ ���ٴ� ���Դϴ�.\r\n��ο��� ������, �ǹ��� ��𿡵� ���ٴ� ����.",
        "�޽��� �������� �繫��� �纻�� ���ʿ� �غ��ص׽��ϴ�.\r\n�ɹ� ���� �����ܺ��ñ� ���մϴ�."
    };
    private string[] _2ndInterrogationStageLine_john =
    {
        "�� ���� ����Ʈ�� ������ ����� ���Խ��ϴ�.",
        "���� ����ø����, ��� ������ ��â�� �Կ��� ������ 24���̳� �߰ߵƽ��ϴ�.\r\n�ܺ� ������ �����̰�, ���� �������� ���� ��� �־����ϴ�.",
        "�׸��� �� �ϳ�... �� ���� ���� ������ ���ٱ�������� ���� ���� ��ϵ� Ȯ�εƽ��ϴ�.",
        "2018�� 9�� 3���ڷ� ���� �ߺε� ������,\r\n�������� ���� �õ��� ���Ȱ ħ�ذ� ������ ��õ� �ֽ��ϴ�.",
        "�� ��� �ڷ�� ���� �ֽ��ϴ�.\r\n�ɹ� ���� �� �����غ�����."
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
                    cue.sprite = cueSprites[0]; // William�� ť �̹���
                    break;
                case SpecificNPC.Character.Emma:
                    lines = _2ndInterrogationStageLine_emma;
                    cue.sprite = cueSprites[1]; // Emma�� ť �̹���
                    break;
                case SpecificNPC.Character.John:
                    lines = _2ndInterrogationStageLine_john;
                    cue.sprite = cueSprites[2]; // John�� ť �̹���
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

                // ����� ���� ������ ���
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

    // Ÿ���� ȿ�� �ڷ�ƾ
    private IEnumerator TypeText(string fullText, float delay)
    {
        investigatorLine.text = "";
        foreach (char letter in fullText)
        {
            investigatorLine.text += letter;
            yield return new WaitForSeconds(delay); // ���� �� ����
        }
    }     
    */
}
