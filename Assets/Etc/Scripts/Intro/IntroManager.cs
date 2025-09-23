﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.VisualScripting; // 꼭 필요!
using UnityEngine.Localization.Settings;


public class IntroManager : MonoBehaviour
{
    public Dropdown localeDropdown;
    public Image screen;
    public Image language;
    public GameObject dialogue;
    private Image[] slides = new Image[2];
    private Text line;
    private GameObject endWaitingMark;       

    public AudioClip[] audioClips; // 오디오 클립 배열 (필요시 사용)
    public Camera[] cameras;
    public GameObject john;
    public GameObject wrench;

    private bool isChanging;
    [SerializeField] private float delay = 0.1f;     // 글자당 출력 속도 (초)
    private Action[] methods;
    private AudioSource audioSource;

    void Start()
    {
        SwitchCamera(0);
        screen.gameObject.SetActive(true);

        // 메소드들을 배열에 등록
        methods = new Action[]
        {
            FirstScene,
            SecondScene,
            ThirdScene,
            FourthScene,
            FifthScene,
            SixthScene
        };
        audioSource = GetComponent<AudioSource>();

        line = dialogue.transform.GetChild(1).GetChild(1).GetComponent<Text>();
        endWaitingMark = dialogue.transform.GetChild(1).GetChild(2).gameObject;
        slides[0] = dialogue.transform.GetChild(0).GetComponent<Image>();
        slides[1] = dialogue.transform.GetChild(1).GetComponent<Image>();

        Slide(true, 0f);
        // methods[0]();
        audioSource.loop = false;

        StartCoroutine(ChangeLocaleCoroutine(0));       // 기본 언어 영어로 설정
    }

    private void SwitchCamera(int index)
    {
        if(line != null)
            line.text = "";

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        if(index != -1)
            cameras[index].gameObject.SetActive(true);
    }

    private void SetAudioClip(int index)
    {
        audioSource.clip = audioClips[index];
    }

    public IEnumerator ShowText(string fullText = "", Action onFinished = null)
    {
        line.text = ""; // 초기화

        // 글자 하나씩 출력
        foreach (char c in fullText)
        {
            line.text += c;
            yield return new WaitForSeconds(delay);
        }

        // 모든 텍스트가 출력된 후 → 입력 대기
        bool waitingInput = true;
        endWaitingMark.SetActive(true); // 입력 대기 마크 활성화
        while (waitingInput)
        {
            if (
                Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                Keyboard.current != null && (
                    Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.numpadEnterKey.wasPressedThisFrame
                )
            )
            {
                waitingInput = false;
            }

            yield return null; // 다음 프레임까지 대기
        }

        endWaitingMark.SetActive(false); // 입력 대기 마크 비활성화

        // 입력이 감지되면 다음 액션 실행
        if (onFinished != null)
            onFinished();
    }







    public void FirstScene()
    {
        float decreaseY = 1f;   // 줄일 y 값 (예: 1만큼 내려가기)
        float duration = 5f;    // 이동 시간 (1초)

        language.gameObject.SetActive(false);
        SwitchCamera(0);
        SetAudioClip(0);
        audioSource.Play(); // 오디오 클립 재생

        StartCoroutine(FirstCamera(decreaseY, duration));
    }

    private IEnumerator FirstCamera(float decreaseY, float duration)
    {
        yield return new WaitForSeconds(2f);

            StartCoroutine(FadeUtility.Instance.FadeOut(screen, 3f));

        Vector3 startPos = cameras[0].transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y - decreaseY, startPos.z);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Lerp로 부드럽게 이동
            cameras[0].transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        // 최종 위치 보정
        cameras[0].transform.position = endPos;

