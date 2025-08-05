using OpenAI;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

public class SummaryManager : MonoBehaviour
{
    public GameObject summaryPanel;
    public TextMeshProUGUI summaryText;

    private SpecificNPC specificNPC; // NPC 정보가 필요할 경우 사용
    private List<ChatMessage> chatMessages;
    private OpenAIApi openAIApi;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        summaryPanel.SetActive(false);
        summaryText.text = "";

        openAIApi = new OpenAIApi();
        SetRole();
    }

    /// <summary>
    /// 대화 요약을 위해 system 역할 설정 후 요약 요청
    /// GameManager에서 특정 NPC가 선택되면 호출됩니다.
    /// </summary>
    public async void SetRole()
    {        
        chatMessages = new List<ChatMessage>();
        ChatMessage systemMessage;

        string role = "역할: NPC(용의자)의 발언을 분석하는 역할\r\n";
        string audience = "대상: 진짜 범인이 누구인지 추리하는 플레이어(검사)\r\n";
        string information = "            등장인물 정보:\r\n\r\n            " +
            "1. 헨리 (피해자)\r\n            " +
            "- 병원장, 대외 활동 많고 가정엔 무심\r\n            " +
            "- 엠마와 불륜, 윌리엄에게 투자 받음\r\n           " +
            " - 의료사고에 책임 있음, 은폐 시도\r\n            " +
            "- 존의 협박에 위협을 느끼고 회유 및 제거 시도\r\n\r\n            " +
            "2. 엠마 (용의자 1)\r\n            " +
            "- 밝고 야망 있는 성격, 윌리엄의 연인\r\n            " +
            "- 헨리와 불륜 관계, 관계를 유지하기 위해 거짓말 가능\r\n            " +
            "- 존에게 협박받자 오히려 조종함\r\n            " +
            "- 중립적이지만 자기 이익에 따라 움직이는 성향\r\n\r\n            " +
            "3. 윌리엄 (용의자 2)\r\n            " +
            "- 투자회사 부사장, 엠마의 연인\r\n            " +
            "- 헨리의 병원에 30억 투자\r\n            " +
            "- 병원 회계 부정, 투자금 손실에 예민\r\n            " +
            "- 이성과 원칙을 중시하지만 배신에 민감\r\n\r\n            " +
            "4. 존 (용의자 3)\r\n            " +
            "- 엠마의 전 애인, 집착 성향 있음\r\n            " +
            "- 어머니가 헨리 병원에서 사망 → 분노\r\n            " +
            "- 엠마-헨리 불륜을 알게 되고 협박 시작\r\n            " +
            "- 엠마에게 조종당하면서도 헨리 살해\r\n\r\n            " +
            "사건 개요: \r\n            " +
            "- 사건명: 병원장 헨리 사망 사건\r\n            " +
            "- 피해자: 헨리(Henry), 45세, 병원장\r\n            " +
            "- 사망 장소: 외곽 도로 근처의 창고\r\n            " +
            "- 사망 방식: 피해자는 머리에 충격을 받아 살해당한 것으로 보임\r\n            " +
            "- 사망 시간: 오후 9시~오후 11시 사이로 추정\r\n\r\n            " +
            "시나리오 연대기\r\n            " +
            "- 1월: 헨리 병원에서 의료사고 발생 → 재정 악화 시작\r\n            " +
            "• 피해자는 존의 어머니 (심장 수술 도중 사망)\r\n            " +
            "• 수술 집도의는 헨리였으나 병원은 이를 은폐함 (보상 없음)\r\n\r\n            " +
            "- 3월 초: 헨리, 엠마와 파티에서 만남\r\n            " +
            "- 3월 말: 헨리와 엠마, 불륜 관계 시작\r\n\r\n            " +
            "- 4월 초: 윌리엄과 헨리, 30억 규모 투자 계약 체결\r\n            " +
            "- 4월 말: 존, 헨리와 엠마의 불륜 관계를 알게 됨 → 엠마·헨리를 협박\r\n\r\n            " +
            "- 5월 7일: 헨리 사망 사건 발생\r\n            " +
            "• 헨리는 존을 외딴 창고(또는 오두막)로 유인\r\n            " +
            "• 존과의 말다툼 → 몸싸움 → 헨리 머리 부상\r\n            " +
            "• 존은 둔기로 헨리를 살해            \r\n\r\n            " +
            "- 5월 9일: 검사(플레이어) 사건 배정 및 헨리 자택 수사 시작\r\n";
        string task = "수행해야 할 작업: 용의자의 발언 하나를 분석하여 단일 JSON 객체로 출력\n";
        string format = "출력 구조: \r\n            다음 4개의 항목을 포함하는 단일 JSON 객체로 출력하세요:                       \r\n                " +
            "- `speaker`: 발언한 인물의 이름\r\n                " +
            "- `content`: 발언의 원문 또는 요약 문장\r\n                " +
            "- `summary`: 발언의 내용을 객관적/사실 중심으로 정리한 한 문장 요약\r\n                " +
            "- `is_related`: 해당 발언이 사건과 관련 있으면 true, 아니면 false\r\n\r\n";
        string constraints = "제약 사항:\r\n            " +
            "- 반드시 JSON 형식으로만 출력\r\n            " +
            "- `[ { ... } ]` 형태가 아니라 `{ ... }` 하나만 출력할 것\r\n" +
            "- 주석, 설명, 불필요한 문장 포함하지 말 것\r\n            " +
            "- summary 항목의 내용을 \"~함\", \"~임\", \"~됨\", \"~되었음\", \"~라고 말함\" 등의 객관적 종결어미 사용할 것\r\n            " +
            "- summary 항목에 주어(회자 이름)을 출력하지 말 것\r\n\r\n";
        string example = "예시:\r\n                " +
            "{\r\n                    " +
            "\"speaker\": \"엠마\",\r\n                    " +
            "\"content\": \"그날 밤 헨리는 계속 핸드폰을 확인했어요.\",\r\n                    " +
            "\"summary\": \"엠마는 헨리가 사건 당일 핸드폰을 자주 확인했다고 말했다.\",\r\n                    " +
            "\"is_related\": true\r\n                " +
            "}\r\n";


        systemMessage = new ChatMessage
        {
            Role = "system",
            Content = role + audience + information + task + constraints + format + example
        };

        chatMessages.Add(systemMessage);        
    }

    public void GetMessages(string question, string answer){
        chatMessages.Add(new ChatMessage 
        {
            Role = "user",
            Content = question + answer 
        });

        Summary();
    }

    /// <summary>
    /// 누적된 대화를 요약하고 콘솔에 출력
    /// </summary>
    public async void Summary()
    {
        if (chatMessages == null || chatMessages.Count == 0)
        {   
            Debug.LogWarning("대화 내용이 없습니다.");
            return;
        }
    

        CreateChatCompletionRequest request = new CreateChatCompletionRequest
        {
            Messages = chatMessages,
            Model = "gpt-4o-mini"
        };

        var response = await openAIApi.CreateChatCompletion(request);

        // response != null 생략
        if (response.Choices != null && response.Choices.Count > 0)
        {
            string summary = response.Choices[0].Message.Content;            
            // ShowSummary(summary);
            Debug.Log(summary);
        }
        else
        {
            Debug.LogWarning("요약 생성 실패: 응답이 비어 있거나 오류 발생");
        }        
    }

    public void ShowSummary(string summary){
        summaryPanel.SetActive(true);
        summaryText.text = summary;
    }

    public void HideSummary(){
        summaryPanel.SetActive(false);
    }


    public void SetSpecificNPC(SpecificNPC specificNPC)
    {
        this.specificNPC = specificNPC;

        // Character 열거형에서 한글 이름으로 변환
        string npcName = specificNPC.currentCharacter switch
        {
            SpecificNPC.Character.William => "윌리엄",
            SpecificNPC.Character.John => "존",
            SpecificNPC.Character.Emma => "엠마",
            _ => "알 수 없음"
        };

        // 시스템 메시지 생성
        ChatMessage systemMessage = new ChatMessage
        {
            Role = "system",
            Content = $"{npcName}과(와) 대화 중입니다."
        };

        chatMessages.Add(systemMessage);
    }

}
