using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public GameObject heartPrefab;        // 单个血块预制体
    public Transform heartContainer;      // 血块容器（带 Horizontal Layout Group）
    public Player.PlayerController playerCont;            // 当前血量

    [Header("Sprites")]
    public Sprite fullHeart;              // 满心
    public Sprite emptyHeart;             // 空心

    private List<Animator> hearts = new List<Animator>();
    private int maxHealth = 5;                // 最大血量
    private event Action OnHeartsSpawned;
    public void InitHearts()
    {
        foreach (Transform child in heartContainer)
            Destroy(child.gameObject);
        hearts.Clear();

        StartCoroutine(SpawnHearts());
        OnHeartsSpawned += () =>
        {
            foreach (var anim in hearts)
            {
                anim.SetTrigger("initDone");
            }
        };
    }

    IEnumerator SpawnHearts()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);
            Animator anim = heart.GetComponent<Animator>();
            hearts.Add(anim);

            // 延迟一段时间再继续
            yield return new WaitForSeconds(0.5f); // 0.5 秒为例
        }

        OnHeartsSpawned?.Invoke();
    }


    public void UpdateHearts(int hp)
    {

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartHealth = hp - i;

            if (heartHealth >= 1)
                hearts[i].SetBool("isBreak", false);
            else
                hearts[i].SetBool("isBreak", true);
        }
    }

    private void Start()
    {
        InitHearts();
    }

    private void Update()
    {
        UpdateHearts((int)playerCont.parameter.health);
    }
}
