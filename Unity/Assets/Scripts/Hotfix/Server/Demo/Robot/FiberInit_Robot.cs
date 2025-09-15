namespace ET.Client
{
    [Invoke((long)SceneType.Robot)]
    public class FiberInit_Robot: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();
            root.AddComponent<ObjectWait>();
            
            root.SceneType = SceneType.Demo;

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
            
            root.RemoveComponent<ClientSenderComponent>();
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            long playerId = await clientSenderComponent.LoginAsync(root.Name, "");
            root.GetComponent<PlayerComponent>().MyId = playerId;
            
            await EnterMapHelper.EnterMapAsync(root);
            
            root.AddComponent<AIComponent, int>(1);
        }
    }
}