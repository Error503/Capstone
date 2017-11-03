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
var nodeData;
var edgeData;
var network;
var nodeValidator;
var contentType = 0;

$(document).ready(function () {
    // If there is a content type defined
    if (document.forms['node-form']['ContentType'].value != 0) {
        // Parse the contentType
        contentType = Number.parseInt(document.forms['node-form']['ContentType'].value);
        // Set the value of the OtherNames field
        $('#other-name-chips').material_chip({ data: makeChips(JSON.parse(document.forms['node-form']['OtherNames'].value)) });
        // If this is a media node,
        if (contentType === 3) {
            // Set the value of the Genres field
            $('#genre-chips').material_chip({ data: makeChips(JSON.parse(document.forms['node-form']['Genres'].value)) });
        }
        // Create the network 
        nodeData = new vis.DataSet();
        edgeData = new vis.DataSet();
        var source = {
            id: 'source',
            shape: 'diamond',
            mass: 2,
            group: 'original',
            label: createLabel(document.forms['node-form']['CommonName'].value, 15),
            title: document.forms['node-form']['CommonName'].value
        };

        // Get the relationship data
        var relationships = JSON.parse($(document.forms['node-form']['Relationships'].value));

        // Loop through the related nodes
        for (var i = 0; i < relationships.length; i++) {
            // Create the node
            var added = nodeData.add({
                shape: 'dot',
                mass: 2,
                group: relationships[i].targetType,
                label: createLabel(relationships[i].targetName, 15),
                title: relationships[i].targetName,
                nid: relationships[i].targetId,
                targetType: relationships[i].targetType,
                targetName: relationships[i].targetName,
                roles: relationships[i].roles
            });
            // Add the edge
            edgeData.add({
                from: added,
                to: 'source',
                label: createLabel(relationships[i].roles[0], 15)
            });
        }
        // Create the network
        network = new vis.network(document.getElementById('visualization-target'), { nodes: nodeData, edges: edgeData }, options);
        // Setup validation
    } else {
        // Create the default network
        nodeData = new vis.DataSet([{ id: 'source', label: 'Source', shape: 'diamond', group: 'original', mass: 2 }]);
        edgeData = new vis.DataSet();
        network = new vis.Network(document.getElementById('visualization-target'), { nodes: nodeData, edges: edgeData }, options);
        // Create an event to handle the dropdown change
        $('#ContentType').on('change', function (event) {
            if ($(this).val() != 0 && $(this).val() != contentType) {
                contentType = $(this).val();
                // Get the node information partial
                $.ajax({
                    method: 'get',
                    url: '/edit/getInformation',
                    data: { type: Number.parseInt($(this).val()), visual: true },
                    success: function (response) {
                        $('#node-information').empty().append(response);
                        setupInputEvents();
                        updateSourceNode();
                    },
                    error: function (response) { console.error(response); }
                });
            }
        });
    }
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
            // Switch to the relationship tab
            $('ul.tabs').tabs('select_tab', 'rel-tab');
        } else {
            enterAddMode();
            // Switch to the node tab
            $('ul.tabs').tabs('select_tab', 'node-tab');
        }
    }).on('oncontext', function (props) {
        props.event.preventDefault(); // Prevent default functionality  
    });
    // Setup the event for the form submission
    $('#submit-button').on('click', function (event) {
        console.log("Submit button clicked");
        var nodes = nodeData.get(); // Get the related nodes
        var relationshipsValue = [];
        for (var i = 0; i < nodes.length; i++) {
            // If the node is not the source node,
            if (nodes[i].id != 'source') {
                relationshipsValue.push({
                    targetId: nodes[i].nid,
                    targetName: nodes[i].title.toLowerCase().trim(),
                    roles: nodes[i].roles
                });
            }
        }
        var form = document.forms['node-form'];
        // Set the value of the input field
        form['Relationships'].value = JSON.stringify(relationshipsValue);
        // Set the value of the OtherNames field
        form['OtherNames'].value = JSON.stringify(getChipData('#other-name-chips'));
        // Set type specific data
        if (contentType === 2) {
            // This is a media node
            // Set the value of the Genres field
            form['Genres'].value = JSON.stringify(getChipData('#genre-chips'));
        } else if (contentType === 3) {
            // This is a person node
            form['CommonName'].value = form['GivenName'].value + ' ' + form['FamilyName'].value;
        }
        //// Validate the form through JQuery Validate
        //if (nodeValidator.form()) {
        //    // The form is valid, submit the form
        //    $('#node-form').submit();
        //} else {
        //    nodeValidator.focusInvalid();
        //}
        $('#node-form').submit();
    });

    // Set up the relationship form
    setupRelationshipForm();
    // Set up materialize form fields
    materializeSetup();
});

function setupInputEvents(type) {
    // Display the form controls
    $('#submission-controls').show();
    $('select').material_select();
    // Setup event handling for changes in the node form
    $('#node-form').find('input:not([type="hidden"])').on('change', updateSourceNode);
    $('#other-name-chips').material_chip({ data: [] });
    // If this is a media node,
    if (contentType == 2) {
        // Set up chips
        $('#genre-chips').material_chip({ data: [] });
    }
    // Setup validation
}

function updateSourceNode(event) {
    var form = document.forms['node-form']; // Get the form
    var displayName = contentType == 3 ? (form['GivenName'].value + ' ' + form['FamilyName'].value) : form['CommonName'].value;
    // Update the node
    nodeData.update({
        id: 'source',
        group: "" + form['ContentType'].value,
        label: displayName !== null && displayName !== "" ? displayName : 'Source',
        title: displayName !== null && displayName !== "" ? displayName : 'Source'
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
function makeChips(chipData) {
    var result = [];
    for (var i = 0; i < chipData.length; i++) {
        result.push({ tag: chipData[i] });
    }
    return result;
}