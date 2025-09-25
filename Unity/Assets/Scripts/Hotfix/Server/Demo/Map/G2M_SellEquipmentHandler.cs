namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_SellEquipmentHandler : MessageHandler<Scene, G2M_SellEquipment, M2G_SellEquipment>
    {
        protected override async ETTask Run(Scene scene, G2M_SellEquipment request, M2G_SellEquipment response)
        {
            try
            {
                long playerId = request.PlayerId;
                long equipmentId = request.EquipmentId;
                
                Log.Info($"收到装备出售请求: PlayerId={playerId}, EquipmentId={equipmentId}");
                
                // 获取装备缓存组件
                EquipmentTempCacheComponent equipmentCache = scene.GetComponent<EquipmentTempCacheComponent>();
                if (equipmentCache == null)
                {
                    response.Error = ErrorCode.ERR_CacheComponentNotFound;
                    response.Message = "装备缓存组件未找到";
                    return;
                }
                
                Equipment equipmentToSell = null;
                
                // 首先检查临时缓存中是否有这个装备
                Equipment tempEquipment = equipmentCache.GetTempEquipment(playerId);
                if (tempEquipment != null && tempEquipment.Id == equipmentId)
                {
                    equipmentToSell = tempEquipment;
                    // 从临时缓存中移除
                    equipmentCache.ClearTempEquipment(playerId);
                }
                else
                {
                    // 如果不在临时缓存中，检查已装备的装备
                    PlayerEquipmentService playerEquipmentService = scene.GetComponent<PlayerEquipmentService>();
                    if (playerEquipmentService == null)
                    {
                        response.Error = ErrorCode.ERR_PlayerEquipmentServiceNotFound;
                        response.Message = "玩家装备服务未找到";
                        return;
                    }
                    
                    // 查找装备在哪个槽位
                    var playerEquipments = playerEquipmentService.GetPlayerEquipments(playerId);
                    foreach (var kvp in playerEquipments)
                    {
                        if (kvp.Value != null && kvp.Value.Id == equipmentId)
                        {
                            equipmentToSell = kvp.Value;
                            // 从装备槽中移除
                            playerEquipmentService.SetEquippedEquipment(playerId, kvp.Key, null);
                            break;
                        }
                    }
                }
                
                if (equipmentToSell == null)
                {
                    response.Error = ErrorCode.ERR_EquipmentNotFound;
                    response.Message = "未找到指定装备";
                    return;
                }
                
                // 出售装备并获得奖励
                var (success, spiritStone, exp, levelChanged, newLevel, newMajorRealm, newMinorRealm, newCurrentExp, newSpiritStone) = await PlayerEquipmentServiceSystem.SellEquipment(equipmentToSell, playerId, scene);
                
                if (!success)
                {
                    response.Error = ErrorCode.ERR_SellEquipmentFailed;
                    response.Message = "装备出售失败";
                    return;
                }
                
                response.Success = true;
                response.SpiritStoneGained = spiritStone;
                response.ExpGained = exp;
                response.LevelChanged = levelChanged;
                response.NewLevel = newLevel;
                response.NewMajorRealm = newMajorRealm;
                response.NewMinorRealm = newMinorRealm;
                response.NewCurrentExp = newCurrentExp;
                response.NewSpiritStone = newSpiritStone;
                
                Log.Info($"装备出售成功: PlayerId={playerId}, 装备={equipmentToSell.Name}, 灵石+{spiritStone}, 经验+{exp}");
            }
            catch (System.Exception e)
            {
                Log.Error($"装备出售处理异常: {e.Message}");
                response.Error = ErrorCode.ERR_SellEquipmentFailed;
                response.Message = "装备出售失败";
            }
        }
    }
}