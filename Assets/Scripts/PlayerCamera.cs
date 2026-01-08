using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float yOffset = 2.0f;
    [SerializeField] private float smoothing = 0.05f;

    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layer;
        [Range(0f, 1f)]
        public float parallaxFactor = 0.5f;
    }

    [SerializeField] private ParallaxLayer[] layers;

    private Vector3 lastPlayerPos;

    [SerializeField] private Transform BorderMax;
    [SerializeField] private Transform BorderMin;

    private float camHalfWidth;
    public bool isLimitPos = false;

    // --- 震动参数 ---
    private Coroutine shakeRoutine;
    private Vector3 shakeOffset = Vector3.zero; // 震动偏移

    void Start()
    {
        if (player != null)
            lastPlayerPos = player.position;

        yOffset += player.position.y;

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            camHalfWidth = cam.orthographicSize * cam.aspect;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float targetX = player.position.x;
        float targetY = yOffset;

        if (isLimitPos)
        {
            float quarterWidth = camHalfWidth * 0.75f;
            float minLimit = BorderMin.position.x + quarterWidth;
            float maxLimit = BorderMax.position.x - quarterWidth;

            targetX = Mathf.Clamp(targetX, minLimit, maxLimit);
        }

        // 基础相机位置
        Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);
        Vector3 basePos = transform.position;
        if (targetPos != transform.position)
            basePos = Vector3.Lerp(transform.position, targetPos, smoothing);

        // 在基础位置上叠加震动偏移
        transform.position = basePos + shakeOffset;

        // 计算玩家移动量（用基础位置，而不是加了震动的）
        Vector3 delta = basePos - lastPlayerPos;

        foreach (var l in layers)
        {
            if (l.layer != null)
            {
                l.layer.position += new Vector3(delta.x * l.parallaxFactor, 0f, 0f);
            }
        }

        lastPlayerPos = basePos;
    }

    // ---------------- 相机震动 ----------------
    public void Shake(float duration = 0.5f, float magnitude = 0.05f)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;
        float seed = Random.value * 100f; // 随机种子

        while (elapsed < duration)
        {
            // 用 Perlin Noise 生成平滑值
            float offsetX = (Mathf.PerlinNoise(seed, elapsed * 10f) - 0.5f) * 2f * magnitude;
            float offsetY = (Mathf.PerlinNoise(seed + 1f, elapsed * 10f) - 0.5f) * 2f * magnitude;

            shakeOffset = new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeRoutine = null;
    }

}
