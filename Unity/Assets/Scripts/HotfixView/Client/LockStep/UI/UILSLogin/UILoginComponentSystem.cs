using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILSLoginComponent))]
    [FriendOf(typeof(UILoginComponent))]
    [FriendOfAttribute(typeof(ET.Client.UILSLoginComponent))]
    public static partial class UILSLoginComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILSLoginComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.loginBtn = rc.Get<GameObject>("LoginBtn");

            self.loginBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnLogin(); });
            self.account = rc.Get<GameObject>("Account");
            self.password = rc.Get<GameObject>("Password");
        }


        public static void OnLogin(this UILSLoginComponent self)
        {
            string account = self.account.GetComponent<InputField>().text;
            string password = self.password.GetComponent<InputField>().text;
            
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
                return;
                
            self.LoginAsync(account, password).Coroutine();
        }

        private static async ETTask LoginAsync(this UILSLoginComponent self, string account, string password)
        {
            try
            {
                self.Root().RemoveComponent<ClientSenderComponent>();
                ClientSenderComponent clientSenderComponent = self.Root().AddComponent<ClientSenderComponent>();
                
                long playerId = await clientSenderComponent.LoginAsync(account, password);
                self.Root().GetComponent<PlayerComponent>().MyId = playerId;
                await EventSystem.Instance.PublishAsync(self.Root(), new LoginFinish());
            }
            catch (System.Exception ex)
            {
                Log.Error($"登录异常: {ex.Message}");
            }
        }
    }
}
