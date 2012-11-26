using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ED47.Stack.Web;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;

namespace ED47.BusinessAccessLayer.Excel
{
    public class ExcelFile
    {
        private int sheetNameCounter = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelFile"/> class.
        /// </summary>
        public ExcelFile()
        {
            Sheets = new List<ExcelSheet>();
        }

        /// <summary>
        /// Gets or sets the sheets.
        /// </summary>
        /// <value>
        /// The sheets.
        /// </value>
        public ICollection<ExcelSheet> Sheets { get; private set; }

        /// <summary>
        /// Adds a new sheet.
        /// </summary>
        /// <param name="data">The data to show in the sheet.</param>
        /// <param name="name">The sheet's name.</param>
        /// <returns>The new sheet</returns>
        public ExcelSheet AddSheet(JsonObjectList data, string name = null)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                name = "Content " + sheetNameCounter.ToString(CultureInfo.InvariantCulture);
                sheetNameCounter++;
            }

            var newSheet = new ExcelSheet(name)
                               {
                                   Data = data
                               };

            this.Sheets.Add(newSheet);
            return newSheet;
        }

        /// <summary>
        /// Saves the Excel file to a file.
        /// </summary>
        /// <param name="fileInfo">The file info to save into.</param>
        public void Write(FileInfo fileInfo)
        {
            var excelPackage = new ExcelPackage(fileInfo);

            foreach (var excelSheet in Sheets)
            {
                excelSheet.Write(excelPackage);
            }

            excelPackage.Save();
        }
    }
}
