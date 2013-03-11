//Base ED47 form.
if (window.Ext) {
    Ext.define("ED47.ui.Form", {
        extend: "Ext.form.Panel",

        constructor: function(config) {
            var defaultConfig = {
                defaultType: "textfield",
                border: false,
                renderTo: config.model.Id
            };

            Ext.apply(defaultConfig, config);

            ED47.ui.Form.superclass.constructor.call(this, defaultConfig);

            ED47.views.current.on('startedit', this.onStartEdit, this);
        },

        bindStore: function(storeId) {
            var view = this;
            ED47.Stores.get(storeId, function(store) {
                store.forms.push(view.getForm());

                Ext.each(view.form.getFields().items, function(item) {
                    item.on("blur", function(field, context) {
                        var record = view.form.getRecord();

                        if (!view.form.isDirty())
                            return;

                        if (!record)
                            return;

                        if (!view.form.isValid()) return;

                        view.form.updateRecord();
                    }, view);
                });
                var fct = function() {
                    Ext.defer(function() {
                        if (!store.preselectedRecordId) {
                            if (!view.doNotSelectFirstRecord) store.select(view, store.getAt(0));
                        } else
                            store.select(view, store.getById(store.preselectedRecordId));
                    }, 100);

                };
                if (store.count() > 0) {
                    fct();
                } else {
                    store.on("load", fct, view);
                }

            });
        },
        onStartEdit: function() {
            // select the first form field in the child Item and set the focus on it
            if (this.getForm().getFields().first().getXType() == 'displayfield') return;
            if (this.getForm().getFields().first().getXType() == "fieldset") return;
            this.getForm().getFields().first().focus(true);
        }
    });
}