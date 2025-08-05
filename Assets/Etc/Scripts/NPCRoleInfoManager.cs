using System.Diagnostics;

public class NPCRoleInfoManager
{
    public string GetRole(string npcName) => GetNPCRoleByName(npcName)?.role;
    public string GetAudience(string npcName) => GetNPCRoleByName(npcName)?.audience;
    public string GetInformation(string npcName) => GetNPCRoleByName(npcName)?.information;
    
    public string GetTask_1(string npcName) => GetNPCRoleByName(npcName)?.task_1;
    public string GetTask_2(string npcName) => GetNPCRoleByName(npcName)?.task_2;
    public string GetTask_final(string npcName) => GetNPCRoleByName(npcName)?.task_final;

    public string GetRule(string npcName) => GetNPCRoleByName(npcName)?.rule;
    public string GetStyle(string npcName) => GetNPCRoleByName(npcName)?.style;

    public string GetConstraint(string npcName) => GetNPCRoleByName(npcName)?.constraint;
    public string GetSpecificConstraint(string npcName) => GetNPCRoleByName(npcName)?.specificConstraint;
    
    public string GetFormat(string npcName) => GetNPCRoleByName(npcName)?.format;

    private NPCRoleInfo GetNPCRoleByName(string npcName)
    {
        if (JsonManager.npcRoleInfoList != null)
        {
            foreach (NPCRoleInfo info in JsonManager.npcRoleInfoList.npcRoleInfoList)
            {
                string name = info.name;
                if (name == npcName)
                {
                    return info;
                }
            }
        }
        return null;
    }
}