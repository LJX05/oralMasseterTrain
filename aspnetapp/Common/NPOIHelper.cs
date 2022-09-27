using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.Util;

namespace aspnetapp.Common
{
    public class NPOIHelper
    {
        /// <summary>
        /// 生成表格文件
        /// </summary>
        /// <param name="physicalPath"></param>
        /// <param name="dt"></param>
        public static void CreateExcel(string physicalPath, DataTable dt)
        {

            // 创建excel 文件对象
            HSSFWorkbook book = new HSSFWorkbook();
            // 创建工作表
            ISheet sheet = book.CreateSheet();
            // 创建行
            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                row.CreateCell(i).SetCellValue(dt.Columns[i].ColumnName);
            }
            //填充内容
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row_1 = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    row_1.CreateCell(j).SetCellValue(dt.Rows[i][dt.Columns[j].ColumnName] + "");
                }
            }
            //填充数据
            using (FileStream fsm = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
            {
                book.Write(fsm);
            }
        }

        /// <summary>
        /// 将DataTable导出到EXCEL
        /// </summary>
        /// <param name="dtDetail">二维数据表</param>
        /// <param name="fileTemp">模板文件路径</param>
        /// <param name="fileRpt">输出文件路径</param>
        /// <returns></returns>
        public static void exportToExcel(DataTable dtDetail, IList<SimpleItem> listField, Stream stream)
        {
            // 创建excel 文件对象
            HSSFWorkbook book = new HSSFWorkbook();
            // 创建工作表
            ISheet sheet = book.CreateSheet();
            // 创建行
            IRow row = sheet.CreateRow(0);

            ICellStyle headerStyle = book.CreateCellStyle();
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BorderLeft = BorderStyle.Thin;
            headerStyle.BorderRight = BorderStyle.Thin;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.WrapText = true;
            //设置背景颜色...
            HSSFPalette palette = book.GetCustomPalette(); //调色板实例

            palette.SetColorAtIndex((short)8, (byte)184, (byte)204, (byte)228);
            HSSFColor hssFColor = palette.FindColor((byte)184, (byte)204, (byte)228);
            headerStyle.FillForegroundColor = hssFColor.Indexed;
            headerStyle.FillPattern = FillPattern.SolidForeground;

            //设置标题
            int num = 0;
            foreach (SimpleItem item in listField)
            {
                if (!dtDetail.Columns.Contains(item.value))
                    continue;
                ICell cell = row.CreateCell(num++);
                cell.SetCellValue(item.text);
                cell.CellStyle = headerStyle;
                sheet.SetColumnWidth(num - 1, Convert.ToInt32(item.tag) * 256);
                row.Height = 22 * 20;
            }
            ICellStyle cellStyle = book.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Left;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.WrapText = true;

            int iRow = 1;
            //填充内容
            foreach (DataRow rowDetail in dtDetail.Rows)
            {
                int iCol = 0;
                row = sheet.CreateRow(iRow++);
                row.Height = 22 * 20;
                foreach (SimpleItem item in listField)
                {
                    if (!dtDetail.Columns.Contains(item.value))
                        continue;
                    var cell = row.CreateCell(iCol++);
                    cell.SetCellValue(rowDetail[item.value] + "");
                    cell.CellStyle = cellStyle;
                }
            }
            book.Write(stream);
        }

        /// <summary>
        /// 将DataTable导出到EXCEL
        /// </summary>
        /// <param name="listData">二维数据列表</param>
        /// <param name="fileTemp">模板文件路径</param>
        /// <param name="fileRpt">输出文件路径</param>
        /// <returns></returns>
        public static Stream exportToExcel(IList<object> listData, string fileTemp)
        {
            string content = System.IO.File.ReadAllText(fileTemp);
            var listField = JsonConvert.DeserializeObject<IList<SimpleItem>>(content);
            // 创建excel 文件对象
            HSSFWorkbook book = new HSSFWorkbook();
            // 创建工作表
            ISheet sheet = book.CreateSheet();
            // 创建行
            IRow row = sheet.CreateRow(0);

            ICellStyle headerStyle = book.CreateCellStyle();
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BorderLeft = BorderStyle.Thin;
            headerStyle.BorderRight = BorderStyle.Thin;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.WrapText = true;
            //设置背景颜色...
            HSSFPalette palette = book.GetCustomPalette(); //调色板实例

            palette.SetColorAtIndex((short)8, (byte)184, (byte)204, (byte)228);
            HSSFColor hssFColor = palette.FindColor((byte)184, (byte)204, (byte)228);
            headerStyle.FillForegroundColor = hssFColor.Indexed;
            headerStyle.FillPattern = FillPattern.SolidForeground;

            var firstObj = listData.FirstOrDefault();
            var listProp = firstObj.GetType().GetProperties().ToList();
            //设置标题
            int num = 0;
            foreach (SimpleItem item in listField)
            {
                if (!listProp.Any(o => o.Name == item.value)) continue;
                ICell cell = row.CreateCell(num++);
                cell.SetCellValue(item.text);
                cell.CellStyle = headerStyle;
                sheet.SetColumnWidth(num - 1, Convert.ToInt32(item.tag) * 256);
                row.Height = 22 * 20;
            }
            ICellStyle cellStyle = book.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Left;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.WrapText = true;

            //填充内容
            int iRow = 1;
            foreach (var obj in listData)
            {
                int iCol = 0;
                row = sheet.CreateRow(iRow++);
                row.Height = 22 * 20;
                foreach (SimpleItem item in listField)
                {
                    var itemProp = listProp.FirstOrDefault(o => o.Name == item.value);
                    if (itemProp == null) 
                    { 
                        continue;
                    }
                    var cell = row.CreateCell(iCol++);
                    cell.CellStyle = cellStyle;
                    object val = itemProp.GetValue(obj);
                    if (val == null) continue;
                    if (itemProp.PropertyType.Name == "Int32")
                    {
                        cell.SetCellValue(Convert.ToInt32(val));
                    }
                    else
                    {
                        cell.SetCellValue(val + "");
                    }
                }
            }
            var fos = new MemoryStream();
            book.Write(fos);
            return fos;
        }
    }
}
