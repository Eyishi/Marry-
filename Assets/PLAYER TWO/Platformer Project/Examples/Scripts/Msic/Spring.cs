using System;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class Spring : MonoBehaviour,IEntityContact
    {
        
        public float force = 25f;//力
        public AudioClip clip;

        protected AudioSource m_audio;
        protected Collider m_collider;
        private void Start()
        {
            tag = GameTags.Spring;
            m_collider = GetComponent<Collider>();

            if (!TryGetComponent(out m_audio))
            {
                m_audio = gameObject.AddComponent<AudioSource>();
            }
        }

        public void OnEntityContact(Entity entity)
        {
            if (entity.IsPointUnderStep(m_collider.bounds.max) &&
                entity is Player player && player.isAlive)
            {
                ApplyForce(player);
                player.SetJumps(1);//算是跳一次
                player.ResetAirSpins();
                player.ResetAirDash();
                player.states.Change<FallPlayerState>();
            }
        }
        /// <summary>
        /// 应用弹簧力
        /// </summary>
        /// <param name="player">The Player you want to apply force to.</param>
        public void ApplyForce(Player player)
        {
            if (player.verticalVelocity.y <= 0)
            {
                m_audio.PlayOneShot(clip);
                player.verticalVelocity = Vector3.up * force;
            }
        }
    }
}