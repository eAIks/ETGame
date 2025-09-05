namespace ET.Server
{
    [EntitySystemOf(typeof(Account))]
    [FriendOf(typeof(Account))]
    public static partial class AccountSystem
    {
        [EntitySystem]
        private static void Awake(this Account self, string accountName, string password)
        {
            self.AccountName = accountName;
            self.Password = password;
            self.CreateTime = TimeInfo.Instance.ServerNow();
        }
    }
}