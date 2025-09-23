namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_CancelTempEquipmentHandler : MessageSessionHandler<C2G_CancelTempEquipment, G2C_CancelTempEquipment>
    {
        protected override async ETTask Run(Session session, C2G_CancelTempEquipment request, G2C_CancelTempEquipment response)
        {
            // 验证session和player
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

                // 直接从SessionPlayerComponent获取PlayerID（已在登录时缓存）
                long playerID = sessionPlayerComponent.PlayerID;
                
                if (playerID == 0)
                {
                    response.Error = ErrorCode.ERR_PlayerIDInvalid;
                    response.Message = "玩家ID无效";
                    return;
                }

                // 创建G2M消息发送到Map服务器
                G2M_CancelTempEquipment mapRequest = G2M_CancelTempEquipment.Create();
                mapRequest.PlayerId = playerID;
                
                // 查找MainScene配置
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
                M2G_CancelTempEquipment mapResponse = await root.GetComponent<MessageSender>().Call(mapActorId, mapRequest) as M2G_CancelTempEquipment;
                
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
                
                // 将Map服务器的响应转发给客户端
                response.Success = mapResponse.Success;
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
                Log.Error($"取消临时装备失败: {e.Message}");
                response.Error = ErrorCode.ERR_CancelTempEquipmentFailed;
                response.Message = "取消临时装备失败";
            }
        }
    }
}