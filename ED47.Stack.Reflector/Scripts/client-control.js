if (window.Ext) {
    Ext.ns("ED47.Stack.ClientControl");
    ED47.Stack.ClientControl.create = function(id, fullname) {
        var readyFct = function() {


            ED47.views.Controls[id] = Ext.create(fullname, {
                model: ED47.views.Models[id]
            });
        };

        if (ED47.views.Render.IsRendered)
            readyFct();
        else
            ED47.views.Render.on("ready", readyFct);
    };

    Ext.define("ED47.ui.RenderPage", {
        statics: {
            render: function(fullName) {
                Ext.create(fullName);
            }
        }
    });
}
