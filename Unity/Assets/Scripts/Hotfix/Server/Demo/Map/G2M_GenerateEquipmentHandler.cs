namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_GenerateEquipmentHandler : MessageHandler<Scene, G2M_GenerateEquipment, M2G_GenerateEquipment>
    {
        protected override async ETTask Run(Scene scene, G2M_GenerateEquipment request, M2G_GenerateEquipment response)
        {
            try
            {
                if (scene == null)
                {
                    response.Error = ErrorCode.ERR_InternalError;
                    response.Message = "Map服务器场景无效";
                    return;
                }

                // 获取装备生成器组件
                var equipmentGenerator = scene.GetComponent<EquipmentGeneratorComponent>();
                if (equipmentGenerator == null)
                {
                    equipmentGenerator = scene.AddComponent<EquipmentGeneratorComponent>();
                }
                
                // 使用装备生成器生成装备（基于鼎炉等级）
                bool success = await equipmentGenerator.GenerateEquipmentForPlayer(request.PlayerId);
                if (!success)
                {
                    response.Error = ErrorCode.ERR_GenerateEquipmentFailed;
                    response.Message = "装备生成失败";
                    return;
                }
                
                // 获取生成的装备信息
                var equipmentTempCache = scene.GetComponent<EquipmentTempCacheComponent>();
                if (equipmentTempCache == null)
                {
                    response.Error = ErrorCode.ERR_ComponentNotFound;
                    response.Message = "装备缓存不存在";
                    return;
                }
                
                var equipment = equipmentTempCache.GetTempEquipment(request.PlayerId);
                var slotIndex = equipmentTempCache.GetTempSlotIndex(request.PlayerId);
                
                if (equipment == null)
                {
                    response.Error = ErrorCode.ERR_EquipmentNotFound;
                    response.Message = "装备未找到";
                    return;
                }
                
                // 转换为EquipmentProto并返回
                response.Equipment = ConvertToEquipmentProto(equipment);
                response.SlotIndex = slotIndex;
                response.Error = ErrorCode.ERR_Success;
                response.Message = "装备生成成功";
                
                Log.Info($"为玩家生成装备成功: PlayerId={request.PlayerId}, Equipment={equipment.Name}, SlotIndex={slotIndex}");
            }
            catch (System.Exception e)
            {
                Log.Error($"Map服务器生成装备异常: {e.Message}");
                response.Error = ErrorCode.ERR_InternalError;
                response.Message = "装备生成失败";
            }
            
            await ETTask.CompletedTask;
        }
        
        private static EquipmentProto ConvertToEquipmentProto(Equipment equipment)
        {
            var proto = EquipmentProto.Create();
            proto.Id = equipment.Id;
            proto.Name = equipment.Name;
            proto.Level = equipment.Level;
            proto.Attack = equipment.Attack;
            proto.Defense = equipment.Defense;
            proto.Health = equipment.Health;
            proto.Quality = equipment.Quality;
            proto.EquipType = equipment.SlotType;
            proto.Color = equipment.Color; // 设置装备品质颜色
            return proto;
        }
    }
}