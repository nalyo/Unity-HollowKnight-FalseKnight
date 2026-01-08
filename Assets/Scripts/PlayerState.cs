using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using FalseKnight;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class IdleState : IState
    {
        private FSM manager;
        private Parameter parameter;

        public IdleState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        // Start is called before the first frame update
        public void OnEnter()
        {
            parameter.animator.Play("Idle");
        }

        // Update is called once per frame
        public void OnUpdate()
        {
            if (Mathf.Abs(parameter.dirX) > .1f && parameter.rb.velocity.y == 0)
            {
                manager.ChangeState<RunState>();
            }
            else if (parameter.rb.velocity.y < 0)
            {
                manager.ChangeState<FallState>();
            }
        }

        public void OnExit()
        {
        }
    }

    public class ProstrateState : IState
    {
        private FSM manager;
        private Parameter parameter;

        public ProstrateState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        // Start is called before the first frame update
        public void OnEnter()
        {
            parameter.animator.Play("Prostrate");
        }

        // Update is called once per frame
        public void OnUpdate()
        {
            if (Mathf.Abs(parameter.dirX) > .1f)
            {
                manager.ChangeState<ProstrateRiseState>();
            }
        }

        public void OnExit()
        {
        }
    }

    public class ProstrateRiseState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;

        public ProstrateRiseState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        // Start is called before the first frame update
        public void OnEnter()
        {
            parameter.animator.Play("ProstrateRise");
        }

        // Update is called once per frame
        public void OnUpdate()
        {
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime >= .95f)
            {
                manager.ChangeState<IdleState>();
            }
        }

        public void OnExit()
        {
        }
    }

    public class RunState : IState
    {
        private FSM manager;
        private Parameter parameter;

        private enum RunPhase { Antic, Running, ToIdle }
        private RunPhase phase;

        public RunState(FSM manager, Parameter parameter)
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
            phase = RunPhase.Antic;
            parameter.animator.Play("RunAntic");
            parameter.audioSource.clip = parameter.runAudio;
            parameter.audioSource.loop = true;
            parameter.audioSource.Play();
        }

        private void EnterRunning()
        {
            phase = RunPhase.Antic;
            parameter.animator.Play("Running");
        }
        private void EnterToIdle()
        {
            phase = RunPhase.ToIdle;
            parameter.animator.Play("RunToIdle");
        }

        public void OnUpdate()
        {
            if (Mathf.Abs(parameter.dirX) < .1f)
            {
                EnterToIdle();
            }
            bool dirChanged = (parameter.transform.localScale.x * parameter.dirX) > 0;
            AnimatorStateInfo animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            switch (phase)
            {
                case RunPhase.Antic:
                    if (dirChanged)
                    {
                        manager.ChangeState<TurnState>();
                    }
                    if (animInfo.normalizedTime >= .95f)
                    {
                        EnterRunning();
                    }
                    break;

                case RunPhase.Running:
                    if(dirChanged)
                    {
                        manager.ChangeState<TurnState>();
                    }
                    break;
                
                case RunPhase.ToIdle:
                    if (dirChanged)
                    {
                        manager.ChangeState<TurnState>();
                    }
                    if (animInfo.normalizedTime >= .95f)
                    {
                        manager.ChangeState<IdleState>();
                    }
                    break;
            }
        }

        public void OnExit()
        {
            parameter.audioSource.loop = false;
            parameter.audioSource.Pause();
        }
    }

    public class TurnState : IState
    {
        private FSM manager;
        private Parameter parameter;

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
            
            AnimatorStateInfo animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            
            if (animInfo.normalizedTime >= .95f)
            {
                Transform transform = parameter.transform;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                manager.ChangeState<RunState>();
            }
        }

        public void OnExit()
        {
        }
    }



    public class JumpState : IState
    {
        private FSM manager;
        private Parameter parameter;

        public JumpState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        // Start is called before the first frame update
        public void OnEnter()
        {
            parameter.rb.gravityScale = 1.0f; // 设置较小的重力
            parameter.rb.velocity = new Vector2(parameter.rb.velocity.x, parameter.jumpForce);
            parameter.animator.Play("Jump");
            parameter.audioSource.clip = parameter.jumpAudio;
            parameter.audioSource.Play();
        }

        // Update is called once per frame
        public void OnUpdate()
        {
            Transform transform = parameter.transform;
            if (parameter.dirX > 0.0f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (parameter.dirX < 0.0f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            if (parameter.rb.velocity.y < 0)
                manager.ChangeState<FallState>();
        }

        public void OnExit()
        {
        }
    }


    public class FallState : IState
    {
        private FSM manager;
        private Parameter parameter;

        public FallState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        // Start is called before the first frame update
        public void OnEnter()
        {
            parameter.animator.Play("Fall");
            parameter.rb.gravityScale = 3.0f;
        }

        // Update is called once per frame
        public void OnUpdate()
        {
            Transform transform = parameter.transform;
            if (parameter.dirX > 0.0f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (parameter.dirX < 0.0f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            if (Mathf.Abs(parameter.rb.velocity.y) < .01f) manager.ChangeState<LandState>();
        }
        public void OnExit()
        {
            parameter.rb.gravityScale = 2.0f;
        }
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

        // Start is called before the first frame update
        public void OnEnter()
        {
            parameter.animator.Play("Land");
        }

        // Update is called once per frame
        public void OnUpdate()
        {
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime >= .95f)
            {
                manager.ChangeState<IdleState>();
                parameter.audioSource.clip = parameter.landAudio;
                parameter.audioSource.Play();
            }
        }

        public void OnExit()
        {
        }
    }



    public class DamagedState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;
        private float timer;
        float maxXVelocity = 4f;

        public DamagedState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            timer = 0.0f;
            // 播放倒地/被击倒动画（请确认 Animator 中有此 clip）
            parameter.animator.Play("Damaged");
            parameter.audioSource.clip = parameter.damagedAudio;
            parameter.audioSource.loop = false;
            parameter.audioSource.Play();




            // 先清掉残余速度，避免叠加
            parameter.rb.velocity = Vector2.zero;

            // 计算击退方向
            Vector2 knockDir = parameter.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // 加一个向后的冲击 + 向上的力
            Vector2 impulse = knockDir * 3f + Vector2.up * 10f; // 参数可调
            parameter.rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        public void OnUpdate()
        {

            parameter.rb.velocity = new Vector2(parameter.rb.velocity.x,
                Mathf.Clamp(parameter.rb.velocity.y, -maxXVelocity, maxXVelocity));
            timer += Time.deltaTime;
            if (timer >= 0.5f)
            {
                manager.ChangeState<IdleState>();
            }
        }

        public void OnExit()
        {
            // 清理（如果需要）
        }
    }

    public class SlashState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;
        private int slashCount;

        public SlashState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        // Start is called before the first frame update
        public void OnEnter()
        {
            if(slashCount % 2 == 0)
                parameter.animator.Play("Slash", 0, 0.0f);
            else
                parameter.animator.Play("SlashAlt", 0, 0.0f);
            slashCount = (slashCount + 1) % 2;
            parameter.slashHitboxColl.enabled = true;
            parameter.audioSource.clip = parameter.slashAudio;
            parameter.audioSource.loop = false;
            parameter.audioSource.Play();
        }

        // Update is called once per frame
        public void OnUpdate()
        {
            if (parameter.playerHitBox.isTriggered)
            {
                float dir = parameter.transform.localScale.x < 0 ? 1f : -1f;
                parameter.rb.velocity = new Vector2(- dir * 1.0f, parameter.rb.velocity.y);
            }else
            {
                parameter.rb.velocity = new Vector2(parameter.dirX * parameter.moveSpeed, parameter.rb.velocity.y);
            }
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime >= .95f)
            {
                manager.ChangeState<IdleState>();
            }
        }

        public void OnExit()
        {
            if (parameter.playerHitBox.isEnemy && parameter.soul < 90.0f)
            {
                parameter.soul += 5.0f;
                parameter.soulUI.Fill(parameter.soul / 90.0f);
                parameter.playerHitBox.isEnemy = false;
            }
            parameter.slashHitboxColl.enabled = false;
            parameter.playerHitBox.isTriggered = false;
        }
    }

    public class UpSlashState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;

        public UpSlashState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("UpSlash", 0, 0.0f);
            parameter.upSlashHitboxColl.enabled = true;
            parameter.audioSource.clip = parameter.slashAudio;
            parameter.audioSource.loop = false;
            parameter.audioSource.Play();
        }

        public void OnUpdate()
        {
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime >= .95f)
            {
                manager.ChangeState<IdleState>();
            }
        }

        public void OnExit()
        {
            if (parameter.playerHitBox.isEnemy && parameter.soul < 90.0f)
            {
                parameter.soul += 5.0f;
                parameter.soulUI.Fill(parameter.soul / 90.0f);
                parameter.playerHitBox.isEnemy = false;
            }
            parameter.upSlashHitboxColl.enabled = false;
        }
    }

    public class DownSlashState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;

        public DownSlashState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("DownSlash", 0, 0.0f);
            parameter.downSlashHitboxColl.enabled = true;
            parameter.audioSource.clip = parameter.slashAudio;
            parameter.audioSource.loop = false;
            parameter.audioSource.Play();
        }

        public void OnUpdate()
        {
            if (parameter.playerDownHitBox.isTriggered)
            {
                parameter.rb.velocity = new Vector2(parameter.rb.velocity.x, 6f); // 强制给一个往上的速度
                parameter.playerDownHitBox.isTriggered = false;
            }
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime >= .95f)
            {
                manager.ChangeState<IdleState>();
            }
        }

        public void OnExit()
        {
            if (parameter.playerHitBox.isEnemy && parameter.soul < 90.0f)
            {
                parameter.soul += 5.0f;
                parameter.soulUI.Fill(parameter.soul / 90.0f);
                parameter.playerHitBox.isEnemy = false;
            }
            parameter.downSlashHitboxColl.enabled = false;
        }
    }

    public class DeathState : IState
    {
        private FSM manager;
        private Parameter parameter;
        private AnimatorStateInfo animInfo;

        public DeathState(FSM manager, Parameter parameter)
        {
            this.manager = manager;
            this.parameter = parameter;
        }

        public void OnEnter()
        {
            parameter.animator.Play("Death");
            parameter.audioSource.clip = parameter.damagedAudio;
            parameter.audioSource.loop = false;
            parameter.audioSource.Play(); 
        }

        public void OnUpdate()
        {
            parameter.rb.velocity = new Vector2(0.0f, 0.0f);
            animInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime >= .01f)
            {
                Time.timeScale = 1.0f;
                parameter.animator.speed = 1.0f;
            }else
            {
                Time.timeScale = 0.2f;
                parameter.animator.speed = 0.2f;
            }
            if (animInfo.normalizedTime >= .95f)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void OnExit()
        {
        }
    }
}