using UnityEngine;
using System.Collections;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Ledge Hanging Player State")]
	public class LedgeHangingPlayerState : PlayerState
	{
		protected bool m_keepParent; //有没有抓的东西
		protected Coroutine m_clearParentRoutine; //清除父节点

		protected const float k_clearParentDelay = 0.25f;

		protected override void OnEnter(Player player)
		{
			if (m_clearParentRoutine != null)
				player.StopCoroutine(m_clearParentRoutine);
			
			m_keepParent = false;
			//皮肤偏移
			player.skin.position += player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
			player.ResetJumps();
			player.ResetAirSpins();
			player.ResetAirDash();
		}

		//清除父节点
		protected override void OnExit(Player player)
		{
			m_clearParentRoutine = player.StartCoroutine(ClearParentRoutine(player));
			player.skin.position -= player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
		}

		protected override void OnStep(Player player)
		{
			var ledgeTopMaxDistance = player.radius + player.stats.current.ledgeMaxForwardDistance;
			var ledgeTopHeightOffset = player.height * 0.5f + player.stats.current.ledgeMaxDownwardDistance;
			var topOrigin = player.position + Vector3.up * ledgeTopHeightOffset + 
			                player.transform.forward * ledgeTopMaxDistance; //顶点
			var sideOrigin = player.position + Vector3.up * (player.height * 0.5f) + 
			                 Vector3.down * player.stats.current.ledgeSideHeightOffset; //边缘的起点
			var rayDistance = player.radius + player.stats.current.ledgeSideMaxDistance;//射线的距离
			var rayRadius = player.stats.current.ledgeSideCollisionRadius; //射线的半径

			//是否在墙上
			if (Physics.SphereCast(sideOrigin, rayRadius, player.transform.forward, out var sideHit,//边缘有没有碰到
				rayDistance, player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore) &&
				Physics.Raycast(topOrigin, Vector3.down, out var topHit, player.height,
				player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
			{
				var inputDirection = player.inputs.GetMovementDirection();
				var ledgeSideOrigin = sideOrigin + player.transform.right * 
					(Mathf.Sign(inputDirection.x) * player.radius);//起点
				var ledgeHeight = topHit.point.y - player.height * 0.5f;
				var sideForward = -new Vector3(sideHit.normal.x, 0, sideHit.normal.z).normalized;
				var destinationHeight = player.height * 0.5f + Physics.defaultContactOffset;
				var climbDestination = topHit.point + Vector3.up * destinationHeight +
					player.transform.forward * player.radius;

				player.FaceDirection(sideForward);

				if (Physics.Raycast(ledgeSideOrigin, sideForward, rayDistance,
					player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
				{
					//把速度减小
					player.lateralVelocity = 
						player.transform.right * (inputDirection.x * player.stats.current.ledgeMovementSpeed);
				}
				else
				{
					player.lateralVelocity = Vector3.zero;
				}

				player.transform.position = new Vector3(sideHit.point.x, ledgeHeight, sideHit.point.z)
				                            - sideForward * player.radius - player.center;

				//松开键
				if (player.inputs.GetReleaseLedgeDown())
				{
					player.FaceDirection(-sideForward);
					player.states.Change<FallPlayerState>();
				}//跳跃键
				else if (player.inputs.GetJumpDown())
				{
					player.Jump(player.stats.current.maxJumpHeight);
					player.states.Change<FallPlayerState>();
				}
				//是否可以转到攀爬
				else if (inputDirection.z > 0 && player.stats.current.canClimbLedges &&
						((1 << topHit.collider.gameObject.layer) & player.stats.current.ledgeClimbingLayers) != 0 &&
						player.FitsIntoPosition(climbDestination))
				{
					m_keepParent = true;
					player.states.Change<LedgeClimbingPlayerState>();
					player.playerEvents.OnLedgeClimbing?.Invoke();
				}
			}
			else//不在墙上，自然掉落
			{
				player.states.Change<FallPlayerState>();
			}
		}

		public override void OnContact(Player player, Collider other) { }

		//把抓取的节点从父节点去掉
		protected virtual IEnumerator ClearParentRoutine(Player player)
		{
			if (m_keepParent) yield break;

			yield return new WaitForSeconds(k_clearParentDelay);

			player.transform.parent = null;
		}
	}
}
