using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem; // �� �ʿ�!

public class IntroManager : MonoBehaviour
{
    public Image screen;    
    public GameObject dialogue;
    private Image[] slides = new Image[2];
    private Text line;
    private GameObject endWaitingMark;


    public AudioClip[] audioClips; // ����� Ŭ�� �迭 (�ʿ�� ���)
    public Camera[] cameras;
    public GameObject john;
    public GameObject wrench;


    private float delay = 0.1f;     // ���ڴ� ��� �ӵ� (��)
    private Action[] methods;
    private AudioSource audioSource;

    void Start()
    {
        SwitchCamera(-1);
        screen.gameObject.SetActive(true);

        // �޼ҵ���� �迭�� ���
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
        methods[0]();
        audioSource.loop = false;
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
        line.text = ""; // �ʱ�ȭ

        // ���� �ϳ��� ���
        foreach (char c in fullText)
        {
            line.text += c;
            yield return new WaitForSeconds(delay);
        }

        // ��� �ؽ�Ʈ�� ��µ� �� �� �Է� ���
        bool waitingInput = true;
        endWaitingMark.SetActive(true); // �Է� ��� ��ũ Ȱ��ȭ
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

            yield return null; // ���� �����ӱ��� ���
        }

        endWaitingMark.SetActive(false); // �Է� ��� ��ũ ��Ȱ��ȭ

        // �Է��� �����Ǹ� ���� �׼� ����
        if (onFinished != null)
            onFinished();
    }







    public void FirstScene()
    {
        float decreaseY = 1f;   // ���� y �� (��: 1��ŭ ��������)
        float duration = 5f;    // �̵� �ð� (1��)

        SwitchCamera(0);
        SetAudioClip(0);
        audioSource.Play(); // ����� Ŭ�� ���

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

            // Lerp�� �ε巴�� �̵�
            cameras[0].transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        // ���� ��ġ ����
        cameras[0].transform.position = endPos;
        StartCoroutine(ShowText("��...", SecondScene));
    }
    
    private void SecondScene()
    {
        SwitchCamera(1);
        line.color = Color.red; // �ؽ�Ʈ ���� �ʱ�ȭ
        StartCoroutine(SecondCamera());
    }

    private IEnumerator SecondCamera()
    {        
        // ī�޶� ���� �ݺ� �̵� �ڷ�ƾ ����
        Coroutine bobRoutine = StartCoroutine(CameraBob(cameras[1].transform, 0.05f, 0.5f));

        // ù ���
        yield return StartCoroutine(ShowText("��... ��..."));

        // �� �ִϸ��̼� Ʈ����
        john.GetComponent<Animator>().SetTrigger("SecondScene");

        // �� ��° ��� (������ ���� �� ����)
        yield return StartCoroutine(ShowText("�� ����", () =>
        {
            // ī�޶� ���� �̵� ����
            StopCoroutine(bobRoutine);
            cameras[1].transform.localPosition = Vector3.zero; // ���� ��ġ�� ����
            ThirdScene(); // ���� �� ȣ��
        }));
    }

    private IEnumerator CameraBob(Transform cam, float amplitude, float frequency)
    {
        Vector3 originalPos = cam.localPosition;

        while (true) // ���� ������ �Ѿ�� ������ ��� �ݺ�
        {
            float newY = Mathf.Sin(Time.time * frequency) * amplitude;
            cam.localPosition = originalPos + new Vector3(0f, newY, 0f);

            yield return null;
        }
    }


    private void ThirdScene()
    {
        SwitchCamera(2);

        float moveX = -2f;    // x������ �̵��� �� (��: 2��ŭ ����������)
        float duration = 5f; // �̵� �ð� (��)

        StartCoroutine(ThirdCamera(moveX, duration));
    }

    private IEnumerator ThirdCamera(float moveX, float duration)
    {
        john.GetComponent<Animator>().SetBool("ThirdScene", true);
        StartCoroutine(ShowText("��Ӵϰ� �׷� ������ ���� �Ŷ��...", FourthScene));

        Vector3 startPos = cameras[2].transform.position;
        Vector3 endPos = new Vector3(startPos.x + moveX, startPos.y, startPos.z);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // �ε巴�� x��ǥ �̵�
            cameras[2].transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        // ���� ��ġ ����
        cameras[2].transform.position = endPos;
    }

    private void FourthScene()
    {
        SwitchCamera(3);
        john.GetComponent<Animator>().SetBool("ThirdScene", false);

        float moveX = -2f;    // x������ �̵��� �� (��: 2��ŭ ����������)
        float duration = 5f; // �̵� �ð� (��)

        line.color = Color.white; // �ؽ�Ʈ ���� �ʱ�ȭ
        StartCoroutine(FourthCamera(moveX, duration));
    }

    private IEnumerator FourthCamera(float moveX, float duration)
    {
        // ��鸲 �ڷ�ƾ ����
        Coroutine shakeRoutine = StartCoroutine(CameraShake(cameras[3].transform, 0.001f, 5f));

        yield return StartCoroutine(ShowText("��...", FifthScene));
    }

    private IEnumerator CameraShake(Transform cam, float magnitude, float frequency)
    {
        Vector3 originalPos = cam.localPosition;

        while (true) // ��� ����
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

        Vector3 target = john.transform.position; // ���� �߽����� �� �
        StartCoroutine(FifthCamera(target, 12f, 0.25f));        
    }

    private IEnumerator FifthCamera(Vector3 targetPos, float radius = 5f, float angularSpeed = 0.5f)
    {
        Transform cam = cameras[4].transform;
        float angle = 0f;

        // ī�޶� ȸ�� �ڷ�ƾ ����
        Coroutine rotateRoutine = StartCoroutine(CameraRotate(cam, targetPos, radius, angularSpeed));

        // ��� ���
        yield return StartCoroutine(ShowText("�װ� ������ ������ �� ���� �ǰ���"));
        DropWrench(); // ��ġ ����߸���
        yield return StartCoroutine(ShowText("���ʿ� ������ �� ������ ������ �̿��ϰ� ���� ���� �༮�̴ϱ�", () =>
        {
            // ��簡 ������ ī�޶� ȸ�� ����
            StopCoroutine(rotateRoutine);

            // SixthScene ����
            SixthScene();
        }));
    }


    private IEnumerator CameraRotate(Transform cam, Vector3 targetPos, float radius, float angularSpeed)
    {
        float angle = 0f;
        Vector3 originalPos = cam.position;

        while (true) // ���� �� ������ �ݺ�
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
            // �θ� ���� ����
            wrench.transform.SetParent(null);

            // Rigidbody�� ���ٸ� �߰�
            Rigidbody rb = wrench.GetComponent<Rigidbody>();
            if (rb == null)
                rb = wrench.AddComponent<Rigidbody>();

            // �ʱ� ���� ���� Ƣ����� (�ɼ�)
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
        yield return StartCoroutine(ShowText("��� �װ� ���� �����"));
        john.GetComponent<Animator>().SetTrigger("SixthScene");
        yield return StartCoroutine(ShowText("�ۺ��̴�, �"));
        john.GetComponent<Animator>().SetBool("SixthScene2", true);

        StartCoroutine(FadeUtility.Instance.FadeOut(line, 3f));
        yield return new WaitForSeconds(3f);
        StartCoroutine(FadeUtility.Instance.FadeIn(screen, 5f));
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

}
