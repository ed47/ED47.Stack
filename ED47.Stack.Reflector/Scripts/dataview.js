Ext.define("ED47.ui.DataView", {
    extend: "Ext.view.View",

    constructor: function (config) {
        var view = this;
        this.selectedObject = null;
        this.data = null;

        this.addEvents(
		    "select",
		    "open",
		    "edit"
	    );

        if (!config) config = { model: { Config: {}} };
        if (!config.model) config.model = { Config: {} };
        if (!config.model.Config) config.model.Config = {};

        if (!config.model.Config.DisplayField)
            config.model.Config.DisplayField = "{Name}";

        config.stylePrefix = config.stylePrefix ? config.stylePrefix : "gr";

        config.openEvent = config.openEvent ? config.openEvent : "dblclick";

        var defaultConfig = {
            renderTo: config.model.Id,
            tpl: "<tpl for='.'><div class='gr-item gr-view-wrap gr-view-item gr-view-item-detail'>" + config.model.Config.DisplayField + "</div></tpl>",
            singleSelect: true,
            border: true,
            autoScroll: true,
            padding: 4,
            overItemCls: config.stylePrefix + "-view-over",
            selectedCls: config.stylePrefix + "-view-selected",
            itemSelector: "div." + config.stylePrefix + "-view-wrap",
            emptyText: "No data",
            style: {
                backgroundColor: '#fff',
                border: 'solid 1px #ccc'
            },
            trackOver: true
        };
        config = Ext.merge(defaultConfig, config);
        config = Ext.merge(config, config.model.Config);


        if (config.model.StoreId) {
            ED47.Stores.get(config.model.StoreId, function (store) {
                this.Store = store;
                view.bindStore(store.storeId);
                view.on('select', function (s, rec) {
                    store.select(s, rec);
                });

                this.Store.on('select', function (s, rec) {
                    view.selModel.select(rec);
                });

            }, this);
        }

        ED47.ui.DataView.superclass.constructor.call(this, config);
        this.on(config.openEvent, this.onOpen, this);
    },

    onOpen: function () {
        var sel = this.getSelectedRecords();
        if (sel.length > 0) {
            if (sel[0] && sel[0].data) {
                this.selectedObject = sel[0].data.Self;
                this.fireEvent("open", this.selectedObject, this);
            }
        }
    },

    setData: function (data) {
        this.data = data;

        this.store.loadData(data);
        this.selectedObject = null;
    },

    removeAll: function () {
        this.Store.removeAll();
    },

    select: function (object) {
        var i = this.Store.indexOf(object.Record);
        if (i > -1)
            this.selectRange(i, i, false);
    }
});