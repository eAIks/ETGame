using Unity.Mathematics;

namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_PlayerEnterMapHandler : MessageHandler<Scene, G2M_PlayerEnterMapRequest, M2G_PlayerEnterMapResponse>
    {
        protected override async ETTask Run(Scene scene, G2M_PlayerEnterMapRequest request, M2G_PlayerEnterMapResponse response)
        {
            Log.Info($"G2M_PlayerEnterMapHandler: 开始处理角色进入地图请求 - Account={request.Account}, PlayerId={request.PlayerId}, ServerId={request.ServerId}");
            
            try
            {
                // 第一步：查询或创建角色数据（这是Map服务器的核心职责）
                Log.Info("G2M_PlayerEnterMapHandler: 开始查询/创建角色数据");
                PlayerData playerData = await PlayerDataService.QueryOrCreatePlayerData(
                    scene, request.Account, request.PlayerId, request.ServerId);
                    
                if (playerData == null)
                {
                    Log.Error($"G2M_PlayerEnterMapHandler: 查询或创建角色数据失败: Account={request.Account}, PlayerId={request.PlayerId}");
                    response.Error = ErrorCode.ERR_SystemError;
                    response.Message = "角色数据加载失败";
                    return;
                }
                
                Log.Info($"G2M_PlayerEnterMapHandler: 角色数据获取成功, Name={playerData.Name}, Health={playerData.Health}, Attack={playerData.Attack}");

                // Map服务器只负责角色数据查询/创建，不创建Unit
                // Unit的创建和场景切换应该由Gate服务器或其他专门的服务处理
                
                // 返回角色数据的ID，让Gate服务器知道角色已准备就绪
                response.UnitId = request.PlayerId;
                Log.Info($"G2M_PlayerEnterMapHandler: 角色数据处理完成，PlayerId={request.PlayerId}");
                
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