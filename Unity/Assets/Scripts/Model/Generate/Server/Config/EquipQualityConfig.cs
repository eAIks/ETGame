using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class EquipQualityConfigCategory : Singleton<EquipQualityConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, EquipQualityConfig> dict = new();
		
        public void Merge(object o)
        {
            EquipQualityConfigCategory s = o as EquipQualityConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public EquipQualityConfig Get(int id)
        {
            this.dict.TryGetValue(id, out EquipQualityConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (EquipQualityConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, EquipQualityConfig> GetAll()
        {
            return this.dict;
        }

        public EquipQualityConfig GetOne()
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

	public partial class EquipQualityConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>品质ID</summary>
		public int QualityId { get; set; }
		/// <summary>品质</summary>
		public string Quality { get; set; }
		/// <summary>名称</summary>
		public string QualityName { get; set; }
		/// <summary>主属性基础值</summary>
		public int BaseAttr { get; set; }
		/// <summary>浮动值</summary>
		public double Difference { get; set; }
		/// <summary>附加属性条数</summary>
		public int ExtraAttrSlots { get; set; }
		/// <summary>稀有词条出现概率</summary>
		public double RareAffixRate { get; set; }
		/// <summary>UI 品质颜色</summary>
		public string Color { get; set; }

	}
}
