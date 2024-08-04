using Scene;
using GameData;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace NetWork
{
    /// <summary>
    /// ネットワークの管理を行うクラス
    /// </summary>
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        #region PublicField
        /// <summary>シングルトン</summary>
        public static NetworkManager instance = null;
        #endregion

        #region PrivateField
        /// <summary>ルーム名</summary>
        private const string roomPrefix = "Room_";
        /// <summary>ランダムなルーム名の最大値</summary>
        private int maxRoomSuffix = 10000;
        /// <summary>マッチング処理のコンポーネント</summary>
        private MatchingController matchingController;
        /// <summary>ルーム情報を管理するクラス</summary>
        private RoomList roomList = new RoomList();
        #endregion

        #region UnityEvent
        private void Awake()
        {
            // シングルトンの実装
            if (instance == null)
            {
                instance = this;

                Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する処理
        /// </summary>
        public void ConnectUsingSettings()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// マスターサーバーへの接続が成功した時に呼ばれるコールバック
        /// </summary>
        public override void OnConnectedToMaster()
        {
            // ロビーに参加してルームリストの更新を待つ
            PhotonNetwork.JoinLobby();
        }

        /// <summary>
        /// ルームリストが更新されたときに呼ばれるコールバックメソッド
        /// </summary>
        /// <param name="updatedRoomList">更新されたルームリスト</param>
        public override void OnRoomListUpdate(List<RoomInfo> updatedRoomList)
        {
            roomList.Update(updatedRoomList);
            FindAndJoinRoom(); // ルームリストが更新されたときに再チェック
        }

        /// <summary>
        /// ゲームサーバーへの接続が成功した時に呼ばれるコールバック
        /// </summary>
        public override void OnJoinedRoom()
        {
            foreach (var property in PhotonNetwork.CurrentRoom.CustomProperties)
            {
                Debug.Log($"{property.Key}: {property.Value}");
            }

            // 役割に応じてプロパティを更新
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out object role) &&
                role is string playerRole)
            {
                Hashtable updatedProperties = new Hashtable();

                if (playerRole == "Seeker")
                {
                    updatedProperties["SeekerCount"] = (int)PhotonNetwork.CurrentRoom.CustomProperties["SeekerCount"] + 1;
                }
                else if (playerRole == "Hider")
                {
                    updatedProperties["HiderCount"] = (int)PhotonNetwork.CurrentRoom.CustomProperties["HiderCount"] + 1;
                }

                PhotonNetwork.CurrentRoom.SetCustomProperties(updatedProperties);
            }

            matchingController.MatchingStart();
        }

        /// <summary>
        /// ランダムルームへの参加に失敗したときに呼ばれるコールバックメソッド
        /// </summary>
        /// <param name="returnCode">失敗の原因を示すコード</param>
        /// <param name="message">失敗の詳細メッセージ</param>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to join random room: " + message);
            // ルームの参加失敗時に新しいルームを作成する処理
            CreateRoom();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            foreach (DictionaryEntry entry in propertiesThatChanged)
            {
                Debug.Log($"{entry.Key}: {entry.Value}");
            }
        }

        /// <summary>
        /// ゲームサーバーから退出する処理
        /// </summary>
        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                // PhotonのLeaveRoomメソッドを使用してゲームサーバーから退出する
                matchingController.MatchingFinish();

                if (PhotonNetwork.IsMasterClient)
                {
                    CloseRoom();
                }
                else
                {
                    PhotonNetwork.LeaveRoom();
                }

                if (PhotonNetwork.LocalPlayer != null)
                {
                    PhotonNetwork.LocalPlayer.TagObject = null;
                }
            }

            // Photonサーバーとの接続を切断する
            PhotonNetwork.Disconnect();
        }

        /// <summary>
        /// 役割を割り当てる処理
        /// </summary>
        public void AssignRoles()
        {
            Hashtable customProperties = new Hashtable();
            customProperties["Role"] = GameDataManager.Instance().GetPlayerRole();
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// 初期化
        /// </summary>
        private void Init()
        {
            // シーン自動同期の有効化
            PhotonNetwork.AutomaticallySyncScene = true;

            matchingController = FindObjectOfType<MatchingController>();

            // 既にルームに入っている場合は退出する
            if (PhotonNetwork.InRoom)
            {
                LeaveRoom();
            }

            matchingController.MatchingCompletedSubject.Subscribe(_ =>
            {
                var tilteController = FindObjectOfType<Title.TitleController>();
                if (tilteController != null)
                {
                    tilteController.MatchingCompleted();
                }

                LoadGameScene();
            }).AddTo(this);
        }

        private void FindAndJoinRoom()
        {
            foreach (var room in roomList)
            {
                // ルームのカスタムプロパティから SeekerCount と HiderCount を取得
                if (room.CustomProperties.TryGetValue("SeekerCount", out object seekerCount) &&
                    room.CustomProperties.TryGetValue("HiderCount", out object hiderCount))
                {
                    Debug.Log($"seekerCount:{(int)seekerCount}");
                    Debug.Log($"hiderCount:{(int)hiderCount}");

                    // 現在の役割を取得
                    PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out object role);
                    string playerRole = role as string;

                    // プレイヤーの役割に応じて条件を設定
                    if (playerRole == "Seeker")
                    {
                        // 鬼がいないルームに参加
                        if ((int)seekerCount == 0)
                        {
                            PhotonNetwork.JoinRoom(room.Name);
                            return; // 参加できたら処理を終了
                        }
                    }
                    else if (playerRole == "Hider")
                    {
                        // 鬼が1人以上いて隠れる側のプレイヤー数が4未満のルームに参加
                        if ((int)hiderCount < 4)
                        {
                            PhotonNetwork.JoinRoom(room.Name);
                            return; // 参加できたら処理を終了
                        }
                    }
                }
            }

            // 条件を満たすルームが見つからなかった場合、新しいルームを作成
            CreateRoom();
        }

        /// <summary>
        /// 新しいルームを作成する
        /// </summary>
        private void CreateRoom()
        {
            // ランダムなルーム名を生成する
            string randomRoomName = GenerateRandomRoomName();
            // ルームのオプションを設定する
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 5 };
            roomOptions.CustomRoomProperties = new Hashtable() { { "IsOpen", true }, { "SeekerCount", 0 }, { "HiderCount", 0 } };
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "IsOpen", "SeekerCount", "HiderCount" };
            // ルームを作成する
            PhotonNetwork.CreateRoom(randomRoomName, roomOptions, TypedLobby.Default);

            Debug.Log($"CreateRoom:{randomRoomName}");
        }

        /// <summary>
        /// ランダムなルーム名を生成する
        /// </summary>
        /// <returns>生成されたランダムなルーム名</returns>
        private string GenerateRandomRoomName()
        {
            int randomSuffix = Random.Range(0, maxRoomSuffix);
            return $"{roomPrefix}{randomSuffix}";
        }

        /// <summary>
        /// ゲームシーンに移行する処理
        /// </summary>
        private void LoadGameScene()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;

                SceneLoader.Instance().PhotonNetworkLoad(SceneLoader.SceneName.Game);
            }
        }

        /// <summary>
        /// ルームを閉じる処理
        /// </summary>
        private void CloseRoom()
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (!player.IsLocal)
                {
                    PhotonNetwork.CloseConnection(player);
                }
            }
            PhotonNetwork.LeaveRoom();
        }
        #endregion
    }
}