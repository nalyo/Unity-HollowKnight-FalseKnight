using UnityEngine;


public class PlayerHitBox : MonoBehaviour
{
    public float damage = 0f;
    public Vector2 triggerCenter;
    public bool isTriggered;
    public bool isEnemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Armor"))
        {
            Vector2 weaponCenter = GetComponent<Collider2D>().bounds.center;
            Vector2 enemyCenter = other.bounds.center;

            // X 轴取中点，Y 轴用武器中心
            triggerCenter = new Vector2(
                weaponCenter.x,
                weaponCenter.y
            );

            //other.GetComponent<FalseKnight.FalseKnightController>().TakeDamage(damage, triggerCenter);
            isTriggered = true;
            if (other.CompareTag("Enemy"))
                isEnemy = true;
        }
    }
}
