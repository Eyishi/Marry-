using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Walk Player State")]
    public class WalkPlayerState : PlayerState
    {
        protected override void OnEnter(Player entity)
        {
            
        }

        protected override void OnExit(Player entity)
        {
          
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();
            player.SnapToGround();
            player.Jump();
            player.Fall();
            player.Spin();
            player.PickAndThrow();
            player.Dash();
            //player.RegularSlopeFactor();
            var inputDirection = player.inputs.GetMovementCameraDirection();
            //Debug.Log(inputDirection);
            if (inputDirection.sqrMagnitude > 0) //判断这个方向是否有效
            {
                var dot = Vector3.Dot(inputDirection, player.lateralVelocity);

                if (dot >= player.stats.current.brakeThreshold)//制动的阈值
                {
                    player.Accelerate(inputDirection); //朝着这个方向移动
                    player.FaceDirectionSmooth(player.lateralVelocity);//平滑移动
                }
                //反方向
                else
                {
                    player.states.Change<BrakePlayerState>();
                }
            }
            //没有移动后减速
            else
            {
                player.Friction();

                if (player.lateralVelocity.sqrMagnitude <= 0)
                {
                    player.states.Change<IdlePlayerState>();
                }
            }
            if(player.inputs.GetCrouchAndCraw())
            {
                player.states.Change<CrouchPlayerState>();
            }
        }

        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
            player.GrabPole(other);
        }
    }
}