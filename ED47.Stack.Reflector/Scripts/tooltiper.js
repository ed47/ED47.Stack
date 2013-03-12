function displayLearnMore (element) {
    var title = element.attr("data-title");
    var content = element.data("content");
    var xoffset = element.data("xoffset") ? parseInt(element.data("xoffset")) : 20;
    var yoffset = element.data("yoffset") ? parseInt(element.data("yoffset")) : 0;

    var myTooltip = $("<div class='tooltip'>" + (title ? "<h5>" + title + "</h5>" : "") + "<p>" + content + "</p></div>");
    $("body").append(myTooltip);
    
    element.mouseover(function () {
        myTooltip.css({ display: "none" }).show();
    }).mousemove(function (kmouse) {
        myTooltip.css({ left: kmouse.pageX + xoffset, top: kmouse.pageY + yoffset });
    }).mouseout(function () {
        myTooltip.hide();
    });
};