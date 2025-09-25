namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_SellEquipmentHandler : MessageSessionHandler<C2G_SellEquipment, G2C_SellEquipment>
    {
        protected override async ETTask Run(Session session, C2G_SellEquipment request, G2C_SellEquipment response)
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

                G2M_SellEquipment mapRequest = G2M_SellEquipment.Create();
                mapRequest.PlayerId = playerID;
                mapRequest.EquipmentId = request.EquipmentId;
                
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
                
                M2G_SellEquipment mapResponse = await root.GetComponent<MessageSender>().Call(mapActorId, mapRequest) as M2G_SellEquipment;
                
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
                response.SpiritStoneGained = mapResponse.SpiritStoneGained;
                response.ExpGained = mapResponse.ExpGained;
                response.LevelChanged = mapResponse.LevelChanged;
                response.NewLevel = mapResponse.NewLevel;
                response.NewMajorRealm = mapResponse.NewMajorRealm;
                response.NewMinorRealm = mapResponse.NewMinorRealm;
                response.NewCurrentExp = mapResponse.NewCurrentExp;
                response.NewSpiritStone = mapResponse.NewSpiritStone;
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
                Log.Error($"装备出售失败: {e.Message}");
                response.Error = ErrorCode.ERR_ReplaceEquipmentFailed;
                response.Message = "装备出售失败";
            }
        }
    }
}