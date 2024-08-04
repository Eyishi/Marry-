using System;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [Serializable]
    public class EnemyEvents
    {
        /// <summary>
        /// Called when the Player enters this Enemy sight.
        /// 玩家进入敌人视野时
        /// </summary>
        public UnityEvent OnPlayerSpotted;

        /// <summary>
        /// Called when the Player leaves this Enemy sight.
        /// 玩家离开敌人视野时
        /// </summary>
        public UnityEvent OnPlayerScaped;

        /// <summary>
        /// Called when this Enemy touches a Player.
        /// 碰到玩家后
        /// </summary>
        public UnityEvent OnPlayerContact;

        /// <summary>
        /// Called when this Enemy takes damage.
        /// 收到伤害时
        /// </summary>
        public UnityEvent OnDamage;

        /// <summary>
        /// Called when this Enemy loses all health.
        /// 死亡
        /// </summary>
        public UnityEvent OnDie;

        /// <summary>
        /// Called when the Enemy was revived.
        /// 复活
        /// </summary>
        public UnityEvent OnRevive;
    }
}