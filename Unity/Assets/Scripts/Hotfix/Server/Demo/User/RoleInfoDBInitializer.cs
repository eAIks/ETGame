using MongoDB.Driver;

namespace ET.Server
{
    /// <summary>
    /// RoleInfo相关数据库初始化
    /// </summary>
    [FriendOf(typeof(DBComponent))]
    public static class RoleInfoDBInitializer
    {
        /// <summary>
        /// 初始化RoleInfo集合的索引
        /// 使用说明：在服务器启动时调用此方法来确保数据库索引正确设置
        /// </summary>
        public static async ETTask InitializeRoleInfoCollection(DBComponent dbComponent)
        {
            try
            {
                await InitializeRoleInfoIndexes(dbComponent);
                Log.Info("RoleInfo相关索引初始化完成");
            }
            catch (System.Exception ex)
            {
                Log.Error($"初始化RoleInfo数据库索引失败: {ex}");
                throw;
            }
        }
        
        /// <summary>
        /// 创建角色信息数据库索引
        /// </summary>
        private static async ETTask InitializeRoleInfoIndexes(DBComponent dbComponent)
        {
            try
            {
                // 获取RoleInfo集合
                var collection = dbComponent.database.GetCollection<RoleInfo>("ET.Server.User.RoleInfo");
                
                // 创建PlayerId唯一索引（作为主键）
                var playerIdIndexKeys = Builders<RoleInfo>.IndexKeys.Ascending(x => x.PlayerId);
                var playerIdIndexOptions = new CreateIndexOptions 
                { 
                    Unique = true,
                    Name = "PlayerId_unique",
                    Background = true
                };
                
                await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<RoleInfo>(playerIdIndexKeys, playerIdIndexOptions));
                
                Log.Info("RoleInfo PlayerId 唯一索引创建完成");
                
                // 创建昵称索引（便于按昵称查询，非唯一）
                var nickNameIndexKeys = Builders<RoleInfo>.IndexKeys.Ascending(x => x.NickName);
                var nickNameIndexOptions = new CreateIndexOptions 
                { 
                    Name = "NickName_index",
                    Background = true
                };
                
                await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<RoleInfo>(nickNameIndexKeys, nickNameIndexOptions));
                
                Log.Info("RoleInfo NickName 索引创建完成");
            }
            catch (MongoCommandException mongoEx) when (mongoEx.CodeName == "IndexOptionsConflict")
            {
                Log.Info("RoleInfo索引已存在，跳过创建");
            }
            catch (System.Exception ex)
            {
                Log.Error($"初始化RoleInfo数据库索引失败: {ex}");
                throw;
            }
        }
    }
}