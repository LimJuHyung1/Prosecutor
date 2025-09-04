using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WilliamManager : MonoBehaviour
{
    public Image screen;
    public Image image1;
    public Image image2;
    public GameObject dialogue;    
    
    [SerializeField] private Image[] slides = new Image[2];
    [SerializeField] private Text NPCName;
    [SerializeField] private Text line;
    [SerializeField] private GameObject endWaitingMark;

    public AudioClip[] audioClips; // ����� Ŭ�� �迭 (�ʿ�� ���)
    public Camera[] cameras;
    public Sprite[] sprites;    // 0 - ������ ����, 1 - ���, 2 - ���, 3 - � �ý����� �߰ߵ� ���
    public GameObject phone;
    
    public Profile profileSO;
    public ProfileManager profileManager;

    public GameObject dataPanel;
    public GameObject profilePage;
    public Image newsPage;
    public Image crimeScenePage;


    private float delay = 0.05f;     // ���ڴ� ��� �ӵ� (��)
    private Action[] methods;
    private AudioSource audioSource;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SwitchCamera(-1);

        // �޼ҵ���� �迭�� ���
        methods = new Action[]
        {
            FirstScene,
            SecondScene
        };

        image1.gameObject.SetActive(false);
        image2.gameObject.SetActive(false);

        audioSource = GetComponent<AudioSource>();

        NPCName = dialogue.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        line = dialogue.transform.GetChild(1).GetChild(1).GetComponent<Text>();
        endWaitingMark = dialogue.transform.GetChild(1).GetChild(2).gameObject;
        slides[0] = dialogue.transform.GetChild(0).GetComponent<Image>();
        slides[1] = dialogue.transform.GetChild(1).GetComponent<Image>();

        dataPanel.gameObject.SetActive(false);
        // Slide(true, 0f);
        // SwitchCamera(0);
        methods[0]();
    }

    private void SwitchCamera(int index)
    {
        if (line != null)
            line.text = "";

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        if (index != -1)
            cameras[index].gameObject.SetActive(true);
    }

    private void SetAudioClip(int index)
    {
        audioSource.clip = audioClips[index];
    }

    public IEnumerator ShowText(string NPCName = "", string fullText = "", Action onFinished = null)
    {
        this.NPCName.text = NPCName;
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
        SwitchCamera(0);
        Slide(true);

        StartCoroutine(FirstCamera());
    }

    private IEnumerator FirstCamera()
    {
        yield return new WaitForSeconds(2f);

        // yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 3f));
        Slide(true, 0f);

        SetAudioClip(0);
        audioSource.Play();        

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(LookAtPhone(2f));

        SetAudioClip(1);
        audioSource.Play();
        yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 1f));

        yield return new WaitForSeconds(2f);

        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("����", "�˻��, ��� ���� ���̽��ϱ�?"));

        NPCName.color = Color.darkRed;
        yield return StartCoroutine(ShowText("����", "� ������ ��� �����Ͻô� ����?"));

        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("����", "��. �� ����� �˻�Բ� ������ �����Դϴ�."));
        yield return StartCoroutine(ShowText("����", "�� ��� �ڷḦ ������ ���޵帮�ڽ��ϴ�."));

        NPCName.color = Color.darkRed;
        yield return StartCoroutine(ShowText("����", "��, �غ��ϰ� �ְڽ��ϴ�."));

        SetAudioClip(2);
        audioSource.Play();
        yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 1f));

        yield return StartCoroutine(ShowText("����", "(���� ������ �������� ���� ���忡�� ���ص� ä�� �߰ߵǾ���)"));
        yield return StartCoroutine(ShowText("����", "(�켱 ��� �ֺ� �ι����� �����ؾ� �ǰھ�)"));

        yield return StartCoroutine(FadeUtility.Instance.FadeIn(screen, 3f));

        yield return StartCoroutine(ShowText("", "", SecondScene));
    }

    // cameras[0]�� phone�� ���� ȸ���ϴ� �ڷ�ƾ
    private IEnumerator LookAtPhone(float duration = 2f)
    {
        Camera cam = cameras[0];
        if (cam == null || phone == null)
            yield break;

        // ���� ȸ������ ��ǥ ȸ���� ����
        Quaternion startRot = cam.transform.rotation;
        Vector3 dir = (phone.transform.position - cam.transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // �ε巴�� ȸ��
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // ���������� phone�� ��Ȯ�� �ٶ󺸵��� ����
        cam.transform.rotation = targetRot;
    }


    public void SecondScene()
    {
        // SwitchCamera(0);

        StartCoroutine(SecondCamera());
    }

    private IEnumerator SecondCamera()
    {
        // ������ ���� UI Ȱ��ȭ

        // yield return StartCoroutine(FadeUtility.Instance.FadeOut(screen, 3f));

        image1.sprite = sprites[0];
        StartCoroutine(FadeUtility.Instance.FadeIn(image1, 1f));
        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("����", "�� ����� ������, ������ ĳ��Ż�� �λ����Դϴ�."));

        image2.sprite = sprites[1];
        StartCoroutine(FadeUtility.Instance.FadeIn(image2, 3f));
        yield return StartCoroutine(ShowText("����", "�� �� ��, ��� ������ �ž��� �����ߴٴ� ����� ��п� �����ƽ��ϴ�."));
        yield return StartCoroutine(ShowText("����", "�׷��� �ֱ� ���� ������ �����ϴ� �������� �н�ȸ��� �Ƿ��� ���� ��Ȳ�� �߰��߰�,\n���� öȸ�� �����ߴٰ� �մϴ�."));

        NPCName.color = Color.darkRed;
        yield return StartCoroutine(ShowText("����", "� ���忡���� ���ڱ��� ������ �� ������ �̴ϴ�. ��Ե� ������ �߰ڱ���."));

        StartCoroutine(FadeUtility.Instance.FadeOut(image1, 1.5f));
        yield return StartCoroutine(FadeUtility.Instance.FadeOut(image2, 1.5f));
        
        image1.sprite = sprites[2];
        image2.sprite = sprites[3];
        StartCoroutine(FadeUtility.Instance.FadeIn(image1, 1f));
        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("����", "�½��ϴ�. ������ ��� ���� öȸ�� �δ��ϴٸ� �������� ���� �Ҽ��� �����߽��ϴ�."));

        StartCoroutine(FadeUtility.Instance.FadeIn(image2, 1f));
        yield return StartCoroutine(ShowText("����", "�׸��� �� ����, ��� ���ش��� �̴ϴ�."));

        NPCName.color = Color.darkRed;
        yield return StartCoroutine(ShowText("����", "�������� ��ǿ� ������� ���ɼ��� ������ �� ���ڱ���."));
        yield return StartCoroutine(ShowText("����", "�� ��� ���̿� ���� ���� �־����� �� �� ���� �ľ��ؾ߰ڽ��ϴ�."));

        StartCoroutine(FadeUtility.Instance.FadeOut(image1, 1.5f));
        yield return StartCoroutine(FadeUtility.Instance.FadeOut(image2, 1.5f));

        NPCName.color = Color.darkBlue;
        yield return StartCoroutine(ShowText("����", "�� �������� ����ǿ��� ���� ����ϰ� �� �����Դϴ�."));
        yield return StartCoroutine(ShowText("����", "����ǿ� ������ ���� �ڷ� �� ��� ���� �ڷᰡ �غ�Ǿ� �ֽ��ϴ�."));
        yield return StartCoroutine(ShowText("����", "��, ����Ͻ���."));

        // Data UI Ȱ��

        NPCName.text = "";
        line.text = "";
        Slide(false, 1f);        
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Interrogation Room 1");
        // dataPanel.gameObject.SetActive(true);
    }





    public void ProfileButton()
    {
        profileManager.UpdateProfileUI(profileSO);
        profilePage.gameObject.SetActive(true);
    }

    public void NewsButton()
    {
        newsPage.gameObject.SetActive(true);
    }

    public void CrimeSceneButton()
    {
        crimeScenePage.gameObject.SetActive(true);
    }

    public void InvestigateWilliam()
    {
        // SceneManager.LoadScene();
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
