using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOnce : MonoBehaviour
{
    public float additionalTime = 0.0f;
    private AnimatorStateInfo animInfo;
    private Animator animator;
    public AudioClip clip;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("Idle");
        if (clip != null)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        animInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animInfo.normalizedTime >= .95f)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            Destroy(gameObject, additionalTime);
        }
    }
}
