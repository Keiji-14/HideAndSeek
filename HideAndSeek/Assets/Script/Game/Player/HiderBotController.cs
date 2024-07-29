using Game;
using GameData;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class HiderBotController : MonoBehaviourPunCallbacks
{
    #region PrivateField
    /// <summary>現在の変身オブジェクト</summary>
    private GameObject currentObject;
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

        // ランダムなオブジェクトに変身させる
        int randomIndex = Random.Range(0, transformationObjList.Count);
        TransformIntoObject(randomIndex);

        // 初期移動先を設定
        MoveToRandomPosition();

        playerNameDisplay.Init(true);
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
        if (Camera.main == null)
            return;

        Vector3 cameraDirection = Camera.main.transform.forward;

        // HPバーの方向をカメラの方向に向ける
        nameCanvas.transform.LookAt(nameCanvas.transform.position + cameraDirection);
    }

    /// <summary>
    /// プレイヤーを物に変身させる処理
    /// </summary>
    private void TransformIntoObject(int transformIndex)
    {
        photonView.RPC("RPC_TransformIntoObject", RpcTarget.AllBuffered, transformIndex);
    }

    /// <summary>
    /// プレイヤーを物に変身させる処理
    /// </summary>
    [PunRPC]
    private void RPC_TransformIntoObject(int transformIndex)
    {
        if (currentObject != null && currentObject != gameObject)
        {
            Destroy(currentObject);
        }

        var position = transform.position;
        var rotation = transform.rotation;
        currentObject = Instantiate(transformationObjList[transformIndex], position, rotation);
        currentObject.transform.SetParent(this.transform);
        currentObject.transform.localPosition = Vector3.zero;
        currentObject.transform.localRotation = Quaternion.identity;
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