//Basic ED47 button.
Ext.define("Ext.ux.MultilingualTrigger", {
    extend: "Ext.form.FieldContainer",
    alias: "widget.multilingualtrigger",
    constructor: function (config) {
        var me = this;
        if (!config) config = {};

        me.fieldName = config.name;
        me.mtype = config.mtype;
        me.getLanguagesFunc = config.getLanguagesFunc || ED47.Stack.Controllers.MultilingualController.GetTranslations;

        var defaultConfig = {
            xtype: "fieldcontainer",
            layout: "hbox",
            items: [{
                xtype: me.mtype,
                name: me.fieldName,
                flex: 2,
                listeners: {
                    blur: function () {
                        if (!this.getValue())
                            return;

                        var key = null;
                        if (me.ownerCt.getForm) {
                            var model = me.ownerCt.getForm().getRecord();
                            key = model.modelName.split(".")[model.modelName.split(".").length - 1] + "[" + model.data.Id + "]";
                        } else { // grid cellEditor
                            key = me.ownerCt.editingPlugin.activeEditor.editorId + "[" + me.ownerCt.editingPlugin.activeRecord.data.Id + "]";
                        }

                        if (!key) return;

                        var cookieLang = Ext.util.Cookies.get("LanguageCode");
                        var lang = cookieLang ? cookieLang : "en";

                        var multilingualValue = {
                            Key: key,
                            PropertyName: me.fieldName,
                            LanguageIsoCode: lang,
                            Text: this.getValue()
                        };

                        ED47.Stack.Controllers.MultilingualController.SetTranslation(multilingualValue);
                    }
                }
            }, {
                xtype: "button",
                iconCls: "multilingualtrigger",
                style: "margin-left: 5px;",
                handler: me.displayTranslationWindow,
                tooltip: 'Translate',
                scope: this,
                width: 24,
                height: 24
            }]
        };

        Ext.apply(defaultConfig, config);
        ED47.ui.Button.superclass.constructor.call(this, defaultConfig);

    },
    isValid: function () {
        return true;
    },
    reset: function () {
    },
    setValue: function (val) {
        this.items.items[0].setValue(val);
    },
    getValue: function () {
        return this.items.items[0].getValue();
    },
    displayTranslationWindow: function () {
        
        var modelName = null;
        var modelId = null;
        if (this.ownerCt.getForm) {
            modelName = this.ownerCt.getForm().getRecord().modelName;
            modelId = this.ownerCt.getForm().getRecord().data.Id;
        } else { // grid cellEditor
            modelName = this.ownerCt.editingPlugin.activeEditor.editorId;
            modelId = this.ownerCt.editingPlugin.activeRecord.data.Id;
        }
        
        var multilingualFormWindow = new ED47.ui.MultilingualFormWindow({
            fieldName: this.fieldName,
            mtype: this.mtype,
            modelName: modelName,
            modelId: modelId,
            getLanguagesFunc: this.getLanguagesFunc
        });

        multilingualFormWindow.on('multilingualvalidated', function (mls) {
            var cookieLang = Ext.util.Cookies.get("LanguageCode");
            var lang = cookieLang ? cookieLang : "en";
            var ml = mls.where("m=>m.LanguageIsoCode=='" + lang + "'");

            this.items.items[0].setValue(ml[0].Text);
            this.items.items[0].fireEvent('blur');
        }, this);
        multilingualFormWindow.show();
    }
});