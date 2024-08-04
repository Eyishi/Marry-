using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Health")]
	public class Health : MonoBehaviour
	{
		public int initial = 3; //暴露到外面初始化
		public int max = 3;
		public float coolDown = 1f; //冷却时间

		/// <summary>
		/// 血量发生变化
		/// </summary>
		public UnityEvent onChange;

		/// <summary>
		/// Called when it receives damage.
		/// 收到损坏时调用
		/// </summary>
		public UnityEvent onDamage;

		protected int m_currentHealth;
		protected float m_lastDamageTime;

		/// <summary>
		/// Returns the current amount of health.
		/// 当前的血量
		/// </summary>
		public int current
		{
			get { return m_currentHealth; }

			protected set
			{
				var last = m_currentHealth;

				if (value != last)
				{
					m_currentHealth = Mathf.Clamp(value, 0, max);
					onChange?.Invoke();
				}
			}
		}

		/// <summary>
		/// Returns true if the Health is empty.
		/// 玩家的健康值是否为空
		/// </summary>
		public virtual bool isEmpty => current == 0;

		/// <summary>
		/// Returns true if it's still recovering from the last damage.
		/// 是不是在恢复中，相当于一个无敌时间
		/// </summary>
		public virtual bool recovering => Time.time < m_lastDamageTime + coolDown;

		/// <summary>
		/// Sets the current health to a given amount.
		/// </summary>
		/// <param name="amount">The total health you want to set.</param>
		public virtual void Set(int amount) => current = amount;

		/// <summary>
		/// Increases the amount of health.
		/// 增加血量
		/// </summary>
		/// <param name="amount">The amount you want to increase.</param>
		public virtual void Increase(int amount) => current += amount;

		/// <summary>
		/// 受到攻击扣血
		/// </summary>
		/// <param name="amount">The amount you want to decrease.</param>
		public virtual void Damage(int amount)
		{
			if (!recovering)
			{
				current -= Mathf.Abs(amount);
				m_lastDamageTime = Time.time;
				onDamage?.Invoke();
			}
		}

		/// <summary>
		/// Set the current health back to its initial value.
		/// 
		/// </summary>
		public virtual void Reset() => current = initial;

		protected virtual void Awake() => current = initial;
	}
}