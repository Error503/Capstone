function NetworkDisplay(elementId, options) {
    var default_options = {
        autoResize: true,
        height: '100%',
        width: '100%',
        clickToUse: false,
        groups: {
            "1": { color: { background: '#63CE4B' } },
            "2": { color: { background: '#CD7ED1' } },
            "3": { color: { background: '#5BC5D9' } }
        },
        interaction: {
            tooltipDelay: 100,
            navigationButtons: true,
            selectConnectedEdges: false
        }
    };
    var nodeData = new vis.DataSet();
    var edgeData = new vis.DataSet();
    var network = new vis.Network(document.getElementById(elementId), { nodes: nodeData, edges: edgeData }, options != null ? options : default_options);

    // Set up events
    network.on('selectNode', function (props) {
        getNodeInformation(props.nodes[0], props.pointer.DOM);
    });
    network.on('selectEdge', function (props) {
        // Get the edge
        var edge = edgeData.get(props.edges[0]);
        getEdgeInformation(edge.from, edge.to, props.pointer.DOM);
    });
    network.on('doubleClick', function (props) {
        if (props.nodes.length > 0) {
            getNodePaths(props.nodes[0], props.pointer.canvas);
        }
    });
    network.on('oncontext', function (props) {
        var selected = network.getNodeAt(props.pointer.DOM);
        if (selected != null) {
            network.selectNodes([selected]);
            displayContextMenu(selected, props.pointer.DOM);
        }
        // Prevent the default behavior
        props.event.preventDefault();
    });

    return {
        nodes: nodeData,
        edges: edgeData,
        graphic: network,
        // Functions
        addNode: addNode,
        addEdge: addEdge,
        clear: clearData
    };

    function addNode(data, position) {
        if (nodeData.get(data.Id) == null) {
            var characterLimit = 15;
            var capitalizedLabel = capitalizeLabel(data.CommonName, data.DataType);
            nodeData.add({
                id: data.Id,
                group: data.DataType,
                title: capitalizedLabel,
                label: capitalizedLabel.length > characterLimit ? capitalizedLabel.substring(0, characterLimit - 3) + '...' : capitalizedLabel,
                mass: 1.5,
                shape: 'dot',
                x: position != null ? position.x : null,
                y: position != null ? position.y : null,
            });
        }
    }
    function addEdge(sourceId, target) {
        var existingEdges = edgeData.get({
            filter: function (item) {
                return (item.from == sourceId && item.to == target.Id) || (item.from == target.Id && item.to == sourceId);
            }
        });
        if (existingEdges.length === 0) {
            var capitalizedLabel = capitalizeLabel(target.Roles[0], target.DataType);
            edgeData.add({
                from: sourceId,
                to: target.Id,
                label: capitalizedLabel,
                font: { align: 'top' }
            });
        }
    }
    function clearData() {
        this.nodes.clear();
        this.edges.clear();
    }

    function getNodePaths(id, position) {
        $.ajax({
            method: 'get',
            url: '/graph/getnetworkinformation',
            data: { searchText: null, id: id },
            success: function (response) {
                addNode(response.Source, position); // Add the source node
                // Add the related nodes and their edges
                for (var i = 0; i < response.RelatedNodes.length; i++) {
                    addNode(response.RelatedNodes[i], position);  
                    addEdge(response.Source.Id, response.RelatedNodes[i]);
                }
            },
            error: function (response) { console.error(response); }
        });
    }
}

//function getNodeInformation(node, position) {
//    $.ajax({
//        method: 'get',
//        url: '/graph/getnodeinformation',
//        data: { id: node },
//        success: function (response) { displayNodeInformation(response, position); },
//        error: function (response) { console.error(response); }
//    });
//}
//function displayNodeInformation(node, position) {
//    $('#info-popup').removeClass('inactive').addClass('active').css({ top: position.y + 50, left: position.x + 50 });
//    var dateLabel;
//    if (node.ContentType == 1) {
//        dateLabel = "Date Founded";
//    } else if (node.ContentType == 2) {
//        dateLabel = "Release Date";
//    } else if (node.ContentType == 3) {
//        dateLabel = "Date of Birth";
//    }
//    $('#info-table').empty().append(
//        '<tr><td>Name</td><td>' + capitalizeLabel(node.CommonName) + '</td></tr>' +
//        '<tr><td>' + dateLabel + '</td><td>' + (node.ReleaseDate != null ? parseLongDateValue(node.ReleaseDate).toDateString() : "Unknown") + '</td></tr>'
//    );
//}
function getEdgeInformation(from, to, position) {
    $.ajax({
        method: 'get',
        url: '/graph/getedgeinformation',
        data: { source: from, target: to },
        success: function (response) { displayEdgeInformation(response, position); },
        error: function (response) { console.log(response); }
    });
}
function displayEdgeInformation(edge, position) {
    $('#info-popup').removeClass('inactive').addClass('active').css({ top: position.y + 50, left: position.x + 50 });
    var rolesLabel = '';
    for (var i = 0; i < edge.Roles.length; i++) {
        rolesLabel += capitalizeLabel(edge.Roles[i]);
        if (i != edge.Roles.length - 1) {
            rolesLabel += ', ';
        }
    }
    $('#info-table').empty().append(
        '<tr><td>Source</td><td>' + capitalizeLabel(edge.SourceName) + '</td></tr>' +
        '<tr><td>Target</td><td>' + capitalizeLabel(edge.TargetName) + '</td></tr>' + 
        '<tr><td>Roles</td><td>' + rolesLabel + '</td></tr>'
        );
}