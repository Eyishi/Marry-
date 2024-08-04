using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class GameSaver : Singleton<GameSaver>
    {
        //存取类型
        public enum Mode { Binary, JSON, PlayerPrefs } //二进制，json，预制体

        public Mode mode = Mode.Binary;
        public string fileName = "save";
        public string binaryFileExtension = "data";//扩展名

        /// <summary>
        /// The amount of available slots to save data to.
        /// 槽位，就是展示的关卡
        /// </summary>
        protected static readonly int TotalSlots = 5;
        
        /// <summary>
        /// Persists a given Game Data on a disk slot.
        /// 把给定的数据保存到磁盘中
        /// </summary>
        /// <param name="data">The Game Data you want to persist.</param>
        /// <param name="index">The index of the slot.</param>
        public virtual void Save(GameData data, int index)
        {
            switch (mode)
            {
                default:
                case Mode.Binary:
                    SaveBinary(data, index);
                    break;
                case Mode.JSON:
                    SaveJSON(data, index);
                    break;
                case Mode.PlayerPrefs:
                    SavePlayerPrefs(data, index);
                    break;
            }
        }
        /// <summary>
        /// Returns the Game Data or null by reading a given slot.
        /// 加载给定槽位的数据
        /// </summary>
        /// <param name="index">The index of the slot you want to read.</param>
        public virtual GameData Load(int index)
        {
            switch (mode)
            {
                default:
                case Mode.Binary:
                    return LoadBinary(index);
                case Mode.JSON:
                    return LoadJSON(index);
                case Mode.PlayerPrefs:
                    return LoadPlayerPrefs(index);
            }
        }
        
        /// <summary>
        /// Erases the data from a slot.
        /// </summary>
        /// <param name="index">The index of the slot you want to erase.</param>
        public virtual void Delete(int index)
        {
            switch (mode)
            {
                default:
                case Mode.Binary:
                case Mode.JSON:
                    DeleteFile(index);
                    break;
                case Mode.PlayerPrefs:
                    DeletePlayerPrefs(index);
                    break;
            }
        }
        
        /// <summary>
        /// Returns an array of Game Data from all the slots.
        /// 返回游戏关卡数组
        /// </summary>
        /// <returns></returns>
        public virtual GameData[] LoadList()
        {
            var list = new GameData[TotalSlots];

            for (int i = 0; i < TotalSlots; i++)
            {
                var data = Load(i); //单个槽位的加载

                if (data != null)
                {
                    list[i] = data;
                }
            }

            return list;
        }
        //保存 二进制
        protected virtual void SaveBinary(GameData data, int index)
        {
            var path = GetFilePath(index);
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
        }
        //加载
        protected virtual GameData LoadBinary(int index)
        {
            var path = GetFilePath(index);

            if (File.Exists(path))
            {
                var formatter = new BinaryFormatter();
                var stream = new FileStream(path, FileMode.Open);
                var data = formatter.Deserialize(stream);
                stream.Close();
                return data as GameData; //转成GameData
            }

            return null;
        }

        protected virtual void SaveJSON(GameData data, int index)
        {
            var json = data.ToJson();
            var path = GetFilePath(index);
            File.WriteAllText(path, json);
        }

        protected virtual GameData LoadJSON(int index)
        {
            var path = GetFilePath(index);

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return GameData.FromJson(json);
            }

            return null;
        }

        protected virtual void DeleteFile(int index)
        {
            var path = GetFilePath(index);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        protected virtual void SavePlayerPrefs(GameData data, int index)
        {
            var json = data.ToJson();
            var key = index.ToString();
            PlayerPrefs.SetString(key, json);
        }

        protected virtual GameData LoadPlayerPrefs(int index)
        {
            var key = index.ToString();

            if (PlayerPrefs.HasKey(key))
            {
                var json = PlayerPrefs.GetString(key);
                return GameData.FromJson(json);//解析json
            }

            return null;
        }

        protected virtual void DeletePlayerPrefs(int index)
        {
            var key = index.ToString();
			
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
        //获取文件路径 //TODO
        protected virtual string GetFilePath(int index)
        {
            var extension = mode == Mode.JSON ? "json" : binaryFileExtension;
            return Application.persistentDataPath + $"/{fileName}_{index}.{extension}";
        }
    }
}