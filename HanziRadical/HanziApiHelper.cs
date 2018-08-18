using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HanziDictionary
{
    /// <summary>
    /// 聚合接口-汉字
    /// </summary>
    public class HanziApiHelper : HttpRequestBase
    {
        private const string NotFountRadical = "难检字";

        /// <summary>
        /// 查找部首
        /// </summary>
        /// <param name="hanzi"></param>
        /// <returns></returns>
        public static string FindRadical(string hanzi)
        {
            var hanziDetail = FindHanzi(hanzi);

            if (hanziDetail!=null && hanziDetail.Radical!=NotFountRadical)
            {
                return hanziDetail.Radical;
            }

            return string.Empty;
        }

        private const string BiHuaCodeKey = "笔顺编号：";

        /// <summary>
        /// 查找笔顺编号 如：聚12211154323334
        /// </summary>
        /// <param name="hanzi"></param>
        /// <returns></returns>
        public static string FindBiHuaCode(string hanzi)
        {
            var hanziDetail = FindHanzi(hanzi);

            var bihuaCodeStr=hanziDetail?.SimpleDetailContent.FirstOrDefault(i=>i.Contains(BiHuaCodeKey));
            if (!string.IsNullOrWhiteSpace(bihuaCodeStr))
            {
                return bihuaCodeStr.Replace(bihuaCodeStr, BiHuaCodeKey);
            }

            return string.Empty;
        }

        public static HanziDetail FindHanzi(string hanzi)
        {
            //1.根据汉字查询字典
            string url1 = "http://v.juhe.cn/xhzd/query";

            var parameters1 = new Dictionary<string, string>();

            parameters1.Add("word", hanzi); //填写需要查询的汉字，UTF8 urlencode编码
            parameters1.Add("key", "1810cd6518b7e8e644edfd6ee0ddc547");//你申请的key
            parameters1.Add("dtype", ""); //返回数据的格式,xml或json，默认json

            string result2 = SendPost(url1, parameters1, "get");

            var hanziRequestResponse = JsonConvert.DeserializeObject<HanziRequestResponse>(result2);

            //HanziDetail hanziDetail = null;
            //if (hanziRequestResponse.ErrorCode == "0" && hanziRequestResponse.Result != null)
            //{
            //    hanziDetail = hanziRequestResponse.Result;
            //}

            return hanziRequestResponse.Result;
        }
    }

    [DataContract]
    public class HanziRequestResponse
    {
        [DataMember(Name = "reason")]
        public string Reason { get; set; }

        [DataMember(Name = "error_code")]
        public string ErrorCode { get; set; }

        [DataMember(Name = "result")]
        public HanziDetail Result { get; set; }
    }
    [DataContract]
    public class HanziDetail
    {
        [DataMember(Name = "zi")]
        public string Hanzi { get; set; }

        /// <summary>
        /// 部首
        /// </summary>
        [DataMember(Name = "bushou")]
        public string Radical { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        [DataMember(Name = "pinyin")]
        public string Pinyin { get; set; }

        /// <summary>
        /// 笔画数
        /// </summary>
        [DataMember(Name = "bihua")]
        public string Bihua { get; set; }

        /// <summary>
        /// 五笔
        /// </summary>
        [DataMember(Name = "wubi")]
        public string WuBi { get; set; }

        /// <summary>
        /// 极简介绍
        /// </summary>
        [DataMember(Name = "jijie")]
        public List<string> SimpleDetailContent { get; set; }

        /// <summary>
        /// 详简
        /// </summary>
        [DataMember(Name = "xiangjie")]
        public List<string> DetailContent { get; set; }
    }
}
