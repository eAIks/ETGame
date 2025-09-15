namespace ET.Server
{
    [ChildOf(typeof(Scene))]
    public sealed class AccountServer : Entity, IAwake<string, int>
    {
        /// <summary>
        /// 账号名称
        /// </summary>
        public string Account { get; set; }
        
        /// <summary>
        /// 账号的唯一标识符UUID
        /// </summary>
        public string AccountUUID { get; set; }
        
        /// <summary>
        /// ServerInfo中的ServerId，用于标识配置的服务器
        /// </summary>
        public int ServerId { get; set; }
        
        public long LoginTime { get; set; }
        
        /// <summary>
        /// 玩家在指定服务器的唯一ID，在确认区服时生成并与ServerId绑定
        /// </summary>
        public long PlayerID { get; set; }
    }
}