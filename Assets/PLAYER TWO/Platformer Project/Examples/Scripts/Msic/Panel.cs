using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Panel")]
	public class Panel : MonoBehaviour, IEntityContact
	{
		public bool autoToggle; //开关，对外
		public bool requireStomp;
		public bool requirePlayer;//是不是只有player能碰
		public AudioClip activateClip;
		public AudioClip deactivateClip;

		/// <summary>
		/// 启动时
		/// </summary>
		public UnityEvent OnActivate;

		/// <summary>
		/// Called when the Panel is deactivated.
		/// 停用时
		/// </summary>
		public UnityEvent OnDeactivate;

		protected Collider m_collider;
		protected Collider m_entityActivator; //实体的碰撞器
		protected Collider m_otherActivator;//其他的碰撞器

		protected AudioSource m_audio;

		/// <summary>
		/// 转盘是否被激活
		/// </summary>
		public bool activated { get; protected set; }

		/// <summary>
		/// Activate this Panel.
		/// 激活转盘
		/// </summary>
		public virtual void Activate()
		{
			if (!activated)
			{
				if (activateClip) //播放音乐
				{
					m_audio.PlayOneShot(activateClip);
				}

				activated = true;
				OnActivate?.Invoke();
			}
		}

		/// <summary>
		/// Deactivates this Panel.
		/// 停用这个转盘
		/// </summary>
		public virtual void Deactivate()
		{
			if (activated)
			{
				if (deactivateClip)
				{
					m_audio.PlayOneShot(deactivateClip);
				}

				activated = false;
				OnDeactivate?.Invoke();
			}
		}

		protected virtual void Start()
		{
			gameObject.tag = GameTags.Panel;
			m_collider = GetComponent<Collider>();
			m_audio = GetComponent<AudioSource>();
		}

		protected virtual void Update()
		{
			if (m_entityActivator || m_otherActivator)
			{
				var center = m_collider.bounds.center;
				var contactOffset = Physics.defaultContactOffset + 0.1f;//偏移
				var size = m_collider.bounds.size + Vector3.up * contactOffset;
				var bounds = new Bounds(center, size); //包围盒
				
				//跟实体 和 其他 有没有交集(接触)
				var intersectsEntity = m_entityActivator && bounds.Intersects(m_entityActivator.bounds);
				var intersectsOther = m_otherActivator && bounds.Intersects(m_otherActivator.bounds);

				if (intersectsEntity || intersectsOther)
				{
					//激活自己
					Activate();
				}
				else
				{
					m_entityActivator = intersectsEntity ? m_entityActivator : null;
					m_otherActivator = intersectsOther ? m_otherActivator : null;

					if (autoToggle)
					{
						Deactivate();
					}
				}
			}
		}

		public void OnEntityContact(Entity entity)
		{
			if (entity.velocity.y <= 0 && entity.IsPointUnderStep(m_collider.bounds.max))
			{
				if ((!requirePlayer || entity is Player) &&
					(!requireStomp || (entity as Player).states.IsCurrentOfType(typeof(StompPlayerState))))
				{
					m_entityActivator = entity.controller;
				}
			}
		}

		// 一个刚体接触另一个刚体时，每帧调用一次
		protected virtual void OnCollisionStay(Collision collision)
		{
			if (!(requirePlayer || requireStomp) && !collision.collider.CompareTag(GameTags.Player))
			{
				m_otherActivator = collision.collider;
			}
		}
	}
}