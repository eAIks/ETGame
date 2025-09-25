namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_GetPlayerEquipmentsHandler : MessageSessionHandler<C2G_GetPlayerEquipments, G2C_GetPlayerEquipments>
    {
        protected override async ETTask Run(Session session, C2G_GetPlayerEquipments request, G2C_GetPlayerEquipments response)
        {
            SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
            if (sessionPlayerComponent?.Player == null)
            {
                response.Error = ErrorCode.ERR_SessionPlayerInvalid;
                response.Message = "玩家会话无效";
                return;
            }

            Player player = sessionPlayerComponent.Player;

            try
            {
                Scene root = session.Root();

                long playerID = sessionPlayerComponent.PlayerID;
                
                if (playerID == 0)
                {
                    response.Error = ErrorCode.ERR_PlayerIDInvalid;
                    response.Message = "玩家ID无效";
                    return;
                }

                G2M_GetPlayerEquipments mapRequest = G2M_GetPlayerEquipments.Create();
                mapRequest.PlayerId = playerID;
                
                StartSceneConfig mapConfig = null;
                foreach (StartSceneConfig config in StartSceneConfigCategory.Instance.Maps)
                {
                    if (config.Name == "MainScene" && config.Zone == session.Zone())
                    {
                        mapConfig = config;
                        break;
                    }
                }
                
                if (mapConfig == null)
                {
                    response.Error = ErrorCode.ERR_MapConfigNotFound;
                    response.Message = "Map服务器配置未找到";
                    return;
                }
                
                ActorId mapActorId = mapConfig.ActorId;
                M2G_GetPlayerEquipments mapResponse = await root.GetComponent<MessageSender>().Call(mapActorId, mapRequest) as M2G_GetPlayerEquipments;
                
                if (mapResponse == null)
                {
                    response.Error = ErrorCode.ERR_MapServerNoResponse;
                    response.Message = "Map服务器无响应";
                    return;
                }
                
                if (mapResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = mapResponse.Error;
                    response.Message = mapResponse.Message;
                    return;
                }
                
                response.Equipments = mapResponse.Equipments;
                response.SlotIndexes = mapResponse.SlotIndexes;
            }
            catch (RpcException e) when (e.Error == ErrorCore.ERR_NotFoundActor)
            {
                response.Error = ErrorCode.ERR_MapActorNotFound;
                response.Message = "Map服务器不可用";
            }
            catch (RpcException e) when (e.Error == ErrorCore.ERR_MessageTimeout)
            {
                response.Error = ErrorCore.ERR_MessageTimeout;
                response.Message = "请求超时";
            }
            catch (System.TimeoutException)
            {
                response.Error = ErrorCore.ERR_MessageTimeout;
                response.Message = "请求超时";
            }
            catch (System.Exception e)
            {
                Log.Error($"获取玩家装备失败: {e.Message}");
                response.Error = ErrorCode.ERR_GetPlayerEquipmentsFailed;
                response.Message = "获取玩家装备失败";
            }
        }
    }
}