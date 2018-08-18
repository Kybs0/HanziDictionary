using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.Mapping;

namespace HanziDictionary
{
    class HanziSqlliteHelper
    {
        private static readonly string DbName = "cw.db3";
        private readonly DbCache<HanziDb> _hanziCache;
        private readonly DbCache<HanziRadicalDb> _hanziRadicalCache;
        private List<HanziInfo> _hanzis = new List<HanziInfo>();
        private List<HaiziRadical> _hanziRadicals = new List<HaiziRadical>();

        public HanziSqlliteHelper(string dbPath)
        {
            var exists = File.Exists(dbPath);
            _hanziCache = new DbCache<HanziDb>(() => $"Data Source={dbPath}");
            _hanziRadicalCache = new DbCache<HanziRadicalDb>(() => $"Data Source={dbPath}");
        }

        public List<HanziInfo> GetAllHanziInfos()
        {
            _hanzis = _hanziCache.Execute(db => db.HanziList).ToList();
            return _hanzis;
        }

        public void SaveHanziList(List<HanziInfo> hanziInfos)
        {
            _hanziCache.ExecuteTransaction(hanziDb =>
            {
                hanziDb.Insert(hanziInfos);
            });
        }

        public void SaveHanzi(HanziInfo hanziInfo)
        {
            _hanziCache.ExecuteTransaction(hanziDb =>
            {
                hanziDb.Insert(hanziInfo);
            });
        }

        public void UpdateHanziInfo(HanziInfo hanziInfo)
        {
            _hanziCache.ExecuteTransaction(hanziDb => { hanziDb.Update(hanziInfo); });
        }
    }
    public partial class HanziDb : DataConnection
    {
        public LinqToDB.ITable<HanziInfo> HanziList => GetTable<HanziInfo>();

        public HanziDb()
        {
            InitDataContext();
        }

        public HanziDb(string configuration)
            : base(configuration)
        {
            InitDataContext();
        }

        public HanziDb(IDataProvider dataProvider, string connectionString)
            : base(dataProvider, connectionString)
        {
            InitDataContext();
        }

        partial void InitDataContext();
    }

    public partial class HanziRadicalDb : DataConnection
    {
        public LinqToDB.ITable<HanziInfo> ClassicalWorks => GetTable<HanziInfo>();

        public HanziRadicalDb()
        {
            InitDataContext();
        }

        public HanziRadicalDb(string configuration)
            : base(configuration)
        {
            InitDataContext();
        }

        public HanziRadicalDb(IDataProvider dataProvider, string connectionString)
            : base(dataProvider, connectionString)
        {
            InitDataContext();
        }

        partial void InitDataContext();
    }

    /// <summary>
    /// 部首
    /// </summary>
    [Table("RadicalList")]
    public class HaiziRadical
    {
        [Column, NotNull]
        public int ID { get; set; }

        [Column, Nullable]
        public string Radical { get; set; }

        [Column, Nullable]
        public string StrokeCount { get; set; }

        [Column, Nullable]
        public string StrokeCode { get; set; }

    }
    /// <summary>
    /// 汉字
    /// </summary>
    [Table("HanziList")]
    public class HanziInfo
    {
        [Column, NotNull]
        public int ID { get; set; }

        [Column, Nullable]
        public string Hanzi { get; set; }

        [Column, Nullable]
        public string Pinyin { get; set; }

        [Column, Nullable]
        public string Radical { get; set; }

        [Column, Nullable]
        public int StrokeCount { get; set; }

        [Column, Nullable]
        public string StrokeCode { get; set; }
        [Column, Nullable]
        public string WuBi { get; set; }

        [Column, Nullable]
        public string SimpleIntroduction { get; set; }

        [Column, Nullable]
        public string DetailIntroduction { get; set; }
    }
}
