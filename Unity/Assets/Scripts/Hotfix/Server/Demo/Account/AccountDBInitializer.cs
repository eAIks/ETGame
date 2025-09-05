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
                // 获取Account集合
                var collection = dbComponent.database.GetCollection<Account>("ET.AccountInfo");
                
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
            catch (System.Exception ex)
            {
                Log.Error($"初始化账号数据库索引失败: {ex}");
                throw;
            }
        }
    }
}