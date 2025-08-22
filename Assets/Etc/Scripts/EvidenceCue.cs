using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceCue : MonoBehaviour
{
    public float amplitude = 10f;   // �̵� ����
    public float frequency = 1f;    // �̵� �ӵ�

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
        StartCoroutine(FloatEffect());
    }

    private IEnumerator FloatEffect()
    {
        while (true)
        {
            float y = Mathf.Sin(Time.time * frequency) * amplitude;
            rectTransform.anchoredPosition = startPos + new Vector2(0, y);
            yield return null;
        }
    }
}
