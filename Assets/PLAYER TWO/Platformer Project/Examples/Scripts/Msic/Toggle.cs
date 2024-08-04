using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Toggle")]
    public class Toggle : MonoBehaviour
    {
        public bool state = true; //默认
        public float delay;
        public Toggle[] multiTrigger;

        /// <summary>
        /// Called when the Toggle is activated.
        /// 开启
        /// </summary>
        public UnityEvent onActivate;

        /// <summary>
        /// Called when the Toggle is deactivated.
        /// 关闭
        /// </summary>
        public UnityEvent onDeactivate;

        /// <summary>
        /// Sets the state of the Toggle.
        /// 设置开关
        /// </summary>
        /// <param name="value">The state you want to set.</param>
        public virtual void Set(bool value)
        {
            StopAllCoroutines();
            StartCoroutine(SetRoutine(value));
        }
        //设置火焰
        protected virtual IEnumerator SetRoutine(bool value)
        {
            yield return new WaitForSeconds(delay);

            if (value)
            {
                if (!state)
                {
                    state = true;

                    foreach (var toggle in multiTrigger)
                    {
                        toggle.Set(state);
                    }

                    onActivate?.Invoke();
                }
            }
            else if (state)
            {
                state = false;

                foreach (var toggle in multiTrigger)
                {
                    toggle.Set(state);
                }

                onDeactivate?.Invoke();
            }
        }
    }
}