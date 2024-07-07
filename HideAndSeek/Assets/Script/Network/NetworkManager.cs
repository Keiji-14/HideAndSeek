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

        #region PrivateField
        private PhotonView photonView;
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
            PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
        }

        /// <summary>
        /// ゲームサーバーへの接続が成功した時に呼ばれるコールバック
        /// </summary>
        public override void OnJoinedRoom()
        {
            matchingController.MatchingStart();
        }

        /// <summary>
        /// ゲームサーバーから退出する処理
        /// </summary>
        public void LeaveRoom()
        {
            // PhotonのLeaveRoomメソッドを使用してゲームサーバーから退出する
            matchingController.MatchingFinish();
            PhotonNetwork.LeaveRoom();
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// 初期化
        /// </summary>
        private void Init()
        {
            photonView = GetComponent<PhotonView>();

            matchingController.MatchingCompletedSubject.Subscribe(_ =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    AssignRolesAndLoadGameScene();
                }
            }).AddTo(this);
        }

        /// <summary>
        /// 役割を割り当ててゲームシーンに移行する処理
        /// </summary>
        private void AssignRolesAndLoadGameScene()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int seekerIndex = Random.Range(0, PhotonNetwork.PlayerList.Length);
                int playerIndex = 0;

                foreach (var player in PhotonNetwork.PlayerList)
                {
                    ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
                    if (playerIndex == seekerIndex)
                    {
                        customProperties["Role"] = "Seeker";
                    }
                    else
                    {
                        customProperties["Role"] = "Hider";
                    }
                    player.SetCustomProperties(customProperties);
                    playerIndex++;
                }

                photonView.RPC("LoadGameScene", RpcTarget.All);
            }
        }

        [PunRPC]
        private void LoadGameScene()
        {
            SceneLoader.Instance().PhotonNetworkLoad(SceneLoader.SceneName.Game);
        }
        #endregion
    }
}