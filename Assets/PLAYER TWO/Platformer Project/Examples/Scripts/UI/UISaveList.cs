using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Save Card")]
    public class UISaveList : MonoBehaviour
    {
        public bool focusFirstElement = true;//第一个元素做特殊处理
        public UISaveCard card;//槽位
        public RectTransform container;//父节点

        protected List<UISaveCard> m_cardList = new List<UISaveCard>();//所有的存档点

        
        protected virtual void Awake()
        {
            //加载所有的数据
            var data = GameSaver.instance.LoadList();

            for (int i = 0; i < data.Length; i++)
            {
                m_cardList.Add(Instantiate(this.card, container));//往存档点加入
                m_cardList[i].Fill(i, data[i]);//填充数据
            }

            if (focusFirstElement)//设置按钮的选中
            {
                if (m_cardList[0].isFilled)
                {
                    //聚焦
                    //EventSystem.current.SetSelectedGameObject(m_cardList[0].loadButton.gameObject);
                }
                else
                {
                    //EventSystem.current.SetSelectedGameObject(m_cardList[0].newGameButton.gameObject);
                }
            }
        }
    }
}