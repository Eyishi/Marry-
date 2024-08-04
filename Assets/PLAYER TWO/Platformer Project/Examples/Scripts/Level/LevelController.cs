using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Controller")]
    public class LevelController : MonoBehaviour
    {
        protected LevelFinisher m_finisher => LevelFinisher.instance;
        protected LevelRespawner m_respawner => LevelRespawner.instance;
        protected LevelScore m_score => LevelScore.instance;
        protected LevelPauser m_pauser => LevelPauser.instance;

        public virtual void Finish() => m_finisher.Finish();//进入这个场景
        public virtual void Exit() => m_finisher.Exit();//退出这个场景

        public virtual void Respawn(bool consumeRetries) => m_respawner.Respawn(consumeRetries);//重生
        public virtual void Restart() => m_respawner.Restart();//重开

        public virtual void AddCoins(int amount) => m_score.coins += amount;
        public virtual void CollectStar(int index) => m_score.CollectStar(index);
        public virtual void ConsolidateScore() => m_score.Consolidate();

        public virtual void Pause(bool value) => m_pauser.Pause(value);
    }
}