using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Pauser")]
    public class LevelPauser : Singleton<LevelPauser>
    {
        /// <summary>
        /// 暂停事件
        /// </summary>
        public UnityEvent OnPause;

        /// <summary>
        /// 取消暂停事件
        /// </summary>
        public UnityEvent OnUnpause;

        public UIAnimator pauseScreen;

        /// <summary>
        /// 能不能暂停
        /// </summary>
        public bool canPause { get; set; }

        /// <summary>
        /// 是不是暂停
        /// </summary>
        public bool paused { get; protected set; }

        /// <summary>
        /// 根据传进来的值确定是不是暂停（暂停或解除暂停）
        /// </summary>
        /// <param name="value">The state you want to set the pause to.</param>
        public virtual void Pause(bool value)
        {
            if (paused != value)
            {
                if (!paused)
                {
                    if (canPause)
                    {
                        //出现鼠标
                        Game.LockCursor(false);
                        paused = true;
                        Time.timeScale = 0;
                        pauseScreen.SetActive(true);
                        pauseScreen?.Show();
                        OnPause?.Invoke();
                    }
                }
                else
                {
                    //禁用鼠标
                    Game.LockCursor();
                    paused = false;
                    Time.timeScale = 1;
                    pauseScreen?.Hide();
                    OnUnpause?.Invoke();
                }
            }
        }
    }
}