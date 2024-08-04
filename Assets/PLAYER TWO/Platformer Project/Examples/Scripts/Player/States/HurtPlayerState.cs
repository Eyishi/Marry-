using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class HurtPlayerState : PlayerState
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

            if (player.isGrounded && (player.verticalVelocity.y <= 0))
            {
                if (player.health.current > 0)
                {
                    player.states.Change<IdlePlayerState>();
                }
                else
                {
                    player.states.Change<DiePlayerState>();
                }
            }
        }

        public override void OnContact(Player entity, Collider other)
        {
        }
    }
}