using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class R2G_GetLoginKeyHandler : MessageHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			long key = RandomGenerator.RandInt64();
			// 存储账号、PlayerID、UUID和ServerId的映射关系
			scene.GetComponent<GateSessionKeyComponent>().Add(key, request.Account, request.PlayerID, request.AccountUUID, request.ServerId);
			
			response.Key = key;
			response.GateId = scene.Id;
			response.PlayerID = request.PlayerID;  // 将PlayerID返回给Realm
			
			Log.Info($"Gate generated login key for Account={request.Account}, PlayerID={request.PlayerID}, ServerId={request.ServerId}, Key={key}");
			await ETTask.CompletedTask;
		}
	}
}