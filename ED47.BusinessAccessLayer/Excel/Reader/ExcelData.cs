namespace ED47.BusinessAccessLayer.Excel.Reader
{
    public class ExcelData
    {
        public object ValidValue { get; set; }

        public bool EqualLineFound { get; set; }
        public bool BlankLineFound { get; set; }

        private object _defaultValue = true;
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        private bool _currentPropertyIsBlank = true;
        public bool CurrentPropertyIsBlank
        {
            get { return _currentPropertyIsBlank; }
            set { _currentPropertyIsBlank = value; }
        }

        private bool _isValid = true;
        public bool IsValid
        {
            get { return _isValid; }
            set { _isValid = value; }
        }

        public string ErrorMessage { get; set; }
        public int ExcelLine { get; set; }
        
        public virtual void CustomPropertyValidation(System.Reflection.PropertyInfo p, object xlValue, ExcelData line){
        }

        public bool WasIgnored { get; set; }        
    }
}