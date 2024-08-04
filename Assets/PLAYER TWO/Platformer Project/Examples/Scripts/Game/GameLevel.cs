using System;
using UnityEngine;
//游戏的关卡
namespace PLAYERTWO.PlatformerProject
{
    [Serializable]
    public class GameLevel
    {
        public bool locked; //是否解锁
        public string scene; //场景名
        public string name;//名称
        public string description;//描述
        public Sprite image;//图片

        /// <summary>
        /// 金币数
        /// </summary>
        public int coins { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        /// <value></value>
        public float time { get; set; }

        /// <summary>
        /// 星星
        /// </summary>
        public bool[] stars { get; set; } = new bool[StarsPerLevel];

        /// <summary>
        /// 星星的数量
        /// </summary>
        public static readonly int StarsPerLevel = 3;

        /// <summary>
        /// 加载关卡
        /// </summary>
        /// <param name="data">The Game Data to read the state from.</param>
        public virtual void LoadState(LevelData data)
        {
            locked = data.locked;
            coins = data.coins;
            time = data.time;
            stars = data.stars;
        }

        /// <summary>
        /// 返回一个关卡数据
        /// </summary>
        public virtual LevelData ToData()
        {
            return new LevelData()
            {
                locked = this.locked,
                coins = this.coins,
                time = this.time,
                stars = this.stars
            };
        }

        /// <summary>
        /// 把 时间转成格式化的时间
        /// </summary>
        /// <param name="time">The time you want to fromat.</param>
        public static string FormattedTime(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60f);
            var seconds = Mathf.FloorToInt(time % 60f);
            var milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
            return minutes.ToString("0") + "'" + seconds.ToString("00") + "\"" + milliseconds.ToString("00");
        }
    }
}