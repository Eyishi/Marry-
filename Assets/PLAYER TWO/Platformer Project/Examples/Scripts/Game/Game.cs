using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game")]
	public class Game : Singleton<Game>
	{
		/// <summary>
		/// 生命值变化
		/// </summary>
		public UnityEvent<int> OnRetriesSet;

		/// <summary>
		/// Called when a saving has been requested.
		/// 请求保存
		/// </summary>
		public UnityEvent OnSavingRequested;

		public int initialRetries = 3;
		public List<GameLevel> levels; //当前存档的关卡

		protected int m_retries;
		protected int m_dataIndex;
		protected DateTime m_createdAt;
		protected DateTime m_updatedAt;

		/// <summary>
		/// 人物的生命值
		/// </summary>
		public int retries
		{
			get { return m_retries; }

			set
			{
				m_retries = value;
				OnRetriesSet?.Invoke(m_retries);
			}
		}

		/// <summary>
		/// 设置光标的锁定与解锁
		/// </summary>
		/// <param name="value">If true, the cursor will be hidden.</param>
		public static void LockCursor(bool value = true)
		{
#if UNITY_STANDALONE || UNITY_WEBGL   //平台的判定
			Cursor.visible = value;
			Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif
		}

		/// <summary>
		/// 加载游戏状态
		/// </summary>
		/// <param name="index">The index of the Game Data.</param>
		/// <param name="data">The Game Data to read the state from.</param>
		public virtual void LoadState(int index, GameData data)
		{
			m_dataIndex = index;
			m_retries = data.retries;
			m_createdAt = DateTime.Parse(data.createdAt);
			m_updatedAt = DateTime.Parse(data.updatedAt);
			
			//每个关卡
			for (int i = 0; i < data.levels.Length; i++)
			{
				levels[i].LoadState(data.levels[i]);
			}
		}

		/// <summary>
		/// Returns the Game Level array as Level Data.
		/// 返回游戏关卡数组转成的关卡数组
		/// </summary>
		public virtual LevelData[] LevelsData()
		{
			return levels.Select(level => level.ToData()).ToArray();
		}

		/// <summary>
		/// 当前场景的关卡
		/// </summary>
		public virtual GameLevel GetCurrentLevel()
		{
			var scene = GameLoader.instance.currentScene;
			return levels.Find((level) => level.scene == scene);
		}

		/// <summary>
		/// Returns the index from the levels list of the current scene.
		/// 当前关卡的索引
		/// </summary>
		/// <returns></returns>
		public virtual int GetCurrentLevelIndex()
		{
			var scene = GameLoader.instance.currentScene;
			return levels.FindIndex((level) => level.scene == scene);
		}

		/// <summary>
		/// 把游戏数据保存到当前索引
		/// </summary>
		public virtual void RequestSaving()
		{
			GameSaver.instance.Save(ToData(), m_dataIndex);
			OnSavingRequested?.Invoke();
		}

		/// <summary>
		/// 解锁场景名
		/// </summary>
		/// <param name="sceneName">The scene name of the level you want to unlock.</param>
		public virtual void UnlockLevelBySceneName(string sceneName)
		{
			var level = levels.Find((level) => level.scene == sceneName);

			if (level != null)
			{
				level.locked = false;
			}
		}

		/// <summary>
		/// Unlocks the next level from the levels list.
		/// 解锁下一个关卡
		/// </summary>
		public virtual void UnlockNextLevel()
		{
			var index = GetCurrentLevelIndex() + 1;

			if (index >= 0 && index < levels.Count)
			{
				levels[index].locked = false;
			}
		}

		/// <summary>
		/// 游戏数据
		/// </summary>
		public virtual GameData ToData()
		{
			return new GameData()
			{
				retries = m_retries,
				levels = LevelsData(),
				createdAt = m_createdAt.ToString(),
				updatedAt = DateTime.UtcNow.ToString()
			};
		}

		protected override void Awake()
		{
			base.Awake();
			retries = initialRetries;
			DontDestroyOnLoad(gameObject);
		}
	}
}