using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Player))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Animator")]
	public class PlayerAnimator : MonoBehaviour
	{
		[System.Serializable]
		public class ForcedTransition
		{
			[Tooltip("The index of the Player State from the Player State Manager that you want to force a transition from.")]
			public int fromStateId;//从哪个状态来的

			[Tooltip("The index of the layer from your Animator Controller that contains the target animation. (It's 0 if the animation is inside the 'Base Layer')")]
			public int animationLayer;

			[Tooltip("The name of the Animation State you want to play right after finishing the Player State from above.")]
			public string toAnimationState;//去执行的状态
		}

		public Animator animator;

		[Header("Parameters Names")] //参考动画文件
		public string stateName = "State";
		public string lastStateName = "Last State";
		public string lateralSpeedName = "Lateral Speed";
		public string verticalSpeedName = "Vertical Speed";
		public string lateralAnimationSpeedName = "Lateral Animation Speed";
		public string healthName = "Health";
		public string jumpCounterName = "Jump Counter";
		public string isGroundedName = "Is Grounded";
		public string isHoldingName = "Is Holding";
		public string onStateChangedName = "On State Changed";

		[Header("Settings")]
		public float minLateralAnimationSpeed = 0.5f; //最小的动画速度
		public List<ForcedTransition> forcedTransitions;

		protected int m_stateHash;
		protected int m_lastStateHash;
		protected int m_lateralSpeedHash;
		protected int m_verticalSpeedHash;
		protected int m_lateralAnimationSpeedHash;
		protected int m_healthHash;
		protected int m_jumpCounterHash;
		protected int m_isGroundedHash;
		protected int m_isHoldingHash;
		protected int m_onStateChangedHash;

		protected Dictionary<int, ForcedTransition> m_forcedTransitions;

		protected Player m_player;//获取玩家

		/***
		 * 初始化 
		 */
		protected virtual void InitializePlayer()
		{
			m_player = GetComponent<Player>();
			m_player.states.events.onChange.AddListener(HandleForcedTransitions);//监听
		}
		
		/***
		 * 初始化这个m_forcedTransitions
		 */
		protected virtual void InitializeForcedTransitions()
		{
			m_forcedTransitions = new Dictionary<int, ForcedTransition>();

			foreach (var transition in forcedTransitions)
			{
				if (!m_forcedTransitions.ContainsKey(transition.fromStateId))
				{
					m_forcedTransitions.Add(transition.fromStateId, transition);
				}
			}
		}
		/***
		 * 动画的触发器 
		 */
		protected virtual void InitializeAnimatorTriggers()
		{
			m_player.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangedHash));
		}

		/***
		 * 跟动画文件有关
		 */
		protected virtual void InitializeParametersHash()
		{
			m_stateHash = Animator.StringToHash(stateName);
			m_lastStateHash = Animator.StringToHash(lastStateName);
			m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
			m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
			m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
			m_healthHash = Animator.StringToHash(healthName);
			m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
			m_isGroundedHash = Animator.StringToHash(isGroundedName);
			m_isHoldingHash = Animator.StringToHash(isHoldingName);
			m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
		}

		/***
		 * 当玩家动画发生变化时
		 */
		protected virtual void HandleForcedTransitions()
		{
			var lastStateIndex = m_player.states.lastIndex;//最后一个状态的索引

			if (m_forcedTransitions.ContainsKey(lastStateIndex))//获取这个状态
			{
				var layer = m_forcedTransitions[lastStateIndex].animationLayer;
				animator.Play(m_forcedTransitions[lastStateIndex].toAnimationState, layer);//播放对应的动画
			}
		}
		/***
		 * 设置一些触发器的参数？
		 */
		protected virtual void HandleAnimatorParameters()
		{
			var lateralSpeed = m_player.lateralVelocity.magnitude;
			var verticalSpeed = m_player.verticalVelocity.y;
			var lateralAnimationSpeed = Mathf.Max(minLateralAnimationSpeed, 
				lateralSpeed / m_player.stats.current.topSpeed);

			animator.SetInteger(m_stateHash, m_player.states.index);
			animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
			animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
			animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
			animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationSpeed);
			animator.SetInteger(m_healthHash, m_player.health.current);
			animator.SetInteger(m_jumpCounterHash, m_player.jumpCounter);
			animator.SetBool(m_isGroundedHash, m_player.isGrounded);
			animator.SetBool(m_isHoldingHash, m_player.holding);
		}

		protected virtual void Start()
		{
			InitializePlayer();
			InitializeForcedTransitions();
			InitializeParametersHash();
			InitializeAnimatorTriggers();
		}

		protected virtual void LateUpdate() => HandleAnimatorParameters();
	}
}
