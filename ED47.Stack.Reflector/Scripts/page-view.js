/*--- Common PageView object and events ---*/

Ext.define("ED47.ui.PageView", {
    extend: "Ext.util.Observable",
    constructor: function (config) {
        if (ED47.ui.PageView.viewModel)
            this.Model = ED47.ui.PageView.viewModel;

        ED47.ui.PageView.superclass.constructor.call(this, config);
        ED47.ui.PageView.current = this;
        this.addEvents(
            "save",
            "beforesave",
            "aftersave",
            "update",
            "refresh",
            "close", 
            "startedit"
        );
    },
    Save: function () {
        this.fireEvent("beforesave");
        this.fireEvent("save");
        this.fireEvent("aftersave");
    },
    Close: function () {
        this.fireEvent("close");
    },
    Refresh: function () {
        this.fireEvent("refresh");
    }
});

ED47.ui.PageView.render = new Ext.util.Observable();
Ext.onReady(function () {
    ED47.ui.PageView.render.addEvents("ready");
    ED47.ui.PageView.render.on("ready", function () {
        ED47.ui.PageView.render.isRendered = true;
    }, this);
});

//Creates a new shared store
ED47.ui.PageView.createSharedStore = function (modelId) {
    ED47.ui.PageView.render.addEvents(modelId);

    var readyFunction = function () {
        ED47.ui.PageView.controls[modelId] = Ext.create("Ext.data.Store", {
            model: ED47.ui.PageView.models[modelId],
            storeId: modelId
        });
        ED47.ui.PageView.render.fireEvent(modelId);
    };
    if (ED47.ui.PageView.render.isRendered)
        readyFunction();
    else
        ED47.ui.PageView.render.on("ready", readyFunction);
};

/*-------------------------------------------------*/

