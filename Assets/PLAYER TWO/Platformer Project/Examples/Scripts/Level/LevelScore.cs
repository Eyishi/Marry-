using System;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
   [AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Score")]
	public class LevelScore : Singleton<LevelScore>
	{
		/// <summary>
		/// 硬币数量发生变化时
		/// </summary>
		public UnityEvent<int> OnCoinsSet;

		/// <summary>
		/// 收集的星发生变化时
		/// </summary>
		public UnityEvent<bool[]> OnStarsSet;

		
		/// <summary>
		/// 关卡数据加载完后调用
		/// </summary>
		public UnityEvent OnScoreLoaded;

		/// <summary>
		/// 金币数
		/// </summary>
		public int coins
		{
			get { return m_coins; }

			set
			{
				m_coins = value;
				OnCoinsSet?.Invoke(m_coins);
			}
		}

		/// <summary>
		/// 星星数组
		/// </summary>
		public bool[] stars => (bool[])m_stars.Clone();

		/// <summary>
		/// 经过的时间
		/// </summary>
		public float time { get; protected set; }

		/// <summary>
		/// Returns true if the time counter should be updating.
		/// 时间应不应该更新
		/// </summary>
		public bool stopTime { get; set; } = true;

		protected int m_coins;
		protected bool[] m_stars = new bool[GameLevel.StarsPerLevel];

		protected Game m_game;
		protected GameLevel m_level;

		/// <summary>
		/// 重置
		/// </summary>
		public virtual void Reset()
		{
			time = 0;
			coins = 0;

			if (m_level != null)
			{
				m_stars = (bool[])m_level.stars.Clone();
			}
		}

		/// <summary>
		/// Collect a given star from the Stars array.
		/// </summary>
		/// <param name="index">The index of the Star you want to collect.</param>
		public virtual void CollectStar(int index)
		{
			m_stars[index] = true;
			OnStarsSet?.Invoke(m_stars);
		}

		/// <summary>
		/// 将当前关卡中的数据复制到新的关卡中。
		/// </summary>
		public virtual void Consolidate()
		{
			if (m_level != null)
			{
				if (m_level.time == 0 || time < m_level.time)
				{
					m_level.time = time;
				}

				if (coins > m_level.coins)
				{
					m_level.coins = coins;
				}
				
				m_level.stars = (bool[])stars.Clone();
				m_game.RequestSaving();
			}
		}

		protected virtual void Start()
		{
			m_game = Game.instance;
			m_level = m_game?.GetCurrentLevel();//获取当前所在场景

			if (m_level != null)
			{
				m_stars = (bool[])m_level.stars.Clone();
			}

			OnScoreLoaded?.Invoke();
		}
		//更新时间
		protected virtual void Update()
		{
			if (!stopTime)
			{
				time += Time.deltaTime;
			}
		}
	}
}