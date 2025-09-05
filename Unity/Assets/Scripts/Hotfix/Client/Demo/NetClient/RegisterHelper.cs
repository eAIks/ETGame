namespace ET.Client
{
    public static class RegisterHelper
    {
        public static async ETTask<int> Register(Scene root, string account, string password)
        {
            root.RemoveComponent<ClientSenderComponent>();
            
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            
            int errorCode = await clientSenderComponent.RegisterAsync(account, password);

            if (errorCode == ErrorCode.ERR_Success)
            {
                // 注册成功后自动登录
                long playerId = await clientSenderComponent.LoginAsync(account, password);
                root.GetComponent<PlayerComponent>().MyId = playerId;
                await EventSystem.Instance.PublishAsync(root, new LoginFinish());
            }
            
            return errorCode;
        }
    }
}