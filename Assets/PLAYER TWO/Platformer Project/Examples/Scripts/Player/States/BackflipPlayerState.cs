using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class BackflipPlayerState : PlayerState
    {
        protected override void OnEnter(Player player)
        {
            player.SetJumps(1);
            player.playerEvents.OnJump.Invoke();

            if (player.stats.current.backflipLockMovement) //锁定移动
            {
                player.inputs.LockMovementDirection();
            }
        }

        protected override void OnExit(Player player)
        {
            
        }

        protected override void OnStep(Player player)
        {
            player.Gravity(player.stats.current.backflipGravity);
            player.BackflipAcceleration();

            //在地上
            if (player.isGrounded)
            {
                player.lateralVelocity = Vector3.zero;
                player.states.Change<IdlePlayerState>();
            }
            //空中
            else if( player.lateralVelocity.y < 0)
            {
                
            }
        }

        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
            player.GrabPole(other);
        }
    }
}