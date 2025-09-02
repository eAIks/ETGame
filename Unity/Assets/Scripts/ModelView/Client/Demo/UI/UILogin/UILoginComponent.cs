using UnityEngine;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILoginComponent: Entity, IAwake
	{
		public GameObject account;
		public GameObject password;
		public GameObject loginBtn;
		public GameObject registerBtn;
		public GameObject reAccount;
		public GameObject rePassword;
		public GameObject reRegistrBtn;
		public GameObject returnBtn;
		public GameObject loginPanel;
		public GameObject registerPanel;
	}
}
