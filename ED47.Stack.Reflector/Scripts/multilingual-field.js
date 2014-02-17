if (window.Ext) {
    //Basic ED47 button.
    Ext.define("Ext.ux.MultilingualTrigger", {
        extend: "Ext.form.FieldContainer",
        alias: "widget.multilingualtrigger",
        constructor: function(config) {
            var me = this;
            if (!config) config = {};

            me.fieldName = config.name;
            me.mtype = config.mtype;
            me.getLanguagesFunc = config.getLanguagesFunc || ED47.Stack.Controllers.TranslatorApiController.GetTranslations;
            me.fieldConfig = config.fieldConfig || {};

            var defaultConfig = {
                xtype: "fieldcontainer",
                layout: "hbox",
                items: [{
                        xtype: me.mtype,
                        name: me.fieldName,
                        flex: 2,
                        listeners: {
                            blur: function() {
                                if (!this.getValue())
                                    return;

                                if (me.getOwnerCt().getForm) {
                                    var model = me.getOwnerCt().getForm().getRecord();
                                    me.key = model.modelName.split(".")[model.modelName.split(".").length - 1] + "[" + model.data.Id + "]";
                                } else { // grid cellEditor
                                    me.key = me.getOwnerCt().editingPlugin.activeEditor.editorId + "[" + me.getOwnerCt().editingPlugin.activeRecord.data.Id + "]";
                                }

                                if (!me.key && !me.definedKey)
                                    throw 'Key is not defined';

                                var cookieLang = Ext.util.Cookies.get("LanguageCode");
                                var lang = cookieLang ? cookieLang : "en";

                                var multilingualValue = {
                                    Key: me.key,
                                    PropertyName: me.fieldName,
                                    LanguageIsoCode: lang,
                                    Text: this.getValue()
                                };

                                ED47.Stack.Controllers.TranslatorApiController.SetTranslation(multilingualValue);
                            }
                        }
                    }, {
                        xtype: "button",
                        iconCls: "multilingualtrigger",
                        style: "margin-left: 5px;",
                        handler: me.displayTranslationWindow,
                tooltip: me.fieldConfig.tooltip || 'Translate',
                cancelButtonLabel: me.fieldConfig.cancelButtonLabel || 'Cancel',
                saveButtonLabel: me.fieldConfig.saveButtonLabel || 'Save',
                        scope: this,
                        width: 24,
                        height: 24
                    }]
            };

            Ext.apply(defaultConfig, config);
            ED47.ui.Button.superclass.constructor.call(this, defaultConfig);

        },
        getOwnerCt: function() {
            var me = this;
            if (me.ownerCt.xtype == "fieldset")
                return me.ownerCt.ownerCt;
            else
                return me.ownerCt;
        },
        isValid: function() {
            return true;
        },
        reset: function() {
        },
        setValue: function(val) {
            this.items.items[0].setValue(val);
        },
        setKey: function(val) {
            this.definedKey = val;
        },
        getValue: function() {
            return this.items.items[0].getValue();
        },
        displayTranslationWindow: function() {
            var me = this;

            if (me.getOwnerCt().getForm) {
                var model = me.getOwnerCt().getForm().getRecord();
                me.key = model.modelName.split(".")[model.modelName.split(".").length - 1] + "[" + model.data.Id + "]";
            } else { // grid cellEditor
                me.key = me.getOwnerCt().editingPlugin.activeEditor.editorId + "[" + me.getOwnerCt().editingPlugin.activeRecord.data.Id + "]";
            }
            var multilingualFormWindow = new ED47.ui.MultilingualFormWindow({
                fieldName: me.fieldName,
                fieldConfig: this.fieldConfig,
                mtype: me.mtype,
                key: me.definedKey || me.key,
                getLanguagesFunc: this.getLanguagesFunc,
                y: 200
            });

            multilingualFormWindow.on('multilingualvalidated', function(mls) {
                var cookieLang = Ext.util.Cookies.get("LanguageCode");
                var lang = cookieLang ? cookieLang : "en";
                var ml = mls.where("m=>m.LanguageIsoCode=='" + lang + "'");

                this.items.items[0].setValue(ml[0].Text);
                this.items.items[0].fireEvent('blur');
            }, this);

            multilingualFormWindow.show();
        }
    });
}