;(function ($, window, document, undefined) {


    var pluginName = "filebox",
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
           
        }

      
    };

    // Un très léger décorateur autour du constructeur,
    // pour en éviter de multiples instanciations.
    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" + pluginName, new Plugin(this, options));
            }
        });
    };

})(jQuery, window, document);