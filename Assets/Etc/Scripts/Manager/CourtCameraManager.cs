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

    [SerializeField] private float rotateSpeed = 2f; // ī�޶� ȸ�� �ӵ�
    [SerializeField] private float waitTime = 3f;    // �� �ι� �ٶ󺸴� �ð�

    [SerializeField] private float idleDelay = 5f; // �Է� ���� �� �� �� ������ ���� ��������
    private Coroutine idleRoutine;    
    private Coroutine inputCheckRoutine;

    public void StartIdleCameraMovement()
    {
        if (idleRoutine == null)
            idleRoutine = StartCoroutine(IdleCameraRoutine());
    }

    public void StopIdleCameraMovement()
    {
        if (idleRoutine != null)
        {
            StopCoroutine(idleRoutine);
            idleRoutine = null;
            movingCamera.enabled = false;
        }
    }

    public void StartInputCheckRoutine()
    {
        if (inputCheckRoutine == null)
            inputCheckRoutine = StartCoroutine(CheckInputIdle());
    }

    public void StopInputCheckRoutine()
    {
        if (inputCheckRoutine != null)
        {
            StopCoroutine(inputCheckRoutine);
            inputCheckRoutine = null;
        }
    }

    private IEnumerator CheckInputIdle()
    {
        float timer = 0f;

        while (true)
        {
            if (courtManager != null && courtManager.InputField != null)
            {
                if (courtManager.InputField.interactable)
                {
                    // �Է� ������ ���� �� �ð� ����
                    timer += Time.deltaTime;

                    if (timer >= idleDelay)
                    {
                        // ���� �ð� ��� �� ī�޶� ���� ����
                        StartIdleCameraMovement();
                    }
                }
                else
                {
                    // �Է� �Ұ� ���� �� �ʱ�ȭ �� ī�޶� ���� ����
                    timer = 0f;
                    StopIdleCameraMovement();
                }
            }

            yield return null;
        }
    }


    // ���� ī�޶� ���� ����
    private IEnumerator IdleCameraRoutine()
    {
        // ���� ���� �� movingCamera�� Ȱ��ȭ
        SwitchCamera(-1);

        while (true)
        {
            int randomEffect = Random.Range(0, 4); // ���� 0~3 (�� 4�� ����)

            switch (randomEffect)
            {
                case 0:
                    yield return StartCoroutine(QuarterViewOrbit(judgeTarget));
                    break;
                case 1:
                    yield return StartCoroutine(LowViewPan(lawyerTarget));
                    break;
                case 2:
                    yield return StartCoroutine(OverShoulderView(playerTarget));
                    break;
                case 3:
                    yield return StartCoroutine(DramaticZoom(judgeTarget)); // Dramatic Zoom �߰�
                    break;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator QuarterViewOrbit(Transform target, float radius = 5f, float height = 2f, float speed = 20f)
    {
        float duration = waitTime;
        float angle = 0f;
        while (duration > 0f)
        {
            angle += 20f * Time.deltaTime;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0.5f, Mathf.Sin(angle)) * 4f;
            movingCamera.transform.position = target.position + offset + Vector3.up * 2f;
            movingCamera.transform.LookAt(target.position + Vector3.up * 1.5f);

            duration -= Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator LowViewPan(Transform target, float distance = 3f, float height = 0.5f, float angleRange = 10f, float speed = 1f)
    {
        float duration = waitTime;
        float t = 0f;
        while (duration > 0f)
        {
            t += Time.deltaTime;
            float angle = Mathf.Sin(t) * 10f;
            Vector3 pos = target.position - target.forward * 3f + Vector3.up * 0.5f;
            movingCamera.transform.position = pos;
            movingCamera.transform.rotation = Quaternion.LookRotation(target.position - pos) * Quaternion.Euler(0, angle, 0);

            duration -= Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DramaticZoom(Transform target, float targetFOV = 40f, float speed = 1f)
    {
        float duration = waitTime; // �� ���� ���� �ð�
        float startFOV = movingCamera.fieldOfView;

        while (duration > 0f)
        {
            // ��� �ٶ󺸱�
            if (target != null)
            {
                movingCamera.transform.LookAt(target.position + Vector3.up * 1.5f);
            }

            // FOV ����
            movingCamera.fieldOfView = Mathf.Lerp(movingCamera.fieldOfView, targetFOV, Time.deltaTime * speed);

            duration -= Time.deltaTime;
            yield return null;
        }

        // ������ ���� �þ߰����� ����
        movingCamera.fieldOfView = startFOV;
    }

    private IEnumerator OverShoulderView(Transform target, float backDistance = 2f, float height = 1.5f, float shakeIntensity = 0.05f, float shakeSpeed = 2f)
    {
        float duration = waitTime;
        while (duration > 0f)
        {
            Vector3 offset = -target.forward * 2f + Vector3.up * 1.5f;
            Vector3 basePos = target.position + offset;

            // ��¦ ��鸲
            float shakeX = (Mathf.PerlinNoise(Time.time * 2f, 0f) - 0.5f) * 0.05f;
            float shakeY = (Mathf.PerlinNoise(0f, Time.time * 2f) - 0.5f) * 0.05f;

            Vector3 shakenPos = basePos + new Vector3(shakeX, shakeY, 0);
            movingCamera.transform.position = Vector3.Lerp(movingCamera.transform.position, shakenPos, Time.deltaTime * 5f);
            movingCamera.transform.LookAt(target.position + Vector3.up * 1.5f);

            duration -= Time.deltaTime;
            yield return null;
        }
    }








    private void SwitchCamera(int index)
    {
        if (courtManager.LineText != null)
            courtManager.LineText.text = "";

        if(index == -1)
        {
            movingCamera.gameObject.SetActive(true);
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].gameObject.SetActive(false);
            }

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
