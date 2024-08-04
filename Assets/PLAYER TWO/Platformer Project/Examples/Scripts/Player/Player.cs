using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(PlayerInputManager))]
    [RequireComponent(typeof(PlayerStatsManager))]
    [RequireComponent(typeof(PlayerStateManager))]
        
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player")]
    public class Player : Entity<Player>
    {
        
        public PlayerEvents playerEvents;

        public Transform pickableSlot;
        public Transform skin; // 攀爬的开关皮肤

        protected Vector3 m_respawnPosition; //重新生成的点位
        protected Quaternion m_respawnRotation;

        protected Vector3 m_skinInitialPosition;
        protected Quaternion m_skinInitialRotation;

        /// <summary>
        /// 返回玩家输入管理器实例。
        /// </summary>
        public PlayerInputManager inputs { get; protected set; }

        /// <summary>
        /// 玩家的状态管理
        /// </summary>
        public PlayerStatsManager stats { get; protected set; }
        /// <summary>
		/// 血量
		/// </summary>
		public Health health { get; protected set; }

		/// <summary>
		/// 水下
		/// </summary>
		public bool onWater { get; protected set; }

		/// <summary>
		/// 玩家是否挂在一个物体上
		/// </summary>
		public bool holding { get; protected set; }

		/// <summary>
		/// 玩家的跳跃次数
		/// </summary>
		public int jumpCounter { get; protected set; }

		/// <summary>
		/// 空中旋转的次数
		/// </summary>
		public int airSpinCounter { get; protected set; }

		/// <summary>
		/// 返回空中冲刺的次数
		/// </summary>
		/// <value></value>
		public int airDashCounter { get; protected set; }

		/// <summary>
		/// 上一次冲刺的时间
		/// </summary>
		/// <value></value>
		public float lastDashTime { get; protected set; }
		

		/// <summary>
		/// 当前爬取的杆子
		/// </summary>
		public Pole pole { get; protected set; }

		/// <summary>
		/// 水里面的碰撞器
		/// </summary>
		public Collider water { get; protected set; }

		/// <summary>
		/// 当前拾取的物体
		/// </summary>
		public Pickable pickable { get; protected set; }

		/// <summary>
		/// Returns true if the Player health is not empty.
		/// </summary>
		public virtual bool isAlive => !health.isEmpty;

		/// <summary>
		/// 能否站起来
		/// </summary>
		public virtual bool canStandUp => !SphereCast(Vector3.up, originalHeight);

		protected const float k_waterExitOffset = 0.25f;

		protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();
		protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();
		protected virtual void InitializeHealth() => health = GetComponent<Health>();
		
		protected virtual void InitializeTag() => tag = GameTags.Player;

		protected virtual void InitializeRespawn()
		{
			m_respawnPosition = transform.position;
			m_respawnRotation = transform.rotation;
		}

		protected virtual void InitializeSkin()
		{
			if (skin)
			{
				m_skinInitialPosition = skin.localPosition;
				m_skinInitialRotation = skin.localRotation;
			}
		}

		/// <summary>
		/// 重新生成玩家
		/// </summary>
		public virtual void Respawn()
		{
			health.Reset();
			transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation);
			states.Change<IdlePlayerState>();
		}

		/// <summary>
		/// 设置重生位置
		/// </summary>
		public virtual void SetRespawn(Vector3 position, Quaternion rotation)
		{
			m_respawnPosition = position;
			m_respawnRotation = rotation;
		}

		/// <summary>
		/// 尝试对玩家造成伤害
		/// </summary>
		/// <param name="amount">The amount of health you want to decrease.</param>
		public override void ApplyDamage(int amount, Vector3 origin)
		{
			if (!health.isEmpty && !health.recovering)
			{
				health.Damage(amount);
				//受伤的方向
				var damageDir = origin - transform.position;
				damageDir.y = 0;
				damageDir = damageDir.normalized;
				FaceDirection(damageDir);
				lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;
				//不在水里  垂直速度改变
				if (!onWater)
				{
					verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
					states.Change<HurtPlayerState>();
				}
		
				playerEvents.OnHurt?.Invoke();
				
				//角色死亡
				if (health.isEmpty)
				{
					Throw();
					playerEvents.OnDie?.Invoke();
				}
			}
		}

		/// <summary>
		/// Kills the Player.
		/// </summary>
		// public virtual void Die()
		// {
		// 	health.Set(0);
		// 	playerEvents.OnDie?.Invoke();
		// }

		/// <summary>
		/// 进入游泳状态
		/// </summary>
		/// <param name="water">The instance of the water collider.</param>
		public virtual void EnterWater(Collider water)
		{
			if (!onWater && !health.isEmpty)
			{
				Throw();
				onWater = true;
				this.water = water;
				states.Change<SwimPlayerState>();
			}
		}

		/// <summary>
		/// 退出游泳
		/// </summary>
		public virtual void ExitWater()
		{
			if (onWater)
			{
				onWater = false;
			}
		}

		/// <summary>
		/// 指定地方 跳跃
		/// </summary>
		/// <param name="direction">The direction that you want to jump.</param>
		/// <param name="height">The upward force that you want to apply.</param>
		/// <param name="distance">The force towards the direction that you want to apply.</param>
		public virtual void DirectionalJump(Vector3 direction, float height, float distance)
		{
			jumpCounter++;
			verticalVelocity = Vector3.up * height;
			lateralVelocity = direction * distance;
			playerEvents.OnJump?.Invoke();
		}
		
		
		/// <summary>
		/// Attaches the Player to a given Pole.
		/// </summary>
		/// <param name="pole">The Pole you want to attach the Player to.</param>
		public virtual void GrabPole(Collider other)
		{
			if (stats.current.canPoleClimb && velocity.y <= 0
				&& !holding && other.TryGetComponent(out Pole pole))
			{
				this.pole = pole;
				states.Change<PoleClimbingPlayerState>();
			}
		}

		protected override bool EvaluateLanding(RaycastHit hit)
		{
			return base.EvaluateLanding(hit) && !hit.collider.CompareTag(GameTags.Spring);
		}

		protected override void HandleSlopeLimit(RaycastHit hit)
		{
			if (onWater) return;

			var slopeDirection = Vector3.Cross(hit.normal, Vector3.Cross(hit.normal, Vector3.up));
			slopeDirection = slopeDirection.normalized;
			controller.Move(slopeDirection * stats.current.slideForce * Time.deltaTime);
		}

		protected override void HandleHighLedge(RaycastHit hit)
		{
			if (onWater) return;

			var edgeNormal = hit.point - position;
			var edgePushDirection = Vector3.Cross(edgeNormal, Vector3.Cross(edgeNormal, Vector3.up));
			controller.Move(edgePushDirection * (stats.current.gravity * Time.deltaTime));
		}

		/// <summary>
		/// 在给定的方向上平稳地移动玩家。
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		public virtual void Accelerate(Vector3 direction)
		{
			//移动方向和人物不在一个方向的时候需要转身
			//转身
			var turningDrag = isGrounded && inputs.GetRun() 
				? stats.current.runningTurningDrag 
				: stats.current.turningDrag;
			var acceleration = isGrounded && inputs.GetRun() 
				? stats.current.runningAcceleration 
				: stats.current.acceleration;
			var finalAcceleration = isGrounded 
				? acceleration 
				: stats.current.airAcceleration;
			var topSpeed = inputs.GetRun() 
				? stats.current.runningTopSpeed 
				: stats.current.topSpeed;

			Accelerate(direction, turningDrag, finalAcceleration, topSpeed);

			if (inputs.GetRunUp())
			{
				lateralVelocity = Vector3.ClampMagnitude(lateralVelocity, topSpeed);
			}
		}

		/// <summary>
		/// 朝着输入方向移动
		/// </summary>
		public virtual void AccelerateToInputDirection()
		{
			var inputDirection = inputs.GetMovementCameraDirection();
			Accelerate(inputDirection);
		}

		/// <summary>
		/// Applies the standard slope factor to the Player.
		/// </summary>
		// public virtual void RegularSlopeFactor()
		// {
		// 	if (stats.current.applySlopeFactor)
		// 		SlopeFactor(stats.current.slopeUpwardForce, stats.current.slopeDownwardForce);
		// }

		/// <summary>
		/// 水里面平滑的移动
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		public virtual void WaterAcceleration(Vector3 direction) =>
			Accelerate(direction, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed);

		/// <summary>
		/// 爬行的时候移动顽玩家
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		public virtual void CrawlingAccelerate(Vector3 direction) =>
			Accelerate(direction, stats.current.crawlingTurningSpeed, 
				stats.current.crawlingAcceleration, stats.current.crawlingTopSpeed);

		public virtual void Backflip(float force)
		{
			if (stats.current.canBackflip && !holding)
			{
				verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
				lateralVelocity = -transform.forward * force;
				states.Change<BackflipPlayerState>();
				playerEvents.OnBackflip?.Invoke();
			}
		}
		
		/// <summary>
		/// 使用翻转状态平滑的移动玩家
		/// </summary>
		public virtual void BackflipAcceleration()
		{
			var direction = inputs.GetMovementCameraDirection();
			Accelerate(direction, stats.current.backflipTurningDrag, 
				stats.current.backflipAirAcceleration, stats.current.backflipTopSpeed);
		}

		/// <summary>
		/// 通过减速值平滑的把速度减为0
		/// </summary>
		public virtual void Decelerate() => Decelerate(stats.current.deceleration);

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its friction stats.
		/// 通过摩檫力将其速度设置为0
		/// </summary>
		public virtual void Friction()
		{
			if (OnSlopingGround())
				Decelerate(stats.current.slopeFriction);
			else
				Decelerate(stats.current.friction);
		}

		/// <summary>
		/// Applies a downward force by its gravity stats.
		/// 通过重力属性应用向下的力
		/// </summary>
		public virtual void Gravity()
		{
			if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
			{
				var speed = verticalVelocity.y;
				var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
				speed -= force * gravityMultiplier * Time.deltaTime;
				speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
				verticalVelocity = new Vector3(0, speed, 0);
			}
		}

		/// <summary>
		/// 状态触地时 ，施加向下的力
		/// </summary>
		public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

		/// <summary>
		/// 将玩家旋转到指定方向
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void FaceDirectionSmooth(Vector3 direction) => 
			FaceDirection(direction, stats.current.rotationSpeed);

		/// <summary>
		/// 水里面平滑的转向
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void WaterFaceDirection(Vector3 direction) => FaceDirection(direction, stats.current.waterRotationSpeed);

		/// <summary>
		/// 如果玩家没有在地面上，应该为掉落状态
		/// </summary>
		public virtual void Fall()
		{
			if (!isGrounded)
			{
				states.Change<FallPlayerState>();
			}
		}

		/// <summary>
		/// 处理跳跃
		/// </summary>
		public virtual void Jump()
		{
			
				//可以连跳
				var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
				//能否土狼跳跃
				var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);//土狼跳跃
			
				var holdJump = !holding || stats.current.canJumpWhileHolding;

				if ((isGrounded || onRails || canMultiJump || canCoyoteJump) && holdJump)
				{
					//跳跃
					if (inputs.GetJumpDown())
					{
						Jump(stats.current.maxJumpHeight);
					}
				}

				if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
				{
					verticalVelocity = Vector3.up * stats.current.minJumpHeight;
				}
			
			
		}

		/// <summary>
		/// 跳跃指定高度
		/// </summary>
		/// <param name="height">The force you want to apply.</param>
		public virtual void Jump(float height)
		{
			jumpCounter++;
			verticalVelocity = Vector3.up * height;
			states.Change<FallPlayerState>();
			playerEvents.OnJump?.Invoke(); //TODO
		}

		

		/// <summary>
		/// 重置空中冲刺
		/// </summary>
		public virtual void ResetAirDash() => airDashCounter = 0;

		/// <summary>
		/// 重置跳跃次数
		/// </summary>
		public virtual void ResetJumps() => jumpCounter = 0;

		/// <summary>
		/// 设置跳跃高度
		/// </summary>
		/// <param name="amount">The amount of jumps.</param>
		public virtual void SetJumps(int amount) => jumpCounter = amount;

		
		/// <summary>
		/// 重置空中攻击
		/// </summary>
		public virtual void ResetAirSpins() => airSpinCounter = 0;
		
		
		/// <summary>
		/// 检测攻击
		/// </summary>
		public virtual void Spin()
		{
			//是否可以在空中旋转
			var canAirSpin = (isGrounded || stats.current.canAirSpin) && 
			                 airSpinCounter < stats.current.allowedAirSpins;
			
			
			if (stats.current.canSpin && canAirSpin && !holding && inputs.GetSpinDown())
			{
				//空中旋转
				if (!isGrounded)
				{
					airSpinCounter++;
				}
				
				states.Change<SpinPlayerState>();
				playerEvents.OnSpin?.Invoke();
			}
		}

		//拾取
		public virtual void PickAndThrow()
		{
			if (stats.current.canPickUp && inputs.GetPickAndDropDown())
			{
				//没有抓
				if (!holding)
				{
					//  碰撞检测  有没有物体
					if (CapsuleCast(transform.forward,
						stats.current.pickDistance, out var hit))
					{
						if (hit.transform.TryGetComponent(out Pickable pickable))
						{
							PickUp(pickable);
						}
					}
				}
				else
				{
					Throw();
				}
			}
		}
		/// <summary>
		/// 拾取物体
		/// </summary>
		/// <param name="pickable">被拾取物体</param>
		public virtual void PickUp(Pickable pickable)
		{
			if (!holding && (isGrounded || stats.current.canPickUpOnAir))
			{
				holding = true;
				this.pickable = pickable;
				pickable.PickUp(pickableSlot);
				pickable.onRespawn.AddListener(RemovePickable);
				playerEvents.OnPickUp?.Invoke();
			}
		}
		
		public virtual void Throw()
		{
			if (holding)
			{
				var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
				pickable.Release(transform.forward, force);
				pickable = null;
				holding = false;
				playerEvents.OnThrow?.Invoke();
			}
		}
		/// <summary>
		///重置拾取
		/// </summary>
		public virtual void RemovePickable()
		{
			if (holding)
			{
				pickable = null;
				holding = false;
			}
		}

		public virtual void StompAttack()
		{
			if (!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
			{
				states.Change<StompPlayerState>();
			}
		}
		/// <summary>
		/// 人物攀爬攀爬
		/// </summary>
		public virtual void LedgeGrab()
		{
			if (stats.current.canLedgeHang && velocity.y < 0 && !holding &&
				states.ContainsStateOfType(typeof(LedgeHangingPlayerState)) &&
				DetectingLedge(stats.current.ledgeMaxForwardDistance, stats.current.ledgeMaxDownwardDistance, out var hit))
			{
				if (!(hit.collider is CapsuleCollider) && !(hit.collider is SphereCollider))
				{
					var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
					var lateralOffset = transform.forward * ledgeDistance;//水平偏移
					var verticalOffset = Vector3.down * (height * 0.5f) - center;//竖直偏移
					velocity = Vector3.zero;
					transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
					transform.position = hit.point - lateralOffset + verticalOffset;
					states.Change<LedgeHangingPlayerState>();
					playerEvents.OnLedgeGrabbed?.Invoke();
				}
			}
		}
		
		/// <summary>
		/// 人物冲刺
		/// </summary>
		public virtual void Dash()
		{
			var canAirDash = stats.current.canAirDash && !isGrounded &&
				airDashCounter < stats.current.allowedAirDashes;
			var canGroundDash = stats.current.canGroundDash && isGrounded &&
				Time.time - lastDashTime > stats.current.groundDashCoolDown;
		
			if (inputs.GetDashDown() && (canAirDash || canGroundDash))
			{
				if (!isGrounded) airDashCounter++;
		
				lastDashTime = Time.time;
				states.Change<DashPlayerState>();
			}
		}

		/// <summary>
		/// 滑翔
		/// </summary>
		public virtual void Glide()
		{
			if (!isGrounded && inputs.GetGlide() &&
				verticalVelocity.y <= 0 && stats.current.canGlide)
				states.Change<GlidingPlayerState>();
		}
		
		/// <summary>
		/// 设置skin 的父节点
		/// </summary>
		/// <param name="parent">The transform you want to parent the skin to.</param>
		public virtual void SetSkinParent(Transform parent)
		{
			if (skin)
			{
				skin.parent = parent;
			}
		}
		
		/// <summary>
		/// 重置这个skin
		/// </summary>
		public virtual void ResetSkinParent()
		{
			if (skin)
			{
				skin.parent = transform;
				skin.localPosition = m_skinInitialPosition;
				skin.localRotation = m_skinInitialRotation;
			}
		}
		
		//给刚体一个力
		public virtual void PushRigidbody(Collider other)
		{
			if (!IsPointUnderStep(other.bounds.max) &&
				other.TryGetComponent(out Rigidbody rigidbody))
			{
				var force = lateralVelocity * stats.current.pushForce;
				rigidbody.velocity += force / rigidbody.mass * Time.deltaTime;
			}
		}
	
		/// <summary>
		/// 是不是在抓取范围内
		/// </summary>
		/// <param name="forwardDistance"></param>
		/// <param name="downwardDistance"></param>
		/// <param name="ledgeHit"></param>
		/// <returns></returns>
		protected virtual bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgeHit)
		{
			var contactOffset = Physics.defaultContactOffset + positionDelta; //触摸的偏移
			var ledgeMaxDistance = radius + forwardDistance;//摸到的距离
			var ledgeHeightOffset = height * 0.5f + contactOffset;//高度
			var upwardOffset = transform.up * ledgeHeightOffset;//高度偏移
			var forwardOffset = transform.forward * ledgeMaxDistance;//向前的偏移

			if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance,
				Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
				Physics.Raycast(position + forwardOffset * .01f, transform.up, ledgeHeightOffset,
				Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
			{
				ledgeHit = new RaycastHit();
				return false;
			}

			var origin = position + upwardOffset + forwardOffset;
			var distance = downwardDistance + contactOffset;
			
			return Physics.Raycast(origin, Vector3.down, out ledgeHit, distance,
				stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
		}
		

		protected override void Awake()
		{
			base.Awake();
			InitializeInputs();
			InitializeStats();
			InitializeHealth();
			InitializeTag();
			InitializeRespawn();
			InitializeSkin();

			entityEvents.OnGroundEnter.AddListener(() =>
			{
				ResetJumps();
				ResetAirSpins();
				ResetAirDash();
			});
			
			entityEvents.OnRailsEnter.AddListener(() =>
			{
				ResetJumps();
				ResetAirSpins();
				ResetAirDash();
				//StartGrind();
			});
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (other.CompareTag(GameTags.VolumeWater))
			{
				//如果还不是在水里的状态   并且在水里
				if (!onWater && other.bounds.Contains(unsizedPosition))
				{
					EnterWater(other);
				}
				else if (onWater)
				{
					var exitPoint = position + Vector3.down * k_waterExitOffset;
			
					if (!other.bounds.Contains(exitPoint))
					{
						ExitWater();
					}
				}
			}
		}

    }
}