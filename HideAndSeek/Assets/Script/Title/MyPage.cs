using GameData;
using Audio;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Title
{
    /// <summary>
    /// マイページの処理
    /// </summary>
    public class MyPage : MonoBehaviour
    {
        #region PrivateField
        /// <summary>名前変更ボタンを選択した時の処理 </summary>
        private IObservable<Unit> InputChangeNameBtnObservable =>
            changeNameBtn.OnClickAsObservable();
        /// <summary>名前決定ボタンを選択した時の処理 </summary>
        private IObservable<Unit> InputEnterNameBtnObservable =>
            enterNameBtn.OnClickAsObservable();
        /// <summary>閉じるボタンを選択した時の処理 </summary>
        private IObservable<Unit> InputCloseBtnObservable =>
            closeBtn.OnClickAsObservable();
        #endregion

        #region SerializeField
        /// <summary>名前変更ボタン</summary>
        [SerializeField] private Button changeNameBtn;
        /// <summary>名前決定ボタン</summary>
        [SerializeField] private Button enterNameBtn;
        /// <summary>閉じるボタン</summary>
        [SerializeField] private Button closeBtn;
        /// <summary>名前を表示</summary>
        [SerializeField] private Text nameText;
        /// <summary>名前入力場所</summary>
        [SerializeField] private InputField nameInputField;
        /// <summary>マイページウィンドウ</summary>
        [SerializeField] private GameObject myPageWindow;
        /// <summary>名前変更ウィンドウ</summary>
        [SerializeField] private GameObject changeNameWindow;
        #endregion

        #region UnityEvent
        void Update()
        {
            // テキストが含まれているかどうかでボタンを有効にする
            enterNameBtn.interactable = IsInputFieldValue();
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            InputChangeNameBtnObservable.Subscribe(_ =>
            {
                OpenChangeNameWindow();
            }).AddTo(this);

            InputEnterNameBtnObservable.Subscribe(_ =>
            {
                if (nameInputField.text.Length > 0)
                {
                    PlayerPrefs.SetString("UserName", nameInputField.text);

                    PlayerData playerData = new PlayerData(nameInputField.text);
                    GameDataManager.Instance().SetPlayerData(playerData);

                    UpdateViewName();
                }
                changeNameWindow.SetActive(false);

                SE.instance.Play(SE.SEName.ButtonSE);
            }).AddTo(this);

            InputCloseBtnObservable.Subscribe(_ =>
            {
                myPageWindow.SetActive(false);
                SE.instance.Play(SE.SEName.ButtonSE);
            }).AddTo(this);
        }

        /// <summary>
        /// マイページを開く処理
        /// </summary>
        public void OpenMyPage()
        {
            UpdateViewName();

            myPageWindow.SetActive(true);
        }

        /// <summary>
        /// 名前変更ウィンドウを開く処理
        /// </summary>
        public void OpenChangeNameWindow()
        {
            nameInputField.text = "";

            nameInputField.onValueChanged.AddListener(OnInputFieldValueChanged);

            changeNameWindow.SetActive(true);
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// 表示する名前を更新する処理
        /// </summary>
        private void UpdateViewName()
        {
            nameText.text = GameDataManager.Instance().GetPlayerData().name;
        }

        /// <summary>
        /// ひらがな、カタカナ、英語、一部の記号以外の文字を削除する処理
        /// </summary>
        private void OnInputFieldValueChanged(string value)
        {
            string filteredText = System.Text.RegularExpressions.Regex.Replace(value, "[^ぁ-んァ-ンa-zA-Z0-9!\"#$%&'()*+,./:;<=>?@[\\]^_`{|}ー~]+", "");

            // テキストを更新する
            nameInputField.text = filteredText;
        }

        /// <summary>
        /// テキストが含まれているかどうかの処理
        /// </summary>
        private bool IsInputFieldValue()
        {
            return nameInputField.text.Length > 0;
        }
        #endregion
    }
}