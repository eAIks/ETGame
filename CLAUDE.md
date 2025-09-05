# CLAUDE.md

此文件为 Claude Code (claude.ai/code) 提供在此代码库中工作的指导。

## 项目概述

ETGame 是一个基于 Unity 的游戏框架，采用 C#/.NET 的客户端-服务器架构。它通过 HybridCLR 实现热更新功能，使用基于 Fiber 的 Actor 并发模型，以及实体组件系统（ECS）架构。

## 构建命令

### 服务端/DotNet 构建
```bash
# 构建整个解决方案
dotnet build

# 以 Release 模式构建
dotnet build -c Release

# 发布 Linux x64 版本
dotnet publish -r linux-x64 --no-self-contained --no-dependencies -c Release
```

### Unity 构建
Unity 项目位于 `Unity/` 目录。使用 Unity 编辑器构建客户端。

## 架构

### 核心组件

1. **实体组件系统 (ECS)**
   - 基类：`Entity`、`Scene`、`Component`
   - 位置：`Unity/Assets/Scripts/Core/Entity/`
   - 系统类型：`IAwakeSystem`、`IUpdateSystem`、`IDestroySystem` 等

2. **基于 Fiber 的 Actor 模型**
   - 主要类：`Fiber`、`MailBoxComponent`、`ProcessInnerSender`
   - 位置：`Unity/Assets/Scripts/Core/Fiber/`
   - 提供基于 Actor 的并发和消息传递

3. **网络层**
   - 传输协议：TCP (`TChannel/TService`)、KCP (`KChannel/KService`)、WebSocket (`WChannel/WService`)
   - 位置：`Unity/Assets/Scripts/Core/Network/`
   - 使用 MemoryPack 进行消息序列化

4. **热更新架构**
   - Unity 端：`Unity.Model`、`Unity.Hotfix`、`Unity.HotfixView`
   - 服务器端：`DotNet.Model`、`DotNet.Hotfix`
   - 使用 Loader 模式进行动态代码加载

### 项目结构

```
ETGame/
├── Unity/              # Unity 客户端项目
│   ├── Assets/
│   │   └── Scripts/
│   │       ├── Core/   # 核心框架（不可热更）
│   │       ├── Model/  # 共享逻辑（可热更）
│   │       ├── ModelView/  # 客户端特定的视图逻辑
│   │       ├── Hotfix/ # 服务端逻辑（可热更）
│   │       └── HotfixView/ # 客户端逻辑（可热更）
│   └── *.csproj 文件
├── DotNet/            # .NET 服务端项目
│   ├── App/           # 入口点
│   ├── Core/          # 核心框架
│   ├── Model/         # 共享模型
│   ├── Hotfix/        # 服务端逻辑
│   ├── Loader/        # 代码加载器
│   └── ThirdParty/    # 第三方库
├── Share/             # Unity 和 DotNet 共享
│   ├── Analyzer/      # Roslyn 分析器
│   └── SourceGenerator/ # 源代码生成器
├── Config/            # 配置文件
│   ├── Excel/         # 基于 Excel 的配置
│   └── Json/          # JSON 配置
└── Tools/             # 构建和实用工具
```

### 关键特性和分析器

- `[ChildOf]` - 标记只能作为特定父类型子对象的实体
- `[ComponentOf]` - 限制组件只能附加到特定实体类型
- `[EnableMethod]` - 在实体上启用特殊方法处理
- `[FriendOf]` - 授予访问指定类型私有成员的权限
- `[EntitySystemOf]` - 将系统与特定组件类型关联

### 开发约定

1. **组件生命周期**：组件遵循 Awake → Update → Destroy 生命周期
2. **命名规范**：系统命名为 `组件名System`，对应其关联的组件
3. **热更新边界**：Model/Hotfix 文件夹中的代码可以热更新，Core 不可以
4. **消息协议**：使用 MemoryPack 进行序列化，消息继承自 `MessageObject`
5. **Actor 模式**：长期运行的实体使用 `MailBoxComponent` 进行消息处理
6. **Fiber 上下文**：每个 Scene 在自己的 Fiber（轻量级线程）中运行

## 开发说明

- C# 版本：11.0
- 禁用警告：0169, 0649, 3021, 8981
- Unity 使用 HybridCLR 进行热更新
- 服务端使用 NLog 记录日志
- 实体 ID 通过 `IdGenerater` 管理，不同实体类型使用不同的 ID 范围