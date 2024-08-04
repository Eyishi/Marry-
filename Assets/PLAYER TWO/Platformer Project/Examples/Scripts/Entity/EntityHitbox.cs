using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity Hitbox")]
	public class EntityHitbox : MonoBehaviour
	{
		[Header("Attack Settings")]
		public bool breakObjects;//可以打坏
		public int damage = 1;

		[Header("Rebound Settings")]
		public bool rebound; //回弹
		public float reboundMinForce = 10f;
		public float reboundMaxForce = 25f;

		[Header("Push Back Settings")]
		public bool pushBack;
		public float pushBackMinMagnitude = 5f;
		public float pushBackMaxMagnitude = 10f;

		protected Entity m_entity;
		protected Collider m_collider;
		
		protected virtual void InitializeEntity()
		{
			if (!m_entity)
			{
				m_entity = GetComponentInParent<Entity>();
			}
		}

		protected virtual void InitializeCollider()
		{
			m_collider = GetComponent<Collider>();
			m_collider.isTrigger = true;
		}

		protected virtual void HandleCollision(Collider other)
		{
			if (other != m_entity.controller)
			{
				//是实体
				if (other.TryGetComponent(out Entity target))
				{
					HandleEntityAttack(target);
					HandleRebound();
					HandlePushBack();
				}
				//其他的物体
				else if (other.TryGetComponent(out Breakable breakable))
				{
					HandleBreakableObject(breakable);
				}
			}
		}
		//对目标攻击
		protected virtual void HandleEntityAttack(Entity other)
		{
			other.ApplyDamage(damage, transform.position);
		}

		//摧毁物体
		protected virtual void HandleBreakableObject(Breakable breakable)
		{
			if (breakObjects)
			{
				breakable.Break();
			}
		}

		//回弹
		protected virtual void HandleRebound()
		{
			if (rebound)
			{
				var force = -m_entity.velocity.y;
				force = Mathf.Clamp(force, reboundMinForce, reboundMaxForce);
				m_entity.verticalVelocity = Vector3.up * force;
			}
		}
		//反向
		protected virtual void HandlePushBack()
		{
			if (pushBack)
			{
				var force = m_entity.lateralVelocity.magnitude;
				force = Mathf.Clamp(force, pushBackMinMagnitude, pushBackMaxMagnitude);
				m_entity.lateralVelocity = -transform.forward * force;
			}
		}

		protected virtual void HandleCustomCollision(Collider other) { }

		protected virtual void Start()
		{
			InitializeEntity();
			InitializeCollider();
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			HandleCollision(other);
			HandleCustomCollision(other);
		}
	}
}
