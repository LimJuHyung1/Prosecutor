using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance;

    public enum Character
    {
        William,
        Emma,
        John
    }
    public Character currentCharacter;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 제거
        }
    }
}
