using System;
using UnityEngine;
using UnityEngine.Splines;
namespace PLAYERTWO.PlatformerProject
{
	public abstract class Entity : MonoBehaviour
	{
			public EntityEvents entityEvents;

			protected Collider[] m_contactBuffer = new Collider[10];
			protected Collider[] m_penetrationBuffer = new Collider[32];

			protected readonly float m_groundOffset = 0.1f; // 偏移值，用来计算是否在地面上的
			protected readonly float m_penetrationOffset = -0.1f;
			protected readonly float m_slopingGroundAngle = 20f;

			/// <summary>
			/// 实体的角色控制器
			/// </summary>
			public CharacterController controller { get; protected set; }

			/// <summary>
			/// 此实体的当前速度。
			/// </summary>
			public Vector3 velocity { get; set; }

			/// <summary>
			/// The current XZ velocity of this Entity.
			/// 此实体的当前XZ速度。
			/// </summary>
			public Vector3 lateralVelocity
			{
				get { return new Vector3(velocity.x, 0, velocity.z); }
				set { velocity = new Vector3(value.x, velocity.y, value.z); }//TODO
			}

			/// <summary>
			/// 当前实体Y轴方向的速度。
			/// </summary>
			public Vector3 verticalVelocity
			{
				get { return new Vector3(0, velocity.y, 0); }
				set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
			}

			/// <summary>
			/// 返回实体在上一帧中的位置。
			/// </summary>
			public Vector3 lastPosition { get; set; }

			/// <summary>
			/// 实体的位置
			/// </summary>
			public Vector3 position => transform.position + center;

			/// <summary>
			/// 获取实体没有变形的位置
			/// </summary>
			public Vector3 unsizedPosition => position - transform.up * height * 0.5f + transform.up * originalHeight * 0.5f;

			/// <summary>
			/// 实体的底部位置(考虑步长偏移)
			/// </summary>
			public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);

			/// <summary>
			/// 上一个位置和当前位置的差
			/// </summary>
			public float positionDelta { get; protected set; }

			/// <summary>
			/// 返回实体的上一帧的接地时间
			/// </summary>
			public float lastGroundTime { get; protected set; }

			/// <summary>
			/// 判断实体是否在地上
			/// </summary>
			public bool isGrounded { get; protected set; } = true;

			/// <summary>
			/// </summary>
			public bool onRails { get; set; }

			public float accelerationMultiplier { get; set; } = 1f;

			public float gravityMultiplier { get; set; } = 1f;//重力的倍数  //可以用来加buf

			public float topSpeedMultiplier { get; set; } = 1f;

			public float turningDragMultiplier { get; set; } = 1f;//多重的拖拽 

			public float decelerationMultiplier { get; set; } = 1f;//减速的乘数

			/// <summary>
			/// 撞击的地方 ， 用来派发给 物体碰撞事件
			/// </summary>
			public RaycastHit groundHit;

			/// <summary>
			/// 导轨的容器
			/// </summary>
			public SplineContainer rails { get; protected set; }

			/// <summary>
			/// 地面的角度 ， 用于斜坡的时候有一个偏移
			/// </summary>
			public float groundAngle { get; protected set; }

			/// <summary>
			/// 地面的法向量
			/// </summary>
			public Vector3 groundNormal { get; protected set; }

			/// <summary>
			/// 返回地面的坡度方向
			/// </summary>
			public Vector3 localSlopeDirection { get; protected set; }

			/// <summary>
			/// 实体的原始高度
			/// </summary>
			public float originalHeight { get; protected set; }

			/// <summary>
			/// 实体碰撞器的高度
			/// </summary>
			public float height => controller.height;

			/// <summary>
			/// 实体碰撞器的半径
			/// </summary>
			public float radius => controller.radius;

			/// <summary>
			/// 角色控制器的中心
			/// </summary>
			public Vector3 center => controller.center;

			protected CapsuleCollider m_collider;//胶囊碰撞器
			protected BoxCollider m_penetratorCollider;

			protected Rigidbody m_rigidbody;
			
			/// <summary>
			/// 给定的点是不是在实体的脚下
			/// </summary>
			/// <param name="point">检测的点.</param>
			public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;

