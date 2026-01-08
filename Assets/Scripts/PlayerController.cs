using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    [System.Serializable]
    public class Parameter
    {
        public float idleTime = 2f;
        public Transform target;
        public Transform transform;
        public float jumpHeight = 1.0f; // 抛物线顶点相对基线的高度
        public Vector3 startPos;
        public Vector3 landingPos;

        public float dirX = 0.0f;
        public LayerMask jumpableGround;
        public float moveSpeed = 7.0f;
        public float jumpForce = 3.0f;
        public float attackRecovery = 0.3f;
        public float health = 5.0f;
        public float soul = 0.0f;
        public Rigidbody2D rb;
        public SpriteRenderer sprite;
        public BoxCollider2D coll;
        public Animator animator;

        public BoxCollider2D slashHitboxColl;
        public BoxCollider2D upSlashHitboxColl;
        public BoxCollider2D downSlashHitboxColl;


        public AudioSource audioSource;
        public AudioClip runAudio;
        public AudioClip slashAudio;
        public AudioClip damagedAudio;
        public AudioClip jumpAudio;
        public AudioClip landAudio;

        public PlayerHitBox playerHitBox;
        public PlayerHitBox playerDownHitBox;

        public SoulUI soulUI;

        public GameObject damagePS;
    }

    public class PlayerController : MonoBehaviour
    {
        public Parameter parameter;

        private FSM fsm = new FSM();
        private bool invincibility = false;
        public float invincibleTime = 1.0f;
        float timer = 0.0f;
        float attackTimer = 0.0f;

        private SpriteRenderer sr;
        private Material mat;

        private float soulDrainAcc;
        private float soulDrainCD = 0.5f;
        private float soulDrainTimer = 0.0f;


        // Start is called before the first frame update
        private void Start()
        {
            fsm.AddState<IdleState>(new IdleState(fsm, parameter));
            fsm.AddState<ProstrateState>(new ProstrateState(fsm, parameter));
            fsm.AddState<ProstrateRiseState>(new ProstrateRiseState(fsm, parameter));
            fsm.AddState<RunState>(new RunState(fsm, parameter));
            fsm.AddState<TurnState>(new TurnState(fsm, parameter));
            fsm.AddState<JumpState>(new JumpState(fsm, parameter));
            fsm.AddState<FallState>(new FallState(fsm, parameter));
            fsm.AddState<LandState>(new LandState(fsm, parameter));
            fsm.AddState<SlashState>(new SlashState(fsm, parameter));
            fsm.AddState<UpSlashState>(new UpSlashState(fsm, parameter));
            fsm.AddState<DownSlashState>(new DownSlashState(fsm, parameter));
            fsm.AddState<DamagedState>(new DamagedState(fsm, parameter));
            fsm.AddState<DeathState>(new DeathState(fsm, parameter));
            fsm.ChangeState<ProstrateState>();

            parameter.slashHitboxColl.enabled = false;
            parameter.upSlashHitboxColl.enabled = false;
            parameter.downSlashHitboxColl.enabled = false;

            sr = GetComponent<SpriteRenderer>();
            mat = Instantiate(sr.sharedMaterial); // clone to avoid changing shared mat
            sr.material = mat;
        }

        // Update is called once per frame
        private void Update()
        {
            attackTimer += Time.deltaTime;
            parameter.dirX = Input.GetAxisRaw("Horizontal");

            fsm.OnUpdate();
            if (fsm.GetCurrentType() == typeof(ProstrateRiseState) || fsm.GetCurrentType() == typeof(ProstrateState)
                || fsm.GetCurrentType() == typeof(DamagedState) || fsm.GetCurrentType() == typeof(DeathState))
                return;
            if(fsm.GetCurrentType() != typeof(SlashState))
                parameter.rb.velocity = new Vector2(parameter.dirX * parameter.moveSpeed, parameter.rb.velocity.y);
            bool isGround = IsGrounded();
            if (Input.GetKeyDown(KeyCode.K) && isGround && !invincibility)
            {
                fsm.ChangeState<JumpState>();
            }
            if (Input.GetKeyUp(KeyCode.K) && parameter.rb.velocity.y > 0)
            {
                parameter.rb.velocity = new Vector2(parameter.rb.velocity.x, parameter.rb.velocity.y * 0.3f);
            }
            if (Input.GetKeyDown(KeyCode.J) && !invincibility && attackTimer > parameter.attackRecovery
                && fsm.GetCurrentType() != typeof(TurnState))
            {
                attackTimer = 0.0f;
                if (Input.GetKey(KeyCode.W))
                    fsm.ChangeState<UpSlashState>();
                else if (Input.GetKey(KeyCode.LeftShift) && !isGround)
                    fsm.ChangeState<DownSlashState>();
                else
                    fsm.ChangeState<SlashState>();
            }
            if(Input.GetKey(KeyCode.U) && fsm.GetCurrentType() == typeof(IdleState) && soulDrainTimer > soulDrainCD)
            {
                if(parameter.soul > 30.0f)
                {
                    float soulDrian = 90.0f * (Time.deltaTime / 3.0f);
                    soulDrainAcc += soulDrian;
                    parameter.soul -= soulDrian;
                    parameter.soulUI.Drain(parameter.soul / 90.0f);
                    if (soulDrainAcc > 30.0f)
                    {
                        soulDrainTimer = 0.0f;
                        soulDrainAcc = 0.0f;
                        parameter.health += 1.0f;
                    }
                }
            }
            if(Input.GetKeyUp(KeyCode.U))
            {
                soulDrainAcc = 0.0f;
                parameter.soulUI.Idle();
            }
            timer += Time.deltaTime;
            soulDrainTimer += Time.deltaTime;
            if (timer > invincibleTime)
                invincibility = false;
        }


        public void TakeDamage(float damage)
        {
            if (invincibility) return;
            else invincibility = true;
            timer = 0.0f;
            parameter.health -= damage;
            if (parameter.health <= 0.0f)
                fsm.ChangeState<DeathState>();
            else
            {
                fsm.ChangeState<DamagedState>();
                StartCoroutine(FlashMaterialCoroutine(1f));
            }
            parameter.damagePS.GetComponent<ParticleSystem>().Play();
        }

        private bool IsGrounded()
        {
            return Physics2D.BoxCast(parameter.coll.bounds.center, parameter.coll.bounds.size, 0.0f, Vector2.down, .1f, parameter.jumpableGround);
        }

        IEnumerator FlashMaterialCoroutine(float duration, float flashSpeed = 5f)
        {
            float t = 0f; 
            mat.SetColor("_FlashColor", Color.black);
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
}
