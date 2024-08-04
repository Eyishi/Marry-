
using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Player))] //表示引入这个脚本的时候也会引入Player
    public class PlayerStateManager : EntityStateManager<Player>
    {

        [ClassTypeName(typeof(PlayerState))] 
        public string[] states; //状态的字符串，因为是通过反射驱动

        /// <summary>
        /// 通过字符串数组获取状态表
        /// </summary>
        /// <returns></returns>
        protected override List<EntityState<Player>> GetStateList() 
        {
            return PlayerState.CreateListFromStringArray(states);
        }
    }
}