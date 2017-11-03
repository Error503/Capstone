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
var nodeData = new vis.DataSet([{ id: 'source', label: 'Source', shape: 'dot', group: 'original', mass: 2 }]);
var edgeData = new vis.DataSet();
var network;
var contentType = 0;

$(document).ready(function () {
    network = new vis.Network(document.getElementById('visualization-target'), { nodes: nodeData, edges: edgeData }, options);
    // Setup events with the network
    network.on('click', function (props) {
        if (props.nodes.length === 0) {
            enterAddMode();
        }
    }).on('selectNode', function (props) {
        // If a node other than the source node was selected,
        if (props.nodes[0] !== 'source') {
            // Enable relationship edit mode
            enterEditMode(nodeData.get(props.nodes[0]));
        } else {
            enterAddMode();
        }
    }).on('oncontext', function (props) {
        // Prevent default functionality
        props.event.preventDefault();
    });
    // Try to create an event on the content type drop down
    $('#ContentType').on('change', function (event) {
        if ($(this).val() != 0) {
            // Get the node information partial
            $.ajax({
                method: 'get',
                url: '/edit/getInformation',
                data: { type: Number.parseInt($(this).val()), visual: true },
                success: function (response) {
                    $('#node-information').empty().append(response);
                    setupInputEvents();
                },
                error: function (response) { console.error(response); }
            });
        }
    });
    // Setup the event for the form submission
    $('#submit-button').on('click', function (event) {
        var relationships = edgeData.get();
    });

    materializeSetup();
    // Set up the relationship form
    setupRelationshipForm();
});

function setupInputEvents() {
    $('select').material_select();
    // Setup event handling for changes in the node form
    $('#node-form').find('input:not([type="hidden"])').on('change', updateSourceNode);
    $('#other-name-chips').material_chip({ data: [] });
    $('#other-name-chips').on('chip.add', function (e, chip) {
        updateSourceNode(e);
    }).on('chip.delete', function (e, chip) {
        updateSourceNode(e);
    });
}

function updateSourceNode(event) {
    console.log(nodeData);
    var form = document.forms['node-form']; // Get the form
    var nodeValues = null;
    // Get the node data
    // Update the node
    nodeData.update({
        id: 'source',
        label: form['CommonName'].value !== "" ? createLabel(form['CommonName'].value, 15) : 'Source',
        title: form['CommonName'].value !== "" ? form['CommonName'].value : 'Source',
        dataValues: nodeValues
    });
}

function materializeSetup() {
    $('select').material_select();
    $('#relationship-chips').material_chip();
}

function getChipData(element) {
    // Get the chip data
    var chipData = $(element).material_chip('data');
    var result = [];
    // Get the chip data
    for (var i = 0; i < chipData.length; i++) {
        result.push(chipData[i].tag);
    }
    return result;
}