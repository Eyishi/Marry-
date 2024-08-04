
using System;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [Serializable]
    public class EntityEvents
    {
        /// <summary>
        /// 掉在地上时调用
        /// </summary>
        public UnityEvent OnGroundEnter;

        /// <summary>
        /// 离开地面时调用
        /// </summary>
        public UnityEvent OnGroundExit;

        /// <summary>
        /// 进入轨道时调用
        /// </summary>
        public UnityEvent OnRailsEnter;

        /// <summary>
        /// 离开轨道时调用
        /// </summary>
        public UnityEvent OnRailsExit;
    }
}