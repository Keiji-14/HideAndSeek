using Photon.Pun;
using UnityEngine;

public class GameController : MonoBehaviour
{
    #region SerializeField
    [SerializeField] private GameObject seekerPrefab;
    [SerializeField] private GameObject hiderPrefab;
    #endregion

    #region UnityEvent

    #endregion

    #region PublicMethod
    /// <summary>
    /// èâä˙âª
    /// </summary>
    public void Init()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
        {
            SpawnPlayer(seekerPrefab);
        }
        else
        {
            SpawnPlayer(hiderPrefab);
        }
    }
    #endregion

    #region PrivateMethod
    private void SpawnPlayer(GameObject prefab)
    {
        var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
        PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
        //PhotonNetwork.Instantiate(prefab.name, position, Quaternion.identity);
    }
    #endregion

}
