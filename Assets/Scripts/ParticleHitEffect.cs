using System.Collections.Generic;
using UnityEngine;

public class ParticleHitEffect : MonoBehaviour
{
    public float timeToDestroy;
    public GameObject hitEffectPrefab; // 带动画的预制体

    private void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    void OnParticleCollision(GameObject other)
    {
        if (hitEffectPrefab == null)
            return;
        // 获取所有碰撞点
        ParticleSystem ps = GetComponent<ParticleSystem>();
        List<ParticleCollisionEvent> events = new List<ParticleCollisionEvent>();
        int count = ps.GetCollisionEvents(other, events);

        for (int i = 0; i < count; i++)
        {
            Instantiate(hitEffectPrefab, events[i].intersection, Quaternion.identity);
        }
    }
}
