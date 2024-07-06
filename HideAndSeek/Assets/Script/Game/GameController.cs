using Photon.Pun;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// �Q�[����ʂ̏����Ǘ�
    /// </summary>
    public class GameController : MonoBehaviour
    {
        #region PrivateField
        private bool gameStarted = false;
        private float remainingTime;
        #endregion

        #region SerializeField
        /// <summary>�B��鎞��</summary>
        [SerializeField] private float gracePeriodSeconds;
        /// <summary>�Q�[���̐�������</summary>
        [SerializeField] private float gameTimeSeconds;
        [Header("Player Prefab")]
        /// <summary>�T�����̃v���C���[�I�u�W�F�N�g</summary>
        [SerializeField] private GameObject seekerPrefab;
        /// <summary>�B��鑤�̃v���C���[�I�u�W�F�N�g</summary>
        [SerializeField] private GameObject hiderPrefab;
        [Header("Component")]
        /// <summary>�Q�[��UI</summary>
        [SerializeField] private GameUI gameUI;
        #endregion

        #region PublicMethod
        /// <summary>
        /// ������
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

            // �B��鑤�̎��Ԃ��J�n
            StartCoroutine(GracePeriodCoroutine());
        }
        #endregion

        #region PrivateMethod
        private void SpawnPlayer(GameObject prefab)
        {
            var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
            PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
        }

        private IEnumerator GracePeriodCoroutine()
        {
            // �P�\���Ԓ��̃J�E���g�_�E���\��
            remainingTime = gracePeriodSeconds;
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                gameUI.UpdateTimer(remainingTime); // GameUI�Ɏc�莞�Ԃ�n��
                yield return null;
            }

            // �Q�[���J�n
            gameStarted = true;
            remainingTime = gameTimeSeconds;
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (gameStarted)
                {
                    remainingTime -= Time.deltaTime;

                    // GameUI�Ɏc�莞�Ԃ�n��
                    gameUI.UpdateTimer(remainingTime);
                    if (remainingTime <= 0)
                    {
                        gameStarted = false;
                        GameOver(false);
                    }
                }
            }).AddTo(this);
        }

        public void OnPlayerCaught()
        {
            if (gameStarted)
            {
                gameStarted = false;
                GameOver(true);
            }
        }

        private void GameOver(bool isSeekerWin)
        {
            // �Q�[���I������
            if (isSeekerWin)
            {
                Debug.Log("Seeker Wins!");
            }
            else
            {
                Debug.Log("Hiders Win!");
            }

            // �����ŃV�[���J�ڂ⃊�U���g�\�����s��
        }
        #endregion

    }
}