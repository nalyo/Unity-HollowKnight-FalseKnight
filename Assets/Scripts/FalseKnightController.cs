using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Unity.VisualScripting;
using UnityEngine;

namespace FalseKnight
{
    [System.Serializable]
    public class Parameter
    {
        public float health = 10;
        public float idleTime = 2f;
        public Animator animator;
        public Transform target;
        public Transform transform;
        public Vector3 startPos;
        public Vector3 landingPos;
        public Transform BorderMax;
        public Transform BorderMin;
        public bool isSetLandingPos = false;
        public Transform fallPos;
        public Transform ragePos;
        public bool isToRage = false;
        public int dir = 1;
        public BoxCollider2D attackRange;
        public BoxCollider2D hitboxColl;
        public BoxCollider2D selfColl;
        public LayerMask playerLayer;
        public float flashTime;
        public float crashDamage;

        public AudioSource audioSource;
        public AudioClip hitSoundClip;
        public AudioClip finalHitSoundClip;
        public AudioClip landClip;
        public AudioClip jumpHitClip;
        public AudioClip attackClip;
        public AudioClip fallClip;
        public AudioClip rageClip;
        public AudioClip headOpenClip;
        public AudioClip []attackClips;

        public GameObject slashEffect;

        public AnimationCurve jumpYCurve;  // Y 方向曲线
        public float jumpHeight = 5f;      // Y 轴最高点
        public float jumpTotalTime;        // 总时长（Up+Hit）


        public GameObject sliceSpreadPrefab;
        public SpriteRenderer sr;

        public FalseKnightHeadController Head;
        public bool isDead = false;

        public GameObject orbBreakPS;
        public GameObject orbBreakSmokePS;

        public ObjectSpawner objectSpawner;
        public GameObject LandSmokePS;
        public GameObject JumpSmokePS;
        public GameObject AttackEffect;

        public PlayerCamera playerCamera;
    }

    public class FalseKnightController : MonoBehaviour
    {
        public Parameter parameter;

        private Material mat;

        private FSM fsm = new FSM();
        private float damageAcc = 0.0f;

        // Start is called before the first frame update
        void Start()
        {
            fsm.AddState<IdleState>(new IdleState(fsm, parameter));
            fsm.AddState<TurnState>(new TurnState(fsm, parameter));
            fsm.AddState<JumpState>(new JumpState(fsm, parameter));
            fsm.AddState<FallState>(new FallState(fsm, parameter));
            fsm.AddState<LandState>(new LandState(fsm, parameter));
            fsm.AddState<AttackState>(new AttackState(fsm, parameter));
            fsm.AddState<JumpAttackUpState>(new JumpAttackUpState(fsm, parameter));
            fsm.AddState<JumpAttackHitState>(new JumpAttackHitState(fsm, parameter));
            fsm.AddState<RollState>(new RollState(fsm, parameter));
            fsm.AddState<HeadOpenState>(new HeadOpenState(fsm, parameter));
            fsm.AddState<RecoverState>(new RecoverState(fsm, parameter));
            fsm.AddState<RageState>(new RageState(fsm, parameter));
            fsm.AddState<DeathFallState>(new DeathFallState(fsm, parameter));

            parameter.landingPos = parameter.transform.position;
            parameter.transform.position = new Vector3(parameter.transform.position.x,
                parameter.transform.position.y + 10f, parameter.transform.position.z);
            fsm.ChangeState<FallState>();

            parameter.animator = GetComponent<Animator>();
            parameter.sr = GetComponent<SpriteRenderer>();
            mat = Instantiate(parameter.sr.sharedMaterial); // clone to avoid changing shared mat
            parameter.sr.material = mat;
        }

        // Update is called once per frame
        void Update()
        {
            fsm.OnUpdate();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision != null && collision.CompareTag("PlayerHit"))
            {
                PlayerHitBox playerHitBox = collision.GetComponent<PlayerHitBox>();
                TakeDamage(playerHitBox.damage, playerHitBox.triggerCenter);
            }
        }


        public void TakeDamage(float damage, Vector2 triggerCenter)
        {
            damageAcc += damage;
            parameter.health -= damage;
            StartCoroutine(FlashMaterialCoroutine(parameter.flashTime));
            if (triggerCenter != Vector2.zero)
            {
                parameter.slashEffect.SetActive(true);
                parameter.slashEffect.transform.position = new Vector3(triggerCenter.x, triggerCenter.y, parameter.slashEffect.transform.position.z);
                float randomRotationZ = UnityEngine.Random.Range(0f, 360f);
                parameter.slashEffect.transform.rotation = Quaternion.Euler(0f, 0f, randomRotationZ);
                parameter.slashEffect.GetComponent<Animator>().Play("slashEffect");
            }

            var obj = Instantiate(parameter.orbBreakPS, transform);
            obj.transform.localScale = transform.localScale;
            obj = Instantiate(parameter.orbBreakSmokePS, transform);
            obj.transform.localScale = transform.localScale;

            if (damageAcc >= 20.0f)
            {
                damageAcc = 0.0f;
                fsm.ChangeState<RollState>();
                parameter.audioSource.clip = parameter.finalHitSoundClip;
                parameter.audioSource.Play();
            }
            else
            {
                parameter.audioSource.clip = parameter.hitSoundClip;
                parameter.audioSource.Play();
            }
        }

        IEnumerator FlashMaterialCoroutine(float duration)
        {
            float t = 0f;
            mat.SetFloat("_Flash", 1f); // 立刻白
            while (t < duration)
            {
                t += Time.deltaTime;
                float v = 1f - (t / duration); // 从 1 -> 0
                mat.SetFloat("_Flash", Mathf.Clamp01(v));
                yield return null;
            }
            mat.SetFloat("_Flash", 0f);
        }
    }
}