using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Starter")]
    public class LevelStarter : Singleton<LevelStarter>
    {
        /// <summary>
        /// 开始事件
        /// </summary>
        public UnityEvent OnStart;

        public float enablePlayerDelay = 1f;

        protected Level m_level => Level.instance;
        //分
        protected LevelScore m_score => LevelScore.instance;
        protected LevelPauser m_pauser => LevelPauser.instance;

        protected Fader m_fader => Fader.instance;

        //游戏的场景刚开始加载
        protected virtual IEnumerator Routine()
        {
            //一开始先禁用玩家的输入
            Game.LockCursor();
            m_level.player.controller.enabled = false;
            m_level.player.inputs.enabled = false;
            yield return new WaitForSeconds(enablePlayerDelay);
            m_score.stopTime = false;
            m_level.player.controller.enabled = true;
            m_level.player.inputs.enabled = true;
            m_pauser.canPause = true;
            OnStart?.Invoke();
        }

        protected virtual void Start()
        {
            StartCoroutine(Routine());
        }
    }
}