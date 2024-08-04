using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class PlayerController : MonoBehaviour
    {
        protected Player m_player;

        public Player player
        {
            get
            {
                if (!m_player)
                {
                    m_player = FindObjectOfType<Player>();
                }
                return m_player;
            }
        }
        
        /// <summary>
        /// 增加血量值
        /// </summary>
        /// <param name="player">The Player instance.</param>
        public void AddHealth() => AddHealth( 1);

        /// <summary>
        /// 增加血量值
        /// </summary>
        /// <param name="player">The Player instance.</param>
        /// <param name="amount">The amount of health.</param>
        public void AddHealth( int amount) => player.health.Increase(amount);
    }
}