using UnityEngine;
using Unity;
using Player;

namespace FalseKnight
{

    public class IdleState : IState
    {
        private FSM manager;
        private Parameter parameter;

        private float timer;

        // 距离阈值
        private float nearDist = 5f;
        private float midDist = 10f;

        public IdleState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("Idle");
            timer = 0f;
        }

        public void OnUpdate()
        {
            timer += Time.deltaTime;

            if (timer > parameter.idleTime * 0.5f 
                && (parameter.transform.position.x - parameter.target.position.x) * parameter.dir < 0)
            {
                manager.ChangeState<TurnState>();
            }

            if (timer > parameter.idleTime)
            {
                // 计算和目标的距离
                float dist = Mathf.Abs(parameter.transform.position.x - parameter.target.position.x);

                if (dist < nearDist)
                {
                    // 近距离：攻击 70%，跳跃 30%
                    float r = Random.value; // [0,1)
                    if (r < 0.5f)
                        manager.ChangeState<JumpAttackUpState>();
                    else if(r < 0.8f)
                        manager.ChangeState<AttackState>();
                    else
                        manager.ChangeState<JumpState>();
                }
                else if (dist < midDist)
                {
                    // 中距离：攻击 60%，跳跃 30%，待机 10%
                    float r = Random.value;
                    if (r < 0.4f)
                        manager.ChangeState<AttackState>();
                    else if (r < 0.9f) // 0.6 ~ 0.9
                        manager.ChangeState<JumpState>();
                    else
                        timer = 0f; // 继续 Idle
                }
                else
                {
                        timer = 0f;
                }


            }
        }

