using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeUtility : MonoBehaviour
{
    /// <summary>
    /// �̱��� �ν��Ͻ�
    /// - `Instance`�� ���� �������� FadeUtility�� ���� ����
    /// </summary>
    public static FadeUtility Instance { get; private set; }
    private bool isFadingLoopActive = false;    // �������� ���̵� ��/�ƿ� �ݺ� ����

    /// <summary>
    /// �̱��� ������ �����Ͽ� �ν��Ͻ��� ����
    /// - �� ���� FadeUtility�� �����ϵ��� ����
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);  // �̹� �ν��Ͻ��� �����ϸ� �ı�
        }
    }

    /// <summary>
    /// �׷���(�̹��� �Ǵ� �ؽ�Ʈ)�� ���̵� ��/�ƿ� ��Ű�� ���� �޼���
    /// </summary>
    /// <param ���̵� ȿ���� ������ UI ���="graphic"></param>
    /// <param ���̵尡 �Ϸ�Ǵ� �ð�="fadeDuration"></param>
    /// <param ���� ���� ��="startAlpha"> (0: ���� ����, 1: ���� ������)</param>
    /// <param ��ǥ ���� ��="targetAlpha"></param>
    /// <param �Ϸ� �� UI ��Ȱ��ȭ ����="deactivateOnComplete"></param>
    private IEnumerator Fade(Graphic graphic, float fadeDuration, float startAlpha, float targetAlpha, bool deactivateOnComplete = false)
    {
        graphic.gameObject.SetActive(true);
        Color color = graphic.color;
        color.a = startAlpha;
        graphic.color = color;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            graphic.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        graphic.color = color;

        if (deactivateOnComplete)
        {
            graphic.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// �׷����� ���̵� �� (������ ��Ÿ���� ��)
    /// </summary>
    /// <param name="graphic">���̵� ȿ���� ������ UI ���</param>
    /// <param name="fadeDuration">���̵尡 �Ϸ�Ǵ� �ð�</param>
    /// <param name="targetAlpha">��ǥ ���� ��</param>
    public IEnumerator FadeIn(Graphic graphic, float fadeDuration, float targetAlpha = 1.0f)
    {
        yield return StartCoroutine(Fade(graphic, fadeDuration, 0f, targetAlpha));
    }


    /// <summary>
    /// �׷����� ���̵� �ƿ� (������ ������� ��)
    /// </summary>
    /// <param name="graphic">���̵� ȿ���� ������ UI ���</param>
    /// <param name="fadeDuration">���̵尡 �Ϸ�Ǵ� �ð�</param>
    /// <param name="alpha">�ʱ� ���� �� (�⺻��: 1.0f)</param>
    public IEnumerator FadeOut(Graphic graphic, float fadeDuration, float alpha = 1.0f)
    {
        yield return StartCoroutine(Fade(graphic, fadeDuration, alpha, 0f, true));
    }

    /// <summary>
    /// �׷����� ���������� ���̵� ��/�ƿ� �ݺ�
    /// </summary>
    /// <param name="graphic">���̵� ȿ���� ������ UI ���</param>
    /// <param name="fadeDuration">���̵尡 �Ϸ�Ǵ� �ð�</param>
    /// <param name="delay">���̵� �� ��� �ð�</param>
    public IEnumerator FadeInOut(Graphic graphic, float fadeDuration, float delay = 0.5f)
    {
        yield return StartCoroutine(FadeIn(graphic, fadeDuration));
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(FadeOut(graphic, fadeDuration));
            yield return new WaitForSeconds(delay);
    }

    /// <summary>
    /// `FadeInOut()`�� �ݺ��� �����ϴ� �޼���
    /// </summary>
    public void StopFadeInOut()
    {
        isFadingLoopActive = false;
    }

    
    /// <summary>
    /// ī�޶� ��ȯ�� ���̵� ȿ���� �Բ� ����
    /// </summary>
    /// <param name="screen">���̵� ȿ���� ������ �̹���</param>
    /// <param name="cameraManager">ī�޶� ������</param>
    /// <param name="player">�÷��̾� ������Ʈ</param>
    /// <param name="npcRole">NPC ����</param>
    /// <param name="spawnManager">NPC ���� ������ (���� ����)</param>
    public IEnumerator SwitchCameraWithFade(Image screen, Player player, SpecificNPC npcRole)
    {
        yield return FadeIn(screen, 1f);

        yield return new WaitForSeconds(1f);
        StartCoroutine(npcRole.TurnTowardPlayer(player.transform));
        player.TurnTowardNPC(npcRole.transform);        

        // if (spawnManager != null)
        //     spawnManager.RelocationNPC(npcRole);

        yield return FadeOut(screen, 1f);
    }
    





    public IEnumerator FadeInAudio(AudioSource source, float duration)
    {
        source.volume = 0f; // ó�� ������ 0���� ����
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            source.volume = Mathf.Lerp(0f, 1f, elapsedTime / duration); // ������ ����
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        source.volume = 1f; // ���� ���� 1
    }
}