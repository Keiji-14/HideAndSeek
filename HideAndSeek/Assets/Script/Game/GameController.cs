using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// ゲーム画面の処理管理
    /// </summary>
    public class GameController : MonoBehaviourPunCallbacks
    {
        #region PrivateField
        /// <summary>ゲームが開始されたかどうかのフラグ</summary>
        private bool gameStarted = false;
        /// <summary>残り時間</summary>
        private float remainingTime;
        /// <summary>上空視点カメラ</summary>
        private Camera overheadCamera;
        /// <summary>隠れる側のプレイヤーオブジェクトの配列</summary>
        private GameObject[] hiders;
        #endregion

        #region SerializeField
        /// <summary>隠れる時の猶予時間</summary>
        [SerializeField] private float gracePeriodSeconds;
        /// <summary>ゲーム全体の制限時間</summary>
        [SerializeField] private float gameTimeSeconds;
        [Header("Player Prefab")]
        /// <summary>探す側のプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject seekerPrefab;
        /// <summary>隠れる側のプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject hiderPrefab;
        /// <summary>上空カメラのオブジェクト</summary>
        [SerializeField] private GameObject overheadCameraPrefab;
        [Header("Component")]
        /// <summary>ゲームUI</summary>
        [SerializeField] private GameUI gameUI;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            StartCoroutine(WaitForCustomProperties());
        }
        #endregion

        #region PrivateMethod
        private IEnumerator WaitForCustomProperties()
        {
            while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Role"))
            {
                yield return null;
            }

            if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
            {
                SpawnPlayer(seekerPrefab);
                StartCoroutine(GracePeriodSeekerCoroutine());
            }
            else
            {
                SpawnPlayer(hiderPrefab);
                StartCoroutine(GracePeriodHiderCoroutine());
            }
        }

        /// <summary>
        /// プレイヤーオブジェクトを生成する処理
        /// </summary>
        /// <param name="prefab">生成するプレイヤーオブジェクト</param>
        private void SpawnPlayer(GameObject prefab)
        {
            if (!HasSpawnedPlayer(PhotonNetwork.LocalPlayer))
            {
                var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);

                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;
            }
        }

        private bool HasSpawnedPlayer(Player player)
        {
            // TagObjectがnullでない場合、すでにプレイヤーが生成されていると見なす
            if (player.TagObject != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 猶予時間中の鬼側の処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator GracePeriodSeekerCoroutine()
        {
            // 上空視点カメラ
            overheadCamera = Instantiate(overheadCameraPrefab).GetComponent<Camera>();
            hiders = GameObject.FindGameObjectsWithTag("Hider");

            // 自プレイヤーのSeekerControllerを取得して移動を無効にする
            GameObject playerObject = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            if (playerObject != null)
            {
                SeekerController seekerController = playerObject.GetComponent<SeekerController>();
                if (seekerController != null)
                {
                    seekerController.enabled = false;
                }
            }

            // 隠れる側のプレイヤーを見えなくする
            foreach (var hider in hiders)
            {
                hider.GetComponent<Renderer>().enabled = false;
            }

            // 猶予時間中のカウントダウン表示
            remainingTime = gracePeriodSeconds;
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                gameUI.UpdateTimer(remainingTime);
                yield return null;
            }

            // 上空視点カメラを削除し、隠れる側のプレイヤーを見えるようにする
            Destroy(overheadCamera.gameObject);
            foreach (var hider in hiders)
            {
                hider.GetComponent<Renderer>().enabled = true;
            }

            // 自プレイヤーのSeekerControllerを有効にする
            if (playerObject != null)
            {
                SeekerController seekerController = playerObject.GetComponent<SeekerController>();
                if (seekerController != null)
                {
                    seekerController.enabled = true;
                }
            }

            // ゲーム開始
            StartGameTimer();
        }

        /// <summary>
        /// 猶予時間中の隠れる側の処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator GracePeriodHiderCoroutine()
        {
            remainingTime = gracePeriodSeconds;
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                gameUI.UpdateTimer(remainingTime);
                yield return null;
            }

            // ゲーム開始
            StartGameTimer();
        }

        /// <summary>
        /// ゲーム開始後のタイマー処理を行う
        /// </summary>
        private void StartGameTimer()
        {
            gameStarted = true;
            remainingTime = gameTimeSeconds;
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (gameStarted)
                {
                    remainingTime -= Time.deltaTime;
                    gameUI.UpdateTimer(remainingTime);
                    if (remainingTime <= 0)
                    {
                        gameStarted = false;
                        GameOver(false);
                    }
                }
            }).AddTo(this);
        }

        /// <summary>
        /// プレイヤーが捕まった時の処理
        /// </summary>
        public void OnPlayerCaught()
        {
            if (gameStarted)
            {
                gameStarted = false;
                GameOver(true);
            }
        }

        /// <summary>
        /// ゲームオーバー時の処理
        /// </summary>
        /// <param name="isSeekerWin">鬼の勝利かどうかのフラグ</param>
        private void GameOver(bool isSeekerWin)
        {
            // ゲーム終了処理
            if (isSeekerWin)
            {
                Debug.Log("Seeker Wins!");
            }
            else
            {
                Debug.Log("Hiders Win!");
            }

            // ここでシーン遷移やリザルト表示を行う
        }
        #endregion

    }
}