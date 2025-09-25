namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_ReplaceEquipmentHandler : MessageSessionHandler<C2G_ReplaceEquipment, G2C_ReplaceEquipment>
    {
        protected override async ETTask Run(Session session, C2G_ReplaceEquipment request, G2C_ReplaceEquipment response)
        {
            SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
            if (sessionPlayerComponent?.Player == null)
            {
                response.Error = ErrorCode.ERR_SessionPlayerError;
                response.Message = "会话玩家信息错误";
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

                G2M_ReplaceEquipment mapRequest = G2M_ReplaceEquipment.Create();
                mapRequest.PlayerId = playerID;
                mapRequest.EquipmentId = request.EquipmentId;
                mapRequest.SlotIndex = request.SlotIndex;
                
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
                    response.Error = ErrorCode.ERR_ReplaceMapConfigNotFound;
                    response.Message = "Map服务器配置未找到";
                    return;
                }
                
                ActorId mapActorId = mapConfig.ActorId;
                
                M2G_ReplaceEquipment mapResponse = await root.GetComponent<MessageSender>().Call(mapActorId, mapRequest) as M2G_ReplaceEquipment;
                
                if (mapResponse == null)
                {
                    response.Error = ErrorCode.ERR_ReplaceMapServerNoResponse;
                    response.Message = "Map服务器无响应";
                    return;
                }
                
                if (mapResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = mapResponse.Error;
                    response.Message = mapResponse.Message;
                    return;
                }
                
                response.Success = mapResponse.Success;
            }
            catch (RpcException e) when (e.Error == ErrorCore.ERR_NotFoundActor)
            {
                response.Error = ErrorCode.ERR_ReplaceMapActorNotFound;
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
                Log.Error($"装备替换失败: {e.Message}");
                response.Error = ErrorCode.ERR_ReplaceEquipmentFailed;
                response.Message = "装备替换失败";
            }
        }

    }
}