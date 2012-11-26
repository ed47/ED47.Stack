//Basic ED47 button.
Ext.define("ED47.ui.Button", {
    extend: "Ext.button.Button",
    constructor: function (config) {

        if (!config) config = { model: { Config: {}} };
        if (!config.model) config.model = { Config: {}} ;
        if (!config.model.Config) config.model.Config = {};
     
        var defaultConfig = {
            
        };

        Ext.apply(defaultConfig, config);
        Ext.apply(defaultConfig, config.model.Config);
    
        ED47.ui.Button.superclass.constructor.call(this, defaultConfig);
    }
});