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
                    response.Error = ErrorCode.ERR_SystemError;
                    response.Message = "Map服务器场景无效";
                    return;
                }

                // 随机生成装备槽位
                var random = new System.Random();
                int randomSlot = random.Next(0, 9);
                
                // 在Map服务器生成随机装备
                Equipment newEquipment = GenerateRandomEquipment(randomSlot);
                
                if (newEquipment == null)
                {
                    response.Error = ErrorCode.ERR_SystemError;
                    response.Message = "装备生成失败";
                    return;
                }
                
                // 获取或创建装备临时缓存组件
                EquipmentTempCacheComponent tempCacheComponent = scene.GetComponent<EquipmentTempCacheComponent>();
                if (tempCacheComponent == null)
                {
                    tempCacheComponent = scene.AddComponent<EquipmentTempCacheComponent>();
                }
                
                // 缓存生成的装备
                tempCacheComponent.CacheEquipment(request.PlayerId, newEquipment, randomSlot);
                
                // 转换为EquipmentProto并返回
                response.Equipment = ConvertToEquipmentProto(newEquipment);
                response.SlotIndex = randomSlot;
                response.Error = ErrorCode.ERR_Success;
            }
            catch (System.Exception e)
            {
                Log.Error($"Map服务器生成装备异常: {e.Message}");
                response.Error = ErrorCode.ERR_SystemError;
                response.Message = "装备生成失败";
            }
            
            await ETTask.CompletedTask;
        }
        
        private static Equipment GenerateRandomEquipment(int slot)
        {
            var random = new System.Random();
            var equipment = new Equipment
            {
                Id = TimeInfo.Instance.ServerFrameTime(),
                Name = $"装备{random.Next(1, 100)}",
                SlotType = slot,
                Attack = random.Next(10, 100),
                Defense = random.Next(5, 50),
                Health = random.Next(50, 200),
                Quality = random.Next(0, 5),
                Level = random.Next(1, 101),
                Icon = "icon_equipment",
                Description = "随机生成的装备"
            };
            return equipment;
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
            return proto;
        }
    }
}