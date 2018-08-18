using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aspose.Cells;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Path = System.IO.Path;
using Style = Aspose.Cells.Style;

namespace HanziDictionary
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "汉字列表.txt");
            HanziFilePathTextBox.Text = filePath;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var hanziLines = File.ReadAllLines(HanziFilePathTextBox.Text);
            var hanziDataList = hanziLines.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();

            string saveDbPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Hanzi.db3");

            using (var backgroundWorker = new BackgroundWorker() { WorkerReportsProgress = true })
            {
                backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
                backgroundWorker.DoWork += BackgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                backgroundWorker.RunWorkerAsync(Tuple.Create(saveDbPath, hanziDataList));
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var destDbPath = Path.Combine(desktopFolder, "Hanzi.db3");
            File.Move(e.Result.ToString(), destDbPath);
            LoadingTextBlock.Text = $"汉字信息下载完成,下载地址：{destDbPath}";
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var workerModel = (Tuple<string, List<string>>)e.Argument;

            List<HanziInfo> hanziInfos = new List<HanziInfo>();
            HanziSqlliteHelper hanziSqlliteHelper = new HanziSqlliteHelper(workerModel.Item1);
            int rowIndex = 1;
            foreach (var hanzi in workerModel.Item2)
            {
                var result = HanziApiHelper.FindHanzi(hanzi);
                if (result == null)
                {
                    continue;
                }

                var radical = result.Radical == "难检字" ? string.Empty : result.Radical;

                var biHuaCode = string.Empty;
                var bihuaCodeStr = result?.SimpleDetailContent.FirstOrDefault(i => i.Contains("笔顺编号："));
                if (!string.IsNullOrWhiteSpace(bihuaCodeStr))
                {
                    biHuaCode = bihuaCodeStr.Replace("笔顺编号：", "");
                }

                //var content = $"{hanzi}|{result.Pinyin}|{radical}|{result.Bihua}|{result.WuBi}|{biHuaCode}";

                Match match = Regex.Match(biHuaCode, "\\d+");
                if (match.Success)
                {
                    biHuaCode = match.Value;
                }
                else if (!match.Success && int.TryParse(match.Value, out int biHuaCodeConvertResult))
                {
                    biHuaCode = biHuaCodeConvertResult == 0 ? string.Empty : biHuaCodeConvertResult.ToString();
                }

                var hanziInfo = new HanziInfo()
                {
                    ID = rowIndex,
                    Hanzi = hanzi,
                    Pinyin = result.Pinyin,
                    Radical = radical == "难检字" ? string.Empty : radical,
                    StrokeCount = Convert.ToInt32(result.Bihua),
                    StrokeCode = biHuaCode,
                    WuBi = result.WuBi,
                    SimpleIntroduction = string.Join("\r\n", result.SimpleDetailContent),
                    DetailIntroduction = string.Join("\r\n", result.DetailContent),
                };

                hanziSqlliteHelper.SaveHanzi(hanziInfo);
                //报告进度
                var progress = rowIndex * 100 / workerModel.Item2.Count;
                worker.ReportProgress(Convert.ToInt32(rowIndex));
                rowIndex++;
            }

            e.Result = workerModel.Item1;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LoadingTextBlock.Text = "已获取" + e.ProgressPercentage + "个汉字";
        }



        #region 废弃代码


        private void GetAllHanziDetailsFromJuHeApi()
        {
            var filePath = @"F:\OneDrive\WorkArea\工作项\汉字\汉字列表.txt";
            var hanziLines = File.ReadAllLines(filePath);
            var hanziList = hanziLines.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();

            var searchedContent = new List<string>();
            foreach (var hanzi in hanziList)
            {
                var result = HanziApiHelper.FindHanzi(hanzi);
                if (result == null)
                {
                    continue;
                }

                var radical = result.Radical == "难检字" ? string.Empty : result.Radical;

                var biHuaCode = string.Empty;
                var bihuaCodeStr = result?.SimpleDetailContent.FirstOrDefault(i => i.Contains("笔顺编号："));
                if (!string.IsNullOrWhiteSpace(bihuaCodeStr))
                {
                    biHuaCode = bihuaCodeStr.Replace("笔顺编号：", "");
                }

                var content = $"{hanzi}|{result.Pinyin}|{radical}|{result.Bihua}|{result.WuBi}|{biHuaCode}";

                searchedContent.Add(content);
            }

            var newfilePath = @"F:\OneDrive\WorkArea\工作项\汉字\汉字列表New.txt";
            File.WriteAllLines(newfilePath, searchedContent);
        }

        private void TranslateTextToExcel()
        {
            var filePath = @"F:\OneDrive\WorkArea\工作项\汉字\汉字列表New.txt";
            var hanziLines = File.ReadAllLines(filePath);
            var hanziDataList = hanziLines.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();

            ExcelOperationHelper.SaveHanziTextLinesToExcel(hanziDataList);
        }

        private void SaveHanziTextLinesToDB()
        {
            var filePath = @"F:\OneDrive\WorkArea\工作项\汉字\汉字列表New.txt";
            var hanziLines = File.ReadAllLines(filePath);
            var hanziDataList = hanziLines.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();

            int rowIndex = 1;
            var searchedContent = new List<HanziInfo>();

            var hanziSqlliteHelper = new HanziSqlliteHelper(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Hanzi.db3"));
            var allHanziInfos = hanziSqlliteHelper.GetAllHanziInfos();
            foreach (var hanziData in hanziDataList)
            {
                var hanziDataTemp = hanziData;
                var hanzi = hanziDataTemp.Substring(0, hanziDataTemp.IndexOf("|"));

                hanziDataTemp = hanziDataTemp.Substring(hanzi.Length + 1, hanziDataTemp.Length - hanzi.Length - 1);
                var pinyin = hanziDataTemp.Substring(0, hanziDataTemp.IndexOf("|"));

                hanziDataTemp = hanziDataTemp.Substring(pinyin.Length + 1, hanziDataTemp.Length - pinyin.Length - 1);
                var radical = hanziDataTemp.Substring(0, hanziDataTemp.IndexOf("|"));

                hanziDataTemp = hanziDataTemp.Substring(radical.Length + 1, hanziDataTemp.Length - radical.Length - 1);
                var biHua = hanziDataTemp.Substring(0, hanziDataTemp.IndexOf("|"));

                hanziDataTemp = hanziDataTemp.Substring(biHua.Length + 1, hanziDataTemp.Length - biHua.Length - 1);
                var wubi = hanziDataTemp.Substring(0, hanziDataTemp.IndexOf("|"));

                hanziDataTemp = hanziDataTemp.Substring(wubi.Length + 1, hanziDataTemp.Length - wubi.Length - 1);
                var biHuaCode = hanziDataTemp;

                //姁|xǔ|女|8|vqkg|53135251y?）〕神态和悦娇媚，如“姣服极丽，姁姁致态。”
                Match match = Regex.Match(biHuaCode, "\\d+");
                var tryParse = int.TryParse(match.Value, out int biHuaCodeConvertResult);

                //var aaa = new HanziInfo()
                //{
                //    ID = rowIndex,
                //    Hanzi = hanzi,
                //    Pinyin = pinyin,
                //    Radical = radical == "难检字" ? string.Empty : radical,
                //    StrokeCount = Convert.ToInt32(biHua),
                //    WuBi = wubi,

                //    StrokeCode = biHuaCodeConvertResult.ToString(),
                //};
                //hanziSqlliteHelper.SaveHanzi(aaa);

                if (allHanziInfos.Any(i => i.Hanzi == hanzi && string.IsNullOrWhiteSpace(i.StrokeCode)))
                {
                    var hanziInfo = allHanziInfos.First(i => i.Hanzi == hanzi);
                    hanziInfo.StrokeCode =
                        biHuaCodeConvertResult == 0 ? string.Empty : biHuaCodeConvertResult.ToString();
                    hanziSqlliteHelper.UpdateHanziInfo(hanziInfo);
                }

                rowIndex++;
            }
        }

        #endregion
    }
}
