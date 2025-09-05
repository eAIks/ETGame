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

            LoginHelper.Login(self.Root(), accountText, passwordText).Coroutine();
        }

        public static void OnRegister(this UILoginComponent self)
        {
            string accountText = self.reAccount.GetComponent<InputField>().text;
            string passwordText = self.rePassword.GetComponent<InputField>().text;

            if (!self.IsValidAccount(accountText))
            {
                Log.Warning("账号格式不正确：应为3-20个字符，只能包含字母、数字和下划线");
                return;
            }

            if (!self.IsValidPassword(passwordText))
            {
                Log.Warning("密码格式不正确：应为6-20个字符");
                return;
            }

            // // 禁用UI防止重复点击
            // self.SetUIInteractable(false);

            self.RegisterAsync().Coroutine();
        }

        private static async ETTask RegisterAsync(this UILoginComponent self)
        {
            string accountText = self.reAccount.GetComponent<InputField>().text;
            string passwordText = self.rePassword.GetComponent<InputField>().text;

            try
            {
                Log.Info("正在注册...");

                // 创建客户端发送组件
                self.Root().RemoveComponent<ClientSenderComponent>();
                ClientSenderComponent clientSenderComponent = self.Root().AddComponent<ClientSenderComponent>();

                // 移除等待时间，问题可能在其他地方
                
                // 调用注册方法
                int errorCode = await clientSenderComponent.RegisterAsync(accountText, passwordText);

                switch (errorCode)
                {
                    case ErrorCode.ERR_Success:
                        Log.Info("注册成功，自动登录中...");
                        // 注册成功后自动登录
                        long playerId = await clientSenderComponent.LoginAsync(accountText, passwordText);
                        self.Root().GetComponent<PlayerComponent>().MyId = playerId;
                        await EventSystem.Instance.PublishAsync(self.Root(), new LoginFinish());
                        break;
                    case ErrorCode.ERR_AccountAlreadyRegister:
                        Log.Warning("账号已存在，请更换账号");
                        self.SetUIInteractable(true);
                        break;
                    case ErrorCode.ERR_AccountNameFormError:
                        Log.Warning("账号格式不正确");
                        self.SetUIInteractable(true);
                        break;
                    case ErrorCode.ERR_PasswordFormError:
                        Log.Warning("密码格式不正确");
                        self.SetUIInteractable(true);
                        break;
                    default:
                        Log.Warning($"注册失败，错误代码: {errorCode}");
                        self.SetUIInteractable(true);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"注册过程中发生异常: {ex.Message}");
                self.SetUIInteractable(true);
            }
        }

        public static void OnShowRegisterPanel(this UILoginComponent self)
        {
            Log.Info("显示注册页面");

            self.loginPanel.SetActive(false);

            self.registerPanel.SetActive(true);
        }

        public static void OnShowLoginPanel(this UILoginComponent self)
        {
            Log.Info("显示登录页面");
            self.loginPanel.SetActive(true);

            self.registerPanel.SetActive(false);

            // 清空注册界面输入
            self.ClearRegisterInput();
        }

 

        public static bool IsValidAccount(this UILoginComponent self, string account)
        {
            if (string.IsNullOrEmpty(account))
                return false;

            if (account.Length < 3 || account.Length > 20)
                return false;

            // 只允许字母、数字和下划线
            for (int i = 0; i < account.Length; i++)
            {
                char c = account[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            return true;
        }

        public static bool IsValidPassword(this UILoginComponent self, string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (password.Length < 6 || password.Length > 20)
                return false;

            return true;
        }

        public static void ClearRegisterInput(this UILoginComponent self)
        {
            if (self.reAccount != null)
            {
                self.reAccount.GetComponent<InputField>().text = "";
            }

            if (self.rePassword != null)
            {
                self.rePassword.GetComponent<InputField>().text = "";
            }
        }

        public static void SetUIInteractable(this UILoginComponent self, bool interactable)
        {
            if (self.reRegistrBtn != null)
            {
                self.reRegistrBtn.GetComponent<Button>().interactable = interactable;
            }

            if (self.returnBtn != null)
            {
                self.returnBtn.GetComponent<Button>().interactable = interactable;
            }

            if (self.reAccount != null)
            {
                self.reAccount.GetComponent<InputField>().interactable = interactable;
            }

            if (self.rePassword != null)
            {
                self.rePassword.GetComponent<InputField>().interactable = interactable;
            }
        }
    }
}