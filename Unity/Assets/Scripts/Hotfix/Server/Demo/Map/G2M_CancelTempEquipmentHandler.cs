using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_CancelTempEquipmentHandler : MessageHandler<Scene, G2M_CancelTempEquipment, M2G_CancelTempEquipment>
    {
        protected override async ETTask Run(Scene scene, G2M_CancelTempEquipment request, M2G_CancelTempEquipment response)
        {
            if (scene == null)
            {
                response.Error = ErrorCode.ERR_InternalError;
                response.Message = "场景为空";
                return;
            }
            
            if (request == null)
            {
                response.Error = ErrorCode.ERR_InternalError;
                response.Message = "请求为空";
                return;
            }
            
            long playerId = request.PlayerId;

            try
            {
                // 获取装备临时缓存组件
                EquipmentTempCacheComponent tempCacheComponent = scene.GetComponent<EquipmentTempCacheComponent>();
                if (tempCacheComponent == null)
                {
                    response.Error = ErrorCode.ERR_ComponentNotFound;
                    response.Message = "装备临时缓存组件不存在";
                    return;
                }

                // 检查是否有缓存的装备
                if (!tempCacheComponent.HasTempEquipment(playerId))
                {
                    response.Error = ErrorCode.ERR_NoTempEquipment;
                    response.Message = "没有待取消的临时装备";
                    return;
                }

                // 清除临时缓存
                tempCacheComponent.ClearTempEquipment(playerId);
                
                response.Success = true;
                response.Error = ErrorCode.ERR_Success;
                response.Message = "取消临时装备成功";
                
                Log.Info($"取消临时装备成功: PlayerId={playerId}");
            }
            catch (Exception e)
            {
                response.Error = ErrorCode.ERR_InternalError;
                response.Message = "服务器内部错误";
                response.Success = false;
                Log.Error($"取消临时装备异常: {e.Message}");
            }
            
            await ETTask.CompletedTask;
        }
    }
}