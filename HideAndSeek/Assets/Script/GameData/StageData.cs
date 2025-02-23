﻿using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// ステージ情報
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "Create Stage Data")]
    public class StageData : ScriptableObject
    {
        #region PublicField
        /// <summary>ステージID</summary>
        public int stageID;
        /// <summary>ステージオブジェクト</summary>
        public GameObject stageObj;
        /// <summary>鬼側の開始区域</summary>
        public Transform seekerStartArea;
        /// <summary>変身するオブジェクトリスト</summary>
        public List<GameObject> transformationObjList = new List<GameObject>();
        /// <summary>botの移動先リスト</summary>
        public List<Transform> botTargetPositionList = new List<Transform>();
        #endregion
    }
}