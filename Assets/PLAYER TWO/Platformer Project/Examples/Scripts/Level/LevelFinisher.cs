using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    //退出
    [AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Finisher")]
	public class LevelFinisher : Singleton<LevelFinisher>
	{
		/// <summary>
		/// 当前关卡执行完后
		/// </summary>
		public UnityEvent OnFinish;

		/// <summary>
		/// 退出
		/// </summary>
		public UnityEvent OnExit;

		public bool unlockNextLevel; //解没解下一关
		public string nextScene;
		public string exitScene; //退出后的场景
		public float loadingDelay = 1f;

		protected Game m_game => Game.instance;
		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPauser m_pauser => LevelPauser.instance;
		protected GameLoader m_loader => GameLoader.instance;
		protected Fader m_fader => Fader.instance;

		//进入关卡
		protected virtual IEnumerator FinishRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_score.stopTime = true;
			m_level.player.inputs.enabled = false;

			yield return new WaitForSeconds(loadingDelay);

			if (unlockNextLevel)
			{
				m_game.UnlockNextLevel();
			}

			Game.LockCursor(false);
			m_score.Consolidate();
			m_loader.Load(nextScene);
			OnFinish?.Invoke();
		}

		/// <summary>
		/// 退出场景
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ExitRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			
			yield return new WaitForSeconds(loadingDelay);
			Game.LockCursor(false); //TODO
			m_loader.Load(exitScene);
			OnExit?.Invoke();
		}

		/// <summary>

		/// 调用协程合并分数并加载下一个场景
		/// </summary>
		public virtual void Finish()
		{
			StopAllCoroutines();
			StartCoroutine(FinishRoutine());
		}

		/// <summary>
		/// 退出
		/// </summary>
		public virtual void Exit()
		{
			//停掉所有的协程
			StopAllCoroutines();
			StartCoroutine(ExitRoutine());
		}
	}
}