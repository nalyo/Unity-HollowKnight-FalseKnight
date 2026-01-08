using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalseKnightHeadController : MonoBehaviour
{
    public float health = 100f;
    public float damageAcc = 0.0f;
    public Color damageColor;
    public GameObject OrangeImapctPS1;
    public GameObject OrangeImapctPS2;

    private Animator animator;
    private SpriteRenderer sr;
    private Material mat;

    public AudioSource creatureSource;
    public AudioSource swordHitSource;
    public AudioClip[] creatureClips;
    public AudioClip swordHitClip;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>(); 
        sr = GetComponent<SpriteRenderer>();
        mat = Instantiate(sr.sharedMaterial); // clone to avoid changing shared mat
        sr.material = mat;
    }

    private void OnEnable()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        animator.Play("Idle");
        timer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("PlayerHit") && timer > 1.0f)
        {
            TakeDamage(collision.GetComponent<PlayerHitBox>().damage);
        }
    }

    public void TakeDamage(float damage)
    {
        animator.Play("Hit", 0, 0f);
        StartCoroutine(ApplyDamageAfterAnim(damage));
        int index = Random.Range(0, creatureClips.Length);
        creatureSource.clip = creatureClips[index];
        creatureSource.Play();
        swordHitSource.clip = swordHitClip;
        swordHitSource.Play();
        StartCoroutine(FlashMaterialCoroutine(0.25f));
        Instantiate(OrangeImapctPS1, transform.position, Quaternion.identity);
        Instantiate(OrangeImapctPS1, transform.position, Quaternion.Euler(0f, 180f, 0f)); 
        var obj = Instantiate(OrangeImapctPS2, transform.position, Quaternion.identity);
        obj.transform.localScale = transform.parent.localScale;
    }

    private IEnumerator ApplyDamageAfterAnim(float damage)
    {
        // 获取动画时长
        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);

        health -= damage;
        damageAcc += damage;
    }

    IEnumerator FlashMaterialCoroutine(float duration, float flashSpeed = 5f)
    {
        float t = 0f;
        mat.SetColor("_FlashColor", damageColor);
        while (t < duration)
        {
            t += Time.deltaTime;
            // 用 PingPong 让数值在 0-1 之间来回
            float v = Mathf.PingPong(t * flashSpeed, 1f);
            mat.SetFloat("_Flash", v);
            yield return null;
        }
        mat.SetFloat("_Flash", 0f); // 结束后恢复正常
    }
}
