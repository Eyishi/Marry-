using System;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [Serializable]
    public class EntityStateManagerEvents
    {

        /// <summary>
        /// 当状态发生更改时调用。
        /// </summary>
        public UnityEvent onChange;

        /// <summary>
        /// 进入
        /// </summary>
        public UnityEvent<Type> onEnter;

        /// <summary>
        /// 退出
        /// </summary>
        public UnityEvent<Type> onExit;
    }
}