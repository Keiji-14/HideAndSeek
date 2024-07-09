using Scene;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// ゲーム画面の管理
    /// </summary>
    public class GameScene : SceneBase
    {
        #region SerializeField
        [SerializeField] private GameController gameController;
        #endregion

        #region UnityEvent
        public override void Start()
        {
            base.Start();

            gameController.Init();
        }
        #endregion
    }
}
