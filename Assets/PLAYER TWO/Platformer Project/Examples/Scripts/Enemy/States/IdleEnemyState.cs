using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class IdleEnemyState : EnemyState
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
            enemy.Friction();
        }

        public override void OnContact(Enemy entity, Collider other)
        {
            throw new System.NotImplementedException();
        }
    }
}