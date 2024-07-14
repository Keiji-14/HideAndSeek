using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiderBotController : MonoBehaviour
{
    [SerializeField] private List<GameObject> transformationOptions;
    [SerializeField] private List<Transform> targetPositions;
    [SerializeField] private float moveSpeed = 3f;

    private GameObject currentForm;

    void Start()
    {
        // ランダムに変身する
        TransformRandomly();

        // ランダムな位置に移動を開始
        StartCoroutine(MoveRandomly());
    }

    private void TransformRandomly()
    {
        if (transformationOptions.Count == 0) return;

        int randomIndex = Random.Range(0, transformationOptions.Count);

        if (currentForm != null)
        {
            Destroy(currentForm);
        }

        currentForm = Instantiate(transformationOptions[randomIndex], transform.position, transform.rotation, transform);
    }

    private IEnumerator MoveRandomly()
    {
        while (true)
        {
            if (targetPositions.Count == 0) yield break;

            int randomIndex = Random.Range(0, targetPositions.Count);
            Vector3 targetPosition = targetPositions[randomIndex].position;

            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // 少し待機してから次の移動を開始
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }
}
