using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;
using Microsoft.Win32;

namespace HanziDictionary
{
    class ExcelOperationHelper
    {
        public static void SaveHanziTextLinesToExcel(List<string> hanziDataList)
        {
            Workbook wb = new Workbook();
            Worksheet sheet = wb.Worksheets[0];

            SetFirRow(sheet);

            SetSheetContent(sheet, hanziDataList);

            //调整列表显示
            sheet.FreezePanes(1, 1, 1, 0);
            sheet.AutoFitColumns();

            SaveFileDialog sfDialog = new SaveFileDialog();
            sfDialog.InitialDirectory = @"F:\OneDrive\WorkArea\工作项\汉字\";
            sfDialog.Filter = "*.Excel文件|*.xlsx";
            if (sfDialog.ShowDialog() == true)
            {
                string filePath = sfDialog.FileName;
                wb.Save(filePath);
            }
        }

        private static void SetSheetContent(Worksheet sheet, List<string> hanziDataList)
        {
            int rowIndex = 1;
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

                sheet.Cells[rowIndex, 0].Value = hanzi;
                sheet.Cells[rowIndex, 1].Value = pinyin;
                sheet.Cells[rowIndex, 2].Value = radical == "难检字" ? string.Empty : radical; ;
                sheet.Cells[rowIndex, 3].Value = biHua;
                sheet.Cells[rowIndex, 4].Value = wubi;
                sheet.Cells[rowIndex, 5].Value = biHuaCode;

                rowIndex++;
            }
        }

        private static void SetFirRow(Worksheet sheet)
        {
            Style style = new Style();
            style.Number = 49;
            sheet.Cells[0, 0].Value = "汉字";
            sheet.Cells[0, 1].Value = "拼音";
            sheet.Cells[0, 2].Value = "部首";
            sheet.Cells[0, 3].Value = "笔画";
            sheet.Cells[0, 4].Value = "五笔";
            sheet.Cells.ApplyColumnStyle(3, style, new StyleFlag());
            sheet.Cells[0, 5].Value = "笔顺编号";
            sheet.Cells.ApplyColumnStyle(1, style, new StyleFlag());
        }
    }
}
