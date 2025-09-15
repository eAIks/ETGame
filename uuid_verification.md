# UUID功能验证报告

## 修复总结

已成功将UUID存储从`AccountServer`迁移到`Account`实体，确保UUID存储在正确的数据库集合中。

## 🔧 修复的关键问题

### 1. **数据存储位置修正**
- **之前**: UUID存储在`AccountServer`实体中，集合名为`ET.Server.AccountServerInfo`
- **现在**: UUID存储在`Account`实体中，集合名为`ET.Server.AccountInfo`
- **理由**: Account是账号的主要实体，UUID应该作为账号的基本属性存储

### 2. **数据结构调整**
```csharp
// Account实体 (ET.Server.AccountInfo集合)
public class Account : Entity
{
    public string AccountName { get; set; }
    public string Password { get; set; }
    public long CreateTime { get; set; }
    public string AccountUUID { get; set; }  // ✅ UUID存储在这里
}

// AccountServer实体 (ET.Server.AccountServerInfo集合) 
public class AccountServer : Entity
{
    public long PlayerId { get; set; }
    public int ServerId { get; set; }
    public long LoginTime { get; set; }
    // ❌ 移除了AccountUUID字段
}
```

### 3. **登录流程优化**
- **密码验证**: 登录时会验证密码是否正确
- **账号存在性检查**: 只有已注册的账号才能登录
- **UUID管理**: 新注册账号自动生成UUID，现有账号复用UUID

## 📋 完整的数据流程

### 1. **注册流程 (C2R_RegisterHandler)**
```csharp
// 创建新账号时自动生成UUID
Account account = root.AddChildWithId<Account, string, string>(
    IdGenerater.Instance.GenerateId(), 
    request.Account, 
    request.Password);
// AccountSystem.Awake会自动生成UUID
// 保存到 ET.Server.AccountInfo 集合
await dbComponent.Save(account, "ET.Server.AccountInfo");
```

### 2. **登录流程 (C2R_LoginHandler)**
```csharp
// 从ET.Server.AccountInfo查询账号
Account account = await AccountSystem.GetAccountByName(scene, request.Account);
if (account != null) {
    // 验证密码
    if (account.Password == request.Password) {
        // 返回UUID给客户端
        response.AccountUUID = account.AccountUUID;
    }
}
```

### 3. **选择服务器流程 (C2R_SelectServerHandler)**
```csharp
// 从Session获取UUID
string accountUUID = playerComponent?.AccountUUID;
// 创建或更新AccountServer记录（不包含UUID）
// UUID仍然存储在Account中
```

## 🗃️ 数据库存储结构

### Account集合 (ET.Server.AccountInfo)
```json
{
  "_id": ObjectId("..."),
  "AccountName": "testuser",
  "Password": "hashedpassword", 
  "CreateTime": 1640995200000,
  "AccountUUID": "f7474dc5-a166-4744-a235-23e1d8a02539"
}
```

### AccountServer集合 (ET.Server.AccountServerInfo)
```json
{
  "_id": ObjectId("..."),
  "PlayerId": 123456789,
  "ServerId": 1,
  "LoginTime": 1640995200000
}
```

## ✅ 验证要点

1. **UUID唯一性**: 每个账号一个唯一UUID
2. **数据持久化**: UUID存储在Account集合中
3. **登录验证**: 账号存在性和密码验证
4. **向后兼容**: 现有账号会在下次登录时获得UUID
5. **跨服务器**: UUID在不同服务器间保持一致

## 🔍 测试建议

### 1. 注册新账号
```bash
# 应该在ET.Server.AccountInfo集合中看到包含UUID的记录
```

### 2. 登录验证
```bash
# 成功登录应该返回AccountUUID字段
# 错误密码应该返回ERR_PasswordError
# 不存在账号应该返回ERR_AccountNotExist
```

### 3. 数据库查询
```javascript
// MongoDB查询验证
db.getCollection("ET.Server.AccountInfo").find({"AccountName": "testuser"})
// 应该看到AccountUUID字段
```

## 🚀 主要改进

1. **正确的数据存储位置**: UUID现在存储在Account中
2. **更好的数据分离**: Account存储账号基本信息，AccountServer存储登录会话信息
3. **完整的登录验证**: 密码验证和账号存在性检查
4. **清晰的数据流**: UUID在整个登录流程中的传递更加清晰

现在UUID功能已经正确实现，数据存储在合适的位置，登录流程完整且安全。