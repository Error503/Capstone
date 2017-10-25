var outside;
var outsideNodes;
$(document).ready(function () {
    var options = {
        autoResize: true,
        height: '100%',
        width: '100%',
        clickToUse: false,
        groups: {
            "1": { color: { background: '#63CE4B' } },
            "2": { color: { background: '#CD7ED1' } },
            "3": { color: { background: '#5BC5D9' } },
            "original": {
                color: {
                    background: '#FFFF4E',
                    border: '#C7C727'
                }
            }
        },
        interaction: {
            tooltipDelay: 100,
            selectConnectedEdges: false
        },
        manipulation: {
            enabled: true,
            initiallyActive: true,
            addNode: nodeAdded,
            deleteNode: nodeDeleted,
            addEdge: false,
            editEdge: false,
            deleteEdge: false
        }
    };
    var targetId = 'network-graph';
    var nodeData = new vis.DataSet([{ id: 'sourceNode', label: 'source', shape: 'dot', size: 20, group: 'original', mass: 2 }]);
    var edgeData = new vis.DataSet();
    var network = new vis.Network(document.getElementById(targetId), { nodes: nodeData, edges: edgeData }, options);
    network.selectNodes(['sourceNode']);
    outside = network;
    outsideNodes = nodeData;

    // Data Model
    var sourceContentType = '';
    var ourId = document.forms['information-form']['Id'].value;
    var otherNames = [];
    var genres = [];
    var relationships = [];

    var selectedNodeId = null;
    var selectedRelationship = null;

    // Grab data if present
    if (document.forms['information-form']['ContentType'].value != "") {
        var form = document.form['information-form'];
        sourceContentType = form['ContentType'].value;
        otherNames = JSON.parse(form['OtherNames'].value);

        if (sourceContentType === 'Media') {
            genres = JSON.parse(form['Genres'].value);
        }

        relationships.push(JSON.parse(form['RelatedCompanies'].value));
        relationships.push(JSON.parse(form['RelatedMedia'].value));
        relationships.push(JSON.parse(form['RelatedPeople'].value));

        // Populate the display
    }

    materializeSetup();
    autocompleteSetup();

    // Set up materialize chip fields
    $('#other-names').material_chip({ data: [] });
    $('#other-names').on('chip.add', function (e, chip) {
        console.log("other name chip added");
    })
    .on('chip.delete', function (e, chip) {
        console.log("other name chip removed");
    });

    $('#relationship-chips').material_chip({ data: [] });
    $('#relationship-chips').on('chip.add', function (e, chip) {
        selectedRelationship.roles.push(chip.tag.toLowerCase().trim());
        console.log(selectedRelationship);
        var edge = edgeData.get({
            filter: function (item) {
                return item.from === selectedNodeId;
            }
        });
        edge[0].label = capitalizeLabel(selectedRelationship.roles[0]);
        edge[0].font = { align: 'top' };
        console.log(edge[0]);
        edgeData.update(edge);
    }).on('chip.delete', function (e, chip) {
        selectedRelationship.roles.splice(selectedRelationship.roles.indexOf(chip.tag.toLowerCase().trim()), 1);
    });

    // Input Events
    $('select[name="ContentType"]').on('change', function (event) {
        if (sourceContentType == '') {
            // Change the content type
            sourceContentType = $(this).val();
            // Get the information for the node

            // Bind to the new data model
            if (sourceContentType == 'Media') {
                $('#genre-chips').material_chip({ data: [] });
                $('#genre-chips').on('chip.add', function (e, chip) {

                }).on('chip.delete', function (e, chip) {
                    genres.splice(genres.indexOf(chip.tag), 1);
                    document.forms['information-form']['Genres'] = JSON.stringify
                });
            }
        }
    });
    $('#information-form').find('input').on('change', function (event) {
        var node = nodeData.get(selectedNodeId);
        node.label = capitalizeLabel(document.forms['information-form']['CommonName'].value);
    });
    $('#submit-button').on('click', function (event) {
        document.forms['information-form']['OtherNames'] = JSON.stringify(genres);
        // Split relationships and fill the data
        if (sourceContentType === 'Media') {
            document.forms['information-form']['Genres'] = JSON.stringify(genres);
        }
    });

    // Set up network events

    network.on('selectNode', function (props) {
        selectedNodeId = props.nodes[0];
        if (props.nodes[0] === 'sourceNode') {
            displaySourceInfo();
        } else {
            // Find the selected relationship
            selectedRelationship = null;
            for (var i = 0; i < relationships.length && selectedRelationship == null; i++) {
                if (relationships[i].nodeId === selectedNodeId) {
                    selectedRelationship = relationships[i];
                }
            }
            displayRelatedInfo();
        }
    });

    function displaySourceInfo() {
        $('#related-node-content').hide();
        $('#source-node-content').show();
    }
    function displayRelatedInfo() {
        $('#source-node-content').hide();
        $('#targetType').val(Number.parseInt(nodeData.get(selectedNodeId).group));
        $('#targetName').val(nodeData.get(selectedNodeId).label);
        // Create chips 
        $('#related-node-content').show();
    }

    // ===== Node Visualization Manipulation Functions =====

    function nodeAdded(node, callback) {
        node.shape = 'dot';
        node.size = 20;
        node.mass = 2;
        callback(node); // Add the node to the graph
        // Also add an edge between the sourceNode and the createdNode
        edgeData.add({ from: node.id, to: 'sourceNode', color: { color: '#848484' } });
        // Add a relationship to the relationship array
        relationships.push({nodeId: node.id, sourceId: ourId, targetId: null, roles: [] });
    }

    function nodeDeleted(nodes, callback) {
        // If the node being deleted is not the source node,
        if (nodes.nodes[0] !== 'sourceNode') {
            callback(nodes); // Delete the node
        } else {
            createToast("The original node cannot be deleted", 2500);
            callback();
        }
    }

    // ===== Autocomplete =====
    function autocompleteSetup() {
        var autocompleteData = {};
        var autocompleteStorage = {};
        var lastSearchText = null;
        var minLength = 3;
        $('input.autocomplete').on('keyup', function (event) {
            var value = $(this).val().toLowerCase().trim();
            // If the length of the current value is greater than minLength,
            if (value.length >= minLength) {
                // If the last search value is null, the current value is shorter
                // than the last search text
                // If the value is longer, then the autocomplete method will handle the filtering
                if (lastSearchText == null || value.length < lastSearchText.length) {
                    // Perform the search
                    performAutocompleteSearch(value);
                    lastSearchText = value;
                }
            } else {
                autocompleteData = null;
                autocompleteStorage = null;
                lastSearchText = null;
            }
        });

        function performAutocompleteSearch(text) {
            $.ajax({
                method: 'get',
                url: '/edit/autocompletesearch',
                data: { type: Number.parseInt($('#targetType').val()), searchText: text },
                success: function (response) {
                    autocompleteData = {};
                    autocompleteStorage = {};
                    for (var i = 0; i < response.length; i++) {
                        autocompleteData[capitalizeLabel(response[i].Item1)] = '';
                        autocompleteStorage[response[i].Item1] = response[i].Item2;
                    }
                    $('.autocomplete').autocomplete({
                        data: autocompleteData,
                        onAutocomplete: function (value) {
                            var node = nodeData.get(selectedNodeId);
                            node.label = value;
                            node.group = '' + $('#targetType').val();
                            nodeData.update(node);
                            selectedRelationship.targetId = autocompleteStorage[value.toLowerCase()];
                        },
                        minLength: 3
                    });
                },
                error: function (response) { console.log(response); }
            });
        }
    }
});

function materializeSetup() {
    $('select').material_select();
    $('.datepicker').pickadate({
        format: 'yyyy-mm-dd',
        selectMonths: true,
        selectYears: 15,
        closeOnSelect: true
    });
}

function capitalizeLabel(label, type) {
    var result = label;
    if (label != null) {
        result = label.replace(/\s(\S)/g, function (substring, args) {
            return substring.toUpperCase();
        });
        result = result.replace(/^(\S)/g, function (substring, args) {
            return substring.toUpperCase();
        });
    }

    return result;
}