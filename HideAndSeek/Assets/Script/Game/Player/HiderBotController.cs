using GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiderBotController : MonoBehaviour
{
    #region PrivateField
    private GameObject currentForm;
    /// <summary>変身するオブジェクトのリスト</summary>
    private List<GameObject> transformationObjList;
    /// <summary>移動先のリスト</summary>
    private List<Transform> targetPositionList;
    #endregion

    #region SerializeField
    /// <summary>移動速度</summary>
    [SerializeField] private float speed = 3f;
    /// <summary>名前用をキャンバス</summary>
    [SerializeField] private Canvas nameCanvas;
    /// <summary>プレイヤー名の表示</summary>
    [SerializeField] private PlayerNameDisplay playerNameDisplay;
    #endregion

    #region UnityEvent
    void Start()
    {
        var stageData = GameDataManager.Instance().GetStageData();
        if (stageData != null)
        {
            transformationObjList = stageData.transformationObjList;
            targetPositionList = stageData.botTargetPositionList;
        }

        // ランダムに変身する
        TransformRandomly();

        playerNameDisplay.Init(true);

        // ランダムな位置に移動を開始
        StartCoroutine(MoveRandomly());
    }

    private void Update()
    {
        RotationCanvas();
    }
    #endregion

    #region PrivateMethod
    /// <summary>
    /// キャンバスをカメラに見えるように回転させる処理
    /// </summary>
    private void RotationCanvas()
    {
        Vector3 cameraDirection = Camera.main.transform.forward;

        // HPバーの方向をカメラの方向に向ける
        nameCanvas.transform.LookAt(nameCanvas.transform.position + cameraDirection);

        // カメラのY軸回転を無視してHPバーを水平に保つ
        Quaternion targetRotation = Quaternion.Euler(0, nameCanvas.transform.rotation.eulerAngles.y, 0);
        nameCanvas.transform.rotation = Quaternion.Lerp(nameCanvas.transform.rotation, targetRotation, Time.deltaTime);
    }

    private void TransformRandomly()
    {
        if (transformationObjList.Count == 0) 
            return;

        // ランダムで変身する番号を選出する
        int randomIndex = Random.Range(0, transformationObjList.Count);

        if (currentForm != null)
        {
            Destroy(currentForm);
        }

        // 選出した番号のオブジェクトを生成する
        currentForm = Instantiate(transformationObjList[randomIndex], transform.position, transform.rotation, transform);
    }

    private IEnumerator MoveRandomly()
    {
        while (true)
        {
            if (targetPositionList.Count == 0) yield break;

            int randomIndex = Random.Range(0, targetPositionList.Count);
            Vector3 targetPosition = targetPositionList[randomIndex].position;

            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
        }
    }
    #endregion
}