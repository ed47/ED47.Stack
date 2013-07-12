;(function ( $, window, document, undefined ) {

    var pluginName = "ajaxlink",
        defaults = {
         
        };
    function Plugin( element, options ) {
        this.element = element;
         this.options = $.extend( {}, defaults, options );
        this._defaults = defaults;
        this._name = pluginName;
        this.init();
    }

    Plugin.prototype = {

        init: function() {
             
            var view = $(this.element);
            var self = this;
            view.on("click",function(e) {
                e.preventDefault();
                self.load();
            });
            
            var cls = self.options.cls || view.data("cls");
            if (view.hasClass(cls))
                this.load();
        },
        
        load : function() {
            var view = $(this.element);
            var self = this;
            var url = self.options.url || view.data("url") || view.attr("href");
            var sel = self.options.target || view.data("target") || view.attr("target");
            var cls = self.options.cls || view.data("cls");
            var target = $(sel);
            if (cls) {
                $("." + cls).removeClass(cls);
                view.addClass(cls);
                view.parents("div.ajaxlink").addClass(cls);
            }
                
            target.html("<div class='ajax-loading'><div style='margin:auto; width:22px'><img src='/content/images/icons/loading.gif'/></div></div>");
                
            $.ajax(url, {
                cache : false,
                success: function(res) {
                    target.html(res);
                    if (self.options.success)
                        self.options.success(self);
                }
            });
        }
        
    };

    $.fn[pluginName] = function ( options ) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" + pluginName, new Plugin( this, options ));
            }
        });
    };

})( jQuery, window, document );

$(function() {
    
    $("a.ajaxlink").livequery(function() { $(this).ajaxlink(); });
})



; (function ($, window, document, undefined) {

    var pluginName = "jOneShot",
        defaults = {

        };
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this.init();
    }

    Plugin.prototype = {

        init: function () {
            var view = $(this.element);
            
            view.on("click", function (e) {
                var button = $(this);
                
                if (button.hasClass("disabled")) {
                    e.stopPropagation();
                    e.preventDefault();
                    return false;
                }
                
                button.addClass("disabled");
                return true;
            });
        }
    };

    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" + pluginName, new Plugin(this, options));
            }
        });
    };

})(jQuery, window, document);

$(function () {

    $("a.one-shot, input[type=submit].one-shot").livequery(function () { $(this).jOneShot(); });
})