using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class DiePlayerState : PlayerState
    {
        protected override void OnEnter(Player player)
        {
            
        }

        protected override void OnExit(Player player)
        {
         
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();//应用重力
            player.Friction();//摩檫力
            player.SnapToGround();//拍打到地上
        }

        public override void OnContact(Player entity, Collider other)
        {
            
        }
    }
}