using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Gravity Field")]
    public class GravityField : MonoBehaviour
    {
        public float force = 75f; //力的大小

        protected Collider m_collider;

        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true; 
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTags.Player))
            {
                if (other.TryGetComponent<Player>(out var player))
                {
                    if (player.isGrounded)//在地上的时候加上力
                    {
                        player.verticalVelocity = Vector3.zero;
                    }

                    player.velocity += transform.up * force * Time.deltaTime;
                }
            }
        }
    }
}