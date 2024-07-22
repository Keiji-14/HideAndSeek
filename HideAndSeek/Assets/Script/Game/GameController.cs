using GameData;
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
        /// <summary>隠れる側のプレイヤー数</summary>
        private int hiderPlayerCount;
        /// <summary>ゲームが開始されたかどうかのフラグ</summary>
        private bool gameStarted = false;
        /// <summary>サーバー時間を保持する変数</summary>
        private double startTime;
        /// <summary>残りの猶予時間</summary>
        private float graceRemainingTime;
        /// <summary>ゲームの残り時間</summary>
        private float gameRemainingTime;
        /// <summary>上空視点カメラ</summary>
        private Camera overheadCamera;
        /// <summary>隠れる側のIDリスト</summary>
        private List<int> capturedHiderIDs = new List<int>();
        /// <summary>隠れる側のプレイヤーオブジェクト</summary>
        private List<GameObject> hiderPlayerObjectList = new List<GameObject>();
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
        /// <summary>隠れる側のボットのプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject hiderBotPrefab;
        /// <summary>上空カメラのオブジェクト</summary>
        [SerializeField] private GameObject overheadCameraPrefab;
        /// <summary>上空カメラのオブジェクトステージ情報</summary>
        [SerializeField] private StageData stageData;
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
            if (PhotonNetwork.IsMasterClient)
            {
                // マスタークライアントがステージデータを生成して他のプレイヤーに共有する
                //photonView.RPC("RPC_SetStageData", RpcTarget.AllBuffered, stageData.stageID);
            }

            StartCoroutine(WaitForCustomProperties());
        }


        /// <summary>
        /// プレイヤーが捕まった時の処理
        /// </summary>
        /// <param name="hiderViewID">捕まったプレイヤーのPhotonViewID</param>
        public void OnPlayerCaught(int hiderViewID)
        {
            if (gameStarted)
            {
                if (!capturedHiderIDs.Contains(hiderViewID))
                {
                    capturedHiderIDs.Add(hiderViewID);
                    hiderPlayerCount--;

                    // 全プレイヤーに捕まったことを通知
                    photonView.RPC("RPC_OnPlayerCaught", RpcTarget.All, hiderViewID);
                    // 全プレイヤーに隠れ側の数を更新
                    photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, hiderPlayerCount);

                    Debug.Log($"Hider player count: {hiderPlayerCount}");
                }
            }
        }

        /// <summary>
        /// 鬼プレイヤーのライフが0になった時の処理
        /// </summary>
        /// <param name="seekerPlayer">鬼プレイヤー</param>
        public void SeekerFailed(GameObject seekerPlayer)
        {
            if (gameStarted)
            {
                photonView.RPC("RPC_DestroySeeker", RpcTarget.All, gameObject);
            }
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// RPCでステージデータを設定する処理
        /// </summary>
        /// <param name="stageID">ステージID</param>
        [PunRPC]
        private void RPC_SetStageData(int stageID)
        {
            // ステージオブジェクトの生成
            if (stageData != null && stageData.stageObj != null)
            {
                GameDataManager.Instance().SetStagerData(stageData);

                // ネットワーク経由でインスタンス化
                Instantiate(stageData.stageObj, Vector3.zero, Quaternion.identity);
            }
        }

        /// <summary>
        /// カスタムプロパティを待つコルーチン
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForCustomProperties()
        {
            while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Role"))
            {
                yield return null;
            }

            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString());

            gameUI.ToggleCanvas(PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString());


            /*if (PhotonNetwork.IsMasterClient)
            {
                int numberOfBots = 3; // 生成するボットの数を指定
                SpawnHiderBots(numberOfBots); // ボットを生成
            }*/

            // 鬼側か隠れる側かを判定する処理
            if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
            {
                SpawnSeekerPlayer(seekerPrefab);
                StartCoroutine(GracePeriodSeekerCoroutine());
            }
            else
            {
                SpawnHiderPlayer(hiderPrefab);
                StartCoroutine(GracePeriodHiderCoroutine());
            }
        }

        /// <summary>
        /// 鬼側のプレイヤーオブジェクトを生成する処理
        /// </summary>
        /// <param name="prefab">生成するプレイヤーオブジェクト</param>
        private void SpawnSeekerPlayer(GameObject prefab)
        {
            if (!HasSpawnedPlayer(PhotonNetwork.LocalPlayer))
            {
                var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);

                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;
            }
        }

        /// <summary>
        /// 隠れる側のプレイヤーオブジェクトを生成する処理
        /// </summary>
        /// <param name="prefab">生成するプレイヤーオブジェクト</param>
        private void SpawnHiderPlayer(GameObject prefab)
        {
            if (!HasSpawnedPlayer(PhotonNetwork.LocalPlayer))
            {
                var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;
                hiderPlayerCount++;

                photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, hiderPlayerCount);
            }
        }

        /// <summary>
        /// 隠れる側のプレイヤー数を更新する処理
        /// </summary>
        /// <param name="hiderCount">現在の隠れる側のプレイヤー数</param>
        [PunRPC]
        private void RPC_UpdateHiderCount(int hiderCount)
        {
            hiderPlayerCount = hiderCount;
            gameUI.UpdateHider(hiderCount);
        }

        /// <summary>
        /// ボットを生成する処理
        /// </summary>
        /// <param name="botNum">生成するボットの数</param>
        private void SpawnHiderBots(int botNum)
        {
            for (int i = 0; i < botNum; i++)
            {
                var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
                var botObject = PhotonNetwork.Instantiate($"Prefabs/{hiderBotPrefab.name}", position, Quaternion.identity);
                hiderPlayerCount++;
            }
            photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, hiderPlayerCount);
        }

        /// <summary>
        /// プレイヤーが既に生成されているかを判定する処理
        /// </summary>
        /// <param name="player">チェックするプレイヤー</param>
        /// <returns>生成されている場合はtrue</returns>
        private bool HasSpawnedPlayer(Player player)
        {
            return player.TagObject != null;
        }

        /// <summary>
        /// 猶予時間中の鬼側の処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator GracePeriodSeekerCoroutine()
        {
            // 上空視点カメラ
            overheadCamera = Instantiate(overheadCameraPrefab).GetComponent<Camera>();
            // プレイヤー生成待ちのため少し待つ
            yield return new WaitForSeconds(0.5f);

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

            hiderPlayerObjectList.Clear(); // リストをクリア
            var hiders = GameObject.FindGameObjectsWithTag("Hider");
            // 隠れる側のプレイヤーを見えなくする
            foreach (var hider in hiders)
            {
                // オブジェクトごと非表示にする
                SetActiveRecursively(hider, false);
                hiderPlayerObjectList.Add(hider);
            }

            graceRemainingTime = gracePeriodSeconds;
            gameRemainingTime = gameTimeSeconds;

            // MasterClientが基準時間を送信
            if (PhotonNetwork.IsMasterClient)
            {
                double masterStartTime = PhotonNetwork.Time;
                photonView.RPC("RPC_StartGracePeriod", RpcTarget.All, masterStartTime);
            }

            yield return null;
        }

        /// <summary>
        /// 猶予時間中の隠れる側の処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator GracePeriodHiderCoroutine()
        {
            // MasterClientが基準時間を送信
            if (PhotonNetwork.IsMasterClient)
            {
                double masterStartTime = PhotonNetwork.Time;
                photonView.RPC("RPC_StartGracePeriod", RpcTarget.All, masterStartTime);
            }

            yield return null;
        }

        /// <summary>
        /// RPCで猶予時間を開始する
        /// </summary>
        /// <param name="masterStartTime">マスタークライアントの開始時間</param>
        [PunRPC]
        private void RPC_StartGracePeriod(double masterStartTime)
        {
            startTime = masterStartTime;
            graceRemainingTime = gracePeriodSeconds - (float)(PhotonNetwork.Time - masterStartTime);
            gameUI.UpdateGameTimer(graceRemainingTime);

            Observable.EveryUpdate().Subscribe(_ =>
            {
                graceRemainingTime = gracePeriodSeconds - (float)(PhotonNetwork.Time - startTime);
                gameUI.UpdateGraceTimer(graceRemainingTime);
                if (graceRemainingTime <= 0)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("RPC_StartGame", RpcTarget.All, startTime);
                    }
                }
            }).AddTo(this);
        }

        /// <summary>
        /// RPCでゲームを開始する
        /// </summary>
        /// <param name="masterStartTime">マスタークライアントの開始時間</param>
        [PunRPC]
        private void RPC_StartGame(double masterStartTime)
        {
            // ゲーム開始
            StartGameTimer(masterStartTime);
        }

        /// <summary>
        /// ゲーム開始後のタイマー処理を行う
        /// </summary>
        /// <param name="masterStartTime">マスタークライアントの開始時間</param>
        private void StartGameTimer(double masterStartTime)
        {
            if (overheadCamera != null)
            {
                Destroy(overheadCamera.gameObject);

                // 自プレイヤーのSeekerControllerを有効にする
                GameObject playerObject = PhotonNetwork.LocalPlayer.TagObject as GameObject;
                if (playerObject != null)
                {
                    SeekerController seekerController = playerObject.GetComponent<SeekerController>();
                    if (seekerController != null)
                    {
                        seekerController.enabled = true;
                    }
                }

                // 猶予時間終了後の処理
                foreach (var hider in hiderPlayerObjectList)
                {
                    // オブジェクトを再表示
                    SetActiveRecursively(hider, true);
                    var hiderController = hider.GetComponent<HiderController>();

                    if (hiderController != null)
                    {
                        hiderController.SetCamera();
                    }
                }
            }

            gameStarted = true;
            startTime = masterStartTime;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (gameStarted)
                {
                    gameRemainingTime = gameTimeSeconds - (float)(PhotonNetwork.Time - (startTime + gracePeriodSeconds));
                    gameUI.UpdateGameTimer(gameRemainingTime);
                    if (gameRemainingTime <= 0)
                    {
                        gameStarted = false;
                        GameOver(false);
                    }
                }
            }).AddTo(this);
        }

        /// <summary>
        /// オブジェクトのアクティブを切り替える処理
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <param name="state">アクティブにするかどうかの判定</param>
        private void SetActiveRecursively(GameObject obj, bool state)
        {
            obj.SetActive(state);
            foreach (Transform child in obj.transform)
            {
                SetActiveRecursively(child.gameObject, state);
            }
        }

        /// <summary>
        /// RPCで鬼側が消滅したことを処理する
        /// </summary>
        /// <param name="seekerPlayer">鬼のプレイヤー</param>
        [PunRPC]
        private void RPC_DestroySeeker(GameObject seekerPlayer)
        {
            if (seekerPlayer != null)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber == seekerPlayer.GetComponent<PhotonView>().Owner.ActorNumber)
                {
                    SetSpectatorMode();
                    PhotonNetwork.Destroy(seekerPlayer);
                }

                gameStarted = false;
                GameOver(false);
            }
        }

        /// <summary>
        /// RPCでプレイヤーが捕まったことを処理する
        /// </summary>
        /// <param name="hiderViewID">捕まったプレイヤーのPhotonViewID</param>
        [PunRPC]
        private void RPC_OnPlayerCaught(int hiderViewID)
        {
            GameObject hider = PhotonView.Find(hiderViewID).gameObject;
            if (hider != null)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber == hider.GetComponent<PhotonView>().Owner.ActorNumber)
                {
                    SetSpectatorMode();
                    PhotonNetwork.Destroy(hider);
                }

                // 隠れる側のプレイヤーが全て捕まった場合
                if (hiderPlayerCount <= 0)
                {
                    gameStarted = false;
                    GameOver(true);
                }
            }
        }

        /// <summary>
        /// 観戦モードにする
        /// </summary>
        private void SetSpectatorMode()
        {
            if (overheadCameraPrefab != null)
            {
                Instantiate(overheadCameraPrefab);
                //overheadCamera = Instantiate(overheadCameraPrefab).GetComponent<Camera>();
            }
        }

        /// <summary>
        /// ゲームオーバー時の処理
        /// </summary>
        /// <param name="isSeekerWin">鬼の勝利かどうかの判定</param>
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
        }
        #endregion

    }
}