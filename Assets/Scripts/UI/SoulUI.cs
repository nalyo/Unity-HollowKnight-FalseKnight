using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulUI : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Animator SoulOrbEyesAnim;
    [SerializeField] private Animator SoulFillAnim;
    private enum SoulState { Idle, Fill, Shrink, Drain }
    private Animator anim;
    private float soulOrbHeight;
    private float currentSoulOrbHeight = 0.0f;
    private float targetSoulOrbHeight = 0.0f;
    private RectTransform rect;
    void Start()
    {
        anim = GetComponent<Animator>();
        rect = GetComponent<RectTransform>();
        soulOrbHeight = -rect.localPosition.y;
    }

    public void Fill(float currentSoulRate)
    {
        //anim.SetInteger("state", (int)SoulState.Fill);
        targetSoulOrbHeight = currentSoulRate * soulOrbHeight;
        if (currentSoulRate > 0.5f)
            SoulOrbEyesAnim.SetBool("isMoreThanHalf", true);
        if (currentSoulRate < 0.95f)
            SoulFillAnim.Play("UnFull", 0, 0.0f);
        else
            SoulFillAnim.Play("Full", 0, 0.0f);
    }
    public void Shrink()
    {
    }
    public void Drain(float currentSoulRate)
    {
        anim.SetInteger("state", (int)SoulState.Drain);
        targetSoulOrbHeight = currentSoulRate * soulOrbHeight;
        if (currentSoulRate < 0.5f)
            SoulOrbEyesAnim.SetBool("isMoreThanHalf", false);
    }

    public void Idle()
    {
        anim.SetInteger("state", (int)SoulState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        currentSoulOrbHeight = soulOrbHeight + rect.localPosition.y;
        if (Mathf.Abs(targetSoulOrbHeight - currentSoulOrbHeight) > Mathf.Epsilon)
        {
            Vector3 targetPos = new Vector3(rect.localPosition.x, targetSoulOrbHeight - soulOrbHeight);
            rect.localPosition = Vector3.Lerp(rect.localPosition, targetPos, 0.1f);
        }
    }
}
