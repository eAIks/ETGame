namespace ET.Server
{
    [EntitySystemOf(typeof(AccountServer))]
    [FriendOf(typeof(AccountServer))]
    public static partial class AccountServerSystem
    {
        [EntitySystem]
        private static void Awake(this AccountServer self, string account, int serverId)
        {
            self.Account = account;
            self.ServerId = serverId;
            self.LoginTime = TimeInfo.Instance.ServerNow();
        }
    }
}