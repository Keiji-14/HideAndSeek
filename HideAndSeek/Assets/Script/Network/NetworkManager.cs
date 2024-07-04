using Scene;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;

namespace NetWork
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        #region PublicField
        public static NetworkManager instance = null;
        #endregion

        #region SerializeField
        [SerializeField] private MatchingController matchingController;
        #endregion

        #region UnityEvent
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region PublicMethod
        public void ConnectUsingSettings()
        {
            // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// マスターサーバーへの接続が成功した時に呼ばれるコールバック
        /// </summary>
        public override void OnConnectedToMaster()
        {
            // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
            PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
        }

        /// <summary>
        /// ゲームサーバーへの接続が成功した時に呼ばれるコールバック
        /// </summary>
        public override void OnJoinedRoom()
        {
            // プレイヤーが4人揃ったら役割を設定
            if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
            {
                AssignRolesAndLoadGameScene();
            }
        }

        /// <summary>
        /// ゲームサーバーから退出する処理
        /// </summary>
        public void LeaveRoom()
        {
            // PhotonのLeaveRoomメソッドを使用してゲームサーバーから退出する
            PhotonNetwork.LeaveRoom();
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// 初期化
        /// </summary>
        private void Init()
        {
            matchingController.MatchingCompletedSubject.Subscribe(_ =>
            {
                AssignRolesAndLoadGameScene();
            }).AddTo(this);
        }

        /// <summary>
        /// 役割を割り当ててゲームシーンに移行する処理
        /// </summary>
        private void AssignRolesAndLoadGameScene()
        {
            // ランダムに鬼を選出
            int seekerIndex = Random.Range(0, 4);
            int playerIndex = 0;

            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (playerIndex == seekerIndex)
                {
                    player.CustomProperties["Role"] = "Seeker";
                }
                else
                {
                    player.CustomProperties["Role"] = "Hider";
                }
                playerIndex++;
            }

            // ゲームシーンに移行
            SceneLoader.Instance().PhotonNetworkLoad(SceneLoader.SceneName.Game);
        }
        #endregion
    }
}