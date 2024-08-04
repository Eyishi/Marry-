using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Mover")]
    public class Mover : MonoBehaviour
    {
        public Vector3 offset;
        public float duration;//持续时间
        public float resetDuration; //重置时间

        protected Vector3 m_initialPosition;

        //请求偏移
        public virtual void ApplyOffset()
        {
            StopAllCoroutines();
            StartCoroutine(ApplyOffsetRoutine(m_initialPosition, m_initialPosition + offset, duration));
        }

        public virtual void Reset()
        {
            StopAllCoroutines();
            StartCoroutine(ApplyOffsetRoutine(transform.localPosition, m_initialPosition, resetDuration));
        }
		
        //不断偏移
        protected virtual IEnumerator ApplyOffsetRoutine(Vector3 from, Vector3 to, float duration)
        {
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                var t = elapsedTime / duration;
                transform.localPosition = Vector3.Lerp(from, to, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = to;
        }

        protected virtual void Start()
        {
            m_initialPosition = transform.localPosition;
        }
    }
}