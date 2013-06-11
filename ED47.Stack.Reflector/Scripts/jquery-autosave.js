// the semi-colon before function invocation is a safety net against concatenated
// scripts and/or other plugins which may not be closed properly.
; (function ($, window, document, undefined) {

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variable rather than global
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    // Create the defaults once
    var pluginName = "autosave";
    var defaults = {
    };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        // jQuery has an extend method which merges the contents of two or
        // more objects, storing the result in the first object. The first object
        // is generally empty as we don't want to alter the default options for
        // future instances of the plugin
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this.init();
        this.isSaving = false;
    }

    Plugin.prototype = {
        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and
            // the options via the instance, e.g. this.element
            // and this.options
            // you can add more functions like the one below and
            // call them like so: this.yourOtherFunction(this.element, this.options).
            var me = this;

            var forms = $(this.element).find("form").addBack("form");

            $.each(forms, function() {
                me.setup($(this));
            });
        },
        setup: function(form) {
            var me = this;
            form.data("initialState", form.serializeArray());
            
            form.submit(function (e) {
                e.preventDefault();
                
                var differences = objectDiff.diff(form.data("initialState"), form.serializeArray());

                if (differences.changed === "equal") {
                    return;
                }
                
                var update = {};

                $.each(differences.value, function () {
                    if (this.changed !== "equal") {
                        if (this.changed === "object change") {
                            update[this.value.name.value] = this.value.value.added;
                        } else if (this.changed === "added" || this.changed === "removed") { //Multiselect, just grab the current values and throw them at the server
                            var currentPropety = this.value.name;

                            values = $("[name='" + currentPropety + "']").val();
                            if (values)
                                values = values.join(",");
                            else
                                values = "";

                            update[currentPropety] = values;
                        }
                    }
                });

                if (!update.Id) {
                    update.Id = form.find("[name='Id']").val();
                }

                $.post(form.attr("action"), update, function (data) {
                    form.find(".field-validation-error")
                        .removeClass("field-validation-error")
                        .addClass("field-validation-valid")
                        .html("");

                    for (var property in data.Values) {
                        var target = form.find("[name='" + property + "']");

                        if (target.length !== 0) {
                            target.val(data.Values[property]);
                            target.closest(".control-group").addClass("success");
                            form.trigger("autosavepropertyset", { form: form, name: property });
                        }
                    }

                    $.each(data.Validations, function() {
                        var validationElement = form.find(".field-validation-valid[data-valmsg-for='" + this.PropertyName + "']");
                        validationElement
                            .removeClass("field-validation-valid")
                            .addClass("field-validation-error")
                            .html(this.ErrorMessage);
                    });

                    form.data("initialState", form.serializeArray());
                    form.trigger("autosaved", { form: form, data: data });
                }).fail(function() {
                    form.find(".control-group").addClass("error");
                });
                
            });

            form.find("select, textarea").on("change", function() {
                form.submit();
            });
        }
    };

    // A really lightweight plugin wrapper around the constructor,
    // preventing against multiple instantiations
    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" + pluginName, new Plugin(this, options));
            }
        });
    };

})(jQuery, window, document);