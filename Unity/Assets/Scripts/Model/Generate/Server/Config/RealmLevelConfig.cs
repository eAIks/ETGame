using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class RealmLevelConfigCategory : Singleton<RealmLevelConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, RealmLevelConfig> dict = new();
		
        public void Merge(object o)
        {
            RealmLevelConfigCategory s = o as RealmLevelConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public RealmLevelConfig Get(int id)
        {
            this.dict.TryGetValue(id, out RealmLevelConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (RealmLevelConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, RealmLevelConfig> GetAll()
        {
            return this.dict;
        }

        public RealmLevelConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
        
        /// <summary>
        /// 根据大境界ID和小境界序号获取境界名称
        /// </summary>
        public string GetRealmName(int majorRealm, int minorRealm)
        {
            foreach (var config in this.dict.Values)
            {
                if (config.StageId == majorRealm && config.SubStage == minorRealm)
                {
                    return $"{config.StageName} {config.SubStageName}";
                }
            }
            return "未知境界";
        }
    }

	public partial class RealmLevelConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>大境界ID</summary>
		public int StageId { get; set; }
		/// <summary>大境界名称</summary>
		public string StageName { get; set; }
		/// <summary>小境界序号</summary>
		public int SubStage { get; set; }
		/// <summary>小境界名称</summary>
		public string SubStageName { get; set; }
		/// <summary>等级下限</summary>
		public int LevelMin { get; set; }
		/// <summary>等级上限</summary>
		public int LevelMax { get; set; }
		/// <summary>跨境界等级</summary>
		public int CrossStageLevel { get; set; }
		/// <summary>升级经验</summary>
		public int ExpPerLevel { get; set; }

	}
}
