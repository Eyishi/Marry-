using System;
using Cinemachine;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        public Player player;
        public float maxDistance = 15f;
        public float initialAngle = 20f;
        public float heightOffset = 1f;
        
        [Header("Following Settings")]
        public float verticalUpDeadZone = 0.15f;
        public float verticalDownDeadZone = 0.15f;
        public float verticalAirUpDeadZone = 4f;
        public float verticalAirDownDeadZone = 0;
        public float maxVerticalSpeed = 10f;
        public float maxAirVerticalSpeed = 100f;
        
        [Header("Orbit Settings")]
        public bool canOrbit = true;//摄像机能滑动
        public bool canOrbitWithVelocity = true;//y轴上的滑动
        public float orbitVelocityMultiplier = 5;
        
        [Range(0, 90)]
        public float verticalMaxRotation = 80;//竖直方向最大旋转

        [Range(-90, 0)]
        public float verticalMinRotation = -20;
        
        protected CinemachineVirtualCamera m_camera;
        protected Cinemachine3rdPersonFollow m_cameraBody;
        protected CinemachineBrain m_brain;
        
        protected Transform m_target;
        
        protected float m_cameraDistance;
        protected float m_cameraTargetYaw;//偏转角
        protected float m_cameraTargetPitch;//俯仰角
        
        protected Vector3 m_cameraTargetPosition;//摄像机位置
        
        protected string k_targetName = "Player Follower Camera Target";
        
        protected virtual void InitializeComponents()
        {
            if (!player)
            {
                player = FindObjectOfType<Player>();
            }

            m_camera = GetComponent<CinemachineVirtualCamera>();
            m_cameraBody = m_camera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
            m_brain = Camera.main.GetComponent<CinemachineBrain>();
        }

        protected virtual void InitializeFollower()
        {
            m_target = new GameObject(k_targetName).transform;
            m_target.position = player.transform.position;
        }
        protected virtual void InitializeCamera()
        {
            m_camera.Follow = m_target.transform;
            m_camera.LookAt = player.transform;

            Reset();
        }
        //设置（重置）一下东西，例如死亡了重置
        public virtual void Reset()
        {
            m_cameraDistance = maxDistance;
            m_cameraTargetPitch = initialAngle;
            m_cameraTargetYaw = player.transform.rotation.eulerAngles.y;
            m_cameraTargetPosition = player.unsizedPosition + Vector3.up * heightOffset;
            //相机移动
            MoveTarget();
            m_brain.ManualUpdate();
        }
        //相机的移动
        protected virtual void MoveTarget()
        {
            m_target.position = m_cameraTargetPosition;
            m_target.rotation = Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0.0f);
            m_cameraBody.CameraDistance = m_cameraDistance;//相机放在后面多远
        }
        protected void Start()
        {
            InitializeComponents();
            InitializeFollower();//跟踪
            InitializeCamera();//初始化相机
        }
        protected virtual void HandleOrbit()
        {
            //能够跟随
            if (canOrbit)
            {
                var direction = player.inputs.GetLookDirection();

                if (direction.sqrMagnitude > 0)
                {
                    var usingMouse = player.inputs.IsLookingWithMouse();
                    float deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;

                    m_cameraTargetYaw += direction.x * deltaTimeMultiplier;
                    m_cameraTargetPitch -= direction.z * deltaTimeMultiplier;
                    m_cameraTargetPitch = ClampAngle(m_cameraTargetPitch, verticalMinRotation, verticalMaxRotation);
                }
            }
        }
        //夹角
        protected virtual float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
            {
                angle += 360;
            }

            if (angle > 360)
            {
                angle -= 360;
            }

            return Mathf.Clamp(angle, min, max);
        }
        
        protected virtual void HandleVelocityOrbit()//TODO
        {
            if (canOrbitWithVelocity && player.isGrounded)
            {
                var localVelocity = m_target.InverseTransformVector(player.velocity);
                m_cameraTargetYaw += localVelocity.x * orbitVelocityMultiplier * Time.deltaTime;
            }
        }
        
        //计算偏移 TODO 
        protected virtual void HandleOffset()
        {
            var target = player.unsizedPosition + Vector3.up * heightOffset;
            var previousPosition = m_cameraTargetPosition;
            var targetHeight = previousPosition.y;

            if (player.isGrounded || VerticalFollowingStates())
            {
                if (target.y > previousPosition.y + verticalUpDeadZone)
                {
                    var offset = target.y - previousPosition.y - verticalUpDeadZone;
                    targetHeight += Mathf.Min(offset, maxVerticalSpeed * Time.deltaTime);
                }
                else if (target.y < previousPosition.y - verticalDownDeadZone)
                {
                    var offset = target.y - previousPosition.y + verticalDownDeadZone;
                    targetHeight += Mathf.Max(offset, -maxVerticalSpeed * Time.deltaTime);
                }
            }
            else if (target.y > previousPosition.y + verticalAirUpDeadZone)
            {
                var offset = target.y - previousPosition.y - verticalAirUpDeadZone;
                targetHeight += Mathf.Min(offset, maxAirVerticalSpeed * Time.deltaTime);
            }
            else if (target.y < previousPosition.y - verticalAirDownDeadZone)
            {
                var offset = target.y - previousPosition.y + verticalAirDownDeadZone;
                targetHeight += Mathf.Max(offset, -maxAirVerticalSpeed * Time.deltaTime);
            }

            m_cameraTargetPosition = new Vector3(target.x, targetHeight, target.z);
        }

        //垂直跟踪状态
        protected virtual bool VerticalFollowingStates()
        {
            // return player.states.IsCurrentOfType(typeof(SwimPlayerState)) ||
            //        player.states.IsCurrentOfType(typeof(PoleClimbingPlayerState)) ||
            //        player.states.IsCurrentOfType(typeof(WallDragPlayerState)) ||
            //        player.states.IsCurrentOfType(typeof(LedgeHangingPlayerState)) ||
            //        player.states.IsCurrentOfType(typeof(LedgeClimbingPlayerState)) ||
            //        player.states.IsCurrentOfType(typeof(RailGrindPlayerState));
            return false;
        }
        
        protected virtual void LateUpdate()
        {
            HandleOrbit();//相机跟随
            HandleVelocityOrbit();//垂直
            HandleOffset();//处理偏移
            MoveTarget();
        }
    }
}