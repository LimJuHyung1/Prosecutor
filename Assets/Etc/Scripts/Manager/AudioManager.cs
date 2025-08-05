using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    public AudioClip profileAudio;
    public AudioClip interrogationAudio;
    
    private AudioSource[] audioSources;
    private int activeSource = 0;
    
    [SerializeField]
    private float crossFadeDuration = 1.0f;
    private bool isCrossFading = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 두 개의 AudioSource 컴포넌트 초기화
        audioSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].volume = i == 0 ? 1 : 0;
            audioSources[i].loop = true;
        }

        // 시작과 함께 profileAudio 재생
        if (profileAudio != null)
        {
            PlayProfileAudio();
        }
    }

    public void PlayProfileAudio()
    {
        PlayAudioWithCrossFade(profileAudio);
    }

    public void PlayInterrogationAudio()
    {
        PlayAudioWithCrossFade(interrogationAudio);
    }

    private void PlayAudioWithCrossFade(AudioClip newClip)
    {
        if (isCrossFading)
        {
            StopAllCoroutines();
        }

        // 현재 재생 중인 음악이 없거나 같은 음악이면 바로 재생
        if (!audioSources[activeSource].isPlaying || audioSources[activeSource].clip == newClip)
        {
            audioSources[activeSource].clip = newClip;
            audioSources[activeSource].Play();
            return;
        }

        // 다음 오디오 소스 준비
        int nextSource = 1 - activeSource;
        audioSources[nextSource].clip = newClip;
        audioSources[nextSource].volume = 0;
        audioSources[nextSource].Play();

        // 크로스페이드 코루틴 시작
        StartCoroutine(CrossFade(nextSource));
    }

    private IEnumerator CrossFade(int nextSource)
    {
        isCrossFading = true;
        float timeElapsed = 0;

        while (timeElapsed < crossFadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / crossFadeDuration;

            audioSources[activeSource].volume = Mathf.Lerp(1, 0, t);
            audioSources[nextSource].volume = Mathf.Lerp(0, 1, t);

            yield return null;
        }

        // 이전 오디오 정지
        audioSources[activeSource].Stop();
        audioSources[activeSource].volume = 0;
        audioSources[nextSource].volume = 1;

        // 활성 소스 전환
        activeSource = nextSource;
        isCrossFading = false;
    }

    // 모든 오디오 정지
    public void StopAllAudio()
    {
        StopAllCoroutines();
        foreach (var source in audioSources)
        {
            source.Stop();
            source.volume = 0;
        }
        audioSources[activeSource].volume = 1;
        isCrossFading = false;
    }
}
