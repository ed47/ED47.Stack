using System;
using OfficeOpenXml;

namespace ED47.BusinessAccessLayer.Excel
{

    
    
    public class ExcelColumn 
    {
        public ExcelColumn()
        {
            Renderer = (el,range) => range.Value = el;
        }
        
        public void Render(object value, ExcelRange range, Action<object, ExcelRange> renderer = null)
        {
            var r = renderer ?? Renderer;
            r(value, range);
         
        }
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public Action<object, ExcelRange> Renderer { get; set; }

        

        /// <summary>
        /// The number format for this column.
        /// </summary>
        public string Format { get; set; }

        public int? ColSpan { get; set; }

        public int? HeaderColSpan { get; set; }
    }
}
