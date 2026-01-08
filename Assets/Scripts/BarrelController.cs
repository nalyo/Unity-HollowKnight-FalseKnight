using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelController : MonoBehaviour
{
    public Transform barrelTransform;
    public GameObject barrelBreak;

    // 旋转参数
    public float rotationSpeed = 360f; // 默认旋转速度
    public Vector3 rotationAxis = Vector3.forward;

    // 当前累积角度
    private float currentAngle = 0f;

    // 大小范围
    public Vector2 scaleRange = new Vector2(0.5f, 0.7f); // 最小-最大缩放
    // 旋转速度范围
    public Vector2 rotationSpeedRange = new Vector2(360f, 450f); // 最小-最大旋转速度
    // 重力范围
    public Vector2 gravityScaleRange = new Vector2(0.5f, 1.0f); // 最小-最大重力

    private Rigidbody2D rb;


    // 被击飞参数
    private bool isHit = false;
    private Vector2 hitDirection;
    public float hitForce = 5.0f;
    public float hitDuration = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        // 随机初始旋转
        barrelTransform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // 随机缩放
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        barrelTransform.localScale = new Vector3(scale, scale, scale);

        // 随机旋转速度
        rotationSpeed = Random.Range(rotationSpeedRange.x, rotationSpeedRange.y);

        // 随机重力
        if (rb != null)
        {
            rb.gravityScale = Random.Range(gravityScaleRange.x, gravityScaleRange.y);
        }
    }

    void Update()
    {
        // 累加旋转角度
        currentAngle += rotationSpeed * Time.deltaTime;

        // 保持角度在 0~360
        if (currentAngle > 360f)
            currentAngle -= 360f;

        // 使用四元数旋转
        barrelTransform.rotation = Quaternion.Euler(rotationAxis * currentAngle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            BreakBarrel();
        }
        else if (collision.CompareTag("Armor"))
        {
            if (isHit)
            {
                // 飞行状态下碰到敌人造成伤害
                collision.GetComponent<FalseKnight.FalseKnightController>()?.TakeDamage(1.0f, Vector2.zero);
                BreakBarrel();
            }
            else
            {
                BreakBarrel();
            }
        }
        else if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player.PlayerController>()?.TakeDamage(1.0f);
            BreakBarrel();
        }
        else if (collision.CompareTag("PlayerHit"))
        {
            // 获取击打方向
            Vector2 playerPos = collision.transform.position;
            hitDirection = (transform.position - (Vector3)playerPos).normalized;
            hitDirection.x *= 4f;
            hitDirection.y = 0.5f;

            // 设置飞行状态
            isHit = true;

            // 应用飞行力
            rb.AddForce(hitDirection * hitForce, ForceMode2D.Impulse);
        }
    }

    private void BreakBarrel()
    {
        Instantiate(barrelBreak, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
