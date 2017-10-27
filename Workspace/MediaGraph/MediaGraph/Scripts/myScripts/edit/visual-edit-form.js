var options = {
    autoResize: true,
    height: '100%',
    width: '100%',
    clickToUse: false,
    groups: {
        "1": { color: { background: '#63CE4B' } },
        "2": { color: { background: '#CD7ED1' } },
        "3": { color: { background: '#5BC5D9' } },
        "original": { color: { background: '#fbf966' } }
    },
    interaction: {
        tooltipDelay: 100,
        navigationButtons: true,
        selectConnectedEdges: false
    }
};
var nodeData = new vis.DataSet([{ id: 'source', label: 'Source', shape: 'dot', group: 'original' }]);
var edgeData = new vis.DataSet();
var network;

$(document).ready(function () {
    network = new vis.Network(document.getElementById('visualization-target'), { nodes: nodeData, edges: edgeData }, options);
    materializeSetup();
});

function materializeSetup() {
    $('select').material_select();
    $('#relationship-chips').material_chip();
}