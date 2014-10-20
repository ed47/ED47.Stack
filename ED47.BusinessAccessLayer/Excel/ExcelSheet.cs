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
        /// Initializes a new instance of the <see cref="ExcelSheet"/> class.
        /// </summary>
        /// <param name="name">The sheet's name.</param>
        public ExcelSheet(string name, JsonObjectList data) : this(name)
        {
           
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

        public ExcelSheet AddColumns(IEnumerable<ExcelColumn> columns)
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
            var ws = CreateSheet(excelPackage);
            var cell = CreateColumns(ws);
            WriteCellData(ws, cell);
        }

        /// <summary>
        /// Creates a sheet on an ExcelPackage.
        /// </summary>
        /// <param name="excelPackage">The ExcelPackage to create a sheet in.</param>
        /// <returns>The created ExcelWorksheet.</returns>
        private ExcelWorksheet CreateSheet(ExcelPackage excelPackage)
        {
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

            return excelPackage.Workbook.Worksheets.Add(this.Name);
        }

        /// <summary>
        /// Creates the columns in a package sheet using the current sheet's configuration.
        /// </summary>
        /// <param name="worksheet">The worksheet to add columns on.</param>
        /// <returns>The cell coordinates.</returns>
        private CellCoordinate CreateColumns(ExcelWorksheet worksheet)
        {
            if (Columns.Count == 0 && Data.Count > 0)
            {
                foreach (var p in Data[0].Properties)
                {
                    Columns.Add(new ExcelColumn
                    {
                        PropertyName = p
                    });
                }
            }

            var cell = new CellCoordinate
                {
                    Row = 1,
                    Column = 1
                };

            if (HeaderColumns.Any())
            {
                foreach (var headerColumn in HeaderColumns)
                {
                    var colSpan = headerColumn.HeaderColSpan ?? headerColumn.ColSpan;

                    if (colSpan.HasValue)
                    {
                        var cells = worksheet.Cells[cell.Row, cell.Column, cell.Row, cell.Column + colSpan.Value];
                        cells.Merge = true;
                        cell.Column += colSpan.Value;
                        HeaderRenderer(headerColumn.DisplayName, cells);
                    }
                    else
                        HeaderRenderer(headerColumn.DisplayName, worksheet.Cells[cell.Row, cell.Column]);

                    cell.Column++;
                }

                cell.Column = 1;
                cell.Row++;
            }

            var hasHeader = Columns.Any(el => !String.IsNullOrEmpty(el.DisplayName));

            if (hasHeader)
            {
                foreach (var c in Columns)
                {
                    var colSpan = c.HeaderColSpan ?? c.ColSpan;

                    if (colSpan.HasValue)
                    {
                        var cells = worksheet.Cells[cell.Row, cell.Column, cell.Row, cell.Column + colSpan.Value];
                        cells.Merge = true;
                        cell.Column += colSpan.Value;
                        HeaderRenderer(c.DisplayName, cells);
                    }
                    else
                        HeaderRenderer(c.DisplayName, worksheet.Cells[cell.Row, cell.Column]);

                    cell.Column++;
                }
                cell.Column = 1;
                cell.Row++;
            }

            return cell;
        }

        /// <summary>
        /// Writes data to the cells of a worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet to write into.</param>
        /// <param name="cellCoordinate">The cell coordinates to start writting to.</param>
        private void WriteCellData(ExcelWorksheet worksheet, CellCoordinate cellCoordinate)
        {
            foreach (var d in Data.Items)
            {
                foreach (var c in Columns)
                {
                    var value = d.GetValue(c.PropertyName);

                    if (c.IsReadOnly)
                        worksheet.Cells[cellCoordinate.Row, cellCoordinate.Column].Style.Locked = true;

                    if (c.Format == null && value is DateTime)
                        worksheet.Cells[cellCoordinate.Row, cellCoordinate.Column].Style.Numberformat.Format = "dd.mm.yyyy";

                    if (c.Format != null)
                        worksheet.Cells[cellCoordinate.Row, cellCoordinate.Column].Style.Numberformat.Format = c.Format;

                    if (c.ColSpan.HasValue)
                    {
                        var cells = worksheet.Cells[cellCoordinate.Row, cellCoordinate.Column, cellCoordinate.Row, cellCoordinate.Column + c.ColSpan.Value];
                        cells.Merge = true;
                        cellCoordinate.Column += c.ColSpan.Value;
                        c.Render(value, cells);
                    }
                    else
                        c.Render(value, worksheet.Cells[cellCoordinate.Row, cellCoordinate.Column]);
                    
                    //worksheet.Column(cellCoordinate.Column).AutoFit();
                    cellCoordinate.Column++;
                }
                
                cellCoordinate.Column = 1;
                cellCoordinate.Row++;
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

        private class CellCoordinate
        {
            public int Column { get; set; }
            public int Row { get; set; }
        }
    }
}
