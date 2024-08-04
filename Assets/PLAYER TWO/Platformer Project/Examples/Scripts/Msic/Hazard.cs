using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Hazard")]
    public class Hazard : MonoBehaviour, IEntityContact
    {
        public bool isSolid; //是不是固体
        public bool damageOnlyFromAbove;//是不是从上面伤害
        public int damage = 1;//伤害值

        protected Collider m_collider;

        protected virtual void Awake()
        {
            tag = GameTags.Hazard;
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = !isSolid;
        }

        //试图伤害
        protected virtual void TryToApplyDamageTo(Player player)
        {
            if (!damageOnlyFromAbove || player.velocity.y <= 0 &&
                player.IsPointUnderStep(m_collider.bounds.max))
            {
                player.ApplyDamage(damage, transform.position);
            }
        }

        public virtual void OnEntityContact(Entity entity)
        {
            if (entity is Player player) //如果是玩家
            {
                TryToApplyDamageTo(player);
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTags.Player))
            {
                if (other.TryGetComponent<Player>(out var player))
                {
                    TryToApplyDamageTo(player);
                }
            }
        }
    }
}