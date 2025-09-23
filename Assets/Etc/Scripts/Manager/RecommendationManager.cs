using Newtonsoft.Json.Linq;
using OpenAI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecommendationManager : MonoBehaviour
{
    public Button[] recommendButtons;
    public ConversationManager conversationManager;

    private List<ChatMessage> chatMessages;
    private OpenAIApi openAIApi;
    private string answer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        openAIApi = new OpenAIApi();
        SetRole();

        SetInteractableButtons(false);
    }

    public void SetRole()
    {
        chatMessages = new List<ChatMessage>();        

        string role = "당신은 'Prosecutor' 게임의 보조 시스템입니다." +
            "당신의 역할은 플레이어(검사)가 NPC에게 한 질문을 분석하고, 이어서 하면 좋은 추가 질문 3가지를 제안하는 것입니다.";

        string audience = "당신의 응답은 'Prosecutor' 게임의 플레이어(검사)가 읽게 될 것입니다.";

        string information = "다음은 'Prosecutor' 게임 속 등장인물에 대한 설명입니다." +
            "• 이름: 윌리엄(William)" +
            "• 성별: 남성" +
            "• 나이: 39세" +
            "• 직업: 오리온 캐피탈(투자회사) 부사장" +
            "• 성격: 차분하고 합리적이나, 감정이 흔들릴 때 신중함 속에 의심이 스며듦" +
            "• 관계: 엠마(윌리엄의 연인), 헨리(윌리엄이 투자한 병원의 병원장), 존(헨리의 병원 의료사고 피해자의 가족)" +

            "5월 7일 밤, 병원장 헨리가 시 외곽 공사 현장에서 사망한 채 발견되었다. " +

            "윌리엄의 민감한 내용: " +
            "윌리엄은 연인인 엠마의 소개로 헨리를 알게 되었고 그의 병원에 투자했다. " +
            "그러나 재무 상태를 검토하는 과정에서 분식회계와 의료사고 은폐 정황을 발견하자 투자 철회를 시도했다. " +
            "이에 헨리는 투자 철회가 부당하다며 소송을 제기했지만, 얼마 지나지 않아 살해당했다." +

            "• 이름: 엠마(Emma) " +
            "• 성별: 여성 " +
            "• 나이: 34세 " +
            "• 직업: 패션 디자이너 " +
            "• 성격: 명랑하고 밝은 성격, 야심이 크고 욕망에 충실함." +
            "• 관계: 윌리엄(현재 엠마의 연인), 헨리(최근 잦은 접촉이 있었던 병원장), 존(과거 엠마의 연인)" +

            "엠마의 민감한 내용: " +
            "엠마는 윌리엄의 연인이었으나, 사교 파티에서 헨리를 만나 불륜 관계로 발전했다. " +
            "이후 엠마의 옛 연인 존이 이 사실을 알게 되었고, 윌리엄에게 폭로하겠다며 그녀를 협박했다. " +
            "엠마는 오히려 이를 역이용해 존을 회유하려 했고, 결국 헨리가 살해당하는 사건으로 이어졌다." +

    "• 이름: 존(John) " +
    "• 성별: 남성 " +
    "• 나이: 37세 " +
    "• 직업: 격투기 선수 " +
    "• 성격: 충동적이고 집착이 강하며, 감정 기복이 심함 " +
    "• 관계: 엠마(존의 과거 연인), 헨리(과거 어머니의 의료사고를 일으킨 병원장), 윌리엄(엠마의 현재 연인) " +

    "존의 민감한 내용: " +
    "사건 발생 한 달 전 존의 어머니가 헨리가 병원장으로 있던 병원에서 의료사고를 당했음에도 아무런 사과를 받지 못했다. " +
    "분노한 존은 헨리가 직접 어머니의 수술 집도의였음을 알게 되었고 그를 조사하기 시작했다. " +
    "그러던 중 엠마와 헨리의 불륜 관계까지 알게 되면서, 이를 빌미로 두 사람을 협박했다. " +
    "엠마에 대한 미련을 버리지 못한 존은 그녀의 회유에 넘어가 결국 헨리를 살해하기로 결심했다. " +
    "사건 당일 그는 헨리를 시 외곽 공사 현장으로 불러내어 살해한 뒤, 시신을 현장에 남겨두고 떠났다.";

        string task = "추천 질문의 목적은 플레이어가 심문을 보다 효과적으로 진행할 수 있도록 돕는 것입니다. " +
     "단, 플레이어가 즉시 범인을 특정할 수 있는 질문은 피해야 합니다. " +
     "질문은 짧고 명확하며 구체적으로 작성되어야 합니다.";

        /*
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
        */

        string constraint =
"제약 사항:\n" +
"- 불필요하게 긴 설명은 하지 말 것\n" +
"- 각 인물의 민감한 내용을 플레이어가 모른다는 전제 하에 추천 질문을 제시할 것 \n" +
"- JSON 형식을 반드시 지킬 것\n";


        string format =
"응답 형식:\n\n" +
"{\n" +
"  \"Question1\": \"(질문 내용)\",\n" +
"  \"Question2\": \"(질문 내용)\",\n" +
"  \"Question3\": \"(질문 내용)\"\n" +
"}";

        ChatMessage systemMessage = new ChatMessage
        {
            Role = "system",
            Content = role + audience + information + task + constraint + format        // style 지워진 상태
        };
        chatMessages.Add(systemMessage);
    }

    public async void GetResponse(string NPCAnswer)
    {
        string question1 = "";
        string question2 = "";
        string question3 = "";        

        ChatMessage newMessage = new ChatMessage
        {
            Content = NPCAnswer,
            Role = "user"
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

            // JSON 유효성 검사
            if (!answer.TrimStart().StartsWith("{"))
            {
                Debug.LogWarning("응답이 JSON 형식이 아닙니다. 원본 출력: " + answer);
                return;
            }

            JObject json = JObject.Parse(answer); // JSON �Ľ�
            question1 = json["Question1"]?.ToString();
            question2 = json["Question2"]?.ToString();
            question3 = json["Question3"]?.ToString();

            recommendButtons[0].transform.GetChild(0).GetComponent<Text>().text = question1;
            recommendButtons[1].transform.GetChild(0).GetComponent<Text>().text = question2;
            recommendButtons[2].transform.GetChild(0).GetComponent<Text>().text = question3;

            Debug.Log(answer);

            SetInteractableButtons(true);
        }        
    }


    public void OnClickRecommendButton(int index)
    {
        conversationManager.GetAskField().text = recommendButtons[index].transform.GetChild(0).GetComponent<Text>().text;
        conversationManager.FocusOnAskField();
    }

    public void SetInteractableButtons(bool state)
    {
        for (int i = 0; i < recommendButtons.Length; i++)
        {
            recommendButtons[i].interactable = state;
            if(state == false)
                recommendButtons[i].transform.GetChild(0).GetComponent<Text>().text = "Waiting...";
        }
    }
}
