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
            // 生成新的UUID
            if (string.IsNullOrEmpty(self.AccountUUID))
            {
                self.AccountUUID = System.Guid.NewGuid().ToString();
            }
        }
        
        /// <summary>
        /// 根据UUID查询Account
        /// </summary>
        public static async ETTask<Account> GetAccountByUUID(Scene scene, string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return null;
            }
            
            DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
            if (dbManagerComponent == null)
            {
                return null;
            }
            
            DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
            var records = await dbComponent.Query<Account>(
                record => record.AccountUUID == uuid, "ET.Server.AccountInfo");
                
            return records.Count > 0 ? records[0] : null;
        }
        
        /// <summary>
        /// 根据账号名查询Account
        /// </summary>
        public static async ETTask<Account> GetAccountByName(Scene scene, string accountName)
        {
            if (string.IsNullOrEmpty(accountName))
            {
                return null;
            }
            
            DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
            if (dbManagerComponent == null)
            {
                return null;
            }
            
            DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
            var records = await dbComponent.Query<Account>(
                record => record.AccountName == accountName, "ET.Server.AccountInfo");
                
            return records.Count > 0 ? records[0] : null;
        }
    }
}