﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 10.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace ED47.Stack.Reflector.Templates
{
    using System.Text;
    using System.Linq;
    using ED47.Stack.Reflector.Metadata;
    using System;
    
    
    #line 1 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class JsModel : JsModelBase
    {
        public virtual string TransformText()
        {
            this.Write("//Model: ");
            
            #line 5 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.ModelInfo.Name));
            
            #line default
            #line hidden
            this.Write("\r\nExt.define(\"ED47.Stack.Models.");
            
            #line 6 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.ModelInfo.Name));
            
            #line default
            #line hidden
            this.Write("\", {\r\n\textend: \"Ext.data.Model\",\r\n\t");
            
            #line 8 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
if(!String.IsNullOrWhiteSpace(this.ModelInfo.IdPropertyName)){
            
            #line default
            #line hidden
            this.Write("\tidProperty: \"");
            
            #line 9 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.ModelInfo.IdPropertyName));
            
            #line default
            #line hidden
            this.Write("\",\r\n\t");
            
            #line 10 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
}
            
            #line default
            #line hidden
            this.Write("\tfields: [\r\n\t\t");
            
            #line 12 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
GenerateProperties();
            
            #line default
            #line hidden
            this.Write("\t],\r\n\r\n\tvalidations: [\r\n\t\t/*validation*/\r\n\t\t");
            
            #line 17 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
GenerateValidations();
            
            #line default
            #line hidden
            this.Write("\t]\r\n});\r\n\r\nED47.Stack.Models.");
            
            #line 21 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.ModelInfo.Name));
            
            #line default
            #line hidden
            this.Write(".getStore = function(id){\r\n\treturn new ED47.views.data.SharedStore({\r\n\t\tmodel: \"E" +
                    "D47.Stack.Models.");
            
            #line 23 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.ModelInfo.Name));
            
            #line default
            #line hidden
            this.Write("\",\r\n\t\tid: id\r\n\t});\r\n};\r\n\r\n");
            
            #line 28 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
GenerateFields();
            
            #line default
            #line hidden
            this.Write("\r\n");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 30 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"

//Generates the Model properties.
protected void GenerateProperties()
{
	string first = ""; 
	foreach(var property in this.ModelInfo.ModelProperties)
	{
        
        #line default
        #line hidden
        
        #line 36 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\t\r\n\t\t");

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(first));

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\t{ name: \"");

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Name));

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\", type: \"");

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.PropertyInfo.Name));

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\", xtype: \"");

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.ExtXType));

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\", allowBlank: ");

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture((!property.IsRequired).ToString().ToLower()));

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(", fieldLabel: \"");

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Label));

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\"\t");

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(string.IsNullOrEmpty(property.Mapping)?"": ", mapping:\""+ property.Mapping+"\""));

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(" \t}");

        
        #line default
        #line hidden
        
        #line 37 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"

		first = ","; 
	}
}

//Generates the Model validations.
protected void GenerateValidations()
{
	string first = "";
	for(int i=0; i<this.ModelInfo.ModelProperties.Count; i++)
	{
		
		var property = this.ModelInfo.ModelProperties[i];
		var hasValidation = false;
		if(property.IsRequired)
		{
        
        #line default
        #line hidden
        
        #line 52 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\t");

        
        #line default
        #line hidden
        
        #line 52 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(first));

        
        #line default
        #line hidden
        
        #line 52 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("{ field: \"");

        
        #line default
        #line hidden
        
        #line 52 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Name));

        
        #line default
        #line hidden
        
        #line 52 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\", type: \"presence\" }\t\t\t\r\n\t\t");

        
        #line default
        #line hidden
        
        #line 53 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"

			first = ",";
		}
		/*if(property.Length != null)
		{
        
        #line default
        #line hidden
        
        #line 57 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\t");

        
        #line default
        #line hidden
        
        #line 57 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(first));

        
        #line default
        #line hidden
        
        #line 57 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("{ \r\n\t\t\t\ttype: \"length\", field: \"");

        
        #line default
        #line hidden
        
        #line 58 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Name));

        
        #line default
        #line hidden
        
        #line 58 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\", \r\n\t\t\t\t");

        
        #line default
        #line hidden
        
        #line 59 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
if(property.Length.MinimumLength>0){
        
        #line default
        #line hidden
        
        #line 59 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(" min: ");

        
        #line default
        #line hidden
        
        #line 59 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Length.MinimumLength));

        
        #line default
        #line hidden
        
        #line 59 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(",");

        
        #line default
        #line hidden
        
        #line 59 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
}
        
        #line default
        #line hidden
        
        #line 59 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(" max: ");

        
        #line default
        #line hidden
        
        #line 59 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Length.MaximumLength));

        
        #line default
        #line hidden
        
        #line 59 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\t\t\t\t\r\n\t\t\t}\r\n\t\t");

        
        #line default
        #line hidden
        
        #line 61 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"

			first = ",";
		}*/ 
		
	}
}

