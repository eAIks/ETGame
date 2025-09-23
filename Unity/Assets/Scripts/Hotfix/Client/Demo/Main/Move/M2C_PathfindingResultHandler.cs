namespace ET.Client
{
	// 暂时禁用UnitComponent相关功能
	// [MessageHandler(SceneType.Demo)]
	public class M2C_PathfindingResultHandler : MessageHandler<Scene, M2C_PathfindingResult>
	{
		protected override async ETTask Run(Scene root, M2C_PathfindingResult message)
		{
			// UnitComponent暂时未使用，忽略寻路结果消息
			Log.Warning("M2C_PathfindingResultHandler: UnitComponent暂时未使用，忽略消息");
			await ETTask.CompletedTask;
		}
	}
}
