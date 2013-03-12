using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ED47.Stack.Web;
using System.Globalization;

namespace ED47.BusinessAccessLayer.Excel.Reader
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExcelGenericAttribute : Attribute
    {
        public bool AllowBlank { get; set; }

        // 0 to read all not empty
        private int _sheetNumberToRead = -1;
        public int SheetNumberToRead {
            get { return _sheetNumberToRead; }
            set { _sheetNumberToRead = value; }
        }  
      
        internal static ExcelGenericAttribute GetAttribute(Type clss) {
            return clss.GetCustomAttributes(typeof(ExcelGenericAttribute), true).FirstOrDefault() as ExcelGenericAttribute;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelColumnAttribute : Attribute {

        public int Column { get; set; }
        public string Description { get; set; }

        private string _culture = "en";
        public string Culture { get { return _culture; } set { _culture = value; } }
     
        public bool AllowBlank { get; set; }
        public bool IgnoreLineIfBlank { get; set; }
        public bool InputOnly { get; set; }
        public bool OutputOnly { get; set; }
        public bool ValidateEmailAddress { get; set; }
        public string ValidRegExpress { get; set; }
        public object DefaultValue { get; set; }
        
        internal static ExcelColumnAttribute GetAttribute(PropertyInfo p) {           
            var attr = GetCustomAttribute(p, typeof(ExcelColumnAttribute)) as ExcelColumnAttribute;
            return attr;
        }

        internal static void Validate(PropertyInfo prop, string value, ExcelData line) {
            line.ErrorMessage= "";

            var attr = GetCustomAttribute(prop, typeof(ExcelColumnAttribute)) as ExcelColumnAttribute;

            if (attr == null){
                line.IsValid = true;
                return;
            }

            if(value == null)
                value ="";

            if (attr.AllowBlank && string.IsNullOrEmpty(value.Trim()))
            {
                line.CurrentPropertyIsBlank = true;
                prop.SetValue(line, attr.DefaultValue,null);
                line.IsValid = true;
                line.ValidValue = value; 
                return;
            }

            line.CurrentPropertyIsBlank = string.IsNullOrEmpty(value.Trim());

            Boolean auxBool;
            if(prop.PropertyType == typeof(Boolean)){
                if (!Boolean.TryParse(value, out auxBool))
                {
                    line.ErrorMessage = string.Format("{0} value [" + value + "] cannot be converted to Boolean", string.IsNullOrEmpty(attr.Description) ? prop.Name : attr.Description);
                    line.IsValid = false;
                }
                else
                    line.ValidValue = auxBool;
                return;
            }

            Int32 auxInt;
            if (prop.PropertyType == typeof(Int32))
            {
                if (!Int32.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture(attr.Culture), out auxInt))
                {
                    line.ErrorMessage = string.Format("{0} value [" + value + "] cannot be converted to Int32", string.IsNullOrEmpty(attr.Description) ? prop.Name : attr.Description);
                    line.IsValid = false;
                }
                else
                    line.ValidValue = auxInt;
                return;
            }

            Double auxDouble;
            if(prop.PropertyType == typeof(Double) ){
                if (!Double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture(attr.Culture), out auxDouble))
                {
                    line.ErrorMessage = string.Format("{0} value [" + value + "] cannot be converted to Double", string.IsNullOrEmpty(attr.Description) ? prop.Name : attr.Description);
                    line.IsValid = false;
                }
                else
                    line.ValidValue = auxDouble;
                return;

            }

            Decimal auxDecimal;
            if (prop.PropertyType == typeof(Decimal)) {
                if (!Decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture(attr.Culture), out auxDecimal))
                {
                    line.ErrorMessage = string.Format("{0} value [" + value + "] cannot be converted to decimal", string.IsNullOrEmpty(attr.Description) ? prop.Name : attr.Description);
                    line.IsValid = false;
                }
                else 
                    line.ValidValue = auxDecimal;
                return;

            }

            if(!attr.AllowBlank && string.IsNullOrEmpty(value.Trim()))
            {
                line.ErrorMessage = string.Format("{0} cannot be empty", string.IsNullOrEmpty(attr.Description) ? prop.Name : attr.Description);
                line.IsValid = false;
                return;
            }

            var valid = true;
            if(attr.ValidateEmailAddress)
            {
                valid &= value.Trim().IsEmail();
                line.ErrorMessage = !valid ? string.Format("Email {0} is invalid ", value) : "";
            }

            if(valid && !string.IsNullOrEmpty(attr.ValidRegExpress))
            {
                Regex re = new Regex(attr.ValidRegExpress);
                valid &= re.IsMatch(value);
                line.ErrorMessage = !valid ? string.Format("Value {0} do not match regular expression", value) : "";
            }

            if (valid)
                line.ValidValue = value;
            else
            {
                line.ValidValue = null;
                line.IsValid = false;
            }
        }
    }
}