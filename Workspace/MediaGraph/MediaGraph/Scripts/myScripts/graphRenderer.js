var canvas;

// On load - select all graph-node-group elements and add functions
// for "mouseenter" and "mouseleave"
$(".graph-node-group").on("mouseenter", function () {
    $(this).children(".relationship-node").css("fill", "rgba(100, 190, 100, .35)");
}).on("mouseleave", function () {
    $(this).children(".relationship-node").css("fill", "white");
});

function dragStartHandler() {
    console.log("drag started");
}

function dragEndHandler() {
    console.log("drag ended");
}

function handleNodeClick(element) {
    console.log("node clicked");
    // JQuery does not support SVG tags in use of append
    $("#popup-info").toggle(true);
    console.log("node x " + element.getAttribute("tot"));
}

function renderNodes(json) {
    // Get the node array
    var nodeArray = json.nodes;
    for (var nodeJson in nodeArray) {
        renderNode(nodeJson);
    }
}

function renderNode(node) {
    var nodeBox = canvas.rect(100, 50).move(100 * node.position.x, 50 * node.positions.y);
    canvas.plain(node.names[0]).move(nodeBox.x() + 10, nodeBox.y() + 15);
    nodeBox.on("mousedown", onNodeClick);
}

function onNodeClick(node) {

}

function renderRelationships(json) {
    // Get the relationship array
    var relationshipArray = json.relationships;

    for (var relationship in relationshipArray) {
    }
}

$(document).ready(function () {
    canvas = SVG('mini-graph-wrap').size(1000, 500);
});

// Takes in the element that was clicked
function getFullData(element) {
    // Call the ajax method
    $.ajax({
        type: "POST",
        url: "/Graph/GetNodeData",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: "{id:'" + element + "'}",
        success: dataCallback
    });
}

function dataCallback(data) {
    console.log("Callback method called.");
    console.log(data);
}

function testAjax() {
    $.ajax({
        type: "POST",
        url: "/Graph/GetNodeData",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: "{id:'00000000-0000-0000-0000-000000000000'}",
        success: function (data) {
            console.log("Response");
            console.log(data);
        }
    })
}