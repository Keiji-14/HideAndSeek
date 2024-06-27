using Scene;
using UnityEngine;

namespace Title
{
    /// <summary>
    /// タイトル画面の管理
    /// </summary>
    public class TitleScene : SceneBase
    {
        #region SerializeField
        [SerializeField] private TitleController titleController;
        #endregion

        #region UnityEvent
        public override void Start()
        {
            base.Start();

            titleController.Init();
        }
        #endregion
    }
}