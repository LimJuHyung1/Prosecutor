using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class CourtCameraManager : MonoBehaviour
{
    public Camera[] cameras;
    public Camera movingCamera;

    public CourtManager courtManager;
    [SerializeField] private float zoomSpeed = 5f;   // 줌 속도 (값이 클수록 빠르게 변함)
    [SerializeField] private float minFOV = 20f;     // 최소 줌 (가까이)
    [SerializeField] private float maxFOV = 60f;     // 최대 줌 (멀리)
    private Dictionary<int, Coroutine> zoomCoroutines = new Dictionary<int, Coroutine>();

    public Transform playerTarget;
    public Transform judgeTarget;
    public Transform lawyerTarget;

    private Coroutine inputCheckCoroutine;



    private void Start()
    {
        // 시작할 때 코루틴 실행
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
                    // Debug.Log("[Camera] InputField 대기 중...");
                }
            }
            yield return new WaitForSeconds(5f); // 5초마다 체크
            SwitchCamera(-1);
            StartCoroutine(OrbitQuarterView(playerTarget));
        }
    }


    // target을 중심으로 쿼터뷰 시점에서 원운동
    public IEnumerator OrbitQuarterView(Transform target, float radius = 5f, float height = 5f, float speed = 10f)
    {
        if (movingCamera == null || target == null) yield break;

        float angle = 0f;

        while (true)
        {
            angle += speed * Time.deltaTime;
            if (angle > 360f) angle -= 360f;

            // 카메라 위치 계산 (XZ 평면에서 회전 + 높이 고정)
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                height,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            movingCamera.transform.position = target.position + offset;

            // 타겟 바라보기
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
    

    // 줌 인 호출
    // 줌 요청 (양수 = 줌 아웃, 음수 = 줌 인)
    public void Zoom(int index, float amount)
    {
        if (index < 0 || index >= cameras.Length) return;

        SwitchCamera(index);

        float targetFOV = Mathf.Clamp(cameras[index].fieldOfView + amount, minFOV, maxFOV);

        // 기존 코루틴 중지
        if (zoomCoroutines.ContainsKey(index) && zoomCoroutines[index] != null)
            StopCoroutine(zoomCoroutines[index]);


        // 새 코루틴 실행
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
        cam.fieldOfView = targetFOV; // 최종 보정

        for (int i = 0; i < cameras.Length; i++)
        {
            if (i != index)
                cameras[i].fieldOfView = 60f;
        }
    }

    // 편의 함수
    public void ZoomIn(int index) => Zoom(index, -10f);
    public void ZoomOut(int index) => Zoom(index, 10f);

}
