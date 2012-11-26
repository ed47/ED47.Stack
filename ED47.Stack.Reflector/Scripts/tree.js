//Base ED47 form.
Ext.define("ED47.ui.Tree", {
    extend: "Ext.tree.Panel",

    constructor: function (config) {
        if (!config) config = {model:{Config:{}}};
        if (!config.model) config.model = { Config: {} };
        if (!config.model.Config) config.model.Config = {};
        
   
        var defaultConfig = {
            defaultType: "textfield",
            border: false,
            renderTo: config.model.Id,
            store: Ext.create('ED47.views.data.TreeStore', { model: config.storeCfg.model, id: config.storeCfg.id })
        };


        Ext.apply(defaultConfig, config);
        Ext.apply(defaultConfig, config.model.Config);
    
        ED47.ui.Form.superclass.constructor.call(this, defaultConfig);

        this.on('selectionchange', function (s, rec) {
            this.store.select(s, rec);
        });

        ED47.views.current.on('startedit', this.onStartEdit, this);
    }


});