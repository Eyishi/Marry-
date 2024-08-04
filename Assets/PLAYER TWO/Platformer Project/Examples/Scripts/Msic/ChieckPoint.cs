using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Checkpoint")]
    public class Checkpoint : MonoBehaviour
    {
        public Transform respawn; //重生点
        public AudioClip clip;

        /// <summary>
        /// Invoked when the Checkpoint is activated.
        /// 检查点激活
        /// </summary>
        public UnityEvent OnActivate;

        protected Collider m_collider;
        protected AudioSource m_audio;

        /// <summary>
        /// Returns true if the Checkpoint is activated.
        /// 检查点是否被激活
        /// </summary>
        public bool activated { get; protected set; }

        /// <summary>
        /// Activates this Checkpoint and set Player respawn transform.
        /// 激活重生点，并使得玩家的重生位置位这个位置
        /// </summary>
        /// <param name="player">The Player you want to set the respawn.</param>
        public virtual void Activate(Player player)
        {
            if (!activated)
            {
                activated = true;
                m_audio.PlayOneShot(clip);
                player.SetRespawn(respawn.position, respawn.rotation);
                OnActivate?.Invoke();
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!activated && other.CompareTag(GameTags.Player))
            {
                if (other.TryGetComponent<Player>(out var player))
                {
                    Activate(player);
                }
            }
        }

        protected virtual void Awake()
        {
            if (!TryGetComponent(out m_audio))
            {
                m_audio = gameObject.AddComponent<AudioSource>();
            }

            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
        }
    }
}