using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(BoxCollider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Item Box")]
    public class ItemBox : MonoBehaviour, IEntityContact
    {
        public Collectable[] collectables;//一个箱子所拥有的金币数
        public MeshRenderer itemBoxRenderer;
        public Material emptyItemBoxMaterial;

        [Space(15)]
        public UnityEvent onCollect;
        public UnityEvent onDisable;

        protected int m_index;//金币下标
        protected bool m_enabled = true; //是否开启
        protected Vector3 m_initialScale;

        protected BoxCollider m_collider;

        protected virtual void InitializeCollectables()
        {
            foreach (var collectable in collectables)
            {
                if (!collectable.hidden)
                {
                    collectable.gameObject.SetActive(false);
                }
                else
                {
                    collectable.collectOnContact = false;
                }
            }
        }
        //收集金币
        public virtual void Collect(Player player)
        {
            if (m_enabled)
            {
                if (m_index < collectables.Length)
                {
                    if (collectables[m_index].hidden)
                    {
                        collectables[m_index].Collect(player);
                    }
                    else
                    {
                        collectables[m_index].gameObject.SetActive(true);
                    }

                    m_index = Mathf.Clamp(m_index + 1, 0, collectables.Length);
                    onCollect?.Invoke();
                }

                if (m_index == collectables.Length)
                {
                    Disable();
                }
            }
        }

        //禁用
        public virtual void Disable()
        {
            if (m_enabled)
            {
                m_enabled = false;
                itemBoxRenderer.sharedMaterial = emptyItemBoxMaterial;
                onDisable?.Invoke();
            }
        }

        protected virtual void Start()
        {
            m_collider = GetComponent<BoxCollider>();
            m_initialScale = transform.localScale;
            InitializeCollectables();
        }
		
        public void OnEntityContact(Entity entity)
        {
            if (entity is Player player)
            {
                if (entity.velocity.y > 0 &&
                    entity.position.y < m_collider.bounds.min.y) //是否碰撞到
                {
                    Collect(player);//处理收集金币
                }
            }
        }
    }
}