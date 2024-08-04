using System;
using System.Linq;
using UnityEngine;

//游戏里的数据
namespace PLAYERTWO.PlatformerProject
{
    [Serializable]
    public class GameData
    {
        //游戏的数据
        public int retries;
        public LevelData[] levels;
        public string createdAt;
        public string updatedAt;

        /// <summary>
        /// Returns a new instance of Game Data at runtime.
        /// 获取游戏数据的实例
        /// </summary>
        public static GameData Create()
        {
            return new GameData()
            {
                retries = Game.instance.initialRetries,
                createdAt = DateTime.UtcNow.ToString(),
                updatedAt = DateTime.UtcNow.ToString(),
                //将序列中的每个元素投影到新表单。
                levels = Game.instance.levels.Select((level) =>
                {
                    return new LevelData()
                    {
                        locked = level.locked
                    };
                }).ToArray()
            };
        }

        /// <summary>
        /// Returns the sum of Stars collected in all Levels.
        /// </summary>
        public virtual int TotalStars()
        {
            return levels.Aggregate(0, (acc, level) =>
            {
                var total = level.CollectedStars();
                return acc + total;
            });
        }

        /// <summary>
        /// Returns the sum of Coins collected in all levels.
        /// </summary>
        /// <returns></returns>
        public virtual int TotalCoins()
        {
            return levels.Aggregate(0, (acc, level) => acc + level.coins);
        }
        //转成json
        public virtual string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        //把读取到的json转成GameData对象
        public static GameData FromJson(string json)
        {
            return JsonUtility.FromJson<GameData>(json);
        }
    }
    
}