        public void OnExit()
        {
            timer = 0;
        }
    }

    public class TurnState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;

        public TurnState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("Turn");
        }

        public void OnUpdate()
        {
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            Transform transform = parameter.transform;
            if (animInfo.normalizedTime >= .95f)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                parameter.dir = -parameter.dir;
                manager.ChangeState<IdleState>();
            }
        }

        public void OnExit()
        {
        }
    }


    public class JumpState : IState
    {
        Vector3 horizStart, horizEnd;
        float totalTime;
        float elapsed;
        float jumpClipLen, landClipLen;

        private FSM manager;
        private Parameter parameter;

        public JumpState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("Jump");
            parameter.LandSmokePS.GetComponent<ParticleSystem>().Play();
            parameter.JumpSmokePS.GetComponent<ParticleSystem>().Play();

            parameter.startPos = parameter.transform.position;

            if (parameter.isSetLandingPos)
            {
                parameter.isSetLandingPos = false;
            }
            else
            {
                parameter.landingPos = parameter.target != null
                    ? GetAttackAlignedLanding(parameter.transform, parameter.attackRange, parameter.target)
                    : parameter.startPos;
                if (Mathf.Abs(parameter.startPos.x - parameter.landingPos.x) < 3f)
                {
                    int randomDir = Random.value < 0.5f ? -1 : 1;
                    parameter.landingPos = parameter.startPos;
                    parameter.landingPos.x += randomDir * 3f;
                }
            }
            parameter.landingPos = Vector3Extensions.Clamp(parameter.landingPos, parameter.BorderMin.position, parameter.BorderMax.position);

            jumpClipLen = GetClipLength("Jump", parameter.animator, 0.4f);

            // 总时长 = 起跳 + 落地
            totalTime = Mathf.Max(0.1f, jumpClipLen * 1.5f);
            parameter.jumpTotalTime = totalTime;

            horizStart = parameter.startPos;
            horizEnd = parameter.landingPos;

            elapsed = 0f;
        }

        public void OnUpdate()
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / totalTime);

            // XZ 插值
            Vector3 pos = Vector3.Lerp(horizStart, horizEnd, u);

            // Y 曲线
            float yOffset = parameter.jumpYCurve.Evaluate(u) * parameter.jumpHeight;
            pos.y = parameter.startPos.y + yOffset;

            parameter.transform.position = pos;

            // 提前切到落地动画
            if (elapsed >= totalTime)
            {
                parameter.transform.position = parameter.landingPos;
                manager.ChangeState<LandState>();
            }
        }

        public void OnExit() 
        {
        }

        private float GetClipLength(string name, Animator anim, float fallback)
        {
            if (anim == null || anim.runtimeAnimatorController == null) return fallback;
            foreach (var c in anim.runtimeAnimatorController.animationClips)
            {
                if (c.name == name) return c.length;
            }
            return fallback;
        }

        private Vector3 GetAttackAlignedLanding(Transform knight, BoxCollider2D attackCol, Transform player)
        {
            if (attackCol == null || player == null)
                return player != null ? player.position : knight.position;

            Vector3 attackCenter = attackCol.bounds.center;
            Vector3 knightPos = knight.position;
            Vector3 offset = attackCenter - knightPos;
            Vector3 desiredLanding = player.position - offset;
            desiredLanding.y = knightPos.y;

            return desiredLanding;
        }
    }

    public class FallState : IState
    {
        float elapsed;
        float fallTime;         // 到地面的时间
        float startY;           // 起始高度
        float groundY;          // 地面高度

        private FSM manager;
        private Parameter parameter;
        private float gravity = -19.81f;

        public FallState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("Fall");

            // 初始位置
            Vector3 startPos = parameter.transform.position;
            startY = startPos.y;

            // 用自由落体公式求落地时间
            groundY = parameter.landingPos.y;
            float distance = startY - groundY;
            fallTime = Mathf.Sqrt(2f * distance / Mathf.Abs(gravity));

            elapsed = 0f;
        }

        public void OnUpdate()
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            if (elapsed >= fallTime)
            {
                // 落地
                parameter.transform.position = new Vector3(parameter.transform.position.x, groundY, parameter.transform.position.z);
                manager.ChangeState<LandState>();
            }
            else
            {
                // 自由落体公式
                float newY = startY + 0.5f * gravity * elapsed * elapsed;
                parameter.transform.position = new Vector3(parameter.transform.position.x, newY, parameter.transform.position.z);
            }
        }

        public void OnExit() { }
    }

    public class LandState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;

        public LandState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("Land");
            parameter.audioSource.clip = parameter.landClip;
            parameter.audioSource.Play();
            parameter.LandSmokePS.GetComponent<ParticleSystem>().Play();
            parameter.playerCamera.Shake();
        }

        public void OnUpdate()
        {

            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime >= 0.95)
            {
                if (parameter.isToRage)
                {
                    manager.ChangeState<RageState>();
                    parameter.isToRage = false;
                }
                else
                    manager.ChangeState<IdleState>();
            }
        }

        public void OnExit() 
        {
        }
    }


    public class AttackState : IState
    {
        private FSM manager;
        private Parameter parameter;

        private enum AttackPhase { Antic, Attack, Recover }
        private AttackPhase phase;

        private float anticTime = 1.0f;
        private float timer = 0.0f;

        public AttackState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            EnterAntic();
        }

        private void EnterAntic()
        {
            phase = AttackPhase.Antic;
            parameter.animator.Play("AttackAntic");
            timer = 0.0f;
            if (parameter.Head.health < 35)
            {
                int index = Random.Range(0, parameter.attackClips.Length);
                parameter.audioSource.PlayOneShot(parameter.attackClips[index]);
            }
        }

        private void EnterAttack()
        {
            phase = AttackPhase.Attack;
            parameter.animator.Play("Attack");
            parameter.audioSource.clip = parameter.jumpHitClip;
            parameter.audioSource.Play();

            GameObject go = Object.Instantiate(parameter.sliceSpreadPrefab);

            Vector3 prefabPos = go.transform.position;
            Vector3 bossPos = parameter.transform.position;
            SliceSpread spread = go.GetComponent<SliceSpread>();
            spread.Init(new Vector3(bossPos.x - parameter.dir * 2f, prefabPos.y, 0f), new Vector3(-parameter.dir, 0f, 0f));


            parameter.objectSpawner.SpawnObjects(3);
            var obj = Object.Instantiate(parameter.AttackEffect, parameter.transform.position, Quaternion.identity);
            obj.transform.localScale = new Vector3(-parameter.dir * obj.transform.localScale.x,
                obj.transform.localScale.y, obj.transform.localScale.z);
            Object.Destroy(obj, 2.0f);
            parameter.playerCamera.Shake();
        }

        private void EnterRecover()
        {
            phase = AttackPhase.Recover;
            parameter.animator.Play("AttackRecover");
        }

        public void OnUpdate()
        {
            AnimatorStateInfo animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            switch (phase)
            {
                case AttackPhase.Antic:
                    timer += Time.deltaTime;
                    if (timer > anticTime)
                    {
                        EnterAttack();
                    }
                    break;

                case AttackPhase.Attack:
                    if (animInfo.normalizedTime >= .95f)
                    {
                        EnterRecover();
                    }
                    break;
                case AttackPhase.Recover:
                    if (animInfo.normalizedTime >= .95f)
                    {
                        manager.ChangeState<IdleState>();
                    }
                    break;
            }
        }

        public void OnExit()
        {
        }
    }


    public class JumpAttackUpState : IState
    {
        Vector3 horizStart, horizEnd;
        float totalTime;
        float elapsed;
        float jumpClipLen;

        private FSM manager;
        private Parameter parameter;

        public JumpAttackUpState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("JumpAttackUp");
            parameter.JumpSmokePS.GetComponent<ParticleSystem>().Play();
            parameter.LandSmokePS.GetComponent<ParticleSystem>().Play();
            if(parameter.Head.health < 35)
            {
                int index = Random.Range(0, parameter.attackClips.Length);
                parameter.audioSource.PlayOneShot(parameter.attackClips[index]);
            }

            parameter.startPos = parameter.transform.position;
            if(parameter.isSetLandingPos)
            {
                parameter.isSetLandingPos = false;
            }
            else
            {
                parameter.landingPos = parameter.target != null
                    ? GetAttackAlignedLanding(parameter.transform, parameter.attackRange, parameter.target)
                    : parameter.startPos;
            }

            parameter.landingPos = Vector3Extensions.Clamp(parameter.landingPos, parameter.BorderMin.position, parameter.BorderMax.position);

            jumpClipLen = JumpMotionHelper.GetClipLength("JumpAttackUp", parameter.animator, 0.4f);

            totalTime = Mathf.Max(0.05f, jumpClipLen * 1.5f);
            parameter.jumpTotalTime = totalTime; // 保存给下一个状态用

            horizStart = parameter.startPos;
            horizEnd = parameter.landingPos;

            elapsed = 0f;
        }

        public void OnUpdate()
        {
            elapsed += Time.deltaTime;
            float tNorm = Mathf.Clamp01(elapsed / totalTime);

            // XZ 插值
            Vector3 pos = Vector3.Lerp(horizStart, horizEnd, tNorm);

            // Y 由曲线控制
            float yOffset = parameter.jumpYCurve.Evaluate(tNorm) * parameter.jumpHeight;
            pos.y = parameter.startPos.y + yOffset;

            parameter.transform.position = pos;

            // 提前切到落地动画
            if (elapsed >= totalTime)
            {
                manager.ChangeState<JumpAttackHitState>();
            }
        }

        public void OnExit()
        {
        }

        private Vector3 GetAttackAlignedLanding(Transform falseKnight, BoxCollider2D attackCol, Transform player)
        {
            if (attackCol == null || player == null)
                return player != null ? player.position : falseKnight.position;

            Vector3 attackCenter = attackCol.bounds.center;
            Vector3 falseknightPos = falseKnight.position;
            Vector3 offset = attackCenter - falseknightPos;
            Vector3 desiredLanding = player.position - offset;
            desiredLanding.y = falseknightPos.y;
            return desiredLanding;
        }
    }


    public class JumpAttackHitState : IState
    {
        private FSM manager;
        private Parameter parameter;

        private AnimatorStateInfo animInfo;

        public JumpAttackHitState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("JumpAttackHit");
            parameter.audioSource.clip = parameter.jumpHitClip;
            parameter.audioSource.Play();
            if (parameter.Head.health <= 0)
            {
                parameter.isDead = true;
            }else
                parameter.LandSmokePS.GetComponent<ParticleSystem>().Play();

            var obj = Object.Instantiate(parameter.AttackEffect, parameter.transform.position, Quaternion.identity);
            obj.transform.localScale = new Vector3(-parameter.dir * obj.transform.localScale.x,
                obj.transform.localScale.y, obj.transform.localScale.z);
            Object.Destroy(obj, 2.0f);
            parameter.playerCamera.Shake();
        }

        public void OnUpdate()
        {
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime >= .95f)
            {
                parameter.transform.position = parameter.landingPos;
                if (parameter.Head.health <= 0)
                {
                    Vector3 currentPos = parameter.transform.position;
                    parameter.landingPos = new Vector3(currentPos.x, currentPos.y - 20f, currentPos.z);
                    manager.ChangeState<DeathFallState>();
                }
                else
                    manager.ChangeState<IdleState>();
            }
        }

        public void OnExit() 
        {
        }
    }


    public class RollState : IState
    {
        private FSM manager;
        private Parameter parameter;

        // 可调整参数（根据需要改为从 Parameter 读取）
        private float knockbackDistance = 2.0f;   // 向后位移距离
        private float knockupHeight = 1.0f;       // 抛物线顶点高度（相对起点）
        private float clipFallback = 0.5f;        // 动画片段长度回退值

        private Vector3 startPos;
        private Vector3 landingPos;
        private float elapsed;
        private float totalTime;
        private enum State { Roll, RollEnd }
        private State currentState;

        public RollState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
            currentState = State.Roll;
        }

        public void OnEnter()
        {
            // 播放倒地/被击倒动画（请确认 Animator 中有此 clip）
            parameter.animator.Play("Roll");

            startPos = parameter.transform.position;

            // 计算向后方向：
            // 如果有目标（player），则朝远离目标的方向后退；
            // 否则根据 localScale.x 猜测朝向，作为后退方向。
            Vector3 backwardDir = Vector3.left;
            if (parameter.target != null)
            {
                Vector3 dir = startPos - parameter.target.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f) backwardDir = dir.normalized;
            }
            else
            {
                // 2D 常见：localScale.x < 0 表示朝左
                if (parameter.transform.localScale.x < 0f) backwardDir = Vector3.right;
                else backwardDir = Vector3.left;
            }

            // 估算着地点（先在同一高度上）
            landingPos = startPos + backwardDir * knockbackDistance;
            landingPos.y = startPos.y;
            landingPos = Vector3Extensions.Clamp(landingPos, parameter.BorderMin.position, parameter.BorderMax.position);


            // 用动画片段长度估算总时间（与 JumpState 的做法一致）
            float clipLen = GetClipLength("Roll", parameter.animator, clipFallback);
            totalTime = Mathf.Max(0.05f, clipLen); // 保证非零
            elapsed = 0f;

            currentState = State.Roll;
        }

        public void OnUpdate()
        {
            elapsed += Time.deltaTime;
            switch (currentState)
            {
                case State.Roll:
                    if (elapsed < 0.05f)
                    {
                        Time.timeScale = 0.1f;
                    }
                    else
                    {
                        Time.timeScale = 1.0f;
                    }
                    float t = Mathf.Clamp(elapsed, 0f, totalTime);
                    float u = totalTime <= 0f ? 1f : t / totalTime; // 0..1

                    // 水平插值
                    Vector3 pos = Vector3.Lerp(startPos, landingPos, u);

                    // 垂直：线性过渡 + 抛物线 bump（在 u=0.5 取最大值）
                    float baseY = Mathf.Lerp(startPos.y, landingPos.y, u);
                    float bump = 4f * knockupHeight * u * (1f - u); // 0 at 0,1 ; peak at u=0.5
                    pos.y = baseY + bump;

                    parameter.transform.position = pos;
                    if (elapsed >= totalTime)
                    {
                        parameter.playerCamera.Shake();
                        parameter.transform.position = landingPos;
                        parameter.LandSmokePS.GetComponent<ParticleSystem>().Play();
                        currentState = State.RollEnd;
                        parameter.audioSource.clip = parameter.landClip;
                        parameter.audioSource.Play();
                    }
                    break;
                case State.RollEnd:
                    if (elapsed >= totalTime + 1.0f)
                    {
                        manager.ChangeState<HeadOpenState>();
                    }
                    break;
            }
        }

        public void OnExit()
        {
        }
        private float GetClipLength(string name, Animator anim, float fallback)
        {
            if (anim == null || anim.runtimeAnimatorController == null) return fallback;
            foreach (var c in anim.runtimeAnimatorController.animationClips)
            {
                if (c.name == name) return c.length;
            }
            return fallback;
        }
    }

    public class HeadOpenState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;

        public HeadOpenState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("HeadOpen");
            parameter.audioSource.clip = parameter.headOpenClip;
            parameter.audioSource.Play();
        }

        public void OnUpdate()
        {
            if (parameter.Head.damageAcc >= 8.0f)
            {
                parameter.Head.damageAcc = 0.0f;
                manager.ChangeState<RecoverState>();
            }
        }

        public void OnExit()
        {
        }
    }

    public class RecoverState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;

        public RecoverState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("Recover");
        }

        public void OnUpdate()
        {
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if(animInfo.normalizedTime >= .95f)
            {
                parameter.isSetLandingPos = true;
                parameter.landingPos = new Vector3(parameter.ragePos.position.x, parameter.landingPos.y, 0.0f);
                parameter.isToRage = true;
                parameter.audioSource.PlayOneShot(parameter.rageClip);
                manager.ChangeState<JumpState>();
            }
        }

        public void OnExit()
        {
        }
    }

    public class RageState : IState
    {
        private FSM manager;
        private Parameter parameter;

        private enum RagePhase { Antic, Rage }
        private RagePhase phase;

        private float anticTime = 1.0f;
        private float timer = 0.0f;
        private int rageCount = 8;

        public RageState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            EnterAntic();
        }

        private void EnterAntic()
        {
            phase = RagePhase.Antic;
            parameter.animator.Play("AttackAntic");
            timer = 0.0f;
        }

        private void EnterRage()
        {
            phase = RagePhase.Rage;
            parameter.animator.Play("Rage", 0, 0f);
            parameter.audioSource.clip = parameter.attackClip;
            parameter.audioSource.Play();
            if (parameter.Head.health > 0 && rageCount != 8)
            {
                parameter.objectSpawner.SpawnObjects(2);
            }

            var obj = Object.Instantiate(parameter.AttackEffect, parameter.transform.position, Quaternion.identity);
            obj.transform.localScale = new Vector3(-parameter.dir * obj.transform.localScale.x,
                obj.transform.localScale.y, obj.transform.localScale.z);
            Object.Destroy(obj, 2.0f);
            parameter.playerCamera.Shake();
        }

        public void OnUpdate()
        {
            AnimatorStateInfo animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            switch (phase)
            {
                case RagePhase.Antic:
                    timer += Time.deltaTime;
                    if (timer > anticTime)
                    {
                        EnterRage();
                    }
                    break;

                case RagePhase.Rage:
                    if (animInfo.normalizedTime >= 1.2f && rageCount != 0)
                    {
                        rageCount--;
                        if (rageCount <= 0)
                        {
                            if (parameter.Head.health <= 0)
                            {
                                parameter.isSetLandingPos = true;
                                parameter.landingPos = parameter.fallPos.position;
                                manager.ChangeState<JumpAttackUpState>();
                                return;
                            }
                            else
                            {
                                manager.ChangeState<IdleState>();
                                return;
                            }
                        }
                        parameter.transform.localScale = 
                            new Vector3(-parameter.transform.localScale.x, parameter.transform.localScale.y, parameter.transform.localScale.z);
                        parameter.dir = -parameter.dir;
                        EnterRage();
                    }
                    break;
            }
        }

        public void OnExit()
        {
            rageCount = 8;
            timer = 0.0f;
        }
    }

    public class DeathFallState : IState
    {
        float elapsed;
        float fallTime;         // 到地面的时间
        float startY;           // 起始高度
        float groundY;          // 地面高度

        private FSM manager;
        private Parameter parameter;
        private float gravity = -19.81f;

        public DeathFallState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("DeathFall");
            parameter.audioSource.clip = parameter.fallClip;
            parameter.audioSource.Play();

            // 初始位置
            Vector3 startPos = parameter.transform.position;
            startY = startPos.y;

            // 用自由落体公式求落地时间
            groundY = parameter.landingPos.y;
            float distance = startY - groundY;
            fallTime = Mathf.Sqrt(2f * distance / Mathf.Abs(gravity));

            elapsed = 0f;
        }

        public void OnUpdate()
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            if (elapsed >= fallTime)
            {
                // 落地
                parameter.transform.position = new Vector3(parameter.transform.position.x, groundY, parameter.transform.position.z);
            }
            else
            {
                // 自由落体公式
                float newY = startY + 0.5f * gravity * elapsed * elapsed;
                parameter.transform.position = new Vector3(parameter.transform.position.x, newY, parameter.transform.position.z);
            }
        }

        public void OnExit() { }
    }
}