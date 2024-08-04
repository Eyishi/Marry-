//控制游戏启动

using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class GameController :MonoBehaviour
    {
        protected Game m_game => Game.instance;
        protected GameLoader m_loader => GameLoader.instance;

        //增加生命值
        public virtual void AddRetries(int amount) => m_game.retries += amount;

        public virtual void LoadScene(string scene) => m_loader.Load(scene);
    }
}