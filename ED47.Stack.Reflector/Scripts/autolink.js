;(function ( $, window, document, undefined ) {

    var pluginName = "autolink",
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
                if (view.data("target") != null) {
                    window.open(view.data("href"), view.data("target"));
                } else {
                    window.location = view.data("href");
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
    
    $(".autolink").livequery(function () {
        $(this).autolink();
        $(this).css("cursor", "pointer");
    });
});

