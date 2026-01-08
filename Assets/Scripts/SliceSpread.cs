using Unity.VisualScripting;
using UnityEngine;

public class SliceSpread : MonoBehaviour
{
    public GameObject slicePrefab;    // 切片预制体（必须带 SpriteRenderer）
    public Sprite[] sliceSprites;     // 每个切片的 sprite
    public float spacing = 0.1f;      // 最终间隔
    public float duration = 2.0f;     // 展开所需时间
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float moveDistance = 1f;               // 移动距离

    [HideInInspector] public Vector3 moveDirection; // 移动方向

    private GameObject[] slices;
    private Vector3[] targetPositions;
    private float timer = 0f;
    private bool initialized = false;
    private Vector3 startPos;

    /// <summary>
    /// 初始化（实例化 prefab 后必须调用）
    /// </summary>
    public void Init(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        startPos = position;
        moveDirection = direction.normalized;

        int count = sliceSprites.Length * 3; // 每个素材重复3次
        slices = new GameObject[count];
        targetPositions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            // 生成切片
            slices[i] = Instantiate(slicePrefab, transform);
            slices[i].transform.localPosition = Vector3.zero;

            // 设置 sprite
            SpriteRenderer sr = slices[i].GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = sliceSprites[i / 3];

            // 计算目标位置（等间隔对称）
            float offset = (i - (count - 1) / 2f) * spacing;
            targetPositions[i] = new Vector3(offset, 0, 0);
        }

        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        if (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            // 用曲线控制速度
            float curvedT = curve.Evaluate(t);

            // 插值移动切片
            for (int i = 0; i < slices.Length; i++)
            {
                slices[i].transform.localPosition = Vector3.Lerp(Vector3.zero, targetPositions[i], curvedT);

                // sprite 渐渐变宽
                SpriteRenderer sr = slices[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float scaleX = Mathf.Lerp(0.5f, 1.5f, curvedT); // 例：最终放大 1.5 倍
                    slices[i].transform.localScale = new Vector3(scaleX, 1f, 1f);
                }
            }

            // 整体移动
            transform.position = Vector3.Lerp(startPos, startPos + moveDirection * moveDistance, curvedT);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
