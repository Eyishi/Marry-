using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Pickable")]
	public class Pickable : MonoBehaviour, IEntityContact
	{
		[Header("General Settings")]
		public Vector3 offset;
		public float releaseOffset = 0.5f;

		[Header("Respawn Settings")]
		public bool autoRespawn;//自动重生
		public bool respawnOnHitHazards;//重生是否打击这个伤害物
		public float respawnHeightLimit = -100;//重生的高度线

		[Header("Attack Settings")]
		public bool attackEnemies = true;//是不是攻击怪物
		public int damage = 1;//伤害
		public float minDamageSpeed = 5f;//最小攻速

		[Space(15)]

		/// <summary>
		///
		/// 被捡起时
		/// </summary>
		public UnityEvent onPicked;

		/// <summary>
		/// Called when this object is Released.
		/// </summary>
		public UnityEvent onReleased;

		/// <summary>
		/// Called when this object is respawned.
		/// 重置对象
		/// </summary>
		public UnityEvent onRespawn;

		protected Collider m_collider;
		protected Rigidbody m_rigidBody;

		//原始
		protected Vector3 m_initialPosition;
		protected Quaternion m_initialRotation;
		protected Transform m_initialParent;

		protected RigidbodyInterpolation m_interpolation;

		public bool beingHold { get; protected set; } //被捡起

		//被捡起
		public virtual void PickUp(Transform slot)
		{
			if (!beingHold)
			{
				//设置它的位置
				beingHold = true;
				transform.parent = slot;
				transform.localPosition = Vector3.zero + offset;
				transform.localRotation = Quaternion.identity;
				m_rigidBody.isKinematic = true;
				m_collider.isTrigger = true;
				m_interpolation = m_rigidBody.interpolation;
				m_rigidBody.interpolation = RigidbodyInterpolation.None;
				onPicked?.Invoke();
			}
		}
		/// <summary>
		/// 丢掉
		/// </summary>
		/// <param name="direction">方向</param>
		/// <param name="force">力</param>
		public virtual void Release(Vector3 direction, float force)
		{
			if (beingHold) 
			{
				transform.parent = null;
				transform.position += direction * releaseOffset;
				m_collider.isTrigger = m_rigidBody.isKinematic = beingHold = false;
				m_rigidBody.interpolation = m_interpolation;
				m_rigidBody.velocity = direction * force;
				onReleased?.Invoke();
			}
		}
		//重生  设置位置
		public virtual void Respawn()
		{
			m_rigidBody.velocity = Vector3.zero;
			transform.parent = m_initialParent;
			transform.SetPositionAndRotation(m_initialPosition, m_initialRotation);
			m_rigidBody.isKinematic = m_collider.isTrigger = beingHold = false;
			onRespawn?.Invoke();
		}

		public void OnEntityContact(Entity entity) //打破箱子后应该，尝试伤害一下，因为箱子里可能是炸药，奖品
		{
			if (attackEnemies && entity is Enemy &&
				m_rigidBody.velocity.magnitude > minDamageSpeed)
			{
				entity.ApplyDamage(damage, transform.position);
			}
		}

		protected virtual void EvaluateHazardRespawn(Collider other)
		{
			if (autoRespawn && respawnOnHitHazards && other.CompareTag(GameTags.Hazard))
			{
				Respawn();
			}
		}

		protected virtual void Start()
		{
			m_collider = GetComponent<Collider>();
			m_rigidBody = GetComponent<Rigidbody>();
			m_initialPosition = transform.localPosition;
			m_initialRotation = transform.localRotation;
			m_initialParent = transform.parent;
		}

		protected virtual void Update()
		{
			if (autoRespawn && transform.position.y <= respawnHeightLimit)
			{
				Respawn();
			}
		}

		protected virtual void OnTriggerEnter(Collider other) =>
			EvaluateHazardRespawn(other);

		protected virtual void OnCollisionEnter(Collision collision) =>
			EvaluateHazardRespawn(collision.collider);
	}
}