using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class Collectable:MonoBehaviour
    {
        [Header("General Settings")]
        public bool collectOnContact = true; //标记
        public int times = 1;
        public float ghostingDuration = 0.5f;
        public GameObject display;//显示的物体
        public AudioClip clip;
        public ParticleSystem particle; //特效
        
        [Header("Visibility Settings")] //关于隐藏的设置
        public bool hidden; //是否隐藏
        public float quickShowHeight = 2f;//显示高度
        public float quickShowDuration = 0.25f;//显示是擦汗给你
        public float hideDuration = 0.5f;
        
        [Header("Life Time")]
        public bool hasLifeTime;
        public float lifeTimeDuration = 5f;//阈值
        
        [Header("Physics Settings")]
        public bool usePhysics;//受到重力
        public float minForceToStopPhysics = 3f;
        public float collisionRadius = 0.5f;
        public float gravity = 15f;
        public float bounciness = 0.98f;//反弹的一个系数
        public float maxBounceYVelocity = 10f;
        public bool randomizeInitialDirection = true;//一个随机
        public Vector3 initialVelocity = new Vector3(0, 12, 0);//被撞击之后向上速率
        public AudioClip collisionClip;

        [Space(15)]

        /// <summary>
        /// Called when it has been collected.
        /// 收集后调用
        /// </summary>
        public PlayerEvent onCollect;

        protected Collider m_collider;
        protected AudioSource m_audio;//顶砖块的声音

        protected bool m_vanished;//消失
        protected bool m_ghosting = true; //幻影
        protected float m_elapsedLifeTime;//流逝的生命时间
        protected float m_elapsedGhostingTime;//已经过去的时间
        protected Vector3 m_velocity; //速度
        
        
        protected const int k_verticalMinRotation = 0;//旋转
        protected const int k_verticalMaxRotation = 30;
        protected const int k_horizontalMinRotation = 0;//水平
        protected const int k_horizontalMaxRotation = 360;
        //初始化音效
        protected virtual void InitializeAudio()
        {
            if (!TryGetComponent(out m_audio))
            {
                m_audio = gameObject.AddComponent<AudioSource>();
            }
        }
        //初始化collider
        protected virtual void InitializeCollider()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
        }
        protected virtual void InitializeDisplay()
        {
            //物体是否隐藏
            display.SetActive(!hidden);
        }
        protected virtual void InitializeTransform()
        {
            transform.parent = null; //
            transform.rotation = Quaternion.identity;
        }
        protected virtual void InitializeVelocity()
        {
            var direction = initialVelocity.normalized;
            var force = initialVelocity.magnitude;
            
            //给金币一个随机速度  z  和  y  上
            if (randomizeInitialDirection)
            {
                var randomZ = UnityEngine.Random.Range(k_verticalMinRotation, k_verticalMaxRotation);
                var randomY = UnityEngine.Random.Range(k_horizontalMinRotation, k_horizontalMaxRotation);
                direction = Quaternion.Euler(0, 0, randomZ) * direction;
                direction = Quaternion.Euler(0, randomY, 0) * direction;
            }

            m_velocity = direction * force;
        }
        protected virtual void Awake()
        {
            InitializeAudio();
            InitializeCollider();
            InitializeTransform();
            InitializeDisplay();
            InitializeVelocity();
        }
        //消失
        public virtual void Vanish()
        {
            if (!m_vanished)
            {
                m_vanished = true;
                m_elapsedLifeTime = 0;
                display.SetActive(false);
                m_collider.enabled = false;
            }
        }
        //处理吟诗
        protected virtual void HandleGhosting()
        {
            if (m_ghosting)
            {
                m_elapsedGhostingTime += Time.deltaTime;

                if (m_elapsedGhostingTime >= ghostingDuration)
                {
                    m_elapsedGhostingTime = 0;
                    m_ghosting = false;
                }
            }
        }
        //处理时间
        protected virtual void HandleLifeTime()
        {
            if (hasLifeTime)
            {
                m_elapsedLifeTime += Time.deltaTime;

                if (m_elapsedLifeTime >= lifeTimeDuration)
                {
                    Vanish();
                }
            }
        }
        protected virtual void HandleMovement()
        {
            m_velocity.y -= gravity * Time.deltaTime;
        }
        //处理收集
        protected virtual void HandleSweep()
        {
            var direction = m_velocity.normalized;
            var magnitude = m_velocity.magnitude;
            var distance = magnitude * Time.deltaTime;

            if (Physics.SphereCast(transform.position, collisionRadius, direction,
                    out var hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.CompareTag(GameTags.Player))//不是玩家碰撞到的
                {
                    var bounceDirection = Vector3.Reflect(direction, hit.normal);//反弹
                    m_velocity = bounceDirection * magnitude * bounciness;
                    m_velocity.y = Mathf.Min(m_velocity.y, maxBounceYVelocity);
                    m_audio.Stop();
                    m_audio.PlayOneShot(collisionClip);

                    if (m_velocity.y <= minForceToStopPhysics)
                        usePhysics = false;
                }
            }

            transform.position += m_velocity * Time.deltaTime;//更新轨迹
        }
        protected virtual void Update()
        {
            //没有消失
            if (!m_vanished)
            {
                HandleGhosting();//处理消失(幻影)
                HandleLifeTime();//处理剩余时间
                
                //是否使用物理移动
                if (usePhysics)
                {
                    HandleMovement();
                    HandleSweep();//曲线滑动
                }
            }
        }
        /// <summary>
        /// The collection routine which is trigger the callbacks and activate the reactions.
        /// 收集金币后的回调 （播放音效、和事件触发等）
        /// </summary>
        /// <param name="player">The Player which collected.</param>
        protected virtual IEnumerator CollectRoutine(Player player)
        {
            for (int i = 0; i < times; i++)
            {
                m_audio.Stop();
                m_audio.PlayOneShot(clip);
                onCollect.Invoke(player);
                yield return new WaitForSeconds(0.1f);
            }
        }
        //展示金币
        protected virtual IEnumerator QuickShowRoutine()
        {
            var elapsedTime = 0f;
            var initialPosition = transform.position;
            var targetPosition = initialPosition + Vector3.up * quickShowHeight;

            display.SetActive(true);
            m_collider.enabled = false;

            while (elapsedTime < quickShowDuration)
            {
                var t = elapsedTime / quickShowDuration;
                transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
            yield return new WaitForSeconds(hideDuration);
            transform.position = initialPosition;
            Vanish();
        }
        /// <summary>
        /// Triggers the collection of this Collectable.
        /// 收集金币
        /// </summary>
        /// <param name="player">The Player which collected.</param>
        public virtual void Collect(Player player)
        {
            // 没消失
            if (!m_vanished && !m_ghosting)
            {
                if (!hidden)
                {
                    Vanish();

                    if (particle != null)
                    {
                        particle.Play();
                    }
                }
                else
                {
                    StartCoroutine(QuickShowRoutine());
                }

                StartCoroutine(CollectRoutine(player));
            }
        }
        //处理碰撞
        protected virtual void OnTriggerStay(Collider other)
        {
            if (collectOnContact && other.CompareTag(GameTags.Player))
            {
                if (other.TryGetComponent<Player>(out var player))
                {
                    Collect(player);
                }
            }
        }
		
        protected virtual void OnDrawGizmos()
        {
            if (usePhysics)
            {
                Gizmos.color = Color.green; //绘制一些东西
                Gizmos.DrawWireSphere(transform.position, collisionRadius);
            }
        }
    }
}