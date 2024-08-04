using UnityEngine;
using System.Collections;
//监听玩家的跳跃，旋转板子
namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Grid Platform")]
    public class GridPlatform : MonoBehaviour
    {
        public Transform platform;//平台
        public float rotationDuration = 0.5f;

        protected bool m_clockwise = true;//控制来回旋转变动

        public virtual void Move()
        {
            StopAllCoroutines();
            StartCoroutine(MoveRoutine());
        }
        //转动板子
        protected IEnumerator MoveRoutine()
        {
            var elapsedTime = 0f;
            var from = platform.localRotation;
            var to = Quaternion.Euler(0, 0, m_clockwise ? 180 : 0);
            m_clockwise = !m_clockwise;

            //插值旋转板子到指定位置
            while (elapsedTime < rotationDuration)
            {
                elapsedTime += Time.deltaTime;
                platform.localRotation = Quaternion.Lerp(from, to, elapsedTime / rotationDuration);
                yield return null;
            }

            platform.localRotation = to;
        }

        protected virtual void Start()
        {
            FindObjectOfType<Player>().playerEvents.OnJump.AddListener(Move);
        }
    }
}