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
			self.loginBtn = rc.Get<GameObject>("LoginBtn");
			
			self.loginBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnLogin(); });
			self.account = rc.Get<GameObject>("Account");
			self.password = rc.Get<GameObject>("Password");
			self.registerBtn = rc.Get<GameObject>("RegisterBtn");
			self.registerBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnShowRegisterPanel(); });

			self.reAccount = rc.Get<GameObject>("ReAccount");
			self.rePassword = rc.Get<GameObject>("RePassword");
			self.reRegistrBtn = rc.Get<GameObject>("ReRegisterBtn");
			self.loginBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnRegister(); });

			self.returnBtn = rc.Get<GameObject>("ReturnBtn");
			self.returnBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnShowLoginPanel(); });

			self.loginPanel = rc.Get<GameObject>("LoginPanel");
			self.registerPanel = rc.Get<GameObject>("RegisterPanel");

		}

		
		public static void OnLogin(this UILoginComponent self)
		{
			LoginHelper.Login(
				self.Root(), 
				self.account.GetComponent<InputField>().text, 
				self.password.GetComponent<InputField>().text).Coroutine();
		}

		public static void OnRegister(this UILoginComponent self)
		{
			LoginHelper.Login(
				self.Root(), 
				self.reAccount.GetComponent<InputField>().text, 
				self.rePassword.GetComponent<InputField>().text).Coroutine();
		}
		
		public static void OnShowRegisterPanel(this UILoginComponent self)
		{
			Log.Error("显示注册页面");
			self.loginPanel.SetActive(false);
			self.registerPanel.SetActive(true);
		}

		public static void OnShowLoginPanel(this UILoginComponent self)
		{
			Log.Error("显示登录页面");
			self.loginPanel.SetActive(true);
			self.registerPanel.SetActive(false);
		}
	}
}
