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
        var type = document.forms['node-form']['ContentType'].value;
        if (type == 'Company') {
            contentType = 1;
        } else if (type == 'Media') {
            contentType = 2;
        } else if (type == 'Person') {
            contentType = 3;
        }
        // Set the value of the OtherNames field
        var nameChips = makeChips(JSON.parse(document.forms['node-form']['OtherNames'].value));
        customChips('#other-names-chips', { placeholder: "Other Name", secondaryPlaceholder: "+Name" }, nameChips);
        if (nameChips.lenght > 0) {
            $('label[for="other-name-chips"]').addClass('active');
        }
        // If this is a media node,
        if (contentType == 2) {
            // Set the value of the Genres field
            var genreChips = makeChips(JSON.parse(document.forms['node-form']['Genres'].value));
            customChips('#genre-chips', { placeholder: "Enter a Genre", secondaryPlaceholder: "+Genre" }, genreChips);
            if (genreChips.length > 0) {
                $('label[for="genre-chips"]').addClass('active');
            }
        }
        // Create the network 
        nodeData = new vis.DataSet();
        edgeData = new vis.DataSet();

        var source = {
            id: 'source',
            shape: 'diamond',
            mass: 2,
            group: contentType,
            label: document.forms['node-form']['CommonName'].value != "" ? createLabel(document.forms['node-form']['CommonName'].value, 15) : 'Source',
            title: document.forms['node-form']['CommonName'].value != "" ? document.forms['node-form']['CommonName'].value : 'Source',
            nid: document.forms['node-form']['Id'].value
        };
        nodeData.add(source);

        // Get the relationship data
        var relationships = JSON.parse(document.forms['node-form']['Relationships'].value);

        // Loop through the related nodes
        for (var i = 0; i < relationships.length; i++) {
            // Create the node
            var added = nodeData.add({
                shape: 'dot',
                mass: 2,
                group: relationships[i].TargetType,
                label: createLabel(relationships[i].TargetName, 15),
                title: relationships[i].TargetName,
                nid: relationships[i].TargetId,
                targetType: relationships[i].TargetType,
                targetName: relationships[i].TargetName,
                roles: relationships[i].Roles
            });
            // Add the edge
            edgeData.add({
                from: added[0],
                to: 'source',
                font: { align: 'top' },
                label: createLabel(relationships[i].Roles[0], 15)
            });
        }
        // Create the network
        network = new vis.Network(document.getElementById('visualization-target'), { nodes: nodeData, edges: edgeData }, options);
        // Setup validation
    } else {
        // Create the default network
        nodeData = new vis.DataSet([{ id: 'source', label: 'Source', shape: 'diamond', group: 'original', mass: 2, nid: document.forms['node-form']['Id'].value }]);
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
                    data: { type: Number.parseInt($(this).val()) },
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
        var nodes = nodeData.get(); // Get the related nodes
        var sourceNodeId = nodeData.get('source').nid;
        var relationshipsValue = [];
        for (var i = 0; i < nodes.length; i++) {
            // If the node is not the source node,
            if (nodes[i].id != 'source') {
                relationshipsValue.push({
                    sourceId: sourceNodeId,
                    targetId: nodes[i].nid,
                    targetType: nodes[i].targetType,
                    targetName: nodes[i].targetName,
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
        if (contentType == 2) {
            // This is a media node
            // Set the value of the Genres field
            form['Genres'].value = JSON.stringify(getChipData('#genre-chips'));
        } else if (contentType == 3) {
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
    $('select').material_select();
});

function setupInputEvents(type) {
    // Display the form controls
    $('#submission-controls').show();
    $('select').material_select();
    // Setup event handling for changes in the node form
    $('#node-form').find('input:not([type="hidden"])').on('change', updateSourceNode);
    customChips('#other-name-chips', { placeholder: 'Second Name', secondaryPlaceholder: '+Name' });
    // If this is a media node,
    if (contentType == 2) {
        // Set up chips
        customChips('#genre-chips', { placeholder: 'Enter a Genre', secondaryPlaceholder: '+Genre' });
    }
    // Setup validation
}

function updateSourceNode(event) {
    var form = document.forms['node-form']; // Get the form
    var displayName = contentType == 3 ? (form['GivenName'].value + ' ' + form['FamilyName'].value).trim() : form['CommonName'].value;
    // Update the node
    nodeData.update({
        id: 'source',
        group: "" + form['ContentType'].value,
        label: displayName != null && displayName != "" ? createLabel(displayName, 15) : 'Source',
        title: displayName != null && displayName != "" ? displayName : 'Source'
    });
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