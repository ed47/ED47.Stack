if (window.Ext) {
    //Basic ED47 panel.
    Ext.define("ED47.Stack.Panel", {
        extend: "Ext.panel.Panel",
        constructor: function(config) {

            if (!config) config = { model: { Config: {} } };
            if (!config.model) config.model = { Config: {} };
            if (!config.model.Config) config.model.Config = {};

            var defaultConfig = {
                bodyPadding: 5
            };

            Ext.apply(defaultConfig, config);
            Ext.apply(defaultConfig, config.model.Config);

            ED47.ui.Button.superclass.constructor.call(this, defaultConfig);
        }
    });
}