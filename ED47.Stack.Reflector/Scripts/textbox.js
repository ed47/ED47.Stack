if (window.Ext) {
    /// <reference path="http://localhost:51594/Scripts/extjs/ext-debug-w-comments.js" />
    Ext.define("ED47.ui.TextBox", {
        extend: "Ext.form.field.Text",
        constructor: function(config) {

            if (!config) config = { model: { Config: {} } };
            if (!config.model) config.model = { Config: {} };
            if (!config.model.Config) config.model.Config = {};

            var defaultConfig = {
                
            };

            Ext.apply(defaultConfig, config);
            Ext.apply(defaultConfig, config.model.Config);

            ED47.ui.TextBox.superclass.constructor.call(this, defaultConfig);
        }
    });
}