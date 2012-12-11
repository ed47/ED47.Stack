using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using ED47.Stack.Web;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ED47.BusinessAccessLayer.Excel
{
    public class ExcelSheet
    {
        /// <summary>
        /// The name of the sheet.
        /// </summary>
        public string Name { get; set; }
        public Action<object, ExcelRange> HeaderRenderer { get; set; }
        public List<ExcelColumn> Columns { get; private set; }
        public List<ExcelColumn> HeaderColumns { get; private set; }

        /// <summary>
        /// Gets or sets the data to export.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public JsonObjectList Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelSheet"/> class.
        /// </summary>
        /// <param name="name">The sheet's name.</param>
        public ExcelSheet(string name)
        {
            Columns = new List<ExcelColumn>();
            HeaderColumns = new List<ExcelColumn>();
            this.Name = name;

            HeaderRenderer = (value, range) =>
                                 {
                                     range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                     range.Style.Font.Bold = true;
                                     range.Style.Fill.BackgroundColor.SetColor(Color.Black);
                                     range.Style.Font.Color.SetColor(Color.White);
                                     range.Value = value;
                                     range.Style.WrapText = true;
                                 };
        }

        /// <summary>
        /// Adds the columns.
        /// </summary>
        /// <param name="properties">The property names to add columns for.</param>
        /// <returns></returns>
        public ExcelSheet AddColumns(params string[] properties)
        {
            Columns.AddRange(
                properties.Select(el => 
                    new ExcelColumn { PropertyName = el }
            ));

            return this;
        }


        /// <summary>
        /// Adds columns to the sheet.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public ExcelSheet AddColumns(params ExcelColumn[] columns)
        {
            Columns.AddRange(columns);
            return this;
        }

        /// <summary>
        /// Writes the sheet into the specified excel package.
        /// </summary>
        /// <param name="excelPackage">The excel package.</param>
        public void Write(ExcelPackage excelPackage)
        {
            //Add the Content sheet
            var nameSuffix = 1;
            while (excelPackage.Workbook.Worksheets.Any(el => el.Name == this.Name))
            {
                var previousSuffix = this.Name.LastIndexOf(" " + (nameSuffix - 1).ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);

                if (previousSuffix > 0)
                {
                    this.Name = this.Name.Substring(0, previousSuffix + 1);
                }

                this.Name += " " + nameSuffix++;
            }
            var ws = excelPackage.Workbook.Worksheets.Add(this.Name);

            if(Columns.Count == 0 && Data.Count > 0)

            {
                foreach (var p in Data[0].Properties)
                {
                    Columns.Add(new ExcelColumn
                        {
                                        PropertyName = p
                                    });
                }
            }

            int i = 1, j = 1;

            if (HeaderColumns.Any())
            {
                foreach (var headerColumn in HeaderColumns)
                {
                    var colSpan = headerColumn.HeaderColSpan ?? headerColumn.ColSpan;

                    if (colSpan.HasValue)
                    {
                        var cells = ws.Cells[i, j, i, j + colSpan.Value];
                        cells.Merge = true;
                        j += colSpan.Value;
                        HeaderRenderer(headerColumn.DisplayName, cells);
                    }
                    else
                        HeaderRenderer(headerColumn.DisplayName, ws.Cells[i, j]);

                    j++;
                }

                j = 1;
                i++;
            }

            var hasHeader = Columns.Any(el => !String.IsNullOrEmpty(el.DisplayName));
            
            if(hasHeader)
            {
                foreach (var c in Columns)
                {
                    var colSpan = c.HeaderColSpan ?? c.ColSpan;

                    if (colSpan.HasValue)
                    {
                        var cells = ws.Cells[i, j, i, j + colSpan.Value];
                        cells.Merge = true;
                        j += colSpan.Value;
                        HeaderRenderer(c.DisplayName, cells);
                    }
                    else
                        HeaderRenderer(c.DisplayName, ws.Cells[i, j]);

                    j++;
                }
                j = 1;
                i++;
            }
           
            foreach (var d in Data.Items)
            {
                foreach (var c in Columns)
                {
                    var value = d.GetValue(c.PropertyName);
                    
                    if(c.Format == null && value is DateTime)
                        ws.Cells[i, j].Style.Numberformat.Format = "dd.mm.yyyy";
                    
                    if(c.Format != null)
                        ws.Cells[i, j].Style.Numberformat.Format = c.Format;
                    
                    if (c.ColSpan.HasValue)
                    {
                        var cells = ws.Cells[i, j, i, j + c.ColSpan.Value];
                        cells.Merge = true;
                        j += c.ColSpan.Value;
                        c.Render(value, cells);
                    }
                    else
                        c.Render(value, ws.Cells[i,j]);
                    j++;
                }
                j = 1;
                i++;
            }
        }

        /// <summary>
        /// Adds header columns which are independant from the data columns.
        /// </summary>
        /// <param name="columns">The columns to add to the header.</param>
        /// <returns>The current ExcelSheet.</returns>
        public ExcelSheet AddHeader(params ExcelColumn[] columns)
        {
            HeaderColumns.AddRange(columns);
            return this;
        }
    }
}
