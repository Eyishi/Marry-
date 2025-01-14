﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Waypoint/Waypoint Manager")]
    public class WaypointManager: MonoBehaviour
    {
        public WaypointMode mode;
        public float waitTime; //改变导航点的时间
        public List<Transform> waypoints; //导航点

        protected Transform m_current;

        protected bool m_pong;
        protected bool m_changing;//是不是在改变导航点的过程中

        /// <summary>
        /// 返回当前导航点
        /// </summary>
        public Transform current
        {
            get
            {
                if (!m_current)
                {
                    m_current = waypoints[0];
                }

                return m_current;
            }

            protected set { m_current = value; }
        }
        
        /// <summary>
        /// Returns the index of the current Waypoint.
        /// 返回索引
        /// </summary>
        public int index => waypoints.IndexOf(current);

        /// <summary>
        /// Changes the current Waypoint to the next one based on the Waypoint Mode.
        /// 改成下一个导航点
        /// </summary>
        public virtual void Next()
        {
            if (m_changing)
            {
                return;
            }
            //两个点
            if (mode == WaypointMode.PingPong)
            {
                if (!m_pong)
                {
                    m_pong = (index + 1 == waypoints.Count);
                }
                else
                {
                    m_pong = (index - 1 >= 0);
                }

                var next = !m_pong ? index + 1 : index - 1;//因为是来回走
                StartCoroutine(Change(next));
            }
            //循坏点
            else if (mode == WaypointMode.Loop)
            {
                if (index + 1 < waypoints.Count)
                {
                    StartCoroutine(Change(index + 1));
                }
                else
                {
                    StartCoroutine(Change(0));
                }
            }
            else if (mode == WaypointMode.Once)
            {
                if (index + 1 < waypoints.Count)
                {
                    StartCoroutine(Change(index + 1));
                }
            }
        }
        //改变导航点
        protected virtual IEnumerator Change(int to)
        {
            m_changing = true;
            yield return new WaitForSeconds(waitTime);
            current = waypoints[to];
            m_changing = false;
        }
    }
}