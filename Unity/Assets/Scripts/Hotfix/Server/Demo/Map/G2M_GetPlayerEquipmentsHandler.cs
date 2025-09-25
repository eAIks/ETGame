using System;
using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_GetPlayerEquipmentsHandler : MessageHandler<Scene, G2M_GetPlayerEquipments, M2G_GetPlayerEquipments>
    {
        protected override async ETTask Run(Scene scene, G2M_GetPlayerEquipments request, M2G_GetPlayerEquipments response)
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
                // 获取数据库组件
                DBManagerComponent dbManagerComponent = scene.Root().GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    response.Error = ErrorCode.ERR_ComponentNotFound;
                    response.Message = "数据库管理组件不存在";
                    return;
                }

                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                if (dbComponent == null)
                {
                    response.Error = ErrorCode.ERR_ComponentNotFound;
                    response.Message = $"数据库组件不存在 Zone={scene.Zone()}";
                    return;
                }

                // 从数据库查询玩家的所有装备
                List<UserEquipmentInfo> equipmentInfos = await dbComponent.Query<UserEquipmentInfo>(
                    info => info.PlayerId == playerId, 
                    "ET.Server.User.Equipment.info"
                );

                // 初始化返回的列表
                response.Equipments = new List<EquipmentProto>();
                response.SlotIndexes = new List<int>();

                if (equipmentInfos != null && equipmentInfos.Count > 0)
                {
                    // 转换为EquipmentProto并添加到响应
                    foreach (UserEquipmentInfo equipmentInfo in equipmentInfos)
                    {
                        EquipmentProto equipmentProto = ConvertToEquipmentProto(equipmentInfo);
                        response.Equipments.Add(equipmentProto);
                        response.SlotIndexes.Add(equipmentInfo.SlotIndex);
                    }
                    
                    Log.Info($"从数据库加载玩家装备: PlayerId={playerId}, 装备数量={equipmentInfos.Count}");
                }
                else
                {
                    Log.Info($"玩家暂无装备: PlayerId={playerId}");
                }

                // 同步到内存装备服务
                await SyncToMemoryEquipmentService(scene, playerId, equipmentInfos);
                
                response.Error = ErrorCode.ERR_Success;
                response.Message = "获取装备成功";
            }
            catch (Exception e)
            {
                response.Error = ErrorCode.ERR_InternalError;
                response.Message = "服务器内部错误";
                Log.Error($"获取玩家装备异常: PlayerId={playerId}, Error={e.Message}");
            }
            
            await ETTask.CompletedTask;
        }
        
        /// <summary>
        /// 将UserEquipmentInfo转换为EquipmentProto
        /// </summary>
        private static EquipmentProto ConvertToEquipmentProto(UserEquipmentInfo equipmentInfo)
        {
            var proto = EquipmentProto.Create();
            proto.Id = equipmentInfo.EquipmentId;
            proto.Name = equipmentInfo.Name;
            proto.Level = equipmentInfo.Level;
            proto.Attack = equipmentInfo.Attack;
            proto.Defense = equipmentInfo.Defense;
            proto.Health = equipmentInfo.Health;
            proto.Quality = equipmentInfo.Quality;
            proto.EquipType = equipmentInfo.SlotType;
            
            // 从品质配置获取颜色信息
            var qualityConfig = EquipQualityConfigCategory.Instance.Get(equipmentInfo.Quality);
            proto.Color = qualityConfig?.Color ?? "#FFFFFF"; // 默认白色
            
            return proto;
        }
        
        /// <summary>
        /// 同步装备数据到内存装备服务
        /// </summary>
        private static async ETTask SyncToMemoryEquipmentService(Scene scene, long playerId, List<UserEquipmentInfo> equipmentInfos)
        {
            try
            {
                // 获取玩家装备服务
                PlayerEquipmentService playerEquipmentService = scene.GetComponent<PlayerEquipmentService>();
                if (playerEquipmentService == null)
                {
                    playerEquipmentService = scene.AddComponent<PlayerEquipmentService>();
                }

                // 确保玩家数据存在
                playerEquipmentService.EnsurePlayerData(playerId);

                // 清空现有装备并重新加载
                Dictionary<int, Equipment> playerEquipments = playerEquipmentService.GetPlayerEquipments(playerId);
                
                // 先清空所有槽位
                for (int i = 0; i < 9; i++)
                {
                    playerEquipments[i] = null;
                }

                // 从数据库数据重新填充
                if (equipmentInfos != null)
                {
                    foreach (UserEquipmentInfo equipmentInfo in equipmentInfos)
                    {
                        Equipment equipment = equipmentInfo.ToEquipment();
                        if (equipmentInfo.SlotIndex >= 0 && equipmentInfo.SlotIndex < 9)
                        {
                            playerEquipments[equipmentInfo.SlotIndex] = equipment;
                        }
                    }
                }
                
                Log.Info($"同步装备到内存成功: PlayerId={playerId}");
            }
            catch (Exception e)
            {
                Log.Error($"同步装备到内存失败: PlayerId={playerId}, Error={e.Message}");
            }
            
            await ETTask.CompletedTask;
        }
    }
}