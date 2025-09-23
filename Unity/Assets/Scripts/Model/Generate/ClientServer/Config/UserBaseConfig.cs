using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class UserBaseConfigCategory : Singleton<UserBaseConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, UserBaseConfig> dict = new();
		
        public void Merge(object o)
        {
            UserBaseConfigCategory s = o as UserBaseConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public UserBaseConfig Get(int id)
        {
            this.dict.TryGetValue(id, out UserBaseConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (UserBaseConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, UserBaseConfig> GetAll()
        {
            return this.dict;
        }

        public UserBaseConfig GetOne()
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

	public partial class UserBaseConfig: ProtoObject, IConfig
	{
		/// <summary>ID</summary>
		public int Id { get; set; }
		/// <summary>名称</summary>
		public string Name { get; set; }
		/// <summary>气血</summary>
		public int Health { get; set; }
		/// <summary>攻击</summary>
		public int Attack { get; set; }
		/// <summary>防御</summary>
		public int Defense { get; set; }
		/// <summary>连击率%</summary>
		public int ComboRate { get; set; }
		/// <summary>反击率%</summary>
		public int CounterRate { get; set; }
		/// <summary>暴击率%</summary>
		public int CriticalRate { get; set; }
		/// <summary>吸血率%</summary>
		public int LifeStealRate { get; set; }
		/// <summary>击晕率%</summary>
		public int StunRate { get; set; }
		/// <summary>抗连击率%</summary>
		public int AntiComboRate { get; set; }
		/// <summary>抗反击率%</summary>
		public int AntiCounterRate { get; set; }
		/// <summary>抗暴击率%</summary>
		public int AntiCriticalRate { get; set; }
		/// <summary>抗吸血率%</summary>
		public int AntiLifeStealRate { get; set; }
		/// <summary>抗击晕率%</summary>
		public int AntiStunRate { get; set; }

	}
}
