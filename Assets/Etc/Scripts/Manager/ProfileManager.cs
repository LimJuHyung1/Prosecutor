using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileManager : MonoBehaviour
{
    public Image profileImage;    
    public TextMeshProUGUI name;
    public TextMeshProUGUI age;
    public TextMeshProUGUI gender;
    public TextMeshProUGUI occupation;
    public TextMeshProUGUI relationship;
    public TextMeshProUGUI alibi;
    public TextMeshProUGUI personality;
    public TextMeshProUGUI redFlags;


    public void UpdateProfileUI(Profile profile)
    {

        if (profileImage != null) profileImage.sprite = profile.profileImage;
        if (name != null) name.text = "<b>이름</b> : " + profile.name;
        if (age != null) age.text = "<b>나이</b> : " + profile.age;
        if (gender != null) gender.text = "<b>성별</b> : " + profile.gender;
        if (occupation != null) occupation.text = "<b>직업</b> : " + profile.occupation;
        if (relationship != null) relationship.text = "<b>피해자와의 관계</b> : \n" + profile.relationship;
        if (alibi != null) alibi.text = "<b>사건 당시 행적</b> : \n" + profile.alibi;
        if (personality != null) personality.text = "<b>성격</b> : \n" + profile.personality;
        if (redFlags != null) redFlags.text = "<b>의심 정황</b> : \n" + profile.redFlags;
    }
}
