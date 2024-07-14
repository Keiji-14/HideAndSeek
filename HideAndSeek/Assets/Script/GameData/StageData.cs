using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Create Stage Data")]
public class StageData : ScriptableObject
{
    #region PublicField
    /// <summary>�X�e�[�WID</summary>
    public int stageID;
    /// <summary>�X�e�[�WI�I�u�W�F�N�gD</summary>
    public GameObject stageObj;
    /// <summary>�ϐg����I�u�W�F�N�g���X�g</summary>
    public List<GameObject> transformationObjList = new List<GameObject>();
    /// <summary>bot�̈ړ��惊�X�g</summary>
    public List<Transform> botTargetPositionList = new List<Transform>();
    #endregion
}
