using MongoDB.Driver;

namespace ET.Server
{
    /// <summary>
    /// 账号相关数据库初始化
    /// 注意：实际使用时需要通过DBComponentSystem来访问数据库操作
    /// </summary>
    [FriendOf(typeof(DBComponent))]
    public static class AccountDBInitializer
    {
        /// <summary>
        /// 初始化账号集合的唯一索引
        /// 使用说明：在服务器启动时调用此方法来确保数据库索引正确设置
        /// </summary>
        public static async ETTask InitializeAccountCollection(DBComponent dbComponent)
        {
            try
            {
                // 初始化Account集合索引
                await InitializeAccountIndexes(dbComponent);
                
                // 初始化AccountServer集合索引
                await InitializeAccountServerIndexes(dbComponent);
                
                Log.Info("所有账号相关索引初始化完成");
            }
            catch (System.Exception ex)
            {
                Log.Error($"初始化账号数据库索引失败: {ex}");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化Account集合索引
        /// </summary>
        private static async ETTask InitializeAccountIndexes(DBComponent dbComponent)
        {
            try
            {
                // 获取Account集合
                var collection = dbComponent.database.GetCollection<Account>("ET.Server.AccountInfo");
                
                // 创建账号名唯一索引
                var indexKeysDefinition = Builders<Account>.IndexKeys.Ascending(x => x.AccountName);
                var indexOptions = new CreateIndexOptions 
                { 
                    Unique = true,
                    Name = "AccountName_unique",
                    Background = true  // 后台创建索引，减少阻塞
                };
                
                // 创建索引（如果已存在会自动跳过）
                await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<Account>(indexKeysDefinition, indexOptions));
                
                Log.Info("账号唯一索引初始化完成");
            }
            catch (MongoCommandException mongoEx) when (mongoEx.CodeName == "IndexOptionsConflict")
            {
                Log.Info("账号唯一索引已存在，跳过创建");
            }
        }
        
        /// <summary>
        /// 初始化AccountServer集合索引
        /// </summary>
        private static async ETTask InitializeAccountServerIndexes(DBComponent dbComponent)
        {
            try
            {
                // 获取AccountServer集合
                var collection = dbComponent.database.GetCollection<AccountServer>("ET.Server.AccountServerInfo");
                
                // 创建Account和ServerId的复合唯一索引
                var accountServerIndexKeys = Builders<AccountServer>.IndexKeys
                    .Ascending(x => x.Account)
                    .Ascending(x => x.ServerId);
                    
                var accountServerIndexOptions = new CreateIndexOptions 
                { 
                    Unique = true,
                    Name = "Account_ServerId_unique",
                    Background = true
                };
                
                await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<AccountServer>(accountServerIndexKeys, accountServerIndexOptions));
                
                Log.Info("AccountServer Account_ServerId 唯一索引创建完成");
                
                // 创建AccountUUID索引（非唯一，因为同一UUID可能在不同服务器）
                var uuidIndexKeys = Builders<AccountServer>.IndexKeys.Ascending(x => x.AccountUUID);
                var uuidIndexOptions = new CreateIndexOptions 
                { 
                    Name = "AccountUUID_index",
                    Background = true
                };
                
                await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<AccountServer>(uuidIndexKeys, uuidIndexOptions));
                
                Log.Info("AccountServer AccountUUID 索引创建完成");
                
                // 创建PlayerID索引（非唯一，便于查询）
                var playerIdIndexKeys = Builders<AccountServer>.IndexKeys.Ascending(x => x.PlayerID);
                var playerIdIndexOptions = new CreateIndexOptions 
                { 
                    Name = "PlayerID_index",
                    Background = true
                };
                
                await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<AccountServer>(playerIdIndexKeys, playerIdIndexOptions));
                
                Log.Info("AccountServer PlayerID 索引创建完成");
            }
            catch (MongoCommandException mongoEx) when (mongoEx.CodeName == "IndexOptionsConflict")
            {
                Log.Info("AccountServer索引已存在，跳过创建");
            }
        }
    }
}