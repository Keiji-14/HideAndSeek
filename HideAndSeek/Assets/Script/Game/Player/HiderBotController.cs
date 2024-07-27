using GameData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HiderBotController : MonoBehaviour
{
    #region PrivateField
    private GameObject currentForm;
    /// <summary>NavMeshAgentコンポーネント</summary>
    private NavMeshAgent navMeshAgent;
    /// <summary>変身するオブジェクトのリスト</summary>
    private List<GameObject> transformationObjList;
    /// <summary>移動先のリスト</summary>
    private List<Transform> targetPositionList;
    /// <summary>隠れる側が鬼に見えないようにするためのRendererリスト</summary>
    private List<Renderer> rendererList;
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

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = speed;
        navMeshAgent.baseOffset = 0;

        rendererList = new List<Renderer>(GetComponentsInChildren<Renderer>());

        playerNameDisplay.Init(true);

        // ランダムに変身する
        TransformRandomly();

        // 初期移動先を設定
        MoveToRandomPosition();
    }

    private void Update()
    {
        RotationCanvas();

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            navMeshAgent.isStopped = true;
        }
    }
    #endregion

    #region PublicMethod
    /// <summary>
    /// 隠れる側のプレイヤーを非表示にする
    /// </summary>
    public void HideBot()
    {
        foreach (var renderer in rendererList)
        {
            renderer.enabled = false;
        }
    }

    /// <summary>
    /// 隠れる側のプレイヤーを再表示する
    /// </summary>
    public void ShowBot()
    {
        foreach (var renderer in rendererList)
        {
            renderer.enabled = true;
        }
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

    private void MoveToRandomPosition()
    {
        if (targetPositionList.Count == 0) return;

        int randomIndex = Random.Range(0, targetPositionList.Count);
        Vector3 targetPosition = targetPositionList[randomIndex].position;
        navMeshAgent.SetDestination(targetPosition);
    }
    #endregion
}