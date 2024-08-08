using NetWork;
using GameData;
using Audio;
using System;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Title
{
    /// <summary>
    /// タイトル画面の処理管理
    /// </summary>
    public class TitleController : MonoBehaviourPunCallbacks
    {
        #region PrivateField
        /// <summary>選択した役割</summary>
        private string selectedRole;
        /// <summary>開始ボタンを選択した時の処理</summary>
        private IObservable<Unit> InputStartObservable =>
            startBtn.OnClickAsObservable();
        /// <summary>マイページボタンを選択した時の処理</summary>
        private IObservable<Unit> InputMyPageObservable =>
            myPageBtn.OnClickAsObservable();
        /// <summary>マッチング処理のコンポーネント</summary>
        private MatchingController matchingController;
        #endregion

        #region SerializeField
        /// <summary>開始ボタン</summary>
        [SerializeField] private Button startBtn;
        /// <summary>マイページボタン</summary>
        [SerializeField] private Button myPageBtn;
        /// <summary>初回起動時の処理</summary>
        [SerializeField] private FirstStartup firstStartup;
        /// <summary>タイトル画面のUI</summary>
        [SerializeField] private TitleUI titleUI;
        /// <summary>マイページ画面</summary>
        [SerializeField] private MyPage myPage;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            isMatching = false;
            selectedRole = string.Empty;

            Cursor.lockState = CursorLockMode.None;

            // 初回起動かどうかの判定
            if (!PlayerPrefs.HasKey("FirstTime"))
            {
                firstStartup.Init();
            }
            else
            {
                firstStartup.AlreadyStartUp();
            }

            titleUI.Init();

            InputStartObservable.Subscribe(_ =>
            {
                titleUI.SwicthMatchWindow(true);
                SE.instance.Play(SE.SEName.ButtonSE);
            }).AddTo(this);

            InputMyPageObservable.Subscribe(_ =>
            {
                myPage.OpenMyPage();
                SE.instance.Play(SE.SEName.ButtonSE);
            }).AddTo(this);

            titleUI.SelectedRoleSubject.Subscribe(role =>
            {
                selectedRole = role;
                IsMatching(true);
            }).AddTo(this);

            titleUI.MatchingCancelSubject.Subscribe(_ =>
            {
                IsMatching(false);
            }).AddTo(this);

            myPage.Init();

            matchingController = FindObjectOfType<MatchingController>();

            matchingController.MatchingCountSubject.Subscribe(timer =>
            {
                int minutes = Mathf.FloorToInt(timer / 60f);
                int seconds = Mathf.FloorToInt(timer % 60f);

                titleUI.MatchingTimeUI(minutes, seconds);
            }).AddTo(this);

            GameDataManager.Instance().StageDatabaseInit();
        }

        /// <summary>
        /// マッチング完了時の処理
        /// </summary>
        public void MatchingCompleted()
        {
            photonView.RPC("RPC_MatchingCompleted", RpcTarget.All);
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// マッチングを行うかの処理
        /// </summary>
        /// <param name="isMatching">マッチング中かどうかの判定</param>
        private void IsMatching(bool isMatching)
        {
            titleUI.ViewMatchingUI(isMatching);

            if (isMatching)
            {
                GameDataManager.Instance().SetPlayerRole(selectedRole);
                NetworkManager.instance.AssignRoles();
                NetworkManager.instance.ConnectUsingSettings();
            }
            else
            {
                NetworkManager.instance.LeaveRoom();
            }
        }

        /// <summary>
        /// RPCでマッチング完了時の処理
        /// </summary>
        [PunRPC]
        private void RPC_MatchingCompleted()
        {
            titleUI.MatchingCompletedUI();
            matchingController.MatchingFinish();
            SE.instance.Play(SE.SEName.MatchingSE);
        }
        #endregion
    }
}