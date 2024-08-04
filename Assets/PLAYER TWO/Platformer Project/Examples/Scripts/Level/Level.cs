namespace PLAYERTWO.PlatformerProject
{
    public class Level:Singleton<Level>
    {
        protected Player m_player;

        /// <summary>
        /// 获取玩家
        /// </summary>
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
    }
}