using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using AdvancedPeopleSystem;
using Unity.Cinemachine;

public class Player : MonoBehaviour
{
    public GameObject ThirdPersonAimCamera;
    public ConversationManager conversationManager;
    public CourtManager courtManager;

    [SerializeField] private bool isTalking = false; // ��ȭ ������ ����
    public bool IsTalking
    {
        get { return isTalking; }
        set { isTalking = value; }
    }

    private Animator animator;
    private AudioSource audioSource;
    private CharacterCustomization cc;
    private Coroutine zoomCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        cc = GetComponent<CharacterCustomization>();
        cc.InitColors();
    }


    public void GetLookAtTarget(Transform target)
    {
        ThirdPersonAimCamera.GetComponent<CinemachineCamera>().LookAt = target;
    }    

    public void ZoomCamera(bool isTrue)
    {
        float targetDistance = isTrue ? 4.0f : 3.0f;
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(SmoothZoomCamera(targetDistance, 0.2f));
    }

    private IEnumerator SmoothZoomCamera(float targetDistance, float duration)
    {
        var follow = ThirdPersonAimCamera.GetComponent<CinemachineThirdPersonFollow>();
        float startDistance = follow.CameraDistance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            follow.CameraDistance = Mathf.Lerp(startDistance, targetDistance, elapsed / duration);
            yield return null;
        }
        follow.CameraDistance = targetDistance;
    }



    /// <summary>
    /// �÷��̾ NPC �������� ȸ��
    /// - `Vector3.ProjectOnPlane()`�� ����Ͽ� y�� ȸ���� ����
    /// </summary>
    public void TurnTowardNPC(Transform npcTrans)
    {
        Vector3 direction = npcTrans.position - transform.position;
        direction = Vector3.ProjectOnPlane(direction, Vector3.up); // y���� �������� �ʰ� ��鿡�� ���⸸ ����
        transform.rotation = Quaternion.LookRotation(direction);

        // cameraTransform.GetComponent<CameraScript>().FocusNPC(this.transform);
    }
}
