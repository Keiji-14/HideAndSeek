using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// ステージ情報の管理
    /// </summary>
    [CreateAssetMenu(fileName = "StageDatabase", menuName = "Create Stage Database")]
    public class StageDatabase : ScriptableObject
    {
        #region PublicField
        /// <summary>ステージ情報のリスト</summary>
        public List<StageData> stageDataList = new List<StageData>();
        #endregion
    }
}