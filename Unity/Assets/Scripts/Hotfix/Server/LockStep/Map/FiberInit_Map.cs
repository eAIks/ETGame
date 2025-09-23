using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Map)]
    public class FiberInit_Map: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            
            // 添加数据库管理组件，用于角色数据查询/创建
            root.AddComponent<DBManagerComponent>();
            
            // 添加玩家装备服务组件
            root.AddComponent<PlayerEquipmentService>();
            
            // 移除Unit相关组件，Map服务器专注于角色数据处理
            // root.AddComponent<UnitComponent>();
            // root.AddComponent<AOIManagerComponent>();
            // root.AddComponent<RoomManagerComponent>();
            // root.AddComponent<LocationProxyComponent>();
            // root.AddComponent<MessageLocationSenderComponent>();

            await ETTask.CompletedTask;
        }
    }
}