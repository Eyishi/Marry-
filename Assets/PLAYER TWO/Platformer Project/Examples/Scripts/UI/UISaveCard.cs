using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//挂再卡片上  单个卡片
namespace PLAYERTWO.PlatformerProject
{
    public class UISaveCard : MonoBehaviour
    {
        public string nextScene; //下一个场景

        [Header("Text Formatting")]
        public string retriesFormat = "00";
        public string starsFormat = "00";
        public string coinsFormat = "000";
        public string dateFormat = "MM/dd/y hh:mm";
        
        [Header("Containers")]
        public GameObject dataContainer;//有数据的
        public GameObject emptyContainer;//没有数据的 (empty)

        [Header("UI Elements")]
        public Text retries;
        public Text stars;
        public Text coins;
        public Text createdAt;
        public Text updatedAt;
        public Button loadButton;
        public Button deleteButton;
        public Button newGameButton;
        
        protected int m_index;
        protected GameData m_data;

        //是否填充
        public bool isFilled { get; protected set; }

        //加载关卡
        public virtual void Load()
        {
            //切状态机
            Game.instance.LoadState(m_index, m_data);
            //加载下一个场景
            GameLoader.instance.Load(nextScene);
        }
        //删除事件
        public virtual void Delete()
        {
            GameSaver.instance.Delete(m_index);
            Fill(m_index, null);
            EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
        }
		
        //创建一个新的存档
        public virtual void Create()
        {
            var data = GameData.Create();
            //存储起来
            GameSaver.instance.Save(data, m_index);
            //填充一下
            Fill(m_index, data);
            EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
        }

        //把对应的存档数据填充进槽位
        public virtual void Fill(int index, GameData data)
        {
            m_index = index;
            isFilled = data != null;
            //按钮是否显示
            dataContainer.SetActive(isFilled);
            emptyContainer.SetActive(!isFilled);
            loadButton.interactable = isFilled;
            deleteButton.interactable = isFilled;
            newGameButton.interactable = !isFilled;

            if (data != null)
            {
                m_data = data;
                retries.text = data.retries.ToString(retriesFormat);
                stars.text = data.TotalStars().ToString(starsFormat);
                coins.text = data.TotalCoins().ToString(coinsFormat);
                createdAt.text = DateTime.Parse(data.createdAt).ToLocalTime().ToString(dateFormat);
                updatedAt.text = DateTime.Parse(data.updatedAt).ToLocalTime().ToString(dateFormat);
            }
        }

        protected virtual void Start()
        {
            loadButton.onClick.AddListener(Load);
            deleteButton.onClick.AddListener(Delete);
            newGameButton.onClick.AddListener(Create);
        }
    }
}