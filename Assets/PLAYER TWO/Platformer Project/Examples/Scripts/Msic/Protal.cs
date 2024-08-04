using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(Collider), typeof(AudioSource))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Portal")]
    public class Protal : MonoBehaviour
    {
        public bool useFlash = true;//闪屏
        public Protal exit;//退出的门
        public float exitOffset = 1f;//一些偏移，不需要直接落在门那
        public AudioClip teleportClip;

        protected Collider m_collider;
        protected AudioSource m_audio;

        protected PlayerCamera m_camera;

        public Vector3 position => transform.position;
        public Vector3 forward => transform.forward;

        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            m_audio = GetComponent<AudioSource>();
            m_camera = FindObjectOfType<PlayerCamera>();
            m_collider.isTrigger = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (exit && other.TryGetComponent(out Player player))
            {
                var yOffset = player.unsizedPosition.y - transform.position.y;//y的偏移

                player.transform.position = exit.position + Vector3.up * yOffset;
                player.FaceDirection(exit.forward);
                m_camera.Reset();//重置一下，否则这一帧有错误

                var inputDirection = player.inputs.GetMovementCameraDirection();

                if (Vector3.Dot(inputDirection, exit.forward) < 0)
                {
                    player.FaceDirection(-exit.forward);
                }

                player.transform.position += player.transform.forward * exit.exitOffset;//往前偏移
                player.lateralVelocity = player.transform.forward * player.lateralVelocity.magnitude;

                if (useFlash)
                {
                    Flash.instance?.Trigger();
                }

                m_audio.PlayOneShot(teleportClip);
            }
        }
    }
}