//Generates the Form fields.
protected void GenerateFields()
{		
	foreach(var property in this.ModelInfo.ModelProperties.Where(p => p.DropDownForProperty == null))
	{
        
        #line default
        #line hidden
        
        #line 72 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("ED47.Stack.Models.");

        
        #line default
        #line hidden
        
        #line 72 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(this.ModelInfo.Name));

        
        #line default
        #line hidden
        
        #line 72 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(".get");

        
        #line default
        #line hidden
        
        #line 72 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Name));

        
        #line default
        #line hidden
        
        #line 72 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("Field = function(){\r\n\t\treturn {\t\t\r\n\t\t\tname: \"");

        
        #line default
        #line hidden
        
        #line 74 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Name));

        
        #line default
        #line hidden
        
        #line 74 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\",\r\n\t\t\txtype: \"");

        
        #line default
        #line hidden
        
        #line 75 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.ExtXType));

        
        #line default
        #line hidden
        
        #line 75 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\",\r\n\t\t\tallowBlank: ");

        
        #line default
        #line hidden
        
        #line 76 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture((!property.IsRequired).ToString().ToLower()));

        
        #line default
        #line hidden
        
        #line 76 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(",\r\n\t\t\tfieldLabel: \"");

        
        #line default
        #line hidden
        
        #line 77 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Label));

        
        #line default
        #line hidden
        
        #line 77 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\"");

        
        #line default
        #line hidden
        
        #line 77 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
GenerateComboBoxFieldConfiguration(property.DropDownSourceProperty, property.Name);
        
        #line default
        #line hidden
        
        #line 77 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\t};\r\n\t};\t\r\n\t");

        
        #line default
        #line hidden
        
        #line 80 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
GenerateComboBoxStore(property);
        
        #line default
        #line hidden
        
        #line 80 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\t");

        
        #line default
        #line hidden
        
        #line 81 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
}
}

//Generates the field configuration for combo boxes
protected void GenerateComboBoxFieldConfiguration(ModelPropertyInfo property, string fieldName)
{
	if(property != null)
	{
        
        #line default
        #line hidden
        
        #line 88 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(",\r\n\t\t\tstore: ED47.Stack.Models.");

        
        #line default
        #line hidden
        
        #line 89 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(fieldName));

        
        #line default
        #line hidden
        
        #line 89 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("Store,\r\n\t\t\tqueryMode: \"local\",\t\t\t\r\n\t\t\tdisplayField: \"");

        
        #line default
        #line hidden
        
        #line 91 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.DisplayField));

        
        #line default
        #line hidden
        
        #line 91 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\",\r\n\t\t\tvalueField: \"");

        
        #line default
        #line hidden
        
        #line 92 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.ValueField));

        
        #line default
        #line hidden
        
        #line 92 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\",\r\n\t\t\tforceSelection: true\r\n\t");

        
        #line default
        #line hidden
        
        #line 94 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
}
}

//Generates the store for a combo box
protected void GenerateComboBoxStore(ModelPropertyInfo property)
{
	if(property.DropDownSourceProperty != null)
	{
        
        #line default
        #line hidden
        
        #line 101 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("ED47.Stack.Models.");

        
        #line default
        #line hidden
        
        #line 101 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.Name));

        
        #line default
        #line hidden
        
        #line 101 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("Store = Ext.create(\"Ext.data.Store\"){\t\t\r\n\t\tfields: [\"");

        
        #line default
        #line hidden
        
        #line 102 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.DropDownSourceProperty.ValueField));

        
        #line default
        #line hidden
        
        #line 102 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\", \"");

        
        #line default
        #line hidden
        
        #line 102 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write(this.ToStringHelper.ToStringWithCulture(property.DropDownSourceProperty.DisplayField));

        
        #line default
        #line hidden
        
        #line 102 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
this.Write("\"]\r\n\t};\r\n\t");

        
        #line default
        #line hidden
        
        #line 104 "D:\projets\Grc.Tool\ED47.Stack\ED47.Stack.Reflector\Templates\JsModel.tt"
}
}

        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public class JsModelBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
