using GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiderBotController : MonoBehaviour
{
    private List<GameObject> transformationObjList;
    private List<Transform> targetPositionList;
    private GameObject currentForm;

    [SerializeField] private float moveSpeed = 3f;

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

        // ランダムな位置に移動を開始
        StartCoroutine(MoveRandomly());
    }

    private void TransformRandomly()
    {
        if (transformationObjList.Count == 0) return;

        int randomIndex = Random.Range(0, transformationObjList.Count);

        if (currentForm != null)
        {
            Destroy(currentForm);
        }

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
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}
