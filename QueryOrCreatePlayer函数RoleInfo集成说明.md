# QueryOrCreatePlayerByAccountAndServerId函数RoleInfo集成说明

## 修改概述

已成功在`QueryOrCreatePlayerByAccountAndServerId`函数中集成RoleInfo的创建和存储功能。

## 修改位置

**文件**: `/Unity/Assets/Scripts/Hotfix/Server/Demo/Map/PlayerDataService.cs`
**函数**: `QueryOrCreatePlayerByAccountAndServerId`

## 修改内容

在创建新角色成功后，添加了RoleInfo初始化和数据库存储逻辑：

```csharp
// 原有的PlayerData创建和保存逻辑
if (newPlayerData != null)
{
    Log.Info($"PlayerDataService: 开始保存新角色到数据库，表名={tableName}");
    await dbComponent.Save(newPlayerData, tableName);
    Log.Info($"PlayerDataService: 成功创建并保存新角色: Account={account}, PlayerId={playerId}, ServerId={serverId}");
    
    // 新增：创建新角色成功后，初始化RoleInfo数据并存储到ET.Server.User.RoleInfo表
    Log.Info($"PlayerDataService: 开始创建RoleInfo数据，PlayerId={playerId}");
    RoleInfo roleInfo = await RoleInfoDBSystem.CreateRoleFromAccountServerInfo(scene, account, serverId);
    if (roleInfo != null)
    {
        Log.Info($"PlayerDataService: 成功创建并保存RoleInfo数据: PlayerId={roleInfo.PlayerId}, 昵称={roleInfo.NickName}");
    }
    else
    {
        Log.Error($"PlayerDataService: 创建RoleInfo数据失败，但PlayerData已创建成功");
    }
}
```

## 执行流程

当调用`QueryOrCreatePlayerByAccountAndServerId`函数时：

1. **查询现有角色**: 首先查询是否已存在角色数据
2. **返回现有角色**: 如果存在，更新最后登录时间并返回
3. **创建新角色**: 如果不存在，则：
   - 创建PlayerData实体并保存到`ET.Server.User.{serverId}`表
   - **新增**: 创建RoleInfo实体并保存到`ET.Server.User.RoleInfo`表
   - 返回创建的PlayerData

## RoleInfo初始化数据

当创建新角色时，RoleInfo会包含以下初始化数据：
- **PlayerId**: 从AccountServerInfo获取的PlayerId
- **NickName**: 默认值为"凡人{PlayerId}"
- **MajorRealm**: 0（大境界）
- **MinorRealm**: 0（小境界）
- **Level**: 0（等级）
- **CauldronLevel**: 0（鼎炉等级）
- **CurrentExp**: 0（当前经验值）
- **SpiritStone**: 0（灵石数量）
- **CreateTime**: 当前时间戳
- **LastUpdateTime**: 当前时间戳

## 数据库表

- **PlayerData表**: `ET.Server.User.{serverId}` (原有逻辑)
- **RoleInfo表**: `ET.Server.User.RoleInfo` (新增)

## 日志记录

修改后的函数会产生详细的日志记录：
- RoleInfo创建开始日志
- RoleInfo创建成功日志（包含PlayerId和昵称）
- RoleInfo创建失败日志（如果发生）

## 异常处理

- 如果RoleInfo创建失败，会记录错误日志但不会影响PlayerData的创建
- PlayerData和RoleInfo的创建是独立的操作，互不依赖
- 确保即使RoleInfo创建失败，玩家仍能正常进入游戏

## 验证

项目已通过编译验证，所有修改符合ET框架规范。