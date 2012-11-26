//Basic ED47 window.
Ext.define("ED47.ui.Window", {
    extend: "Ext.window.Window",
    constructor: function (config) {
        var defaultConfig = {

        };

        Ext.apply(defaultConfig, config);

        ED47.ui.Window.superclass.constructor.call(this, defaultConfig);
    }
});