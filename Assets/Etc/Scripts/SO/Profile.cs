using UnityEngine;


[CreateAssetMenu(fileName = "New Suspect", menuName = "Prosecutor/Suspect Data")]
public class Profile : ScriptableObject
{
    [Header("기본 정보")]
    public Sprite profileImage;        // 프로필 이미지
    public string name;         // 이름
    public string age;                 // 나이
    public string gender;              // 성별
    public string occupation;          // 직업

    [Header("피해자와의 관계")]    
    public string relationship;    // 피해자와의 관계
    [Header("사건 당시 행적")]
    public string alibi;             // 사건 당시 행적
//
    [Header("성격")]
    public string personality;             // 성격
    [Header("의심 정황")]
    [TextArea(3, 5)]
    public string redFlags; // 의심 정황
} 