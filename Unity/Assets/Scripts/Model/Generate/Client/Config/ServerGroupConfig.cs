using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class ServerGroupConfigCategory : Singleton<ServerGroupConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, ServerGroupConfig> dict = new();
		
        public void Merge(object o)
        {
            ServerGroupConfigCategory s = o as ServerGroupConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public ServerGroupConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ServerGroupConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ServerGroupConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ServerGroupConfig> GetAll()
        {
            return this.dict;
        }

        public ServerGroupConfig GetOne()
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

	public partial class ServerGroupConfig: ProtoObject, IConfig
	{
		/// <summary>区组ID</summary>
		public int Id { get; set; }
		/// <summary>区组名称</summary>
		public string GroupName { get; set; }
		/// <summary>地区</summary>
		public string Region { get; set; }
		/// <summary>最大玩家数</summary>
		public int MaxPlayers { get; set; }
		/// <summary>状态</summary>
		public int Status { get; set; }
		/// <summary>开服时间</summary>
		public long OpenTime { get; set; }

	}
}
