using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [CreateAssetMenu(fileName = "NewEnemyStats", menuName = "PLAYER TWO/Platformer Project/Enemy/New Enemy Stats")]
    public class EnemyStats : EntityStats<EnemyStats>
    {
        [Header("General Stats")]
        public float gravity = 35f; //
        public float snapForce = 15f;//拍打的力
        public float rotationSpeed = 970f;//旋转速度
        public float deceleration = 28f;//减速
        public float friction = 16f;//摩檫力
        public float turningDrag = 28f;//转动的 阻尼

        [Header("Contact Attack Stats")]
        public bool canAttackOnContact = true;//需不需要攻击
        public bool contactPushback = true;//能否pushback
        public float contactOffset = 0.15f;//需不需要位移   偏移能够接触到的距离
        public int contactDamage = 1;//伤害
        public float contactPushBackForce = 18f;
        public float contactSteppingTolerance = 0.1f;

        [Header("View Stats")] //可视区域
        public float spotRange = 5f;
        public float viewRange = 8f;

        [Header("Follow Stats")]
        public float followAcceleration = 10f;
        public float followTopSpeed = 4f;

        [Header("Waypoint Stats")]
        public bool faceWaypoint = true;
        public float waypointMinDistance = 0.5f;
        public float waypointAcceleration = 10f;
        public float waypointTopSpeed = 2f;
    }
}