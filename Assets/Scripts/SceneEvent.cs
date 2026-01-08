using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEvent : MonoBehaviour
{
    public Transform player;
    public Transform eventPos;
    public AudioSource audioSource;
    public AudioClip bgm;
    public AudioClip bgmFight;
    public GameObject falseKnight;
    public AudioSource doorAudioSource;
    public AudioSource door1AudioSource;
    public AudioSource sceneAudioSource;
    public AudioClip doorCloseClip;
    public AudioClip cellingBreakClip;
    public Animator doorAnim;
    public Animator door1Anim;
    public GameObject breakFloor;
    public BoxCollider2D floor;
    public BoxCollider2D floorBossDeadLeft;
    public BoxCollider2D floorBossDeadRight;
    public GameObject floorBreakSmoke;
    public GameObject cellingBreakSmoke;
    public Animator BossNameAnim;

    [Header("破坏效果参数")]
    public float explosionForce = 5f;     // 初始冲力大小
    public float torqueForce = 5f;        // 随机旋转
    public float autoDestroyDelay = 3f;   // 碎片多久后销毁

    private bool isTriggered = false;
    public PlayerCamera playerCamera;

    void Start()
    {

        audioSource.clip = bgm;
        audioSource.Play();
    }

    void Update()
    {
        if (player.position.x < eventPos.position.x && !isTriggered)
        {
            isTriggered = true;
            playerCamera.isLimitPos = true;
            Invoke("DoorClose", 0.5f);
        }

        if (falseKnight.GetComponent<FalseKnight.FalseKnightController>().parameter.isDead)
        {
            BreakAllObjects();
            breakFloor.SetActive(true);
            floor.enabled = false;
            floorBossDeadLeft.enabled = true;
            floorBossDeadRight.enabled = true;
            floorBreakSmoke.SetActive(true);
        }
    }

    void DoorClose()
    {
        falseKnight.SetActive(true);
        audioSource.clip = bgmFight;
        audioSource.loop = false;
        audioSource.Play();
        doorAnim.Play("Close");
        door1Anim.Play("Close 1");
        doorAudioSource.clip = doorCloseClip;
        door1AudioSource.clip = doorCloseClip;
        doorAudioSource.Play();
        door1AudioSource.Play();
        sceneAudioSource.clip = cellingBreakClip;
        sceneAudioSource.Play();
        cellingBreakSmoke.SetActive(true);
        BossNameAnim.Play("Appear");
    }

    void BreakAllObjects()
    {
        GameObject[] brokenObjects = GameObject.FindGameObjectsWithTag("Broken");

        foreach (GameObject obj in brokenObjects)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = obj.AddComponent<Rigidbody2D>();

            // 开启物理，但不和角色碰撞（可以用 Layer 过滤）
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f; // 让它受重力下落

            // 移除 Collider 避免弹飞角色（可选：换成专用 Layer）
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col != null) Destroy(col);

            // 初始飞散方向：主要向下 + 随机水平
            Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, -0.5f)).normalized;
            rb.velocity = dir * explosionForce;

            // 随机旋转
            rb.angularVelocity = Random.Range(-torqueForce, torqueForce);

            Destroy(obj, autoDestroyDelay);
        }
    }


}
