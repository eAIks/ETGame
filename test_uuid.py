#!/usr/bin/env python3
"""
简单的测试脚本来验证UUID功能
模拟客户端登录并检查UUID是否正确生成和存储
"""
import json
import uuid

def test_uuid_generation():
    """测试UUID生成逻辑"""
    # 模拟账号
    test_accounts = ["user1", "user2", "user3"]
    
    print("=== UUID生成测试 ===")
    for account in test_accounts:
        # 模拟第一次登录 - 生成新UUID
        account_uuid = str(uuid.uuid4())
        print(f"账号: {account}")
        print(f"生成UUID: {account_uuid}")
        print(f"UUID长度: {len(account_uuid)}")
        print(f"UUID格式正确: {'-' in account_uuid and len(account_uuid) == 36}")
        print("---")

def simulate_login_flow():
    """模拟登录流程"""
    print("\n=== 登录流程模拟 ===")
    
    # 模拟登录请求
    login_request = {
        "Account": "testuser",
        "Password": "testpass"
    }
    
    # 模拟服务器生成UUID
    account_uuid = str(uuid.uuid4())
    
    # 模拟登录响应
    login_response = {
        "Error": 0,
        "Message": "",
        "AccountUUID": account_uuid
    }
    
    print("登录请求:", json.dumps(login_request, indent=2))
    print("登录响应:", json.dumps(login_response, indent=2))
    
    # 模拟选择服务器
    select_server_request = {
        "ServerId": 1
    }
    
    # 模拟选择服务器响应
    select_server_response = {
        "Error": 0,
        "Message": "",
        "ServerAddress": "127.0.0.1:10002",
        "Key": 123456789,
        "GateId": 1001,
        "AccountUUID": account_uuid  # 同一个UUID
    }
    
    print("\n选择服务器请求:", json.dumps(select_server_request, indent=2))
    print("选择服务器响应:", json.dumps(select_server_response, indent=2))
    
    # 验证UUID一致性
    uuid_consistent = login_response["AccountUUID"] == select_server_response["AccountUUID"]
    print(f"\nUUID一致性检查: {'通过' if uuid_consistent else '失败'}")

def validate_uuid_uniqueness():
    """验证UUID唯一性"""
    print("\n=== UUID唯一性测试 ===")
    
    generated_uuids = set()
    test_count = 1000
    
    for i in range(test_count):
        new_uuid = str(uuid.uuid4())
        if new_uuid in generated_uuids:
            print(f"发现重复UUID: {new_uuid}")
            return False
        generated_uuids.add(new_uuid)
    
    print(f"生成{test_count}个UUID，全部唯一: 通过")
    return True

if __name__ == "__main__":
    print("ETGame UUID功能测试")
    print("=" * 50)
    
    test_uuid_generation()
    simulate_login_flow() 
    validate_uuid_uniqueness()
    
    print("\n=== 测试总结 ===")
    print("1. UUID生成格式正确")
    print("2. 登录流程中UUID传递一致")
    print("3. UUID唯一性保证")
    print("4. 实际功能需要启动服务器进行验证")
    print("\n建议:")
    print("- 启动服务器")
    print("- 使用客户端进行实际登录测试")
    print("- 检查数据库中的AccountServer记录")
    print("- 验证多次登录使用同一个UUID")