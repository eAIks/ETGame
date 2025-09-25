# RoleInfo角色系统实现说明

## 概述

已成功实现基于AccountServerInfo中PlayerId的角色创建和管理系统，角色数据保存在`ET.Server.User.RoleInfo`数据库表中。

## 实现的功能

### 1. RoleInfo数据模型
**文件位置**: `/Unity/Assets/Scripts/Model/Server/Demo/User/RoleInfo.cs`

包含以下字段（主键为PlayerId）：
- `PlayerId` (long): 玩家唯一ID，作为主键
- `NickName` (string): 角色昵称，默认值为"凡人+PlayerId"
- `MajorRealm` (int): 大境界等级，初始值为0
- `MinorRealm` (int): 小境界等级，初始值为0
- `Level` (int): 角色等级，初始值为0
- `CauldronLevel` (int): 鼎炉等级，初始值为0
- `CurrentExp` (long): 当前经验值，初始值为0
- `SpiritStone` (long): 灵石数量，初始值为0
- `CreateTime` (long): 角色创建时间
- `LastUpdateTime` (long): 最后更新时间

### 2. 角色管理系统方法
**文件位置**: `/Unity/Assets/Scripts/Hotfix/Server/Demo/User/RoleInfoSystem.cs`

提供的主要功能：
- 设置昵称: `SetNickName(string nickName)`
- 增加经验值: `AddExp(long exp)`
- 增加灵石: `AddSpiritStone(long amount)`
- 设置境界: `SetRealm(int majorRealm, int minorRealm)`
- 升级: `LevelUp()`
- 升级鼎炉: `UpgradeCauldron()`
- 更新时间: `UpdateLastTime()`

### 3. 数据库操作系统
**文件位置**: `/Unity/Assets/Scripts/Hotfix/Server/Demo/User/RoleInfoSystem.cs`

核心数据库操作方法：

#### 角色创建
```csharp
// 使用AccountServerInfo中的PlayerId创建角色
RoleInfo roleInfo = await RoleInfoDBSystem.CreateRoleFromAccountServerInfo(scene, account, serverId);
```

#### 角色查询
```csharp
// 方法1: 根据Account和ServerId查询
RoleInfo roleInfo = await RoleInfoDBSystem.GetRoleInfoByAccountAndServerId(scene, account, serverId);

// 方法2: 根据PlayerId查询
RoleInfo roleInfo = await RoleInfoDBSystem.GetRoleInfoByPlayerId(scene, playerId);
```

#### 角色更新
```csharp
// 更新角色信息到数据库
bool success = await RoleInfoDBSystem.UpdateRoleInfo(scene, roleInfo);
```

#### 角色删除
```csharp
// 删除角色信息
bool success = await RoleInfoDBSystem.DeleteRoleInfo(scene, playerId);
```

### 4. 数据库索引初始化
**文件位置**: `/Unity/Assets/Scripts/Hotfix/Server/Demo/User/RoleInfoDBInitializer.cs`

在服务器启动时调用以确保数据库索引正确设置：
```csharp
await RoleInfoDBInitializer.InitializeRoleInfoCollection(dbComponent);
```

创建的索引：
- PlayerId唯一索引（主键）
- NickName普通索引（便于昵称查询）

### 5. 测试示例
**文件位置**: `/Unity/Assets/Scripts/Hotfix/Server/Demo/User/RoleInfoTestExample.cs`

提供完整的测试方法：
```csharp
// 测试角色创建
await RoleInfoTestExample.TestCreateRole(scene, "testAccount", 1);

// 测试角色查询
await RoleInfoTestExample.TestQueryRole(scene, "testAccount", 1);

// 完整测试流程
await RoleInfoTestExample.RunFullRoleTest(scene, "testAccount", 1);
```

## 使用流程

### 1. 角色创建流程
1. 从`ET.Server.AccountServerInfo`表中根据Account和ServerId获取PlayerId
2. 检查`ET.Server.User.RoleInfo`表中是否已存在该PlayerId的角色
3. 如果不存在，创建新角色（昵称默认为"凡人+PlayerId"）
4. 保存角色信息到数据库

### 2. 角色数据管理
- 所有字段（除时间字段外）初始值都为0
- 昵称默认为"凡人+PlayerId"
- 每次修改数据时自动更新`LastUpdateTime`
- 提供便捷的方法进行经验值、灵石、等级等操作

### 3. 数据库表结构
- 表名: `ET.Server.User.RoleInfo`
- 主键: `PlayerId`
- 自动维护创建时间和更新时间

## 注意事项

1. **数据库索引**: 确保在服务器启动时调用`RoleInfoDBInitializer.InitializeRoleInfoCollection()`来初始化数据库索引
2. **权限控制**: 使用了`[FriendOf]`属性来控制对实体字段的访问
3. **异常处理**: 所有数据库操作都包含完整的异常处理和日志记录
4. **环形依赖**: 将索引初始化分离到独立的类中避免环形依赖问题
5. **实体管理**: 角色实体使用ET框架的实体生命周期管理

## 编译验证

项目已通过编译验证，所有代码符合ET框架的编码规范和分析器要求。