using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class ServerConfigCategory : Singleton<ServerConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, ServerConfig> dict = new();
		
        public void Merge(object o)
        {
            ServerConfigCategory s = o as ServerConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public ServerConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ServerConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ServerConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ServerConfig> GetAll()
        {
            return this.dict;
        }

        public ServerConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
    }

	public partial class ServerConfig: ProtoObject, IConfig
	{
		/// <summary>服务器ID</summary>
		public int Id { get; set; }
		/// <summary>所属区组ID</summary>
		public int GroupId { get; set; }
		/// <summary>服务器名称</summary>
		public string ServerName { get; set; }
		/// <summary>主机地址</summary>
		public string Host { get; set; }
		/// <summary>端口</summary>
		public int Port { get; set; }
		/// <summary>最大玩家数</summary>
		public int MaxPlayers { get; set; }
		/// <summary>状态</summary>
		public int Status { get; set; }
		/// <summary>开服时间</summary>
		public long OpenTime { get; set; }
		/// <summary>权重</summary>
		public int Weight { get; set; }

	}
}
