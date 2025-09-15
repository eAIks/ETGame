using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILoginComponent))]
    [FriendOf(typeof(UILoginComponent))]
    public static partial class UILoginComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILoginComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            if (rc == null)
            {
                Log.Error("ReferenceCollector not found on UILogin GameObject");
                return;
            }

            // 登录相关UI元素
            self.loginBtn = rc.Get<GameObject>("LoginBtn");
            self.account = rc.Get<GameObject>("Account");
            self.password = rc.Get<GameObject>("Password");
            self.loginPanel = rc.Get<GameObject>("LoginPanel");
            self.loginBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnLogin(); });

            // 注册相关UI元素（可选）
            self.registerBtn = rc.Get<GameObject>("RegisterBtn");

            self.registerBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnShowRegisterPanel(); });

            // 注册界面UI元素（可选）
            self.reAccount = rc.Get<GameObject>("ReAccount");
            self.rePassword = rc.Get<GameObject>("RePassword");
            self.reRegistrBtn = rc.Get<GameObject>("ReRegisterBtn");
            self.returnBtn = rc.Get<GameObject>("ReturnBtn");
            self.registerPanel = rc.Get<GameObject>("RegisterPanel");

            self.reRegistrBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnRegister(); });

            self.returnBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnShowLoginPanel(); });

            self.loginPanel.SetActive(true);

            self.registerPanel.SetActive(false);
        }

        public static void OnLogin(this UILoginComponent self)
        {
            string accountText = self.account.GetComponent<InputField>().text;
            string passwordText = self.password.GetComponent<InputField>().text;

            if (string.IsNullOrEmpty(accountText) || string.IsNullOrEmpty(passwordText))
            {
                Log.Warning("请输入账号和密码");
                return;
            }

            self.LoginAsync(accountText, passwordText).Coroutine();
        }

        public static void OnRegister(this UILoginComponent self)
        {
            string accountText = self.reAccount.GetComponent<InputField>().text;
            string passwordText = self.rePassword.GetComponent<InputField>().text;

            if (!self.IsValidAccount(accountText) || !self.IsValidPassword(passwordText))
            {
                return;
            }

            self.RegisterAsync(accountText, passwordText).Coroutine();
        }

        private static async ETTask RegisterAsync(this UILoginComponent self, string account, string password)
        {
            try
            {
                self.Root().RemoveComponent<ClientSenderComponent>();
                ClientSenderComponent clientSenderComponent = self.Root().AddComponent<ClientSenderComponent>();
                
                int errorCode = await clientSenderComponent.RegisterAsync(account, password);

                if (errorCode == ErrorCode.ERR_Success)
                {
                    long playerId = await clientSenderComponent.LoginAsync(account, password);
                    self.Root().GetComponent<PlayerComponent>().MyId = playerId;
                    await EventSystem.Instance.PublishAsync(self.Root(), new LoginFinish());
                }
                else
                {
                    Log.Warning($"注册失败: {errorCode}");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"注册异常: {ex.Message}");
            }
        }

        private static async ETTask LoginAsync(this UILoginComponent self, string account, string password)
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

        public static void OnShowRegisterPanel(this UILoginComponent self)
        {
            self.loginPanel.SetActive(false);
            self.registerPanel.SetActive(true);
        }

        public static void OnShowLoginPanel(this UILoginComponent self)
        {
            self.loginPanel.SetActive(true);
            self.registerPanel.SetActive(false);
            self.ClearRegisterInput();
        }

 

        public static bool IsValidAccount(this UILoginComponent self, string account)
        {
            if (string.IsNullOrEmpty(account) || account.Length < 3 || account.Length > 20)
            {
                Log.Warning("账号格式不正确：应为3-20个字符，只能包含字母、数字和下划线");
                return false;
            }

            for (int i = 0; i < account.Length; i++)
            {
                char c = account[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    Log.Warning("账号格式不正确：应为3-20个字符，只能包含字母、数字和下划线");
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidPassword(this UILoginComponent self, string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6 || password.Length > 20)
            {
                Log.Warning("密码格式不正确：应为6-20个字符");
                return false;
            }
            return true;
        }

        public static void ClearRegisterInput(this UILoginComponent self)
        {
            if (self.reAccount != null)
                self.reAccount.GetComponent<InputField>().text = "";
            if (self.rePassword != null)
                self.rePassword.GetComponent<InputField>().text = "";
        }
    }
}