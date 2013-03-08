
///
/// Multilingual field translation window
///
/// Required configuration : languages array, each language has a isoCode and a value (the translated text)
///

Ext.define("ED47.ui.MultilingualFormWindow", {
    extend: "Ext.window.Window",
    constructor: function (config) {
        var me = this;

        me.addEvents("multilingualvalidated");

        if (!config) throw "A configuration object is required.";
        if (!config.key) throw "MultilingualFormWindow requires a key.";
        //if (!config.modelId) throw "MultilingualFormWindow requires a modelId.";
        if (!config.fieldName) throw "MultilingualFormWindow requires a field name.";
        if (!config.mtype) throw "MultilingualFormWindow requires a field type in 'mtype' property.";
        if (!config.getLanguagesFunc) throw "MultilingualFormWindow requires a languages array.";

        me.key = config.key;
        
        me.formPanel = Ext.create("Ext.form.FormPanel", {
            border: false,
            autoScroll: true,
            bodyStyle: "padding: 10px;",
            items: [],
            buttons: [{
                text: config.fieldConfig.cancelButtonLabel || "Cancel",
                handler: function () {
                    me.close();
                }
            }, {
                text: config.fieldConfig.saveButtonLabel || "Save",
                handler: function () {
                    if (!me.multilingualValues || !me.multilingualValues.length) {
                        return;
                    }

                    for (var i = 0, max = this.multilingualValues.length; i < max; i++) {
                        me.multilingualValues[i].Text = me.formPanel.getForm().findField(me.multilingualValues[i].LanguageIsoCode).getValue();
                    }

                    ED47.Stack.Controllers.MultilingualController.SetTranslations(me.multilingualValues);
                    me.fireEvent('multilingualvalidated', me.multilingualValues);
                    me.close();
                },
                scope: this
            }]
        });

        var defaultConfig = {
            //unstyled: true,
            border: false,
            closable: false,
            header: false,
            resizable: true,
            layout: "fit",
            width: 400,
            height: 400,
            modal: true,
            items: [
                me.formPanel
            ]
        };

        Ext.apply(defaultConfig, config);

        ED47.ui.Window.superclass.constructor.call(this, defaultConfig);

        config.getLanguagesFunc({
            PropertyName: config.fieldName,
            Key: config.key
        }, function (r) {
            if ((!r)) {
                return;
            }

            var items = r.data.ResultData.Items;
            me.multilingualValues = items;

            if (!config.fieldConfig)
                config.fieldConfig = {};

            for (var i = 0, max = items.length; i < max; i++) {
                var item = items[i];
                me.formPanel.add(Ext.apply(config.fieldConfig, {
                    xtype: config.mtype,
                    name: item.LanguageIsoCode,
                    value: item.Text,
                    fieldLabel: config.fieldName + "[" + item.LanguageIsoCode + "]"
                }));
            }
        }, this);
    }
});