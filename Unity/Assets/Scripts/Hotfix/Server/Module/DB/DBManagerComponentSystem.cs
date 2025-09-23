using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(DBManagerComponent))]
    [FriendOf(typeof(DBManagerComponent))]
    public static partial class DBManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this DBManagerComponent self)
        {
            Log.Info("DBManagerComponent 初始化完成");
        }
        public static DBComponent GetZoneDB(this DBManagerComponent self, int zone)
        {
            Log.Info($"DBManagerComponentSystem: 获取Zone={zone}的数据库组件");
            
            DBComponent dbComponent = self.GetChild<DBComponent>(zone);
            if (dbComponent != null)
            {
                Log.Info($"DBManagerComponentSystem: 找到已存在的数据库组件 Zone={zone}");
                return dbComponent;
            }

            StartZoneConfig startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
            if (startZoneConfig == null)
            {
                Log.Error($"DBManagerComponentSystem: 未找到Zone={zone}的配置");
                throw new Exception($"zone: {zone} config not found");
            }
            
            if (startZoneConfig.DBConnection == "")
            {
                Log.Error($"DBManagerComponentSystem: Zone={zone}的数据库连接字符串为空");
                throw new Exception($"zone: {zone} not found mongo connect string");
            }

            Log.Info($"DBManagerComponentSystem: 创建新的数据库组件 Zone={zone}, Connection={startZoneConfig.DBConnection}, DBName={startZoneConfig.DBName}");
            
            dbComponent = self.AddChildWithId<DBComponent, string, string>(zone, startZoneConfig.DBConnection, startZoneConfig.DBName);
            
            // 初始化账号数据库索引
            AccountDBInitializer.InitializeAccountCollection(dbComponent).Coroutine();
            
            Log.Info($"DBManagerComponentSystem: 数据库组件创建完成 Zone={zone}");
            return dbComponent;
        }
    }
}