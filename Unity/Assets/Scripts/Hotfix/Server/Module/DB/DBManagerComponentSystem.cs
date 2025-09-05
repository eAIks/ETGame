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
            DBComponent dbComponent = self.GetChild<DBComponent>(zone);
            if (dbComponent != null)
            {
                return dbComponent;
            }

            StartZoneConfig startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
            if (startZoneConfig.DBConnection == "")
            {
                throw new Exception($"zone: {zone} not found mongo connect string");
            }

            dbComponent = self.AddChildWithId<DBComponent, string, string>(zone, startZoneConfig.DBConnection, startZoneConfig.DBName);
            
            // 初始化账号数据库索引
            AccountDBInitializer.InitializeAccountCollection(dbComponent).Coroutine();
            
            return dbComponent;
        }
    }
}