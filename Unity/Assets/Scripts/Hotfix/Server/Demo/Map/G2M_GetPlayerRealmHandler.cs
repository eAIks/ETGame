namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_GetPlayerRealmHandler : MessageHandler<Scene, G2M_GetPlayerRealm, M2G_GetPlayerRealm>
    {
        protected override async ETTask Run(Scene scene, G2M_GetPlayerRealm request, M2G_GetPlayerRealm response)
        {
            long playerId = request.PlayerId;
            Log.Info($"Map服务器收到获取境界信息请求: PlayerId={playerId}");

            try
            {
                // 获取数据库组件
                DBManagerComponent dbManagerComponent = scene.Root().GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    response.Error = ErrorCode.ERR_ComponentNotFound;
                    response.Message = "数据库管理组件未找到";
                    return;
                }

                // 从数据库查询角色信息
                RoleInfo roleInfo = await dbManagerComponent.GetZoneDB(scene.Zone()).Query<RoleInfo>(playerId, "ET.Server.User.RoleInfo");
                if (roleInfo == null)
                {
                    Log.Warning($"未找到玩家角色信息: PlayerId={playerId}，将创建默认角色信息");
                    
                    // 创建默认角色信息
                    roleInfo = scene.AddChildWithId<RoleInfo, long>(playerId, playerId);
                    
                    // 保存到数据库
                    await dbManagerComponent.GetZoneDB(scene.Zone()).Save(roleInfo, "ET.Server.User.RoleInfo");
                    
                    Log.Info($"为玩家{playerId}创建了默认角色信息");
                }

                // 使用RoleInfo的扩展方法创建完整的境界信息
                RealmInfoProto realmInfoProto = roleInfo.ToRealmInfoProto();
                
                Log.Info($"创建境界信息: 等级={realmInfoProto.Level}, 当前经验={realmInfoProto.CurrentExp}, " +
                         $"升级经验={realmInfoProto.ExpForNextLevel}, 灵石={realmInfoProto.SpiritStone}, " +
                         $"境界={realmInfoProto.MajorRealmName}-{realmInfoProto.MinorRealmName}");
                
                response.RealmInfo = realmInfoProto;
                response.Error = ErrorCode.ERR_Success;
                
                Log.Info($"Map服务器返回境界信息: PlayerId={playerId}, {realmInfoProto.MajorRealmName}-{realmInfoProto.MinorRealmName}, 等级:{realmInfoProto.Level}");
            }
            catch (System.Exception e)
            {
                Log.Error($"Map服务器获取玩家境界信息失败: PlayerId={playerId}, 错误: {e.Message}");
                response.Error = ErrorCode.ERR_RealmInfoGetFailed;
                response.Message = "获取境界信息失败";
            }
        }
    }
}