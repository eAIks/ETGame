// MongoDB查询脚本：检查注册的账号
// 使用方法：mongo mongodb://127.0.0.1:27017/ET1 check_accounts.js

print("=== 检查ET1数据库中的账号数据 ===");

// 显示数据库信息
print("当前数据库: " + db.getName());

// 显示所有集合
print("\n所有集合:");
db.getCollectionNames().forEach(function(name) {
    print("  - " + name);
});

// 查找账号集合的不同可能名称
var possibleNames = [
    "ET.AccountInfo",
    "ET.Server.Account", 
    "Account",
    "account",
    "accounts"
];

possibleNames.forEach(function(collectionName) {
    print("\n=== 检查集合: " + collectionName + " ===");
    try {
        var count = db.getCollection(collectionName).count();
        print("文档数量: " + count);
        
        if (count > 0) {
            print("账号数据:");
            db.getCollection(collectionName).find().forEach(function(doc) {
                print("  ID: " + doc._id);
                print("  账号名: " + doc.AccountName);
                print("  创建时间: " + (doc.CreateTime ? new Date(doc.CreateTime) : "未知"));
                print("  ---");
            });
        }
    } catch (e) {
        print("错误: " + e.message);
    }
});

print("\n=== 检查完成 ===");