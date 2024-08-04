using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Fall Player State")]
    public class FallPlayerState : PlayerState
    {
        protected override void OnEnter(Player player)
        {
            
        }

        protected override void OnExit(Player player)
        {
           
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();
            player.SnapToGround();
            player.FaceDirectionSmooth(player.lateralVelocity);
            player.AccelerateToInputDirection();
            player.Jump();
            player.Spin();//攻击
            player.PickAndThrow();
            player.StompAttack();
            player.LedgeGrab();//攀爬处理
            player.Glide();//降落伞
            player.Dash();
            
            if (player.isGrounded)
            {
                player.states.Change<IdlePlayerState>();
            }
        }

        public override void OnContact(Player player, Collider other)
        {
            player.GrabPole(other);
        }
    }
}