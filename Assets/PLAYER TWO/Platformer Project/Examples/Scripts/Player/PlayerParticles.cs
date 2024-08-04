using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Player))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Particles")]
	public class PlayerParticles : MonoBehaviour
	{
		public float walkDustMinSpeed = 3.5f;//走路的速度 > 这个 才有走路特效
		public float landingParticleMinSpeed = 5f; //竖直速度 > 这个 才算是落地

		public ParticleSystem walkDust;//走路
		public ParticleSystem landDust;//落地
		public ParticleSystem hurtDust;//受伤
		public ParticleSystem dashDust;
		public ParticleSystem speedTrails;
		public ParticleSystem grindTrails;

		protected Player m_player;

		/// <summary>
		/// 开始播放特效
		/// </summary>
		/// <param name="particle">The particle you want to play.</param>
		public virtual void Play(ParticleSystem particle)
		{
			if (!particle.isPlaying)
			{
				particle.Play();
			}
		}

		/// <summary>
		/// 停止特效
		/// </summary>
		/// <param name="particle">The particle you want to stop.</param>
		public virtual void Stop(ParticleSystem particle, bool clear = false)
		{
			if (particle.isPlaying)
			{
				//停止的方式
				var mode = clear ? ParticleSystemStopBehavior.StopEmittingAndClear :
					ParticleSystemStopBehavior.StopEmitting;
				particle.Stop(true, mode);
			}
		}
		/// <summary>
		/// 走路特效
		/// </summary>
		protected virtual void HandleWalkParticle()
		{
			if (m_player.isGrounded && !m_player.onRails && !m_player.onWater)
			{
				if (m_player.lateralVelocity.magnitude > walkDustMinSpeed)
				{
					Play(walkDust);
				}
				else
				{
					Stop(walkDust);
				}
			}
			else
			{
				Stop(walkDust);
			}
		}

		
		/// <summary>
		/// 落地特效
		/// </summary>
		protected virtual void HandleLandParticle()
		{
			if (!m_player.onWater &&
				Mathf.Abs(m_player.velocity.y) >= landingParticleMinSpeed)
			{
				Play(landDust);
			}
		}
		//受到攻击
		protected virtual void HandleHurtParticle() => Play(hurtDust);

		protected virtual void OnDashStarted()
		{
			Play(dashDust);
			Play(speedTrails);
		}

		protected virtual void Start()
		{
			m_player = GetComponent<Player>();
			m_player.entityEvents.OnGroundEnter.AddListener(HandleLandParticle);
			m_player.playerEvents.OnHurt.AddListener(HandleHurtParticle);
			m_player.playerEvents.OnDashStarted.AddListener(OnDashStarted);
		}

		protected virtual void Update()
		{
			HandleWalkParticle();
		}
	}
}