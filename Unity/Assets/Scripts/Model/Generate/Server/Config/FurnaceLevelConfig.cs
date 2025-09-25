using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class FurnaceLevelConfigCategory : Singleton<FurnaceLevelConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, FurnaceLevelConfig> dict = new();
		
        public void Merge(object o)
        {
            FurnaceLevelConfigCategory s = o as FurnaceLevelConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public FurnaceLevelConfig Get(int id)
        {
            this.dict.TryGetValue(id, out FurnaceLevelConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (FurnaceLevelConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, FurnaceLevelConfig> GetAll()
        {
            return this.dict;
        }

        public FurnaceLevelConfig GetOne()
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

	public partial class FurnaceLevelConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>鼎炉等级</summary>
		public int FurnaceLevel { get; set; }
		/// <summary>凡品</summary>
		public double Common { get; set; }
		/// <summary>普通</summary>
		public double Basic { get; set; }
		/// <summary>精良</summary>
		public double Fine { get; set; }
		/// <summary>优秀</summary>
		public double Superior { get; set; }
		/// <summary>上品</summary>
		public double Exquisite { get; set; }
		/// <summary>稀有</summary>
		public double Rare { get; set; }
		/// <summary>史诗</summary>
		public double Epic { get; set; }
		/// <summary>传奇</summary>
		public double Legendary { get; set; }
		/// <summary>远古</summary>
		public double Ancient { get; set; }
		/// <summary>神话</summary>
		public double Mythic { get; set; }
		/// <summary>仙品</summary>
		public double Celestial { get; set; }
		/// <summary>圣品</summary>
		public double Holy { get; set; }
		/// <summary>鸿蒙</summary>
		public double Primordial { get; set; }
		/// <summary>混元</summary>
		public double Chaotic { get; set; }
		/// <summary>永恒</summary>
		public double Eternal { get; set; }

	}
}
