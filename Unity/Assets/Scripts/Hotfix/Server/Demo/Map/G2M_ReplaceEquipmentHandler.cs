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
                    response.Message = "没有待替换的装备";
                    return;
                }

                // 获取缓存的装备和槽位
                Equipment tempEquipment = tempCacheComponent.GetTempEquipment(playerId);
                int tempSlotIndex = tempCacheComponent.GetTempSlotIndex(playerId);
                
                if (tempEquipment == null)
                {
                    response.Error = ErrorCode.ERR_TempEquipmentNull;
                    response.Message = "临时装备为空";
                    return;
                }

                // 验证请求的装备ID和槽位是否匹配缓存
                if (tempEquipment.Id != equipmentId || tempSlotIndex != slotIndex)
                {
                    response.Error = ErrorCode.ERR_EquipmentMismatch;
                    response.Message = "装备信息不匹配";
                    return;
                }

                // 验证槽位索引（UIMain固定9个槽位：0-8）
                if (slotIndex < 0 || slotIndex >= 9)
                {
                    response.Error = ErrorCode.ERR_SlotIndexInvalid;
                    response.Message = "槽位索引无效";
                    return;
                }

                // 获取玩家装备服务
                PlayerEquipmentService playerEquipmentService = scene.GetComponent<PlayerEquipmentService>();
                if (playerEquipmentService == null)
                {
                    response.Error = ErrorCode.ERR_ComponentNotFound;
                    response.Message = "玩家装备服务不存在";
                    return;
                }

                // 替换装备到内存
                playerEquipmentService.SetEquippedEquipment(playerId, slotIndex, tempEquipment);
                
                // 保存装备到数据库
                await SaveEquipmentToDatabase(scene, playerId, slotIndex, tempEquipment);
                
                // 清除临时缓存
                tempCacheComponent.ClearTempEquipment(playerId);
                
                response.Success = true;
                response.Error = ErrorCode.ERR_Success;
                response.Message = "装备替换成功";
                
                Log.Info($"装备替换成功: PlayerId={playerId}, EquipmentId={equipmentId}, SlotIndex={slotIndex}");
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
        
        /// <summary>
        /// 保存装备到数据库
        /// </summary>
        private static async ETTask SaveEquipmentToDatabase(Scene scene, long playerId, int slotIndex, Equipment equipment)
        {
            try
            {
                // 获取数据库组件
                DBManagerComponent dbManagerComponent = scene.Root().GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("DBManagerComponent not found");
                    return;
                }

                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                if (dbComponent == null)
                {
                    Log.Error($"DBComponent not found for zone {scene.Zone()}");
                    return;
                }

                // 构造复合主键ID：playerId + slotIndex
                // 使用位移操作确保唯一性：playerId左移16位 + slotIndex
                long compositeId = (playerId << 16) + slotIndex;

                // 创建装备信息实体，使用AddChildWithId设置ID
                UserEquipmentInfo equipmentInfo = scene.AddChildWithId<UserEquipmentInfo, long, int>(compositeId, playerId, slotIndex);
                equipmentInfo.SetFromEquipment(equipment);

                // 保存到数据库，使用自定义集合名
                await dbComponent.Save(equipmentInfo, "ET.Server.User.Equipment.info");
                
                Log.Info($"装备保存到数据库成功: PlayerId={playerId}, SlotIndex={slotIndex}, EquipmentId={equipment.Id}");
            }
            catch (System.Exception e)
            {
                Log.Error($"保存装备到数据库失败: PlayerId={playerId}, SlotIndex={slotIndex}, Error={e.Message}");
                throw;
            }
        }
    }
}