using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene
{
    /// <summary>
    /// シーン読み込みの処理
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        #region PublicField
        /// <summary>シーンがロードされた時に発生するイベント</summary>
        public event Action<string> OnSceneLoaded;
        #endregion

        #region PrivateField
        private static SceneLoader instance = null;
        /// <summary>シーン名</summary>
        private Dictionary<SceneName, string> SceneNames = new Dictionary<SceneName, string>()
        {
            {SceneName.Title,   "Title"},
            {SceneName.Game,    "Game"},
        };
        #endregion

        #region PublicMethod
        /// <summary>シーンの種類</summary>
        public enum SceneName
        {
            Title,
            Game,
        }

        /// <summary>
        /// インスタンス化
        /// </summary>
        /// <returns></returns>
        public static SceneLoader Instance()
        {
            // オブジェクトを生成し、自身をAddCompleteして、DontDestroyに置く
            if (instance == null)
            {
                var obj = new GameObject("SceneLoader");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<SceneLoader>();
            }

            return instance;
        }

        /// <summary>
        /// シーンロード
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <param name="isAdditive">シーン追加するかどうか</param>
        public void Load(SceneName sceneName, bool isAdditive = false)
        {
            StartCoroutine(LoadAsync(SceneNames[sceneName], isAdditive));
        }

        /// <summary>
        /// PhotonNetworkを使用したシーンロード
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        public void PhotonNetworkLoad(SceneName sceneName)
        {
            StartCoroutine(PhotonNetworkLoadAsync(SceneNames[sceneName]));
        }

        /// <summary>
        /// シーンアンロード
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        public void UnLoad(SceneName sceneName)
        {
            StartCoroutine(UnLoadAsync(SceneNames[sceneName]));
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// 非同期シーンロード
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <param name="isAdditive">シーン追加するかどうか</param>
        private IEnumerator LoadAsync(string sceneName, bool isAdditive = false)
        {
            // シーンを非同期でロードする
            var loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var async = SceneManager.LoadSceneAsync(sceneName, loadMode);

            // ロードが完了するまで待機する
            while (!async.isDone)
            {
                yield return null;
            }

            // シーンが追加された後に、アクティブ状態を変更する
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            // シーンロード完了を通知する
            OnSceneLoaded?.Invoke(sceneName);
        }

        /// <summary>
        /// PhotonNetworkを使用した非同期シーンロード
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        public IEnumerator PhotonNetworkLoadAsync(string sceneName)
        {
            yield return new WaitForSeconds(3.0f);

            PhotonNetwork.LoadLevel(sceneName);

            // シーンロード完了を待機
            while (PhotonNetwork.LevelLoadingProgress < 1)
            {
                Debug.Log($"Loading progress: {PhotonNetwork.LevelLoadingProgress}");
                yield return null;
            }

            // シーンロード完了を通知する
            OnSceneLoaded?.Invoke(sceneName);
        }

        /// <summary>
        /// 非同期シーンアンロード
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        private IEnumerator UnLoadAsync(string sceneName)
        {
            // シーンを非同期でアンロードする
            var asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

            // ロードが完了するまで待機する
            while (!asyncUnload.isDone)
            {
                yield return null;
            }
        }
        #endregion
    }
}