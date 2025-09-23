using UnityEngine;
using OpenAI;
using System.Collections.Generic;
using static SpecificNPC;
using UnityEditor.Experimental.GraphView;


public class Judge : MonoBehaviour
{
    protected Animator animator;
    protected string answer;
    protected NPCRoleInfoManager roleInfoManager;
    protected AudioSource audioSource;
    protected List<ChatMessage> chatMessages;
    protected OpenAIApi openAIApi;

    public CourtManager courtManager;
    public SpecificNPC specificNPC; // SpecificNPC 스크립트 참조

    public CourtRole currentRole = CourtRole.Judge;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        openAIApi = new OpenAIApi();
        SetRole();

    }

    public void SetRole()
    {
        chatMessages = new List<ChatMessage>();

        string name = "판사";

        string role = "당신은 'Prosecutor' 라는 게임 속 인물로 판사 역할을 맡았습니다." +
            "당신은 법정을 주재하며 검사와 변호사 사이의 공방을 공정하게 중재하고, 법정 질서를 유지하며 최종 판결을 내립니다.";

        string audience = "플레이어는 검사이며, 당신은 검사와 변호사 모두의 발언을 듣고 필요 시 개입하거나 판정을 내립니다.";

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

        /*
        string information =
"판사의 기본 원칙:\n" +
"- 법정 질서를 유지해야 합니다.\n" +
"- 검사와 변호사가 규칙을 벗어나는 발언을 할 경우 제지해야 합니다.\n" +
"- 증거와 증언의 신빙성을 평가하고, 법적 절차 준수를 강조해야 합니다.\n" +
"- 최종적으로 피고인의 유무죄에 대한 판결을 내리는 권위를 가집니다.\n";
        */

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

        string task = "당신은 판사의 입장에서 검사와 변호사의 공방을 통제하고, 법정의 질서를 유지하며, 중립적이지만 단호한 발언을 해야 합니다.";

        string style =
            "- 판사의 말투 예시:\n\n" +

            "[중립적이고 단호한 태도]\n" +
            "\"이 법정은 사실과 증거만을 근거로 판단합니다.\"\n" +
            "\"불필요한 논쟁은 중단하십시오.\"\n" +

            "[분노]\n" +
            "\"법정을 농락하지 마십시오. 더 이상의 기만은 허용되지 않습니다.\"\n" +

            "[놀람]\n" +
            "\"지금 제출된 증거는 사건의 판도를 바꿀 수 있습니다.\"\n" +

            "[실망]\n" +
            "\"거짓은 법정에서 용납되지 않습니다.\"\n" +

            "[슬픔]\n" +
            "\"이 비극적인 사건을 다루게 된 것이 유감입니다.\"";

        string constraint =
            "제약 사항:" +
            "- 모든 응답은 짧게 유지할 것" +
            "- 특정 인물의 편을 직접 들지 말 것" +
            "- 법정 질서를 우선시하며 검사와 변호사 발언을 제지하거나 정리하는 방향으로 답변할 것" +           
            "- JSON 형식의 응답 형식을 엄격히 따를 것";

        string format = "응답 형식은 두가지 유형이 있습니다. " +
            "1. 플레이어나 변호사로부터 질문 받아 응답하는 경우" +
            "2. 플레이어와 변호사의 발언을 판단하는 경우" +            

            "응답 형식:\n\n" +
"{\n" +
"  \"emotion\": \"Neutral | Joy | Sadness | Anger | Fear | Disgust | Surprise\",\n" +
"  \"target\": \"Player | Lawyer | None\",\n" +
"  \"convictionScore\": -5에서 +5까지의 정수값,\n" +
"  \"response\": \"짧은 길이의 한국어 답변\",\n" +
"}\n\n" +

"'emotion' 필드는 캐릭터의 감정 상태를 나타냅니다.\n" +

"'target'은 자신의 응답을 전달할 상대방을 나타냅니다.\r\n " +
" - None의 경우 trustScore만 출력할 경우 사용합니다.\r\n " +

"'convictionScore'은 판정에 영향을 주는 수치입니다.\n" +
"플레이어/변호사의 말이 얼마나 판사에게 설득력 있게 들렸는지 수치화하는 개념입니다.\r\n " +
"플레이어의 발언이 논리적이고 사건 핵심과 관련이 있다면 +\r\n " +
"플레이어의 발언이 논리적이지 않고 핵심과 관련이 없다면 -\r\n " +
"플레이어의 발언에 변호사의 반박이 논리적이라면 -\r\n " +
"플레이어의 발언에 변호사의 반박이 실패한다면 +\r\n " +
"재판과 관계 없는 중립적인 발언이라면 0\r\n " +

"출력 예시 (질문에 대한 응답):\n" +
"{\r\n  \"emotion\": \"Neutral\",\r\n  \"target\": \"Player\",\r\n  \"convictionScore\": 0,\r\n  \"response\": \"그 질문은 사건의 핵심과 직접적인 관련이 없습니다.\"\r\n}" +

"출력 예시 (발언에 대한 판단):\n" +
"{\r\n  \"emotion\": \"Neutral\",\r\n  \"target\": \"None\",\r\n  \"convictionScore\": 3,\r\n  \"response\": \"검사의 주장은 설득력이 있습니다.\"\r\n}";


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

        string content = question;    // Content에 들어갈 내용
        string role = noResponse ? "system" : "user";        

        ChatMessage newMessage = new ChatMessage
        {
            Content = content,
            Role = role
        };

        chatMessages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest
        {
            Messages = chatMessages,
            Model = "gpt-4o-mini"
        };



        var response = await openAIApi.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;

            chatMessages.Add(chatResponse);

            answer = chatResponse.Content;
            Debug.Log("Judge : " + answer);
            
            courtManager.ShowJudgeAnswer(answer, noResponse);
        }
    }



    public string GetRole(string npcName) => roleInfoManager.GetRole(npcName);
    public string GetAudience(string npcName) => roleInfoManager.GetAudience(npcName);
    public string GetInformation(string npcName) => roleInfoManager.GetInformation(npcName);

    public string GetTask(string npcName) => roleInfoManager.GetTask(npcName);
    public string GetTask_final(string npcName) => roleInfoManager.GetTask_final(npcName);

    public string GetRule(string npcName) => roleInfoManager.GetRule(npcName);
    public string GetStyle(string npcName) => roleInfoManager.GetStyle(npcName);

    public string GetConstraint(string npcName) => roleInfoManager.GetConstraint(npcName);
    public string GetHint(string npcName) => roleInfoManager.GetHint(npcName);
    public string GetHintFinal(string npcName) => roleInfoManager.GetHintFinal(npcName);

    public string GetFormat(string npcName) => roleInfoManager.GetFormat(npcName);

}
