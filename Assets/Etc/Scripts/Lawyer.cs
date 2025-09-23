using AdvancedPeopleSystem;
using OpenAI;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static SpecificNPC;


public class Lawyer : NPC
{
    protected List<ChatMessage> chatMessages;
    protected OpenAIApi openAIApi;
    private NPCEmotionHandler emotionHandler;

    public RuntimeAnimatorController maleAnimationController;
    public RuntimeAnimatorController femaleAnimationController;

    public CourtManager courtManager;
    public SpecificNPC specificNPC; // SpecificNPC 스크립트 참조
    public List<EmotionClipList> audioClipsPerEmotion;  // 0: Neutral, 1: Joy, 2: Sadness, 3: Anger, 4: Fear, 5: Disgust, 6: Surprise    

    public CourtRole currentRole = CourtRole.Lawyer;

    protected void Start()
    {
        openAIApi = new OpenAIApi();
        SetRole();

        emotionHandler = new NPCEmotionHandler(animator, audioSource, cc, audioClipsPerEmotion);

        // 시작 시 랜덤 외형 적용
        RandomizeAppearancePartial();

        if (cc.Settings.name.Contains("Male"))
            {
            animator.runtimeAnimatorController = maleAnimationController;
        }
        else if (cc.Settings.name.Contains("Female"))
        {
            animator.runtimeAnimatorController = femaleAnimationController;
        }
    }

    /// <summary>
    /// 외형 랜덤 초기화
    /// </summary>
    public void RandomizeAppearancePartial()
    {
        if (cc == null)
        {
            Debug.LogWarning("CharacterCustomization 컴포넌트가 없습니다.", this);
            return;
        }

        // 1. 성별 랜덤 선택 (0 = Male, 1 = Female 예시)
        int genderIndex = Random.Range(0, cc.Settings.settingsSelectors.Count);
        cc.SwitchCharacterSettings(genderIndex);

        // 2. 헤어 랜덤 선택
        var hairPresets = cc.GetElementsPresets(CharacterElementType.Hair);
        if (hairPresets != null && hairPresets.Count > 0)
        {
            int randomHairIndex = Random.Range(0, hairPresets.Count);
            cc.SetElementByIndex(CharacterElementType.Hair, randomHairIndex);
        }

        // 3. 수염 랜덤 선택
        var beardPresets = cc.GetElementsPresets(CharacterElementType.Beard);
        if (beardPresets != null && beardPresets.Count > 0)
        {
            int randomBeardIndex = Random.Range(0, beardPresets.Count);
            cc.SetElementByIndex(CharacterElementType.Beard, randomBeardIndex);
        }

        // 4. 머리 색 랜덤
        cc.SetBodyColor(BodyColorPart.Hair, Random.ColorHSV());

        // 6. 눈 색 랜덤
        cc.SetBodyColor(BodyColorPart.Eye, Random.ColorHSV());

        if (cc.Settings.name.Contains("Male"))
        {
            cc.SetElementByIndex(CharacterElementType.Shirt, 4);
            cc.SetElementByIndex(CharacterElementType.Pants, 4);
            cc.SetElementByIndex(CharacterElementType.Shoes, 0);
        }
        else if (cc.Settings.name.Contains("Female"))
        {
            cc.SetElementByIndex(CharacterElementType.Shirt, 7);
            cc.SetElementByIndex(CharacterElementType.Pants, 6);
            cc.SetElementByIndex(CharacterElementType.Shoes, 1);

            var breastSizeData = cc.GetBlendshapeData(CharacterBlendShapeType.BreastSize);
            float randomValue = Random.Range(0f, 100f); // 0 ~ 100 사이 랜덤 값
            cc.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, randomValue);
        }

        var fatData = cc.GetBlendshapeData(CharacterBlendShapeType.Fat);
        if (fatData != null)
        {
            cc.SetBlendshapeValue(CharacterBlendShapeType.Fat, Random.Range(0f, 100f));
        }

        // Muscles
        var muscleData = cc.GetBlendshapeData(CharacterBlendShapeType.Muscles);
        if (muscleData != null)
        {
            cc.SetBlendshapeValue(CharacterBlendShapeType.Muscles, Random.Range(0f, 100f));
        }

