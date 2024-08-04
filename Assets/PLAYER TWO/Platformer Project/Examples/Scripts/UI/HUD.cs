using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
    public class HUD : MonoBehaviour
    {
        public string retriesFormat = "00";
		public string coinsFormat = "000";//硬币
		public string healthFormat = "0";//生命值

		[Header("UI Elements")]
		public Text retries;
		public Text coins;
		public Text health;
		public Text timer;
		public Image[] starsImages; //星星的图

		protected Game m_game;
		protected LevelScore m_score; 
		protected Player m_player;

		protected float timerStep;
		protected static float timerRefreshRate = .1f;//刷新的速率

		/// <summary>
		/// 更新金币
		/// </summary>
		protected virtual void UpdateCoins(int value)
		{
			coins.text = value.ToString(coinsFormat);
		}

		/// <summary>
		/// Set the retries counter to a given value.
		/// </summary>
		protected virtual void UpdateRetries(int value)
		{
			retries.text = value.ToString(retriesFormat);
		}

		/// <summary>
		///	更新血量
		/// 更新当前血量
		/// </summary>
		protected virtual void UpdateHealth()
		{
			health.text = m_player.health.current.ToString(healthFormat);
		}

		/// <summary>
		/// 更新星星是否显示
		/// </summary>
		protected virtual void UpdateStars(bool[] value)
		{
			for (int i = 0; i < starsImages.Length; i++)
			{
				starsImages[i].enabled = value[i];
			}
		}

		/// <summary>
		/// Set the timer text to the Level Score time.
		/// 设置时间
		/// </summary>
		protected virtual void UpdateTimer()
		{
			timerStep += Time.deltaTime;
			//大于  这个速率才加上
			if (timerStep >= timerRefreshRate)
			{
				var time = m_score.time;
				timer.text = GameLevel.FormattedTime(m_score.time);
				timerStep = 0;
			}
		}

		/// <summary>
		/// Called to force an updated on the HUD.
		/// 初始化一下
		/// </summary>
		public virtual void Refresh()
		{
			UpdateCoins(m_score.coins);
			UpdateRetries(m_game.retries);
			UpdateHealth();
			UpdateStars(m_score.stars);
		}

		protected virtual void Awake()
		{
			m_game = Game.instance;
			m_score = LevelScore.instance;
			m_player = FindObjectOfType<Player>();
			
			//加载
			m_score.OnScoreLoaded.AddListener(() =>
			{
				m_score.OnCoinsSet?.AddListener(UpdateCoins);
				m_score.OnStarsSet?.AddListener(UpdateStars);
				m_game.OnRetriesSet?.AddListener(UpdateRetries);
				m_player.health.onChange?.AddListener(UpdateHealth);
				Refresh();
			});
		}

		protected virtual void Update() => UpdateTimer();
    }
}