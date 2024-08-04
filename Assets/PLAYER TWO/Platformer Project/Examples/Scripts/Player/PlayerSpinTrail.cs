using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class PlayerSpinTrail : MonoBehaviour
    {
        public Transform hand; //特效位置

        protected Player m_player;
        protected TrailRenderer m_trail; //获取组件
        
        protected virtual void InitializeTrail()
        {
            m_trail = GetComponent<TrailRenderer>();
            m_trail.enabled = false;
        }

        protected virtual void InitializeTransform()
        {
            transform.parent = hand;
            transform.localPosition = Vector3.zero;//在手上
        }

        protected virtual void InitializePlayer()
        {
            m_player = GetComponentInParent<Player>();
            m_player.states.events.onChange.AddListener(HandleActive); //增加一个状态改变的 处理
        }

        protected virtual void HandleActive()
        {
            //如果现在是旋转攻击的状态机
            if (m_player.states.IsCurrentOfType(typeof(SpinPlayerState)))
            {
                m_trail.enabled = true;
            }
            else
            {
                m_trail.enabled = false;
            }
        }

        protected virtual void Start()
        {
            InitializeTrail();
            InitializeTransform();
            InitializePlayer();
        }
    }
    
    
}