			/// <summary>
			/// 是否在斜坡上
			/// </summary>
			/// <returns></returns>
			public virtual bool OnSlopingGround()
			{
				if (isGrounded && groundAngle > m_slopingGroundAngle)
				{
					if (Physics.Raycast(transform.position, -transform.up, out var hit, height * 2f,
						Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
						return Vector3.Angle(hit.normal, Vector3.up) > m_slopingGroundAngle;
					else
						return true;
				}

				return false;
			}

			/// <summary>
			/// 重置一下碰撞器的高度
			/// </summary>
			/// <param name="height">The desired height.</param>
			public virtual void ResizeCollider(float height)
			{
				var delta = height - this.height;
				controller.height = height;
				controller.center += Vector3.up * (delta * 0.5f);
			}

			public virtual bool CapsuleCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
				QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
			{
				return CapsuleCast(direction, distance, out _, layer, queryTriggerInteraction);
			}

			//检测
			public virtual bool CapsuleCast(Vector3 direction, float distance,
				out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
				QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
			{
				var origin = position - direction * radius + center;
				var offset = transform.up * (height * 0.5f - radius);
				var top = origin + offset;
				var bottom = origin - offset;
				return Physics.CapsuleCast(top, bottom, radius, direction,
					out hit, distance + radius, layer, queryTriggerInteraction);
			}

			public virtual bool SphereCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
				QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
			{
				return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
			}
			
			//检测是否在地面
			public virtual bool SphereCast(Vector3 direction, float distance, 
				out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
				QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
			{
				var castDistance = Mathf.Abs(distance - radius);
				return Physics.SphereCast(position, radius, direction,
					out hit, castDistance, layer, queryTriggerInteraction);
			}
			
			//碰撞了哪些实体
			public virtual int OverlapEntity(Collider[] result, float skinOffset = 0)
			{
				var contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
				var overlapsRadius = radius + contactOffset;
				var offset = (height + contactOffset) * 0.5f - overlapsRadius;
				var top = position + Vector3.up * offset;
				var bottom = position + Vector3.down * offset;
				return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapsRadius, result);
			}

			public virtual void ApplyDamage(int damage, Vector3 origin) { }
	}
	
	public abstract class Entity<T> : Entity where T : Entity<T>
	{
		protected IEntityContact[] m_contacts;

		/// <summary>
		/// 返回此实体的状态管理器。
		/// </summary>
		public EntityStateManager<T> states { get; protected set; }

	    protected virtual void Awake()
	    {
		    //状态管理
		    InitializeStateManager();
	        InitializeController();
	        //InitializePenetratorCollider();
	        InitializeStateManager();
	    }
		
	    protected virtual void InitializeController()
	    {
		    controller = GetComponent<CharacterController>();

		    if (!controller)
		    {
			    controller = gameObject.AddComponent<CharacterController>();
		    }

		    controller.skinWidth = 0.005f;
		    controller.minMoveDistance = 0;
		    originalHeight = controller.height;
	    }
	    
	    protected virtual void InitializePenetratorCollider()
	    {
		    var xzSize = radius * 2f - controller.skinWidth;
		    m_penetratorCollider = gameObject.AddComponent<BoxCollider>();
		    m_penetratorCollider.size = new Vector3(xzSize, height - controller.stepOffset, xzSize);
		    m_penetratorCollider.center = center + Vector3.up * controller.stepOffset * 0.5f;
		    m_penetratorCollider.isTrigger = true;
	    }
		//初始化这个碰撞器
	    protected virtual void InitializeCollider()
	    {
		    m_collider = gameObject.AddComponent<CapsuleCollider>();
		    m_collider.height = controller.height;
		    m_collider.radius = controller.radius;
		    m_collider.center = controller.center;
		    m_collider.isTrigger = true;
		    m_collider.enabled = false;
	    }

	    protected virtual void InitializeRigidbody()
	    {
		    m_rigidbody = gameObject.AddComponent<Rigidbody>();
		    m_rigidbody.isKinematic = true;
	    }

	    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();
	    
	    protected virtual void HandleStates() => states.Step();
	    protected virtual void HandleController()
	    {
		    if (controller.enabled)
		    {
			    controller.Move(velocity * Time.deltaTime);
			    return;
		    }

		    transform.position += velocity * Time.deltaTime;
	    }
	    
		
			protected virtual void HandleGround()
			{

				var distance = (height * 0.5f) + m_groundOffset; //高度
				//射线检测 在不在地面上
				if (SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
				{
					if (!isGrounded)
					{
						//在地面上了
						if (EvaluateLanding(hit))
						{
							EnterGround(hit);
						}
						else
						{
							HandleHighLedge(hit);
						}
					}
					else if (IsPointUnderStep(hit.point))
					{
						UpdateGround(hit);
					
						if (Vector3.Angle(hit.normal, Vector3.up) >= controller.slopeLimit)
						{
							//HandleSlopeLimit(hit);
						}
					}
					else
					{
						HandleHighLedge(hit);
					}
				}
				//离开地面
				else
				{
					ExitGround();
				}
			}
			
			/// <summary>
			/// 测碰撞处理  人物跳跃时顶到箱子
			/// 并且  给碰撞到的物体派发碰撞事件
			/// </summary>
			protected virtual void HandleContacts()
			{
				//获取碰撞到的物体
				var overlaps = OverlapEntity(m_contactBuffer);

				for (int i = 0; i < overlaps; i++)
				{
					// 不是触发器 并且被撞击的位置不能是自己
					if (!m_contactBuffer[i].isTrigger && m_contactBuffer[i].transform != transform)//被撞击
					{
						OnContact(m_contactBuffer[i]);

						var listeners = m_contactBuffer[i].GetComponents<IEntityContact>();//广播
						
						//这个是调用被碰撞到的物体
						foreach (var contact in listeners)
						{
							contact.OnEntityContact((T)this);
						}

						if (m_contactBuffer[i].bounds.min.y > controller.bounds.max.y)
						{
							verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
						}
					}
				}
			}
			/// <summary>
			/// 处理位置相关  
			/// </summary>
			protected virtual void HandlePosition()
			{
				positionDelta = (position - lastPosition).magnitude;
				lastPosition = position;
			}

			protected virtual void HandlePenetration()
			{
				var xzSize = m_penetratorCollider.size.x * 0.5f;
				var ySize = (height - controller.stepOffset * 0.5f) * 0.5f;
				var origin = position + Vector3.up * controller.stepOffset * 0.5f;
				var halfExtents = new Vector3(xzSize, ySize, xzSize);
				var overlaps = Physics.OverlapBoxNonAlloc(origin, halfExtents, m_penetrationBuffer,
					Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

				for (int i = 0; i < overlaps; i++)
				{
					if (!m_penetrationBuffer[i].isTrigger && m_penetrationBuffer[i].transform != transform &&
						(lateralVelocity.sqrMagnitude == 0 || m_penetrationBuffer[i].CompareTag(GameTags.Platform)))
					{
						if (Physics.ComputePenetration(m_penetratorCollider, position, Quaternion.identity,
							m_penetrationBuffer[i], m_penetrationBuffer[i].transform.position,
							m_penetrationBuffer[i].transform.rotation, out var direction, out float distance))
						{
							var pushDirection = new Vector3(direction.x, 0, direction.z).normalized;
							transform.position += pushDirection * distance;
						}
					}
				}
			}
			//进入地面
			protected virtual void EnterGround(RaycastHit hit)
			{
				if (!isGrounded)
				{
					groundHit = hit;
					isGrounded = true;
					entityEvents.OnGroundEnter?.Invoke();
				}
			}

		//离开地面，把对应的值改变
		protected virtual void ExitGround()
		{
			if (isGrounded)
			{
				isGrounded = false;
				transform.parent = null;
				lastGroundTime = Time.time;
				verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
				entityEvents.OnGroundExit?.Invoke();
			}
		}
		
		protected virtual void EnterRail(SplineContainer rails)
		{
			if (!onRails)
			{
				onRails = true;
				this.rails = rails;
				entityEvents.OnRailsEnter.Invoke();
			}
		}
		protected virtual void UpdateGround(RaycastHit hit)
		{
			if (isGrounded)
			{
				groundHit = hit;
				groundNormal = groundHit.normal;
				groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
				localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
				transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
			}
		}
		//不断检测是不是在地面上
		protected virtual bool EvaluateLanding(RaycastHit hit)
		{
			return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
		}

		protected virtual void HandleSlopeLimit(RaycastHit hit)
		{
			
		}

		protected virtual void HandleHighLedge(RaycastHit hit)
		{
			
		}

		protected virtual void OnUpdate() { }

		//与其他物体发生碰撞
		protected virtual void OnContact(Collider other)
		{
			if (other)
			{
				states.OnContact(other);
			}
		}

		/// <summary>
		/// 根据输入方向平滑的移动玩家
		/// </summary>
		/// <param name="direction">移动的方向</param>
		/// <param name="turningDrag">转向的速度</param>
		/// <param name="acceleration">加速度</param>
		/// <param name="topSpeed">最大移动速度</param>
		public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
		{
			if (direction.sqrMagnitude > 0)
			{
				var speed = Vector3.Dot(direction, lateralVelocity);
				var velocity = direction * speed;
				var turningVelocity = lateralVelocity - velocity;
				var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
				var targetTopSpeed = topSpeed * topSpeedMultiplier;

				if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
				{
					speed += acceleration * accelerationMultiplier * Time.deltaTime;
					speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
				}

				velocity = direction * speed;
				turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
				lateralVelocity = velocity + turningVelocity;
			}
		}

		/// <summary>
		/// 平滑的把速度减为0
		/// </summary>
		/// <param name="deceleration">减速的大小</param>
		public virtual void Decelerate(float deceleration)
		{
			var delta = deceleration * decelerationMultiplier * Time.deltaTime;
			lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
		}

		/// <summary>
		/// 平滑的将垂直速度移动为0 （向下的重力）
		/// </summary>
		/// <param name="gravity">How fast it will move over time.</param>
		public virtual void Gravity(float gravity)
		{
			if (!isGrounded)
			{
				verticalVelocity += Vector3.down * (gravity * gravityMultiplier * Time.deltaTime);
			}
		}

		/// <summary>
		/// Increases the lateral velocity based on the slope angle.
		/// </summary>
		/// <param name="upwardForce">The force applied when moving upwards.</param>
		/// <param name="downwardForce">The force applied when moving downwards.</param>
		// public virtual void SlopeFactor(float upwardForce, float downwardForce)
		// {
		// 	if (!isGrounded || !OnSlopingGround()) return;
		//
		// 	var factor = Vector3.Dot(Vector3.up, groundNormal);
		// 	var downwards = Vector3.Dot(localSlopeDirection, lateralVelocity) > 0;
		// 	var multiplier = downwards ? downwardForce : upwardForce;
		// 	var delta = factor * multiplier * Time.deltaTime;
		// 	lateralVelocity += localSlopeDirection * delta;
		// }

		/// <summary>
		/// 施加向下的力
		/// </summary>
		/// <param name="force">The force you want to apply.</param>
		public virtual void SnapToGround(float force)
		{
			if (isGrounded && (verticalVelocity.y <= 0))
			{
				verticalVelocity = Vector3.down * force;
			}
		}

		/// <summary>
		/// 旋转玩家到指定的方向
		/// </summary>
		/// <param name="direction">The direction you want to face.</param>
		public virtual void FaceDirection(Vector3 direction)
		{
			if (direction.sqrMagnitude > 0)
			{
				var rotation = Quaternion.LookRotation(direction, Vector3.up);
				transform.rotation = rotation;
			}
		}

		/// <summary>
		/// 将玩家向指定方向旋转
		/// </summary>
		/// <param name="direction">The direction you want to face. 方向</param>
		/// <param name="degreesPerSecond">How fast it should rotate over time.速度</param>
		public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
		{
			if (direction != Vector3.zero)
			{
				var rotation = transform.rotation;
				var rotationDelta = degreesPerSecond * Time.deltaTime;
				var target = Quaternion.LookRotation(direction, Vector3.up);
				transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
			}
		}

		/// <summary>
		/// 这个点可不可以爬
		/// </summary>
		/// <param name="position">The position you want to test if the Entity collider fits.</param>
		public virtual bool FitsIntoPosition(Vector3 position)
		{
			var bounds = controller.bounds;
			var radius = controller.radius - controller.skinWidth;
			var offset = height * 0.5f - radius;
			var top = position + Vector3.up * offset;
			var bottom = position - Vector3.up * offset;

			return !Physics.CheckCapsule(top, bottom, radius,
				Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
		}

		/// <summary>
		/// Enables or disables the custom collision. Disabling the Character Controller.
		/// </summary>
		/// <param name="value">If true, enables the custom collision.</param>
		public virtual void UseCustomCollision(bool value)
		{
			controller.enabled = !value;

			if (value)
			{
				InitializeCollider();
				InitializeRigidbody();
			}
			else
			{
				Destroy(m_collider);
				Destroy(m_rigidbody);
			}
		}
	    private void Update()
	    {
		    if (controller.enabled || m_collider != null)
		    {
			    HandleStates();
			    HandleController();
			    HandleGround();//地面的检测
			    HandleContacts();//处理碰撞
			    OnUpdate();   
		    }
	    }

	    private void LateUpdate()
	    {
		    if (controller.enabled)
		    {
			    HandlePosition();
		    }
	    }
	}
}
