using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//玩家的死亡
namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Respawner")]
	public class LevelRespawner : Singleton<LevelRespawner>
	{
		/// <summary>
		/// Called after the Respawn routine ended.
		/// 重新生成
		/// </summary>
		public UnityEvent OnRespawn;

		/// <summary>
		/// Called after the Game Over routine ended.
		/// Game Over结束后
		/// </summary>
		public UnityEvent OnGameOver;

		public float respawnFadeOutDelay = 1f;//重新生成的 游戏时间(淡出)
		public float respawnFadeInDelay = 0.5f;//淡入
		public float gameOverFadeOutDelay = 5f;//game over淡出
		public float restartFadeOutDelay = 0.5f;

		protected List<PlayerCamera> m_cameras;

		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPauser m_pauser => LevelPauser.instance;
		protected Game m_game => Game.instance;
		protected Fader m_fader => Fader.instance;

		//重生的协程
		protected virtual IEnumerator RespawnRoutine(bool consumeRetries)
		{
			//是否消耗生命值
			if (consumeRetries)
			{
				m_game.retries--;
			}

			m_level.player.Respawn();
			m_score.coins = 0;
			ResetCameras();
			OnRespawn?.Invoke();

			yield return new WaitForSeconds(respawnFadeInDelay);

			m_fader.FadeIn(() =>
			{
				m_pauser.canPause = true;
				m_level.player.inputs.enabled = true;
			});
		}

		protected virtual IEnumerator GameOverRoutine()
		{
			m_score.stopTime = true;
			yield return new WaitForSeconds(gameOverFadeOutDelay);
			GameLoader.instance.Reload();
			OnGameOver?.Invoke();
		}

		protected virtual IEnumerator RestartRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			yield return new WaitForSeconds(restartFadeOutDelay);
			GameLoader.instance.Reload();
		}

		protected virtual IEnumerator Routine(bool consumeRetries)
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;

			if (consumeRetries && m_game.retries == 0)
			{
				StartCoroutine(GameOverRoutine());
				yield break;
			}

			yield return new WaitForSeconds(respawnFadeOutDelay);
			//重新生成
			m_fader.FadeOut(() => StartCoroutine(RespawnRoutine(consumeRetries)));
		}
		//重置所有相机
		protected virtual void ResetCameras()
		{
			foreach (var camera in m_cameras)
			{
				camera.Reset();
			}
		}

		/// <summary>
		/// Invokes either Respawn or Game Over routine depending of the retries available.
		/// 重新生成  
		/// </summary>
		/// <param name="consumeRetries">If true, reduces the retries counter by one or call the game over routine.
		/// 是否消费生命值</param>
		public virtual void Respawn(bool consumeRetries)
		{
			StopAllCoroutines();
			StartCoroutine(Routine(consumeRetries));
		}

		/// <summary>
		/// Restarts the current Level loading the scene again.
		/// 重新启动当前关卡，加载场景
		/// </summary>
		public virtual void Restart()
		{
			StopAllCoroutines();
			StartCoroutine(RestartRoutine());
		}

		protected virtual void Start()
		{
			m_cameras = new List<PlayerCamera>(FindObjectsOfType<PlayerCamera>());
			m_level.player.playerEvents.OnDie.AddListener(() => Respawn(true));
		}
	}
}
