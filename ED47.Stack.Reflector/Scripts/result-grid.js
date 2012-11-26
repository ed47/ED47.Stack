//Basic ED47 result grid.
Ext.define("ED47.ui.ResultGrid", {
    extend: "Ext.grid.Panel",
    constructor: function (config) {
        var defaultConfig = {
            width: "100%"
        };

        Ext.apply(defaultConfig, config);

        ED47.ui.Grid.superclass.constructor.call(this, defaultConfig);
    }
});