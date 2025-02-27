using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Veco
{
    public enum MonsterState
    {
        idle,
        run,
        attack,
        die,
        stun,
    }

    [Serializable]
    public class MonsterStatus
    {
        MonsterStateMono owner = null;
        [SerializeField] int hp;
        public int damage;
        [SerializeField] public int maxHp;

        public int Hp
        {
            get => hp;
            set
            {
                hp = value;
                if(hp > maxHp) hp = maxHp;
                if(hp <= 0)
                {
                    hp = 0;
                    owner.Dead();
                }
            }
        }

        public MonsterStatus(MonsterStatusSO so, object owner)
        {
            int waveNum;
            if (WaveManager.Instance != null)
                waveNum = WaveManager.Instance.WaveNumber;
            else
                waveNum = 1;

            this.owner = (MonsterStateMono)owner;
            this.maxHp = (int)(so.defaultMaxHp * waveNum);
            this.hp = this.maxHp;
            this.damage = (int)(so.defaultAttackDamage * waveNum);
        }
    }
    public class MonsterAI : MonsterStateMono, IHitable, IPoolingalbe
    {
        [SerializeField] MonsterStatusSO so;
        [SerializeField] bool isAttackCooltime;
        IEnumerator attackCo;

        [Header("Collider")]
        [SerializeField] DetectiveComponent detective;
        public Collider2D attackCol;
        [SerializeField] Collider2D col;

        [Header("Hit UI")]
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] GameObject damageText;
        [SerializeField] Transform hitPos;
        [SerializeField] Image hpimg;
        [SerializeField] TextMeshProUGUI hpText;
        void OnEnable()
        {
            MonsterInit();
        }

        protected override void Awake()
        {
            base.Awake();
            spriteRenderer = GetComponent<SpriteRenderer>();
            detective = GetComponent<DetectiveComponent>();
            col = GetComponent<Collider2D>();            
            status = new MonsterStatus(so, this);
        }
        protected override void Start()
        {
            base.Start();
            detective.DetectiveRange = so.attackRange;
            isAttackCooltime = false;
            isDie = false;
            attackCo = MonsterAttackCo(so.attackSpeed);

            sm.AddState(MonsterState.idle.ToString(), new MonsterIdleState());
            sm.AddState(MonsterState.run.ToString(), new MonsterRunState());
            sm.AddState(MonsterState.attack.ToString(), new MonsterAttackState());
            sm.AddState(MonsterState.die.ToString(), new MonsterDieState());

            sm.GetState(MonsterState.attack.ToString()).onEnter += () => { attackCol.enabled = true; };
            sm.GetState(MonsterState.attack.ToString()).onEnter += () => { SoundManager.Instance.SFXPlay("testSFX"); };
            sm.GetState(MonsterState.attack.ToString()).onExit += () => { attackCol.enabled = false; };

            state = MonsterState.idle;
            sm.SetState(state.ToString());
        }

        void Update()
        {
            sm.Update();

            if (isDie) return;

            if (detective.IsFind && !isAttackCooltime)
            {
                StartCoroutine(attackCo);
            }
            else if (detective.IsFind && isAttackCooltime)
            {
                //ChangeMonsterState(MonsterState.idle);
            }
            else
            {
                ChangeMonsterState(MonsterState.run);
                transform.Translate(Vector3.left * so.moveSpeed);
            }
            hpText.text = status.Hp.ToString();
            MonsterUI();
        }

        //몬스터 status 초기화
        protected override void MonsterInit()
        {
            if (sm != null)
            {
                base.MonsterInit();
                sm.SetState(state.ToString());
                spriteRenderer.material.color = Color.white;
                hpimg.fillAmount = 1f;
                isAttackCooltime = false;
                col.enabled = true;
            }
            status = new MonsterStatus(so, this);
        }


        public override void Dead()
        {
            isDie = true;
            col.enabled = false;
            attackCol.enabled = false;
            WaveManager.Instance.WaveMonsterCount++;
            DataManager.Instance.PlayerItem.gold += 1000;

            StopCoroutine(attackCo);
            ChangeMonsterState(MonsterState.die);
        }

        public void AnimationChangeMonsterState(int selectNumber)
        {
            ChangeMonsterState((MonsterState)selectNumber);
        }

        public void GameObjectDead()
        {
            ReturnPool();
        }


        IEnumerator MonsterAttackCo(float attackCooltime)
        {
            while (!isAttackCooltime)
            {
                ChangeMonsterState(MonsterState.attack);
                isAttackCooltime = true;
                yield return new WaitForSeconds(attackCooltime);
                isAttackCooltime = false;
            }
        }

        public void MonsterUI()
        {          
            hpimg.fillAmount = Mathf.Lerp(hpimg.fillAmount, status.Hp / status.maxHp, Time.deltaTime * 1f);
        }

        public void ReturnPool()
        {
            ObjectPoolManager.Instance.ReturnPool(gameObject);
        }

        public void Hit(int damage)
        {
            damageText.GetComponent<DamageText>().damage = damage;
            damageText.SetActive(true);           
            damageText.transform.position = hitPos.position;
            status.Hp -= damage;
            StartCoroutine(HitCo());            
        }

        IEnumerator HitCo()
        {
            spriteRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);

            if (status.Hp > 0)
            {
                spriteRenderer.material.color = Color.white;
            }
        }
    }
}
