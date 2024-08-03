namespace GameData
{
    /// <summary>
    /// プレイヤー情報
    /// </summary>
    public class PlayerData
    {
        #region PublicField
        /// <summary>名前</summary>
        public string name;
        /// <summary>ゲームの役割</summary>
        public string role;
        #endregion

        public PlayerData(string name)
        {
            this.name = name;
            this.role = string.Empty;
        }
    }
}