        // Thin
        var thinData = cc.GetBlendshapeData(CharacterBlendShapeType.Thin);
        if (thinData != null)
        {
            cc.SetBlendshapeValue(CharacterBlendShapeType.Thin, Random.Range(0f, 100f));
        }
    }

    public Color GetCurrentHairColor()
    {
        if (cc == null)
        {
            Debug.LogWarning("CharacterCustomization 컴포넌트가 없습니다.", this);
            return Color.black; // 기본값
        }

        return cc.GetBodyColor(BodyColorPart.Hair);
    }

    public void SetRole()
    {
        chatMessages = new List<ChatMessage>();

        string name = "변호사";

        string role = "당신은 'Prosecutor' 라는 게임 속 인물로 변호사 역할을 맡았습니다." +
            "플레이어가 검사로서 당신을 상대로 법정 공방을 벌이게 됩니다.";

        string audience = "플레이어는 검사이며, 법정에서 당신과 논리적으로 대립할 것입니다.";

        string williamInformation = "다음은 당신이 변호해야 할 인물에 대한 내용입니다. " +
            "• 이름: 윌리엄(William)" +
            "• 성별: 남성" +
            "• 나이: 39세" +
            "• 직업: 오리온 캐피탈(투자회사) 부사장" +
            "• 성격: 차분하고 합리적이나, 감정이 흔들릴 때 신중함 속에 의심이 스며듦" +
            "• 관계: 엠마(윌리엄의 연인), 헨리(윌리엄이 투자한 병원의 병원장), 존(헨리의 병원 의료사고 피해자의 가족)" +

            "사건 개요: 5월 7일 밤, 병원장 헨리가 시 외곽 폐창고에서 사망한 채 발견되었다. " +
            "사건 배경에는 헨리와 엠마의 관계 의혹, 병원 재정 악화, 투자 갈등, 과거 헨리 병원에서의 의료사고가 얽혀 있다." +
            "알리바이: 5월 6일 오전, 윌리엄은 회사 회의에서 병원 투자 관련 보고를 받았다. 오후에는 병원 회계팀과의 화상 회의로 재무 자료를 검토했다. " +
            "저녁 무렵 연인 엠마와 식사를 했으나, 헨리에 대한 언급으로 대화가 불편해졌다. " +
            "식사 후 윌리엄은 엠마의 동향을 확인하기 위해 휴대폰에 위치추적 앱을 설치하고, 등록 명의를 '엠마'로 설정했다. " +
            "그날 밤, 앱을 통해 엠마의 이동 경로를 추적하며 뒤쫓았으나, 구체적인 목적지는 밝히지 않았다. " +
            "5월 7일 오전, 사무실에서 투자 보고서를 정리하고 오후에는 자택에서 전화 회의를 진행했다. " +
            "저녁 9시 이후 대부분 자택에 머물렀지만, 22시 41분경 차량 블랙박스 기록에 시 외곽 폐창고 인근 도로를 지난 흔적이 남아 있다. " +
            "외출 목적과 경로는 밝히지 않고 있으며, 새벽 1시 전후의 행적은 기억이 불분명하다.";

        string emmaInformation = "다음은 당신이 변호해야 할 인물에 대한 내용입니다. " +
            "• 이름: 엠마(Emma) " +
            "• 성별: 여성 " +
            "• 나이: 34세 " +
            "• 직업: 패션 디자이너 " +
            "• 성격: 명랑하고 밝은 성격, 욕심이 많음" +
            "• 관계: 윌리엄(현재 엠마의 연인), 헨리(최근 잦은 접촉이 있었던 병원장), 존(과거 엠마의 연인)" +

            "사건 개요: 5월 7일 밤, 병원장 헨리가 시 외곽 폐창고에서 사망한 채 발견되었다. " +
            "사건의 배경에는 헨리와 엠마의 관계 의혹, 윌리엄과의 투자 문제, 병원 재정 악화, 과거 의료사고가 얽혀 있다. " +
            "알리바이: 5월 6일 오전, 엠마는 사무실에서 디자인 작업을 진행하고 오후에는 외부 패션 행사에 참석했다. " +
            "저녁에는 연인 윌리엄과 식사를 했으나, 헨리 이야기가 나오면서 대화 분위기가 어색해졌다. " +
            "식사 후, 존과 함께 시 외곽 폐창고로 향했다." +
            "5월 7일 오전~오후: 스튜디오에서 촬영, 오후 6시경 업무 종료 후 귀가" +
            "5월 7일 밤: 자택에 머물렀다고 주장하나, 구체적인 시간대별 행적은 일부 불명확";

        string johnInformation = "다음은 당신이 변호해야 할 인물에 대한 내용입니다. " +
    "• 이름: 존(John) " +
    "• 성별: 남성 " +
    "• 나이: 37세 " +
    "• 직업: 격투기 선수 " +
    "• 성격: 충동적이고 집착이 강하며, 감정 기복이 심함 " +
    "• 관계: 엠마(존의 과거 연인), 헨리(과거 어머니의 의료사고를 일으킨 병원장), 윌리엄(엠마의 현재 연인) " +

    "사건 개요: 5월 7일 밤, 병원장 헨리가 시 외곽 폐창고에서 사망한 채 발견되었다. " +
    "사건의 배경에는 헨리와 엠마의 관계 의혹, 병원 재정 문제, 투자 갈등, 과거 헨리 병원에서 발생한 의료사고가 얽혀 있다. " +

    "알리바이: 5월 6일 오전, 자택에서 머물다 오후에 헨리가 근무하는 병원 근처를 배회했다. " +     // 최종 심문 단계 수사관 발언에 추가할까?
    "저녁 무렵, 엠마와 함께 시 외곽 폐창고를 돌아다니며 외부와 내부 사진을 다수 촬영했다" +    
    "5월 7일 오후, 헨리를 시 외곽 폐창고에서 불러들였다. " +
    "사건 당일 밤중, 폐창고에서 헨리와 격한 언쟁을 벌였으며, 이후 헨리가 사망했다. " +
    "사망 직후 시신을 현장에 남겨두고 떠났으며, 이후의 동선은 불분명하다.";

        // 의뢰인 이름에 따라 정보 선택
        string information = string.Empty;
        if (specificNPC.currentCharacter == Character.William)
        {
            information = williamInformation;
        }
        else if (specificNPC.currentCharacter == Character.Emma)
        {
            information = emmaInformation;
        }
        else if (specificNPC.currentCharacter == Character.John)
        {
            information = johnInformation;
        }
        else
        {
            Debug.LogError("알 수 없는 캐릭터: " + specificNPC.currentCharacter);
        }

        string task = "당신은 변호인의 입장에서 피고인을 방어하고, 플레이어의 주장을 반박하거나 약화시키는 답변을 해야 합니다.";

        string style =
"- 변호사의 말투 예시:\n\n" +

"[논리적이고 침착한 태도]\n" +
"\"검사님, 그 증거가 실제 사건과 어떻게 연결되는지 명확히 해주실 수 있습니까?\"\n" +
"\"추측만으로는 유죄를 입증할 수 없습니다.\"\n" +

"[공격적인 반박]\n" +
"\"그 증거는 위법하게 수집된 가능성이 있습니다. 법적으로 효력이 없습니다.\"\n" +
"\"목격자의 진술은 시간과 장소가 일치하지 않습니다. 신빙성을 인정하기 어렵습니다.\"\n" +

"[전략적인 회피]\n" +
"\"그 부분은 현재 기록에 남아 있는 정보로는 확인할 수 없습니다.\"\n" +
"\"피고인과 헨리 사이의 관계를 단정하는 것은 섣부른 판단입니다.\"\n" +

"[감정 절제와 원칙 강조]\n" +
"\"감정에 휘둘려서는 안 됩니다. 법은 사실과 증거 위에 서야 합니다.\"\n" +
"\"무죄 추정의 원칙을 잊지 마시기 바랍니다.\"\n";

        string constraint =
"제약 사항:" +
"- 모든 응답은 짧게 유지할 것" +
"- 불리한 사실은 직접 인정하지 말 것" +
"- 검사의 주장에 반박하거나 의심을 제기하는 방향으로 답변할 것" +
"- 질문이 불리할 경우 회피하거나 논점을 전환할 것" +
"- JSON 형식의 응답 형식을 엄격히 따를 것";


        string format =
        "응답 형식:\n\n" +
        "{\n" +
        "  \"emotion\": \"Neutral | Joy | Sadness | Anger | Fear | Disgust | Surprise\",\n" +
        "  \"target\": \"Player | Judge\",\n" +
        "  \"response\": \"짧은 길이의 한국어 답변\"\n" +
        "}\n\n" +

        "출력 예시:\n" +
        "{\n" +
        "  \"emotion\": \"Neutral\",\n" +
        "  \"target\": \"Player\",\n" +
        "  \"response\": \"검사님, 그 증거가 사건과 어떻게 연결되는지 명확히 밝혀 주십시오.\"\n" +
        "}\n\n" +

        "{\n" +
        "  \"emotion\": \"Anger\",\n" +
        "  \"target\": \"Judge\",\n" +
        "  \"response\": \"이 증거는 위법하게 수집되었을 가능성이 큽니다. 인정될 수 없습니다.\"\n" +
        "}\n\n" +

        "'emotion' 필드는 캐릭터의 감정 상태를 나타냅니다.\n" +

        "'target'은 자신의 응답을 전달할 상대방을 나타냅니다.\r\n ";

                ChatMessage systemMessage = new ChatMessage
        {
            Role = "system",
            Content = role + audience + information + task + style + constraint + format
        };
        chatMessages.Add(systemMessage);
    }
    
    public async void GetResponse(string question, bool noResponse)
    {
        if (question.Length < 1)
        {
            return;
        }

        string role = noResponse ? "system" : "user";
        

        ChatMessage newMessage = new ChatMessage
        {
            // Content = courtManager.GetAskFieldText(),
            Content = question,
            Role = role
        };

        chatMessages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest
        {
            Messages = chatMessages,
            Model = "gpt-4o-mini"
        };

        if (!noResponse)
        {
            var response = await openAIApi.CreateChatCompletion(request);

            if (response.Choices != null && response.Choices.Count > 0)
            {
                var chatResponse = response.Choices[0].Message;

                chatMessages.Add(chatResponse);

                answer = chatResponse.Content;
                Debug.Log(answer);
                courtManager.ShowLawyerAnswer(answer);
            }

            emotionHandler.PlayEmotion(answer);
        }
    }

    public void PlayOnomatopoeia(string emotion)
    {
        emotionHandler.PlayOnomatopoeia(emotion);
    }


    [System.Serializable]
    public class EmotionClipList
    {
        public List<AudioClip> clips;  // 1차원 리스트
    }


    // 내부 클래스: 감정 표현 처리
    private class NPCEmotionHandler
    {
        private Animator animator;
        private AudioSource audioSource;
        private CharacterCustomization cc;
        private List<string> emotionsList = new List<string>();

        // 0: Neutral, 1: Joy, 2: Sadness, 3: Anger, 4: Fear, 5: Disgust, 6: Surprise
        private List<EmotionClipList> audioClipsPerEmotion;
        private float emotionTime;

        public NPCEmotionHandler(Animator animator, AudioSource audioSource, CharacterCustomization characterCustomization, List<EmotionClipList> audioClipsPerEmotion)
        {
            this.animator = animator;
            this.audioSource = audioSource;
            cc = characterCustomization;
            this.audioClipsPerEmotion = audioClipsPerEmotion;
            InitializeEmotionsList();
        }

        public void InitializeEmotionsList()
        {
            foreach (var e in cc.Settings.characterAnimationPresets)
            {
                emotionsList.Add(e.name);
            }
        }

        public void PlayEmotion(string responseContent)
        {
            emotionTime = Mathf.Clamp(responseContent.Length * 0.2f, 2f, 5f);
            cc.PlayBlendshapeAnimation(emotionsList[5], emotionTime);
        }

        /// <summary>
        /// 의성어 재생
        /// </summary>
        public void PlayOnomatopoeia(string emotion)
        {
            int index = -1;

            switch (emotion)
            {
                case "Neutral": index = 0; animator.SetTrigger("Neutral"); break;
                case "Joy": index = 1; animator.SetTrigger("Joy"); break;
                case "Sadness": index = 2; animator.SetTrigger("Sadness"); break;
                case "Anger": index = 3; animator.SetTrigger("Anger"); break;
                case "Fear": index = 4; animator.SetTrigger("Fear"); break;
                case "Disgust": index = 5; animator.SetTrigger("Disgust"); break;
                case "Surprise": index = 6; animator.SetTrigger("Surprise"); break;
                default:
                    Debug.LogWarning("파싱 오류: " + emotion);
                    return;
            }

            // 범위 체크
            if (index >= 0 && index < audioClipsPerEmotion.Count)
            {
                List<AudioClip> clips = audioClipsPerEmotion[index].clips;

                if (clips != null && clips.Count > 0)
                {
                    AudioClip clipToPlay = clips[Random.Range(0, clips.Count)];
                    audioSource.clip = clipToPlay;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogWarning($"'{emotion}' 감정의 효과음이 비어 있습니다.");
                }
            }
        }
    }

}
