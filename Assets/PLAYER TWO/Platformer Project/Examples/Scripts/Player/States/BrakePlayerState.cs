using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class BrakePlayerState:PlayerState
    {
        protected override void OnEnter(Player player)
        {
            
        }

        protected override void OnExit(Player player)
        {
           
        }

        protected override void OnStep(Player player)
        {
           
            var inputDirection = player.inputs.GetMovementCameraDirection();

            //后空翻
            if (player.stats.current.canBackflip &&
                Vector3.Dot(inputDirection, player.transform.forward) < 0 &&
                player.inputs.GetJumpDown())//判断玩家是否能回转，并且输入方向和玩家的方向>90
            {
                player.Backflip(player.stats.current.backflipBackwardTurnForce);
            }
            else
            {
                player.SnapToGround();
                player.Jump();
                player.Fall();
                player.Decelerate();

                if (player.lateralVelocity.sqrMagnitude == 0)
                {
                    player.states.Change<IdlePlayerState>();
                }
            }
        }

        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
            player.GrabPole(other);
        }
    }
}