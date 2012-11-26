/*--- Legacy browser support code ---*/

if (!window.console) {
    window.console = {};
}

if (!console.log)
    console.log = function () {};
if (!console.groupCollapsed)
    console.groupCollapsed = function () {};
if (!console.groupEnd)
    console.groupEnd = function () {};
if (!console.warn)
    console.warn = function () {};
if (!console.error)
    console.error = function () {};
if (!console.info)
    console.info = function () {};
if (!console.debug)
    console.debug = function () { };

/*---------------------------*/

