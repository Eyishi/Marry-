using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(EnemyStatsManager))]
	[RequireComponent(typeof(EnemyStateManager))]
	[RequireComponent(typeof(WaypointManager))]
	[RequireComponent(typeof(Health))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy")]
	public class Enemy : Entity<Enemy>
	{
		public EnemyEvents enemyEvents;

		protected Player m_player;

		protected Collider[] m_sightOverlaps = new Collider[1024]; //怪物周围碰到的
		protected Collider[] m_contactAttackOverlaps = new Collider[1024]; //接触后攻击的

		/// <summary>
		/// Returns the Enemy Stats Manager instance.
		/// enemy 的stats管理的实例
		/// </summary>
		public EnemyStatsManager stats { get; protected set; }

		/// <summary>
		/// Returns the Waypoint Manager instance.
		/// </summary>
		public WaypointManager waypoints { get; protected set; }

		/// <summary>
		/// 健康值
		/// </summary>
		public Health health { get; protected set; }

		/// <summary>
		/// Returns the instance of the Player on the Enemies sight.
		/// 返回玩家在敌人视野中的实例。
		/// </summary>
		public Player player { get; protected set; }

		protected virtual void InitializeStatsManager() => stats = GetComponent<EnemyStatsManager>();
		protected virtual void InitializeWaypointsManager() => waypoints = GetComponent<WaypointManager>();
		protected virtual void InitializeHealth() => health = GetComponent<Health>();
		protected virtual void InitializeTag() => tag = GameTags.Enemy;

		/// <summary>
		/// Applies damage to this Enemy decreasing its health with proper reaction.
		/// 对该敌人施加伤害，通过适当的反应降低其生命值。
		/// </summary>
		/// <param name="amount">The amount of health you want to decrease.</param>
		public override void ApplyDamage(int amount, Vector3 origin)
		{
			if (!health.isEmpty && !health.recovering)
			{
				health.Damage(amount);
				enemyEvents.OnDamage?.Invoke();

				if (health.isEmpty) //死亡
				{
					controller.enabled = false;
					enemyEvents.OnDie?.Invoke();
				}
			}
		}

		/// <summary>
		/// Revives this enemy, restoring its health and reenabling its movements.
		/// </summary>
		public virtual void Revive()
		{
			if (!health.isEmpty) return;

			health.Reset();
			controller.enabled = true;
			enemyEvents.OnRevive.Invoke();
		}
		//加速
		public virtual void Accelerate(Vector3 direction, float acceleration, float topSpeed) =>
			Accelerate(direction, stats.current.turningDrag, acceleration, topSpeed);

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its deceleration stats.
		/// 减速
		/// </summary>
		public virtual void Decelerate() => Decelerate(stats.current.deceleration);

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its friction stats.
		/// 摩檫力减速
		/// </summary>
		public virtual void Friction() => Decelerate(stats.current.friction);

		/// <summary>
		/// Applies a downward force by its gravity stats.
		/// 应用重力
		/// </summary>
		public virtual void Gravity() => Gravity(stats.current.gravity);

		/// <summary>
		/// Applies a downward force when ground by its snap stats.
		/// 应用向下的力
		/// </summary>
		public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

		/// <summary>
		/// Rotate the Enemy forward to a given direction.
		/// 旋转敌人到指定的方向
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);

		//接触后攻击
		public virtual void ContactAttack()
		{
			if (stats.current.canAttackOnContact)
			{
				var overlaps = OverlapEntity(m_contactAttackOverlaps, stats.current.contactOffset);

				for (int i = 0; i < overlaps; i++)
				{
					if (m_contactAttackOverlaps[i].CompareTag(GameTags.Player) &&
						m_contactAttackOverlaps[i].TryGetComponent<Player>(out var player))
					{
						//踩的地方
						var stepping = controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;

						if (!player.IsPointUnderStep(stepping))//如果玩家没有踩在怪物
						{
							if (stats.current.contactPushback)
							{
								lateralVelocity = -transform.forward * stats.current.contactPushBackForce;
							}

							player.ApplyDamage(stats.current.contactDamage, transform.position);
							enemyEvents.OnPlayerContact?.Invoke();
						}
					}
				}
			}
		}

		/// <summary>
		/// 处理周围视图，和检测玩家的行为
		/// </summary>
		protected virtual void HandleSight()
		{
			if (!player)
			{
				//物理检测周围的
				var overlaps = Physics.OverlapSphereNonAlloc(position, stats.current.spotRange, m_sightOverlaps);

				for (int i = 0; i < overlaps; i++)
				{
					if (m_sightOverlaps[i].CompareTag(GameTags.Player))
					{
						if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
						{
							this.player = player;
							enemyEvents.OnPlayerSpotted?.Invoke();
							return;
						}
					}
				}
			}
			else
			{
				var distance = Vector3.Distance(position, player.position);
				//玩家在检测范围外
				if ((player.health.current == 0) || (distance > stats.current.viewRange))
				{
					player = null;
					enemyEvents.OnPlayerScaped?.Invoke();
				}
			}
		}
		//不断检测有没有敌人
		protected override void OnUpdate()
		{
			HandleSight();
			ContactAttack();
		}

		protected override void Awake()
		{
			base.Awake();
			InitializeTag();
			InitializeStatsManager();
			InitializeWaypointsManager();
			InitializeHealth();
		}
	}
}
