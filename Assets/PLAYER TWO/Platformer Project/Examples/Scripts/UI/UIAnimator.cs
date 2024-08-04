using System;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Animator))]
    public class UIAnimator : MonoBehaviour
    {
        /// <summary>
        /// 显示时
        /// </summary>
        public UnityEvent OnShow;

        /// <summary>
        /// 隐藏时调用
        /// </summary>
        public UnityEvent OnHide;

        public bool hidenOnAwake;
        public string normalTrigger = "Normal";
        public string showTrigger = "Show";
        public string hideTrigger = "Hide";

        protected Animator m_animator;

        /// <summary>
        /// 触发动画
        /// </summary>
        public virtual void Show()
        {
            m_animator.SetTrigger(showTrigger);
            OnShow?.Invoke();
        }

        /// <summary>
        /// 隐藏动画
        /// </summary>
        public virtual void Hide()
        {
            m_animator.SetTrigger(hideTrigger);
            OnHide?.Invoke();
        }

        /// <summary>
        /// 设置游戏对象
        /// </summary>
        /// <param name="value">The value you want to pass.</param>
        public virtual void SetActive(bool value) => gameObject.SetActive(value);

        protected virtual void Awake()
        {
            m_animator = GetComponent<Animator>();

            if (hidenOnAwake)
            {
                m_animator.Play(hideTrigger, 0, 1);
            }
        }
    }
}