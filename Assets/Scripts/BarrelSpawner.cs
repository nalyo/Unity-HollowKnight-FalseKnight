using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject prefabToSpawn;  // 旋转物体的Prefab

    private BoxCollider2D spawnAreaCollider;

    void Awake()
    {
        // 获取Collider
        spawnAreaCollider = GetComponent<BoxCollider2D>();
        spawnAreaCollider.isTrigger = true;
    }

    /// <summary>
    /// 调用此方法一次性生成 n 个物体
    /// </summary>
    public void SpawnObjects(int spawnCount = 5)
    {
        if (prefabToSpawn == null || spawnAreaCollider == null)
            return;

        Vector3 center = spawnAreaCollider.bounds.center;
        Vector3 size = spawnAreaCollider.bounds.size;

        for (int i = 0; i < spawnCount; i++)
        {
            // 在Collider范围内随机生成位置
            float x = Random.Range(center.x - size.x / 2f, center.x + size.x / 2f);
            float y = Random.Range(center.y - size.y / 2f, center.y + size.y / 2f);
            float z = Random.Range(center.z - size.z / 2f, center.z + size.z / 2f);
            Vector3 spawnPos = new Vector3(x, y, z);

            // 随机旋转角度
            Quaternion spawnRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            Instantiate(prefabToSpawn, spawnPos, spawnRot);
        }
    }
}
