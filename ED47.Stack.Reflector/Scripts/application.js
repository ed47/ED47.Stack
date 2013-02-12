if (window.Ext) {
    Ext.ns("ED47.views.Models");
    Ext.ns("ED47.views.Controls");

    ED47.views.Render = new Ext.util.Observable();
    Ext.Loader.setConfig({ enabled: true });
    Ext.onReady(function() {

        ED47.views.Render.addEvents("ready");
        ED47.views.Render.on("ready", function() {
            ED47.views.Render.isRendered = true;
        }, this);

        Ext.tip.QuickTipManager.init();
    });
}