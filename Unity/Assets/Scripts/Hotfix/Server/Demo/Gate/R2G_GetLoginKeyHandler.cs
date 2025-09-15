using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class R2G_GetLoginKeyHandler : MessageHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			long key = RandomGenerator.RandInt64();
			// 存储账号和UUID的映射关系
			scene.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
			
			// 如果有AccountUUID，也存储UUID与key的映射关系
			if (!string.IsNullOrEmpty(request.AccountUUID))
			{
				// 可以在这里添加UUID到key的映射逻辑，如果需要的话
				Log.Info($"Gate received AccountUUID: {request.AccountUUID} for account: {request.Account}");
			}
			
			response.Key = key;
			response.GateId = scene.Id;
			response.PlayerID = request.PlayerID;  // 将PlayerID返回给Realm
			
			Log.Info($"Gate generated login key for Account={request.Account}, PlayerID={request.PlayerID}, Key={key}");
			await ETTask.CompletedTask;
		}
	}
}