        var currentLocale = LocalizationSettings.SelectedLocale;
        string localeLine = "";

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "Ugh...";            
        }
        else if(currentLocale.Identifier.Code == "it")
        {
            localeLine = "Ugh...";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "うっ...";            
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "윽...";
        }
        StartCoroutine(ShowText(localeLine, SecondScene));
    }
    
    private void SecondScene()
    {
        SwitchCamera(1);
        line.color = Color.red; // 텍스트 색상 초기화
        StartCoroutine(SecondCamera());
    }

    private IEnumerator SecondCamera()
    {        
        // 카메라 상하 반복 이동 코루틴 시작
        Coroutine bobRoutine = StartCoroutine(CameraBob(cameras[1].transform, 0.05f, 0.5f));

        var currentLocale = LocalizationSettings.SelectedLocale;
        string localeLine = "";

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "Huff... Huff...";
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            localeLine = "Ans... ans...";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "はぁ... はぁ...";
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "헉... 헉...";
        }        

        // 첫 대사
        yield return StartCoroutine(ShowText(localeLine));

        // 존 애니메이션 트리거
        john.GetComponent<Animator>().SetTrigger("SecondScene");

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "Serves you right.";
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            localeLine = "Ben ti sta.";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "ざまあみろ。";
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "꼴 좋다.";
        }

        // 두 번째 대사 (끝나면 다음 씬 실행)
        yield return StartCoroutine(ShowText(localeLine, () =>
        {
            // 카메라 상하 이동 멈춤
            StopCoroutine(bobRoutine);
            cameras[1].transform.localPosition = Vector3.zero; // 원래 위치로 보정
            ThirdScene(); // 다음 씬 호출
        }));
    }

    private IEnumerator CameraBob(Transform cam, float amplitude, float frequency)
    {
        Vector3 originalPos = cam.localPosition;

        while (true) // 다음 씬으로 넘어가기 전까지 계속 반복
        {
            float newY = Mathf.Sin(Time.time * frequency) * amplitude;
            cam.localPosition = originalPos + new Vector3(0f, newY, 0f);

            yield return null;
        }
    }


    private void ThirdScene()
    {
        SwitchCamera(2);

        float moveX = -2f;    // x축으로 이동할 값 (예: 2만큼 오른쪽으로)
        float duration = 5f; // 이동 시간 (초)

        StartCoroutine(ThirdCamera(moveX, duration));
    }

    private IEnumerator ThirdCamera(float moveX, float duration)
    {
        var currentLocale = LocalizationSettings.SelectedLocale;
        string localeLine = "";

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "To think your mother died for that reason...";
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            localeLine = "Sapere che tua madre è morta per quella ragione...";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "母さんがそんな理由で死んだなんて...";
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "어머니가 그런 이유로 죽은 거라니...";
        }


        john.GetComponent<Animator>().SetBool("ThirdScene", true);
        StartCoroutine(ShowText(localeLine, FourthScene));

        Vector3 startPos = cameras[2].transform.position;
        Vector3 endPos = new Vector3(startPos.x + moveX, startPos.y, startPos.z);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 부드럽게 x좌표 이동
            cameras[2].transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        // 최종 위치 보정
        cameras[2].transform.position = endPos;
    }

    private void FourthScene()
    {
        SwitchCamera(3);
        john.GetComponent<Animator>().SetBool("ThirdScene", false);

        float moveX = -2f;    // x축으로 이동할 값 (예: 2만큼 오른쪽으로)
        float duration = 5f; // 이동 시간 (초)

        line.color = Color.white; // 텍스트 색상 초기화
        StartCoroutine(FourthCamera(moveX, duration));
    }

    private IEnumerator FourthCamera(float moveX, float duration)
    {
        var currentLocale = LocalizationSettings.SelectedLocale;
        string localeLine = "";

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "You...";
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            localeLine = "Tu...";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "お前...";
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "너...";
        }


        // 흔들림 코루틴 시작
        Coroutine shakeRoutine = StartCoroutine(CameraShake(cameras[3].transform, 0.001f, 5f));

        yield return StartCoroutine(ShowText(localeLine, FifthScene));
    }

    private IEnumerator CameraShake(Transform cam, float magnitude, float frequency)
    {
        Vector3 originalPos = cam.localPosition;

        while (true) // 계속 유지
        {
            float x = (Mathf.PerlinNoise(Time.time * frequency, 0f) * 2f - 1f) * magnitude;
            float y = (Mathf.PerlinNoise(0f, Time.time * frequency) * 2f - 1f) * magnitude;

            cam.localPosition = originalPos + new Vector3(x, y, 0f);

            yield return null;
        }
    }

    private void FifthScene()
    {
        SwitchCamera(4);

        line.color = Color.red;

        Vector3 target = john.transform.position; // 존을 중심으로 원 운동
        StartCoroutine(FifthCamera(target, 12f, 0.125f));        
    }

    private IEnumerator FifthCamera(Vector3 targetPos, float radius = 5f, float angularSpeed = 0.25f)
    {
        Transform cam = cameras[4].transform;
        float angle = 0f;

        var currentLocale = LocalizationSettings.SelectedLocale;
        string localeLine = "";

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "If you die, Emma will be mine too.";
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            localeLine = "Se muori, anche Emma sarà mia.";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "お前が死ねば、エマも俺のものになる。";
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "네가 죽으면 엠마도 내 것이 되겠지.";
        }


        // 카메라 회전 코루틴 시작
        Coroutine rotateRoutine = StartCoroutine(CameraRotate(cam, targetPos, radius, angularSpeed));

        // 대사 출력
        yield return StartCoroutine(ShowText(localeLine));
        DropWrench(); // 렌치 떨어뜨리기

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "From the start, William’s just a fool Emma has been using.";
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            localeLine = "Fin dall’inizio William non è altro che uno sciocco che Emma ha sfruttato.";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "そもそもウィリアムなんて、エマに利用されているだけのやつだ。";
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "애초에 윌리엄 그 샌님은 엠마가 이용하고 있을 뿐인 녀석이니까.";
        }

        yield return StartCoroutine(ShowText(localeLine, () =>
        {
            // 대사가 끝나면 카메라 회전 멈춤
            StopCoroutine(rotateRoutine);

            // SixthScene 실행
            SixthScene();
        }));
    }


    private IEnumerator CameraRotate(Transform cam, Vector3 targetPos, float radius, float angularSpeed)
    {
        float angle = 0f;
        Vector3 originalPos = cam.position;

        while (true) // 다음 씬 전까지 반복
        {
            angle += Time.deltaTime * angularSpeed;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            cam.position = targetPos + new Vector3(x, 10f, z);
            cam.LookAt(targetPos);

            yield return null;
        }
    }

    public void DropWrench()
    {
        if (wrench != null)
        {
            // 부모 관계 끊기
            wrench.transform.SetParent(null);

            // Rigidbody가 없다면 추가
            Rigidbody rb = wrench.GetComponent<Rigidbody>();
            if (rb == null)
                rb = wrench.AddComponent<Rigidbody>();

            // 초기 힘을 가해 튀어나가게 (옵션)
            rb.AddForce(john.transform.forward * 2f + Vector3.up * 3f, ForceMode.Impulse);
        }

        SetAudioClip(1);        
        audioSource.Play();
    }

    private void SixthScene()
    {
        SwitchCamera(5);

        line.color = Color.red;
        
        StartCoroutine(SixthCamera());
    }

    private IEnumerator SixthCamera()
    {
        var currentLocale = LocalizationSettings.SelectedLocale;
        string localeLine = "";

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "This is all the result of what you did.";
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            localeLine = "È tutto il risultato delle tue azioni.";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "全部お前が招いた結果だ。";
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "모두 네가 만든 결과야.";
        }

        yield return StartCoroutine(ShowText(localeLine));

        if (currentLocale.Identifier.Code == "en")
        {
            localeLine = "Farewell, Henry.";
        }
        else if (currentLocale.Identifier.Code == "it")
        {
            localeLine = "Addio, Henry.";
        }
        else if (currentLocale.Identifier.Code == "ja")
        {
            localeLine = "さらばだ、ヘンリー。";
        }
        else if (currentLocale.Identifier.Code == "ko")
        {
            localeLine = "작별이다, 헨리.";
        }

        john.GetComponent<Animator>().SetTrigger("SixthScene");
        yield return StartCoroutine(ShowText(localeLine));
        john.GetComponent<Animator>().SetBool("SixthScene2", true);

        StartCoroutine(FadeUtility.Instance.FadeOut(line, 3f));
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 5f));
        SceneManager.LoadScene("William");
    }






    public void Slide(bool isTalking, float fadeDuration = 1f)
    {
        SlideMove(0, isTalking, fadeDuration);
        SlideMove(1, isTalking, fadeDuration);
    }

    private void SlideMove(int index, bool isOn, float fadeDuration)
    {
        if (index < 0 || index >= slides.Length)
        {
            Debug.LogError("Slide index out of range.");
        }

        RectTransform tmp = slides[index].GetComponent<RectTransform>();
        float y = isOn ? 0f : (index == 0 ? tmp.rect.height : -tmp.rect.height);
        Vector2 targetPosition = new Vector2(tmp.anchoredPosition.x, y);

        StartCoroutine(SmoothMove(tmp, tmp.anchoredPosition, targetPosition));
    }

    private IEnumerator SmoothMove(RectTransform rect, Vector2 startPos, Vector2 endPos)
    {
        float elapsed = 0f;

        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 1.0f);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rect.anchoredPosition = endPos;
    }





    public void ChangeLocale()
    {
        if (isChanging) return;

        StartCoroutine(ChangeLocaleCoroutine(localeDropdown.value));

        var currentLocale = LocalizationSettings.SelectedLocale;
        if (currentLocale.Identifier.Code == "en" || currentLocale.Identifier.Code == "it") delay = 0.05f;
    }


    IEnumerator ChangeLocaleCoroutine(int index)
    {
        isChanging = true;

        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

        isChanging = false;
    }


    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
