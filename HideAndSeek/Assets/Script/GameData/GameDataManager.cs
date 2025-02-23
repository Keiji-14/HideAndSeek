﻿using UnityEngine;

namespace GameData
{
    /// <summary>
    /// ゲーム情報を管理する処理
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        #region PrivateField
        /// <summary>ゲーム情報管理のインスタンス</summary>
        private static GameDataManager instance = null;
        /// <summary>プレイヤー情報</summary>
        private PlayerData playerData;
        /// <summary>ステージ情報の管理</summary>
        private StageDatabase stageDatabase;
        /// <summary>ステージ情報</summary>
        private StageData stageData;
        #endregion

        #region PublicMethod
        /// <summary>
        /// インスタンス化
        /// </summary>
        /// <returns></returns>
        public static GameDataManager Instance()
        {
            // オブジェクトを生成し、自身をAddCompleteして、DontDestroyに置く
            if (instance == null)
            {
                var obj = new GameObject("GameDataManager");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<GameDataManager>();
            }

            return instance;
        }

        /// <summary>
        ///  プレイヤー情報を初期化
        /// </summary>
        public void PlayerDataInit()
        {
            playerData = new PlayerData(PlayerPrefs.GetString("UserName"));
        }

        /// <summary>
        /// プレイヤー情報を設定する処理
        /// </summary>
        public void SetPlayerData(PlayerData playerData)
        {
            this.playerData = playerData;
        }

        /// <summary>
        /// プレイヤー情報を返す
        /// </summary>
        public PlayerData GetPlayerData()
        {
            return playerData;
        }

        /// <summary>
        /// プレイヤーの役割を設定する処理
        /// </summary>
        public void SetPlayerRole(string role)
        {
            if (playerData != null)
            {
                playerData.role = role;
            }
        }

        /// <summary>
        /// プレイヤーの役割を返す
        /// </summary>
        public string GetPlayerRole()
        {
            return playerData != null ? playerData.role : string.Empty;
        }

        /// <summary>
        ///  ステージ情報管理の初期化
        /// </summary>
        public void StageDatabaseInit()
        {
            stageDatabase = Resources.Load<StageDatabase>("Prefabs/Stage/StageDatabase");
            if (stageDatabase == null)
            {
                Debug.LogError("StageDatabaseがResourcesフォルダ内に見つかりません。");
            }
        }

        /// <summary>
        ///  ステージ情報管理を返す
        /// </summary>
        public StageDatabase GetStageDatabase()
        {
            return stageDatabase;
        }

        /// <summary>
        /// ステージ情報を設定する処理
        /// </summary>
        public void SetStagerData(StageData stageData)
        {
            this.stageData = stageData;
        }

        /// <summary>
        /// ステージ情報を返す
        /// </summary>
        public StageData GetStageData()
        {
            return stageData;
        }
        #endregion
    }
}