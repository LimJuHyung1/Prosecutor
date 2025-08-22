using OpenAI;
using UnityEngine;
using System.Collections.Generic;
using AdvancedPeopleSystem;
using System.Collections;

public class SpecificNPC : NPC
{
    public enum Character
    {        
        William,        
        Emma,
        John
    }

    public List<EmotionClipList> audioClipsPerEmotion;  // 0: Neutral, 1: Joy, 2: Sadness, 3: Anger, 4: Fear, 5: Disgust, 6: Surprise    

    [SerializeField] protected bool isReadyToTalk = true;
    public bool IsReadyToTalk
    {
        get { return isReadyToTalk; }
        set { isReadyToTalk = value; }
    }

    protected List<ChatMessage> chatMessages;
    protected OpenAIApi openAIApi;    
    private NPCEmotionHandler emotionHandler; 

    public enum InterrogationStage
    {
        Stage1,
        Stage2,
        StageFinal
    }

    public InterrogationStage interrogationStage;
    public ConversationManager conversationManager;
    public Character currentCharacter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        openAIApi = new OpenAIApi();
        SetRole();

        emotionHandler = new NPCEmotionHandler(animator, audioSource, cc, audioClipsPerEmotion);
    }

    public void GetConversationManager(ConversationManager conversationManagerParam){
        conversationManager = conversationManagerParam;
    }

    // �� NPC ���� �н�
    public void SetRole()
    {
        string npcName = currentCharacter.ToString();
        chatMessages = new List<ChatMessage>(); // �� NPC���� �� ����Ʈ ����

        string taskContent = "";
        string hint = "";

        switch (interrogationStage)
        {
            case InterrogationStage.Stage1:
                taskContent = GetTask_1(npcName);
                hint = GetHint1(npcName);
                break;
            case InterrogationStage.Stage2:
                taskContent = GetTask_2(npcName);
                hint = GetHint2(npcName);
                break;
            case InterrogationStage.StageFinal:
                taskContent = GetTask_final(npcName);
                hint = GetHintFinal(npcName);
                break;
        }

        ChatMessage systemMessage = new ChatMessage
        {
            Role = "system",
            Content = GetRole(npcName)
            + GetAudience(npcName)
            + GetInformation(npcName)
            + taskContent
            + GetRule(npcName)
            + GetStyle(npcName)
            + GetConstraint(npcName)
            + hint
            + GetFormat(npcName)
        };
        chatMessages.Add(systemMessage);
    }


    // �亯 ���    
    public async void GetResponse()
    {
        if (conversationManager.GetAskFieldTextLength() < 1)
        {
            return;
        }

        ChatMessage newMessage = new ChatMessage
        {
            Content = conversationManager.GetAskFieldText(),      // ���� �Է�
            Role = "user"
        };

        chatMessages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest
        {
            Messages = chatMessages,
            Model = "gpt-4o-mini"                 // chatGPT 3.5 ���� ��� gpt-3.5-turbo
        };

        var response = await openAIApi.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message; // ������ ���� ���� ��ü ����

            chatMessages.Add(chatResponse); // �޼��� ����Ʈ�� �߰�

            answer = chatResponse.Content;  // ������ string���� ��ȯ
            Debug.Log(answer);
            conversationManager.ShowAnswer(answer);  // ȭ�鿡 ���                 
        }

        emotionHandler.PlayEmotion(answer);
    }

    public string GetAnswer(){
        return currentCharacter.ToString() + " : " + answer;
    }

    public void PlayOnomatopoeia(string emotion)
    {
        emotionHandler.PlayOnomatopoeia(emotion);
    }



    public IEnumerator TurnTowardPlayer(Transform target, float duration = 1f)
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;

        Vector3 lookPos = target.position;
        lookPos.y = transform.position.y;
        Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = targetRotation; // ��Ȯ�� ���߱�
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
