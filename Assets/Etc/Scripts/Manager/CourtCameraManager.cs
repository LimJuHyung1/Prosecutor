using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class CourtCameraManager : MonoBehaviour
{
    public Camera[] cameras;
    public Camera movingCamera;

    public CourtManager courtManager;
    [SerializeField] private float zoomSpeed = 5f;   // �� �ӵ� (���� Ŭ���� ������ ����)
    [SerializeField] private float minFOV = 20f;     // �ּ� �� (������)
    [SerializeField] private float maxFOV = 60f;     // �ִ� �� (�ָ�)
    private Dictionary<int, Coroutine> zoomCoroutines = new Dictionary<int, Coroutine>();

    public Transform playerTarget;
    public Transform judgeTarget;
    public Transform lawyerTarget;

    private Coroutine inputCheckCoroutine;



    private void Start()
    {
        // ������ �� �ڷ�ƾ ����
        inputCheckCoroutine = StartCoroutine(CheckInputFieldRoutine());
    }

    private IEnumerator CheckInputFieldRoutine()
    {
        while (!movingCamera.gameObject.activeSelf)
        {
            if (courtManager != null && courtManager.InputField != null)
            {
                if (courtManager.InputField.interactable)
                {                                        
                    // Debug.Log("[Camera] InputField ��� ��...");
                }
            }
            yield return new WaitForSeconds(5f); // 5�ʸ��� üũ
            SwitchCamera(-1);
            StartCoroutine(OrbitQuarterView(playerTarget));
        }
    }


    // target�� �߽����� ���ͺ� �������� ���
    public IEnumerator OrbitQuarterView(Transform target, float radius = 5f, float height = 5f, float speed = 10f)
    {
        if (movingCamera == null || target == null) yield break;

        float angle = 0f;

        while (true)
        {
            angle += speed * Time.deltaTime;
            if (angle > 360f) angle -= 360f;

            // ī�޶� ��ġ ��� (XZ ��鿡�� ȸ�� + ���� ����)
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                height,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            movingCamera.transform.position = target.position + offset;

            // Ÿ�� �ٶ󺸱�
            movingCamera.transform.LookAt(target.position);

            yield return null;
        }
    }










    private void SwitchCamera(int index)
    {
        if (index == -1)
        {
            movingCamera.gameObject.SetActive(true);
            for (int i = 0; i < cameras.Length; i++)
                cameras[i].gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < cameras.Length; i++)
                cameras[i].gameObject.SetActive(false);

            cameras[index].gameObject.SetActive(true);
        }
    }
    

    // �� �� ȣ��
    // �� ��û (��� = �� �ƿ�, ���� = �� ��)
    public void Zoom(int index, float amount)
    {
        if (index < 0 || index >= cameras.Length) return;

        SwitchCamera(index);

        float targetFOV = Mathf.Clamp(cameras[index].fieldOfView + amount, minFOV, maxFOV);

        // ���� �ڷ�ƾ ����
        if (zoomCoroutines.ContainsKey(index) && zoomCoroutines[index] != null)
            StopCoroutine(zoomCoroutines[index]);


        // �� �ڷ�ƾ ����
        zoomCoroutines[index] = StartCoroutine(SmoothZoom(index, targetFOV));
    }

    private IEnumerator SmoothZoom(int index, float targetFOV)
    {
        Camera cam = cameras[index];

        while (Mathf.Abs(cam.fieldOfView - targetFOV) > 0.05f)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        cam.fieldOfView = targetFOV; // ���� ����

        for (int i = 0; i < cameras.Length; i++)
        {
            if (i != index)
                cameras[i].fieldOfView = 60f;
        }
    }

    // ���� �Լ�
    public void ZoomIn(int index) => Zoom(index, -10f);
    public void ZoomOut(int index) => Zoom(index, 10f);

}
