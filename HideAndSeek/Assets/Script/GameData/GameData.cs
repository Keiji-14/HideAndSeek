using UnityEngine;

namespace GameData
{
    public class GameData : MonoBehaviour
    {
        #region PublicField
        /// <summary>鬼側の初期ライフ数</summary>
        public int seekerInitLife;
        /// <summary>隠れる側のbot数</summary>
        public int HiderBotNum;
        /// <summary>ゲームの時間</summary>
        public float GameTimeNum;
        #endregion
    }
}