using System;
using System.Net;


namespace ET.Server
{
	[MessageSessionHandler(SceneType.Realm)]
	public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login>
	{
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response)
		{
			if (string.IsNullOrEmpty(request.Account) || string.IsNullOrEmpty(request.Password))
			{
				response.Error = ErrorCode.ERR_AccountNameFormError;
				return;
			}
			
			Scene scene = session.Scene();
			
			try
			{
				Account account = await AccountSystem.GetAccountByName(scene, request.Account);
				
				string accountUUID = "";
				if (account != null)
				{
					if (account.Password != request.Password)
					{
						response.Error = ErrorCode.ERR_PasswordError;
						response.Message = "密码错误";
						return;
					}
					
					accountUUID = account.AccountUUID;
					if (string.IsNullOrEmpty(accountUUID))
					{
						accountUUID = System.Guid.NewGuid().ToString();
						account.AccountUUID = accountUUID;
						
						DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
						DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
						await dbComponent.Save(account, "ET.Server.AccountInfo");
					}
				}
				else
				{
					response.Error = ErrorCode.ERR_AccountNotExist;
					response.Message = "账号不存在，请先注册";
					return;
				}
				
				SessionPlayerComponent playerComp = session.AddComponent<SessionPlayerComponent>();
				playerComp.Account = request.Account;
				playerComp.Password = request.Password;
				playerComp.AccountUUID = accountUUID;
				
				response.AccountUUID = accountUUID;
				response.Error = ErrorCode.ERR_Success;
			}
			catch (System.Exception e)
			{
				Log.Error($"登录处理失败: {e}");
				response.Error = ErrorCode.ERR_SystemError;
				response.Message = "登录处理失败";
			}
			
			await ETTask.CompletedTask;
		}

		private async ETTask CloseSession(Session session)
		{
			await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
			session.Dispose();
		}
	}
}
