if (window.Ext) {
    //Basic ED47 combo box.
    Ext.define("ED47.ui.ComboBox", {
        extend: "Ext.form.ComboBox",
        constructor: function(config) {

            if (!config) config = { model: { Config: {} } };
            if (!config.model) config.model = { Config: {} };
            if (!config.model.Config) config.model.Config = {};

            var defaultConfig = {
                queryMode: "local",
                editable: false,
                triggerAction: "all",
                forceSelection: true,
                displayField: "Name",
                valueField: "Id"
            };

            Ext.apply(defaultConfig, config);
            Ext.apply(defaultConfig, config.model.Config);

            ED47.ui.ComboBox.superclass.constructor.call(this, defaultConfig);
        }
    });
}