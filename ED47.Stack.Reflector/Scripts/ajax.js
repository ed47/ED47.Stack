/*--- Common Ajax tools and abstractions ---*/
if (window.Ext) {

    Ext.define("ED47.Tools.Ajax", {
        extend: "Ext.util.Observable",
        constructor: function(config) {
            ED47.Tools.Ajax.superclass.constructor.call(this, config);
            this.addEvents(
                "request",
                "success",
                "failure"
            );
        }
    });

    //Abstraction over Ajax request function
    ED47.Tools.Ajax.prototype.request = function(url, httpMethod, parameters, successCallback, failureCallback, context, enableCache) {
        var defaultConfig = {
            url: url,
            params: parameters,
            success: function(response) {
                var jsonResult = Ext.decode(response.responseText, true);
                window.ajaxRequest.fireEvent("success", jsonResult);
                if (successCallback != null) {
                    if (context == null)
                        context = this;

                    successCallback.call(this, { data: jsonResult });
                }
            },
            failure: function(response, options) {
                window.ajaxRequest.fireEvent("failure");
                if (failureCallback != null) {
                    if (context == null)
                        context = this;

                    failureCallback.call(response, options, context);
                }
            },
            method: httpMethod,
            headers: {
                "Accept": "application/json"
            },
            disableCaching: !enableCache
        };

        if (httpMethod == "POST") {
            defaultConfig.params = null;
            defaultConfig.jsonData = parameters;
        }

        Ext.Ajax.request(defaultConfig);


        this.fireEvent("request");
    };

    //Add an ED47.Ajax instance to the window
    Ext.onReady(function() {
        window.ajaxRequest = new ED47.Tools.Ajax();
    });

    //AJAX file download tweak
    ED47.Tools.Ajax.downloadFile = function(url) {
        var iFrameFile;
        var iFrameFileEl = Ext.get('tas-iframe-file');

        if (iFrameFileEl != null) {
            iFrameFileEl.remove();
        }

        iFrameFile = document.createElement('iframe');
        Ext.apply(iFrameFile, {
            id: 'tas-iframe-file',
            name: 'tas-iframe-file',
            className: 'x-hidden',
            src: Ext.SSL_SECURE_URL // for IE
        });
        Ext.getBody().mask("Downloading...");

        var callback = function() {
            Ext.getBody().unmask();
        };

        if (Ext.isIE) {
            callback.defer(2000);
        } else {
            iFrameFile.onload = callback;
        }

        document.body.appendChild(iFrameFile);

        iFrameFile.src = url;
    };
}

/*-------------------------------------------------------------------*/

if(!Array.prototype.indexOf) {
    Array.prototype.indexOf = function (el) {
        for (var i = 0, max = this.length; i < max; i++)
            if (this[i] == el) return i;
        return -1;
    };
}