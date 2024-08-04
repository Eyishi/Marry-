using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class LedgeClimbingPlayerState : PlayerState
    {
        protected IEnumerator m_routine;
        protected override void OnEnter(Player player)
        {
            m_routine = SetPositionRoutine(player);
            player.StartCoroutine(m_routine);
            
        }

        protected override void OnExit(Player player)
        {
            player.ResetSkinParent();
            player.StopCoroutine(m_routine);
        }

        protected override void OnStep(Player player)
        {
        }
        
        protected virtual IEnumerator SetPositionRoutine(Player player)
        {
            var elapsedTime = 0f; //持续时间
            var totalDuration = player.stats.current.ledgeClimbingDuration;//当前消耗时间
            var halfDuration = totalDuration / 2f;//半程时间

            var initialPosition = player.transform.localPosition;
            var targetVerticalPosition = player.transform.position + Vector3.up * (player.height + 
                Physics.defaultContactOffset);
            var targetLateralPosition = targetVerticalPosition + player.transform.forward * (player.radius * 2f);

            if (player.transform.parent != null)
            {
                //世界坐标转为局部坐标
                targetVerticalPosition = player.transform.parent.InverseTransformPoint(targetVerticalPosition);
                targetLateralPosition = player.transform.parent.InverseTransformPoint(targetLateralPosition);
            }

            player.SetSkinParent(player.transform.parent);
            player.skin.position += player.transform.rotation * player.stats.current.ledgeClimbingSkinOffset;

            //攀爬校验
            while (elapsedTime <= halfDuration)
            {
                elapsedTime += Time.deltaTime;
                player.transform.localPosition = Vector3.Lerp(initialPosition, 
                    targetVerticalPosition, elapsedTime / halfDuration);
                yield return null;
            }

            elapsedTime = 0;
            player.transform.localPosition = targetVerticalPosition;

            while (elapsedTime <= halfDuration)
            {
                elapsedTime += Time.deltaTime;
                player.transform.localPosition = Vector3.Lerp(targetVerticalPosition, 
                    targetLateralPosition, elapsedTime / halfDuration);
                yield return null;
            }

            player.transform.localPosition = targetLateralPosition;
            player.states.Change<IdlePlayerState>();
        }
        public override void OnContact(Player entity, Collider other)
        {
        }
    }
}