using Unity.Mathematics;

namespace ET.Client
{
	// 暂时禁用UnitComponent相关功能
	// [MessageHandler(SceneType.Demo)]
	public class M2C_StopHandler : MessageHandler<Scene, M2C_Stop>
	{
		protected override async ETTask Run(Scene root, M2C_Stop message)
		{
			// UnitComponent暂时未使用，忽略停止移动消息
			Log.Warning("M2C_StopHandler: UnitComponent暂时未使用，忽略消息");
			await ETTask.CompletedTask;
		}
	}
}
