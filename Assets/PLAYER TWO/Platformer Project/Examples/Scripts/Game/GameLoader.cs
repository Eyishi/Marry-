using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PLAYERTWO.PlatformerProject
{
    public class GameLoader :Singleton<GameLoader>
    {
        /// <summary>
        /// Called when any loading proccess has started.
        /// 加载开始
        /// </summary>
        public UnityEvent OnLoadStart;

        /// <summary>
        /// Called when any loading proccess has finished.
        /// 加载完成之后调用
        /// </summary>
        public UnityEvent OnLoadFinish;

        
        public UIAnimator loadingScreen;
        
        [Header("Minimum Time")]
        public float startDelay = 1f;//开始的延迟
        public float finishDelay = 1f;//结束的延迟
    
        
        /// <summary>
        /// Returns true if there's any loading in proccess.
        /// 是否加载了场景
        /// </summary>
        public bool isLoading { get; protected set; }

        /// <summary>
        /// Returns the loading percentage.
        /// 加载进度
        /// </summary>
        public float loadingProgress { get; protected set; }

        /// <summary>
        /// Returns the name of the current scene.
        /// 当前场景的名字
        /// </summary>
        public string currentScene => SceneManager.GetActiveScene().name;
        
        /// <summary>
        /// Reloads the current scene.
        /// 重新加载当前场景
        /// </summary>
        public virtual void Reload()
        {
            StartCoroutine(LoadRoutine(currentScene));
        }
        
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="scene"></param>
        public virtual void Load(string scene)
        {
            if (!isLoading && (currentScene != scene))
            {
                StartCoroutine(LoadRoutine(scene));
            }
        }
        //异步加载场景
        protected virtual IEnumerator LoadRoutine(string scene)
        {
            OnLoadStart?.Invoke();
            isLoading = true;
            loadingScreen.SetActive(true);
            loadingScreen.Show();

            yield return new WaitForSeconds(startDelay);

            var operation = SceneManager.LoadSceneAsync(scene);
            loadingProgress = 0;

            while (!operation.isDone)
            {
                loadingProgress = operation.progress;
                yield return null;
            }

            loadingProgress = 1;

            yield return new WaitForSeconds(finishDelay);

            isLoading = false;
            loadingScreen.Hide();
            OnLoadFinish?.Invoke();
        }
    }
}