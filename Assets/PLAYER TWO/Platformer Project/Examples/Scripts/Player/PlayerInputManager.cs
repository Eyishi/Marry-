using UnityEngine;
using UnityEngine.InputSystem;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Input Manager")]
	public class PlayerInputManager : MonoBehaviour
	{
		public InputActionAsset actions;

		protected InputAction m_movement;//移动
		protected InputAction m_run;//跑
		protected InputAction m_jump;//跳跃
		protected InputAction m_dive;//潜水
		protected InputAction m_spin;//攻击
		protected InputAction m_pickAndDrop;
		protected InputAction m_crouch;//蹲伏
		protected InputAction m_airDive;
		protected InputAction m_stomp;
		protected InputAction m_releaseLedge;
		protected InputAction m_pause;
		protected InputAction m_look;
		protected InputAction m_glide;
		protected InputAction m_dash;
		protected InputAction m_grindBrake;

		protected Camera m_camera; //相坤

		protected float m_movementDirectionUnlockTime; //保护的时间，一帧的时间比设定的保护时间小，无效操作
		protected float? m_lastJumpTime;

		protected const string k_mouseDeviceName = "Mouse";//鼠标设备名

		protected const float k_jumpBuffer = 0.15f;//按住了0.15秒

		protected virtual void CacheActions()
		{
			m_movement = actions["Movement"];
			m_run = actions["Run"];
			m_jump = actions["Jump"];
			m_dive = actions["Dive"];
			m_spin = actions["Spin"];
			m_pickAndDrop = actions["PickAndDrop"];
			m_crouch = actions["Crouch"];
			m_airDive = actions["AirDive"];
			m_stomp = actions["Stomp"];
			m_releaseLedge = actions["ReleaseLedge"];
			m_pause = actions["Pause"];
			m_look = actions["Look"];
			m_glide = actions["Glide"];
			m_dash = actions["Dash"];
			m_grindBrake = actions["Grind Brake"];
		}
		//获取移动的方向
		public virtual Vector3 GetMovementDirection() //TODO 
		{
			//一帧的时间比设定的保护时间小，无效操作
			if (Time.time < m_movementDirectionUnlockTime) return Vector3.zero;

			var value = m_movement.ReadValue<Vector2>();//获取按的方向
			return GetAxisWithCrossDeadZone(value);
		}

		//获取看的方向
		public virtual Vector3 GetLookDirection()
		{
			var value = m_look.ReadValue<Vector2>();

			if (IsLookingWithMouse())
			{
				return new Vector3(value.x, 0, value.y);
			}

			return GetAxisWithCrossDeadZone(value);
		}
		
		//获取移动中相机的朝向
		public virtual Vector3 GetMovementCameraDirection()
		{
			var direction = GetMovementDirection();

			if (direction.sqrMagnitude > 0) //
			{
				var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);
				direction = rotation * direction;
				direction = direction.normalized;
			}

			return direction;
		}

		/// <summary>
		/// Remaps a given axis considering the Input System's default deadzone.
		/// This method uses a cross shape instead of a circle one to evaluate the deadzone range.
		/// 到底是往哪走
		/// </summary>
		/// <param name="axis">The axis you want to remap.</param>
		public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
		{
			var deadzone = InputSystem.settings.defaultDeadzoneMin; //TODO
			axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadzone(axis.x, deadzone) : 0;
			axis.y = Mathf.Abs(axis.y) > deadzone ? RemapToDeadzone(axis.y, deadzone) : 0;
			return new Vector3(axis.x, 0, axis.y);
		}

		//是不是用鼠标看的
		public virtual bool IsLookingWithMouse()
		{
			if (m_look.activeControl == null)
			{
				return false;
			}

			return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
		}
		//是否在跑
		public virtual bool GetRun() => m_run.IsPressed();
		public virtual bool GetRunUp() => m_run.WasReleasedThisFrame();

		//跳跃
		public virtual bool GetJumpDown()
		{
			if (m_lastJumpTime != null &&
				Time.time - m_lastJumpTime < k_jumpBuffer)
			{
				m_lastJumpTime = null;
				return true;
			}

			return false;
		}

		public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();
		public virtual bool GetDive() => m_dive.IsPressed();
		public virtual bool GetSpinDown() => m_spin.WasPressedThisFrame();
		public virtual bool GetPickAndDropDown() => m_pickAndDrop.WasPressedThisFrame();//拾取
		public virtual bool GetCrouchAndCraw() => m_crouch.IsPressed();
		public virtual bool GetAirDiveDown() => m_airDive.WasPressedThisFrame();
		public virtual bool GetStompDown() => m_stomp.WasPressedThisFrame();
		public virtual bool GetReleaseLedgeDown() => m_releaseLedge.WasPressedThisFrame();//松手
		public virtual bool GetGlide() => m_glide.IsPressed();//滑翔
		public virtual bool GetDashDown() => m_dash.WasPressedThisFrame();
		public virtual bool GetGrindBrake() => m_grindBrake.IsPressed();
		public virtual bool GetPauseDown() => m_pause.WasPressedThisFrame();

		/// <summary>
		/// 将值重新映射到0-1范围。
		/// </summary>
		/// <param name="value">The value you wants to remap.</param>
		/// <param name="deadzone">The minimun deadzone value.</param>
		protected float RemapToDeadzone(float value, float deadzone) => (value - deadzone) / (1 - deadzone);

		/// <summary>
		/// 锁定移动方向输入
		/// </summary>
		/// <param name="duration">The duration of the locking state in seconds.</param>
		public virtual void LockMovementDirection(float duration = 0.25f)
		{
			m_movementDirectionUnlockTime = Time.time + duration;
		}

		protected virtual void Awake() => CacheActions();

		protected virtual void Start()
		{
			m_camera = Camera.main;
			actions.Enable();
		}

		protected virtual void Update()
		{
			if (m_jump.WasPressedThisFrame())//这一帧有没有按
			{
				m_lastJumpTime = Time.time;
			}
		}

		protected virtual void OnEnable() => actions?.Enable();
		protected virtual void OnDisable() => actions?.Disable();
	}
}
