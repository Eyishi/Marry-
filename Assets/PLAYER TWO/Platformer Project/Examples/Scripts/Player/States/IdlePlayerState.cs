using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Idle Player State")]
    public class IdlePlayerState : PlayerState
    {
       
        protected override void OnEnter(Player player)
        {
            
        }

        protected override void OnExit(Player player)
        {
            
        }

        protected override void OnStep(Player player)
        {
            player.Gravity(); //重力
            player.SnapToGround();
            player.Jump();
            player.Fall();
            player.Spin();
            player.PickAndThrow();
            player.Dash();
            player.Friction();
            
            //获取移动的方向
            var inputDir = player.inputs.GetMovementDirection(); 
            //是否有效
            if (inputDir.sqrMagnitude > 0 || player.lateralVelocity.sqrMagnitude > 0)
            {
                player.states.Change<WalkPlayerState>();
            }
            else if(player.inputs.GetCrouchAndCraw())
            {
                player.states.Change<CrouchPlayerState>();
            }
        }

        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
        }
    }
}