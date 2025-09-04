using AdvancedPeopleSystem;
using UnityEngine;

public class NPC : MonoBehaviour
{
    protected Animator animator;
    protected AudioSource audioSource;
    protected CharacterCustomization cc;
    protected string answer;
    protected NPCRoleInfoManager roleInfoManager;

    private Transform probesAnchor;

    protected void Awake()
    {        
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 0.6f;

        cc = GetComponent<CharacterCustomization>();
        cc.InitColors();

        // NPCEmotionHandler
        roleInfoManager = new NPCRoleInfoManager();

        // "Probes Anchor" 자식 오브젝트 참조
        Transform foundProbesAnchor = transform.Find("Probes Anchor");
        if (foundProbesAnchor != null)
        {
            probesAnchor = foundProbesAnchor;
        }
        else
        {
            probesAnchor = null;
            Debug.LogWarning("\"Probes Anchor\" 오브젝트를 찾을 수 없습니다.", this);
        }
    }

    public Transform GetAnchor()
    {
        return probesAnchor;
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
