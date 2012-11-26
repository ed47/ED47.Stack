//Basic ED47 button.
Ext.define("ED47.ui.Toolbar", {
    extend: "Ext.toolbar.Toolbar",
    constructor: function (config) {
        if (!config || config == undefined || !config.menuItems) throw 'An array of items is required';
        var view = this;
        this.config = config;

        var defaultConfig = {
            width: '100%',
            items: []
        };

        Ext.apply(defaultConfig, config);


        ED47.ui.Toolbar.superclass.constructor.call(this, defaultConfig);

        this.defineItems(this.config.menuItems, this);
    },
    defineItems: function (items, control) {
        Ext.each(items, function (item) {
            if (item instanceof Ext.Action) {
                control.add(item);
            } else {
                var menu = new Ext.menu.Menu({
                    plain: true,
                    items: []
                });
                this.defineItems(item.items, menu)
                control.add({
                    text: item.text,
                    menu: menu
                });
            }
        }, this);
    } 
});