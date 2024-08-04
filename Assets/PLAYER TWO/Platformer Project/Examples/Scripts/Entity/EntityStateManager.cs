using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class EntityStateManager : MonoBehaviour
    {
        public EntityStateManagerEvents events;
    }

    public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
    {
        //所有状态
        protected List<EntityState<T>> m_list = new List<EntityState<T>>();

        //方便查找状态
        protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

        //获取状态列表
        protected abstract List<EntityState<T>> GetStateList();

        //当前的状态
        public EntityState<T> current { get; protected set; }

        //某一个实体，例如玩家，敌人？
        public T entity { get; protected set; }

        //上一个状态
        public EntityState<T> last { get; protected set; }

        /// <summary>
        /// Return the index of the current Entity State.
        /// 返回当前实体状态的索引
        /// </summary>
        public int index => m_list.IndexOf(current);
        /// <summary>
        /// Return the index of the current Entity State.
        /// 返回上一个实体状态的索引。
        /// </summary>
        public int lastIndex => m_list.IndexOf(last);

        //初始化这个实体
        protected virtual void InitializeEntity() => entity = GetComponent<T>();

        /// <summary>
        /// 初始化所有状态
        /// </summary>
        public virtual void InitializeStates()
        {
            m_list = GetStateList();
            foreach (var state in m_list)
            {
                var type = state.GetType();
                if (!m_states.ContainsKey(type)) //把状态存到
                {
                    m_states.Add(type, state);
                }
            }

            if (m_list.Count > 0)
            {
                current = m_list[0];
            }
        }

        protected void Start()
        {
            InitializeEntity();
            InitializeStates();
        }

        public virtual void Step()
        {
            if (current != null && Time.timeScale > 0)
            {
                current.Step(entity);
            }
        }

        /// <summary>
        /// 根据它 在状态数组(states)的索引更改为给定的状态(states)索引。
        /// </summary>
        /// <param name="to">更改状态的索引.</param>
        public virtual void Change(int to)
        {
            if (to >= 0 && to < m_list.Count)
            {
                Change(m_list[to]);
            }
        }

        /// <summary>
        /// 根据这个 TState，改变当前状态到 Tstate
        /// </summary>
        /// <typeparam name="TState">要更改为的状态的类.</typeparam>
        public virtual void Change<TState>() where TState : EntityState<T>
        {
            var type = typeof(TState);

            //如果状态存在，切换状态
            if (m_states.ContainsKey(type)) 
            {
                Change(m_states[type]);
            }
        }

        /// <summary>
        /// 根据他的实体改变到指定的状态
        /// </summary>
        /// <param name="to">更改的状态的实例.</param>
        public virtual void Change(EntityState<T> to)
        {
            if (to != null && Time.deltaTime > 0)
            {
                if (current != null)
                {
                    current.Exit(entity); //当前状态退出
                    events.onExit.Invoke(current.GetType());
                    last = current;
                }

                //进入下一个状态
                current = to;
                current.Enter(entity);
                events.onEnter.Invoke(current.GetType());
                events.onChange?.Invoke();
            }
        }
        
        /// <summary>
        /// 判断当前的状态是不是type
        /// </summary>
        /// <param name="type">The type you want to compare to.</param>
        public virtual bool IsCurrentOfType(Type type)
        {
            if (current == null)
            {
                return false;
            }

            return current.GetType() == type;
        }
        
        /// <summary>
        /// 是否有给定的状态
        /// </summary>
        /// <param name="type">The Type of the State you want to find.</param>
        public virtual bool ContainsStateOfType(Type type) => m_states.ContainsKey(type);
        //跟别的物体碰撞
        public virtual void OnContact(Collider other)
        {
            if (current != null && Time.timeScale > 0)
            {
                current.OnContact(entity, other);
            }
        }
    }
}