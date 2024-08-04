using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Player))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Footsteps")]
   public class PlayerFootsteps : MonoBehaviour
	{
		[System.Serializable]
		//不同物体对应的声音
		public class Surface
		{
			public string tag;
			public AudioClip[] footsteps;//脚步声
			public AudioClip[] landings;//地面上的声音
		}

		public Surface[] surfaces;
		public AudioClip[] defaultFootsteps;
		public AudioClip[] defaultLandings;

		[Header("General Settings")]
		public float stepOffset = 1.25f; //步长，多少播放一次声音
		public float footstepVolume = 0.5f;

		protected Vector3 m_lastLateralPosition;//上一个脚步
		protected Dictionary<string, AudioClip[]> m_footsteps = new Dictionary<string, AudioClip[]>();
		protected Dictionary<string, AudioClip[]> m_landings = new Dictionary<string, AudioClip[]>();

		protected Player m_player;
		protected AudioSource m_audio;

		protected virtual void PlayRandomClip(AudioClip[] clips)
		{
			if (clips.Length > 0)
			{
				var index = Random.Range(0, clips.Length);
				m_audio.PlayOneShot(clips[index], footstepVolume);
			}
		}

		//播放撞击地面的声音
		protected virtual void Landing()
		{
			if (!m_player.onWater)
			{
				if (m_landings.ContainsKey(m_player.groundHit.collider.tag)) //如果撞击了地面
				{
					PlayRandomClip(m_landings[m_player.groundHit.collider.tag]);
				}
				else
				{
					PlayRandomClip(defaultLandings);
				}
			}
		}

		protected virtual void Start()
		{
			m_player = GetComponent<Player>();
			m_player.entityEvents.OnGroundEnter.AddListener(Landing);

			if (!TryGetComponent(out m_audio))
			{
				m_audio = gameObject.AddComponent<AudioSource>();
			}

			foreach (var surface in surfaces)
			{
				m_footsteps.Add(surface.tag, surface.footsteps);
				m_landings.Add(surface.tag, surface.landings);
			}
		}

		//脚步的声音
		protected virtual void Update()
		{
			if (m_player.isGrounded && m_player.states.IsCurrentOfType(typeof(WalkPlayerState)))
			{
				var position = transform.position;
				var lateralPosition = new Vector3(position.x, 0, position.z);
				var distance = (m_lastLateralPosition - lateralPosition).magnitude;
				
				//每隔 一段距离才播放音效
				if (distance >= stepOffset)
				{
					if (m_footsteps.ContainsKey(m_player.groundHit.collider.tag))
					{
						PlayRandomClip(m_footsteps[m_player.groundHit.collider.tag]);
					}
					else
					{
						PlayRandomClip(defaultFootsteps);
					}

					m_lastLateralPosition = lateralPosition;
				}
			}
		}
	}
}