    using System;
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    public abstract class EntityState<T> where T : Entity<T>
    {
        public UnityEvent onEnter;
        public UnityEvent onExit;

        //当前状态持续时间时间
        public float timeSinceEntered { get; protected set; }

        public void Enter(T entity)
        {
            timeSinceEntered = 0;
            onEnter?.Invoke();
            OnEnter(entity);
        }

        public void Exit(T entity)
        {
            onExit?.Invoke();
            OnExit(entity);
        }

        public void Step(T entity)
        {
            //调用state对应的Step
            OnStep(entity);
            timeSinceEntered += Time.deltaTime;
        }

        protected abstract void OnEnter(T entity);
        protected abstract void OnExit(T entity);
        protected abstract void OnStep(T entity);

        /// <summary>
        /// 返回具有给定类型名称的实体状态的新实例。通过反射，通过类型名获取实体的所有状态
        /// </summary>
        /// <param name="typeName">The type name of the Entity State class.</param>
        public static EntityState<T> CreateFromString(System.Type type)
        {
            // newTODO
            return (EntityState<T>)System.Activator
                .CreateInstance(type);
        }

        /// <summary>
        /// 根据 一个状态名数组(字符串数组)获取一个状态数组
        /// </summary>
        /// <param name="array">名字数组.</param>
        public static List<EntityState<T>> CreateListFromStringArray(string[] array)
        {
            var list = new List<EntityState<T>>();

            foreach (var typeName in array)
            {
                
                Type type = Type.GetType("PLAYERTWO.PlatformerProject."+typeName);
                list.Add(CreateFromString(type));
            }

            return list;
        }

        /// <summary>
        /// Called when the Entity is in contact with a collider.
        /// 当实体与碰撞器碰撞时调用
        /// </summary>
        public abstract void OnContact(T entity, Collider other);
    }
}