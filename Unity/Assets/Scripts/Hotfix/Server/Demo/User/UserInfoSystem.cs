namespace ET.Server
{
    /// <summary>
    /// UserInfo相关的系统方法，用于数据库操作
    /// </summary>
    [FriendOf(typeof(UserInfo))]
    public static class UserInfoDBSystem
    {
        /// <summary>
        /// 如果用户角色信息不存在则创建
        /// </summary>
        public static async ETTask CreateUserInfoIfNotExists(Scene scene, long playerID, string account)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("CreateUserInfoIfNotExists: DBManagerComponent not found");
                    return;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                // 查询是否已存在角色
                var existingUsers = await dbComponent.Query<UserInfo>(
                    user => user.PlayerID == playerID, 
                    "ET.Server.UserInfo");
                    
                if (existingUsers.Count == 0)
                {
                    // 创建新角色，使用默认配置ID = 1
                    long entityId = IdGenerater.Instance.GenerateId();
                    UserInfo newUser = scene.AddChildWithId<UserInfo, long, int>(entityId, playerID, 1);
                    newUser.SetName($"Player_{playerID}"); // 默认角色名
                    
                    await dbComponent.Save(newUser, "ET.Server.UserInfo");
                    Log.Info($"创建新角色: PlayerID={playerID}, Account={account}, Name={newUser.Name}");
                }
                else
                {
                    Log.Info($"角色已存在: PlayerID={playerID}, Account={account}");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"CreateUserInfoIfNotExists failed: {e}");
            }
        }
        
        /// <summary>
        /// 更新用户最后登录时间
        /// </summary>
        public static async ETTask UpdateUserInfoLastLoginTime(Scene scene, long playerID)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    return;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                var existingUsers = await dbComponent.Query<UserInfo>(
                    user => user.PlayerID == playerID, 
                    "ET.Server.UserInfo");
                    
                if (existingUsers.Count > 0)
                {
                    var userInfo = existingUsers[0];
                    userInfo.UpdateLastLoginTime();
                    await dbComponent.Save(userInfo, "ET.Server.UserInfo");
                    Log.Info($"更新用户最后登录时间: PlayerID={playerID}");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"UpdateUserInfoLastLoginTime failed: {e}");
            }
        }
        
        /// <summary>
        /// 根据PlayerID获取用户信息
        /// </summary>
        public static async ETTask<UserInfo> GetUserInfoByPlayerID(Scene scene, long playerID)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("GetUserInfoByPlayerID: DBManagerComponent not found");
                    return null;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                var existingUsers = await dbComponent.Query<UserInfo>(
                    user => user.PlayerID == playerID, 
                    "ET.Server.UserInfo");
                    
                return existingUsers.Count > 0 ? existingUsers[0] : null;
            }
            catch (System.Exception e)
            {
                Log.Error($"GetUserInfoByPlayerID failed: {e}");
                return null;
            }
        }
    }
}