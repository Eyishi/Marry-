using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class SpinPlayerState:PlayerState
    {
        protected override void OnEnter(Player player)
        {
            //不在地面上
            if (!player.isGrounded)
            {
                player.verticalVelocity = Vector3.up * player.stats.current.airSpinUpwardForce;
            }
        }

        protected override void OnExit(Player player)
        {
            
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();
            player.SnapToGround();
            player.AccelerateToInputDirection();
            player.Jump();
            
            //经过的时间大于攻击持续的时间
            if (timeSinceEntered >= player.stats.current.spinDuration)
            {
                if (player.isGrounded)
                {
                    player.states.Change<IdlePlayerState>();
                }
                else
                {
                    player.states.Change<FallPlayerState>();
                }
            }
        }

        public override void OnContact(Player player, Collider other)
        {
            
        }
    }
}