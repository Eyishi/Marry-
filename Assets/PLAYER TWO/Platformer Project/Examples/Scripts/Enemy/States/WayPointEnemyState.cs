using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class WayPointEnemyState : EnemyState
    {
        protected override void OnEnter(Enemy enemy)
        {
            
        }

        protected override void OnExit(Enemy enemy)
        {
        }

        protected override void OnStep(Enemy enemy)
        {
            enemy.Gravity();
            enemy.SnapToGround();

            var destination = enemy.waypoints.current.position;//目的地
            destination = new Vector3(destination.x, enemy.position.y, destination.z);//目标有可能在空中
            var head = destination - enemy.position;//目标指向当前位置
            var distance = head.magnitude;//长度
            var direction = head / distance; //方向

            if (distance <= enemy.stats.current.waypointMinDistance) //要到了
            {
                enemy.Decelerate();//，开始减速
                enemy.waypoints.Next();  
            }
            else
            {
                //加速
                enemy.Accelerate(direction, enemy.stats.current.waypointAcceleration, enemy.stats.current.waypointTopSpeed);
				
                if (enemy.stats.current.faceWaypoint)//面向这个点
                {
                    enemy.FaceDirectionSmooth(direction);
                }
            }
        }

        public override void OnContact(Enemy entity, Collider other)
        {
        }
    }
}