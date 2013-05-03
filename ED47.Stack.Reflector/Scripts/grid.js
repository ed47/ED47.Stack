//Basic ED47 grid.
if (window.Ext) {
    Ext.define("ED47.ui.Grid", {
        extend: "Ext.grid.Panel",
        constructor: function(config) {
            var defaultConfig = {
                renderTo: config.model.Id,
                disableSelectFirst: false,
                width: "100%"
            };

            Ext.apply(defaultConfig, config);

            ED47.ui.Grid.superclass.constructor.call(this, defaultConfig);

            if (config.height == "100%") {
                this.on("afterlayout", this.expandHeight, this);
            }

        },

        expandHeight: function() {
            var h = this.getEl().parent().getHeight();
            var h2 = this.getHeight();
            if (Math.abs(h - h2) < 5) return;
            this.un("afterlayout", this.expandHeight);
            this.setHeight(h);

        },

        bindDataStore: function(storeId, sorter, direction) {
            var me = this;
            if (!direction) direction = "ASC";
            ED47.Stores.get(storeId, function(store) {
                store.sorters = new Ext.util.MixedCollection();
                store.sorters.add("sorter", { property: sorter, direction: direction });
                me.reconfigure(store);

                var selectFirst = function() {
                    Ext.defer(function() {
                        if (!store.preselectedRecordId)
                            me.getSelectionModel().select(store.getAt(0));
                        else
                            me.getSelectionModel().select(store.getById(store.preselectedRecordId));
                    }, 100);
                };
                if (!me.disableSelectFirst) {
                    if (store.count() > 0) {
                        store.sort(sorter, direction);
                        selectFirst();
                    } else {
                        store.on("load", function() {
                            store.sort(sorter, direction);
                            selectFirst();
                        });
                    }
                }
            });
        },

        statics: {
            //Navigates a property tree to render a value.
            //Example: "myData.Element1.valueToRender" as dataIndex will render the value for "valueToRender" property.
            deepRenderer: function(dataIndex, value, record) {
                var split = dataIndex.split(".");
                var currentLevel = record.data;

                for (var i = 0; i < split.length; i++) {
                    currentLevel = currentLevel[split[i]];
                }

                return currentLevel;
            }
        }
    });
}