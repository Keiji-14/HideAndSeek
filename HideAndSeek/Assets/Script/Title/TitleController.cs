using NetWork;
using GameData;
using Audio;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Title
{
    /// <summary>
    /// タイトル画面の処理管理
    /// </summary>
    public class TitleController : MonoBehaviour
    {
        #region PrivateField
        /// <summary>マッチング中を計測する値</summary>
        private float matchingTime;
        /// <summary>初期化用の値</summary>
        private const float resetCount = 0.0f;
        /// <summary>マッチング中かどうかの処理</summary>
        private bool isMatching = false;
        /// <summary>選択した役割</summary>
        private string selectedRole;
        /// <summary>開始ボタンを選択した時の処理</summary>
        private IObservable<Unit> InputStartObservable =>
            startBtn.OnClickAsObservable();
        /// <summary>マイページボタンを選択した時の処理</summary>
        private IObservable<Unit> InputMyPageObservable =>
            myPageBtn.OnClickAsObservable();
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

        #region UnityEvent
        public void FixedUpdate()
        {
            if (!isMatching)
                return;

            MatchingTimeCount();
        }
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

            GameDataManager.Instance().StageDatabaseInit();
        }

        /// <summary>
        /// マッチング完了時の処理
        /// </summary>
        public void MatchingCompleted()
        {
            titleUI.MatchingCompletedUI();
            SE.instance.Play(SE.SEName.MatchingSE);
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// マッチング中の時間計測の処理
        /// </summary>
        private void MatchingTimeCount()
        {
            matchingTime += Time.deltaTime;

            int minutes = Mathf.FloorToInt(matchingTime / 60f);
            int seconds = Mathf.FloorToInt(matchingTime % 60f);
            
            titleUI.MatchingTimeUI(minutes, seconds);
        }

        /// <summary>
        /// マッチングを行うかの処理
        /// </summary>
        /// <param name="isMatching">マッチング中かどうかの判定</param>
        private void IsMatching(bool isMatching)
        {
            this.isMatching = isMatching;
            matchingTime = resetCount;

            titleUI.ViewMatchingUI(isMatching);

            if (isMatching)
            {
                NetworkManager.instance.ConnectUsingSettings();
                GameDataManager.Instance().SetPlayerRole(selectedRole);
            }
            else
            {
                NetworkManager.instance.LeaveRoom();
            }
        }
        #endregion
    }
}