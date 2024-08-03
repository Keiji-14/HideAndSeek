using GameData;
using Scene;
using Player;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// ゲーム画面の処理を管理する
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
        /// <summary>捕まえたプレイヤーのIDリスト</summary>
        private List<int> capturedHiderIDList = new List<int>();
        /// <summary>変身可能なオブジェクト番号リスト</summary>
        private List<int> availableTransformIndexList;
        /// <summary>隠れる側のプレイヤーリスト</summary>
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
        /// <summary>消滅時のエフェクト</summary>
        [SerializeField] private GameObject destroyEffectObj;
        /// <summary>ステージ情報</summary>
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
            GameDataManager.Instance().SetStagerData(stageData);

            StartCoroutine(WaitForCustomProperties());
        }

        /// <summary>
        /// プレイヤーが捕まえた時の処理
        /// </summary>
        /// <param name="seekerViewID">捕まえた鬼のPhotonViewID</param>
        /// <param name="hiderViewID">捕まったプレイヤーのPhotonViewID</param>
        public void OnPlayerCaught(int seekerViewID, int hiderViewID)
        {
            if (gameStarted && !capturedHiderIDList.Contains(hiderViewID))
            {
                capturedHiderIDList.Add(hiderViewID);

                GameObject hiderPlayer = PhotonView.Find(hiderViewID).gameObject;
                var bot = hiderPlayer.GetComponent<HiderBotController>();

                if (bot == null)
                {
                    if (PhotonNetwork.LocalPlayer.ActorNumber == hiderPlayer.GetComponent<PhotonView>().Owner.ActorNumber)
                    {
                        SetSpectatorMode();
                    }
                }

                // 捕まえた通知を送信
                string hiderName = hiderPlayer.GetComponent<PhotonView>().Owner.NickName;

                photonView.RPC("RPC_DisplayCaughtMessage", RpcTarget.All, seekerViewID, hiderViewID, hiderName);

                // 全プレイヤーに隠れ側の数を更新
                var subtractHider = -1;
                photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, subtractHider);
                // 全プレイヤーに捕まえた事を通知
                photonView.RPC("RPC_OnPlayerCaught", RpcTarget.All, hiderViewID);
            }
        }

        /// <summary>
        /// 鬼プレイヤーのライフが0になった時の処理
        /// </summary>
        /// <param name="seekerViewID">鬼プレイヤーのPhotonViewID</param>
        public void SeekerFailed(int seekerViewID)
        {
            if (gameStarted)
            {
                photonView.RPC("RPC_DestroySeeker", RpcTarget.All, seekerViewID);
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

            gameUI.ToggleCanvas(PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString());

            if (PhotonNetwork.IsMasterClient)
            {
                int numberOfBots = 2; // 生成するボットの数を指定
                SpawnHiderBots(numberOfBots);
            }

            // 鬼側か隠れる側かを判定する処理
            if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
            {
                SpawnSeekerPlayer(seekerPrefab);
                StartCoroutine(GracePeriodSeekerCoroutine());
            }
            else if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Hider")
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
                var position = new Vector3(-18f, 3f, -3f);
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;

                var seeker = playerObject.GetComponent<SeekerController>();
                seeker.SwitchAudioListener(false);

                StartCoroutine(gameUI.ViewGraceText("他の人が隠れている間にマップを覚えましょう"));
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
                var position = new Vector3(Random.Range(-4f, 4f), 3f, Random.Range(-8f, 8f));
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;

                var addHider = 1;

                StartCoroutine(gameUI.ViewGraceText("鬼から見つからないように隠れましょう"));
                photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, addHider);
            }
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
                var botPlayerObject = botObject.GetComponent<HiderBotController>();
                botPlayerObject.Init("bot");

                var addHider = 1;
                photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, addHider);
            }
        }

        /// <summary>
        /// 隠れる側のプレイヤー数を更新する処理
        /// </summary>
        /// <param name="changeValue">変更する数値</param>
        [PunRPC]
        private void RPC_UpdateHiderCount(int changeValue)
        {
            hiderPlayerCount += changeValue;
            gameUI.UpdateHider(hiderPlayerCount);
        }

        /// <summary>
        /// プレイヤーが既に生成されているかを判定する処理
        /// </summary>
        /// <param name="player">チェックするプレイヤー</param>
        /// <returns>生成されている場合はtrue</returns>
        private bool HasSpawnedPlayer(Photon.Realtime.Player player)
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
            yield return new WaitForSeconds(1f);

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

            // リストをクリア
            hiderPlayerObjectList.Clear();
            var hiders = GameObject.FindGameObjectsWithTag("Hider");
            // 隠れる側のプレイヤーを見えなくする
            foreach (var hider in hiders)
            {
                HiderBotController botController = hider.GetComponent<HiderBotController>();
                if (botController != null)
                {
                    botController.HideBot();
                }
                else
                {
                    // オブジェクトごと非表示にする
                    SetActiveRecursively(hider, false);
                }
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
            gameUI.UpdateGameTimer(gameTimeSeconds);

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
            // 開始時の鬼の処理
            if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
            {
                StarGameSeekerInit();
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
        /// ゲーム開始直後の鬼の処理
        /// </summary>
        private void StarGameSeekerInit()
        {
            if (overheadCamera != null)
            {
                // 上空カメラを削除する
                Destroy(overheadCamera.gameObject);
                overheadCamera = null;
            }

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
                var hiderBotController = hider.GetComponent<HiderBotController>();

                if (hiderController != null)
                {
                    // オブジェクトを再表示
                    SetActiveRecursively(hider, true);
                    hiderController.SetCamera();
                }
                else if (hiderBotController != null)
                {
                    hiderBotController.ShowBot();
                }
            }
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
        /// <param name="seekerViewID">鬼のプレイヤーID</param>
        [PunRPC]
        private void RPC_DestroySeeker(int seekerViewID)
        {
            GameObject seekerPlayer = PhotonView.Find(seekerViewID).gameObject;
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
            GameObject hiderPlayer = PhotonView.Find(hiderViewID).gameObject;
            if (hiderPlayer != null)
            {
                var bot = hiderPlayer.GetComponent<HiderBotController>();
                if (bot == null)
                {
                    if (PhotonNetwork.LocalPlayer.ActorNumber == hiderPlayer.GetComponent<PhotonView>().Owner.ActorNumber)
                    {
                        SetSpectatorMode();
                    }
                }

                // MasterClientが消滅演出を行う
                if (PhotonNetwork.IsMasterClient)
                {
                    // エフェクトの生成座標
                    var pos = new Vector3(hiderPlayer.transform.position.x, hiderPlayer.transform.position.y + 0.5f, hiderPlayer.transform.position.z);

                    PhotonNetwork.Instantiate($"Effect/{destroyEffectObj.name}", pos, Quaternion.identity);
                }

                // 所有者が消滅処理を行う
                PhotonView hiderPhotonView = hiderPlayer.GetComponent<PhotonView>();
                if (hiderPhotonView.IsMine)
                {
                    // オブジェクトを削除
                    PhotonNetwork.Destroy(hiderPlayer);
                }
            }

            Debug.Log($"hiderPlayerCount:{hiderPlayerCount}");
            // 隠れる側のプレイヤーが全て捕まった場合
            if (hiderPlayerCount <= 0)
            {
                gameStarted = false;
                GameOver(true);
            }
        }

        /// <summary>
        /// 捕まえたプレイヤーと鬼のメッセージを表示するRPC
        /// </summary>
        /// <param name="seekerViewID">捕まえた鬼のPhotonViewID</param>
        /// <param name="hiderViewID">捕まったプレイヤーのPhotonViewID</param
        /// <param name="hiderName">捕まったプレイヤーの名前</param>
        [PunRPC]
        private void RPC_DisplayCaughtMessage(int seekerViewID, int hiderViewID, string hiderName)
        {
            GameObject seekerPlayer = PhotonView.Find(seekerViewID).gameObject;
            GameObject hiderPlayer = PhotonView.Find(hiderViewID).gameObject;

            if (seekerPlayer != null && hiderPlayer != null)
            {
                var bot = hiderPlayer.GetComponent<HiderBotController>();
                bool isBot = bot != null;

                string caughtHiderName = isBot ? bot.GetBotName() : hiderName;

                // 隠れる側のプレイヤーかどうか
                if (PhotonNetwork.LocalPlayer.ActorNumber == hiderPlayer.GetComponent<PhotonView>().Owner.ActorNumber && !isBot)
                {
                    StartCoroutine(gameUI.ViewCaughtPlayerName($"{seekerPlayer.GetComponent<PhotonView>().Owner.NickName}に捕まりました"));
                }
                else if (PhotonNetwork.LocalPlayer.ActorNumber == seekerPlayer.GetComponent<PhotonView>().Owner.ActorNumber)
                {
                    StartCoroutine(gameUI.ViewCaughtPlayerName($"{caughtHiderName}を捕まえました"));
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
                overheadCamera = Instantiate(overheadCameraPrefab).GetComponent<Camera>();
                overheadCamera = Camera.main;
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
                gameUI.ViewSeekerWin();
            }
            else
            {
                gameUI.ViewHiderWin();
            }
            // タイトル画面に戻る
            StartCoroutine(ReturnToTitle());
        }

        /// <summary>
        /// タイトル画面に戻るコルーチン
        /// </summary>
        private IEnumerator ReturnToTitle()
        {
            // 数秒待機
            yield return new WaitForSeconds(5.0f);

            // シーンをロード
            SceneLoader.Instance().Load(SceneLoader.SceneName.Title);
        }
        #endregion
    }
}