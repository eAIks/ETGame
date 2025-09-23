using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_ReplaceEquipmentHandler : MessageHandler<Scene, G2M_ReplaceEquipment, M2G_ReplaceEquipment>
    {
        protected override async ETTask Run(Scene scene, G2M_ReplaceEquipment request, M2G_ReplaceEquipment response)
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
            long equipmentId = request.EquipmentId;
            int slotIndex = request.SlotIndex;

            try
            {
                // 获取玩家装备服务
                PlayerEquipmentService playerEquipmentService = scene.GetComponent<PlayerEquipmentService>();
                if (playerEquipmentService == null)
                {
                    response.Error = ErrorCode.ERR_ComponentNotFound;
                    response.Message = "玩家装备服务不存在";
                    return;
                }

                // 验证槽位索引（UIMain固定9个槽位：0-8）
                if (slotIndex < 0 || slotIndex >= 9)
                {
                    response.Error = ErrorCode.ERR_SlotIndexInvalid;
                    response.Message = "槽位索引无效";
                    return;
                }

                // 创建新装备
                Equipment newEquipment = PlayerEquipmentServiceSystem.CreateEquipment(
                    equipmentId, 
                    $"装备_{equipmentId}", 
                    slotIndex, 
                    100, // attack
                    50,  // defense
                    200, // health
                    1,   // quality
                    1,   // level
                    "icon_default", 
                    "新生成的装备"
                );

                // 替换装备
                playerEquipmentService.SetEquippedEquipment(playerId, slotIndex, newEquipment);
                
                response.Success = true;
                response.Error = ErrorCode.ERR_Success;
                response.Message = "装备替换成功";
            }
            catch (Exception e)
            {
                response.Error = ErrorCode.ERR_InternalError;
                response.Message = "服务器内部错误";
                response.Success = false;
                Log.Error($"装备替换异常: {e.Message}");
            }
            
            await ETTask.CompletedTask;
        }
    }
}