using Unity.Mathematics;

namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_PlayerEnterMapHandler : MessageHandler<Scene, G2M_PlayerEnterMapRequest, M2G_PlayerEnterMapResponse>
    {
        protected override async ETTask Run(Scene scene, G2M_PlayerEnterMapRequest request, M2G_PlayerEnterMapResponse response)
        {
            Log.Info($"G2M_PlayerEnterMapHandler: 开始处理角色进入地图请求 - Account={request.Account}, ServerId={request.ServerId}");
            
            try
            {
                // 从数据库中查找Account和ServerId，获取PlayerID，如果没有则生成并存储
                Log.Info("G2M_PlayerEnterMapHandler: 开始查询/创建角色数据");
                PlayerData playerData = await PlayerDataService.QueryOrCreatePlayerByAccountAndServerId(
                    scene, request.Account, request.ServerId);
                    
                if (playerData == null)
                {
                    Log.Error($"G2M_PlayerEnterMapHandler: 查询或创建角色数据失败: Account={request.Account}, ServerId={request.ServerId}");
                    response.Error = ErrorCode.ERR_SystemError;
                    response.Message = "角色数据加载失败";
                    return;
                }
                
                Log.Info($"G2M_PlayerEnterMapHandler: 角色数据获取成功, PlayerId={playerData.PlayerId}, Name={playerData.Name}, Health={playerData.Health}, Attack={playerData.Attack}");

                // 返回从数据库查询到或新生成的PlayerID
                response.PlayerId = playerData.PlayerId;
                Log.Info($"G2M_PlayerEnterMapHandler: 角色数据处理完成，返回PlayerId={playerData.PlayerId}");
                
            }
            catch (System.Exception e)
            {
                Log.Error($"G2M_PlayerEnterMapHandler: 处理角色进入地图请求失败: {e.Message}");
                Log.Error($"G2M_PlayerEnterMapHandler: 异常堆栈: {e.StackTrace}");
                response.Error = ErrorCode.ERR_SystemError;
                response.Message = "服务器内部错误";
            }
        }
    }
}