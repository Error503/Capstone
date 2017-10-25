function NetworkDisplay(elementId, options) {
    var default_options = {
        autoResize: true,
        height: '100%',
        width: '100%',
        clickToUse: false,
        groups: {
            "1": { color: { background: '#63CE4B' } },
            "2": { color: { background: '#CD7ED1' } },
            "3": { color: { background: '#5BC5D9' } },
            "cluster1": { color: { background: '#63CE4B' } },
            "cluster2": { color: { background: '#CD7ED1' } },
            "cluster3": { color: { background: '#5BC5D9' } }
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
        // If the node is not a cluster
        if (!network.isCluster(props.nodes[0])) {
            getNodeInformation(props.nodes[0], props.pointer.DOM);
        }
    });
    network.on('selectEdge', function (props) {
        // Get the edge
        var edge = network.clustering.getBaseEdges(props.edges[0]); 
        // If the edge is different than the one in props
        if (edge[0] != props.edges[0]) {
            // This is a clustered edge - 
        } else {
            var edgeObj = edgeData.get(props.edges[0]);
            getEdgeInformation(edgeObj.from, edgeObj.to, props.pointer.DOM);
        }
    });
    network.on('doubleClick', function (props) {
        if (props.nodes.length > 0) {
            // If the node is a cluster,
            if (network.isCluster(props.nodes[0])) {
                // Get the parent of the cluster
                var parentNode = network.getNodesInCluster(props.nodes[0])[0];
                network.openCluster(props.nodes[0]); // Open the cluster
                // The edges will not be labeled when they are opened, so we will have to manually update them
                var edges = edgeData.get(network.getConnectedEdges(parentNode));
                for (var i = 0; i < edges.length; i++) {
                    edgeData.update(edges[i]);
                }
            } else {
                getNodePaths(props.nodes[0], props.pointer.canvas); // Expand relationships
            }
        }
    });
    network.on('oncontext', function (props) {
        var selected = network.getNodeAt(props.pointer.DOM);
        // If the selected node exists and is not a cluster
        if (selected != null && !network.isCluster(selected)) {
            network.selectNodes([selected]);
            generateContextMenu(props.pointer.DOM, generateOptions(selected));
        }
        // Prevent the default behavior
        props.event.preventDefault();
    });

    function generateOptions(id) {
        var options = [
            { text: 'Edit Node', link: '/edit/index/' + id },
            { text: 'Flag for Deletion', link: 'javascript:flag();' }
        ];
        // If the node is a media node, 
        if (nodeData.get(id).group === 2) {
            // Add the cluster option
            options.unshift({ text: 'Cluster Connections', link: 'javascript:display.clusterConnections();' });
        }

        return options;
    }

    return {
        nodes: nodeData,
        edges: edgeData,
        graphic: network,
        // Functions
        addNode: addNode,
        addEdge: addEdge,
        clear: clearData,
        clusterConnections: clusterSelectedNodeConnections,
    };

    function addNode(data, position) {
        if (nodeData.get(data.Id) == null) {
            nodeData.add({
                id: data.Id,
                group: data.DataType,
                title: createLabel(data.CommonName),
                label: createLabel(data.CommonName, LABEL_LENGTH),
                mass: 1.5,
                shape: 'dot',
                x: position != null ? position.x + (Math.random() * 20): null,
                y: position != null ? position.y + (Math.random() * 20) : null,
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
            var capitalizedLabel = createLabel(target.Roles[0], LABEL_LENGTH);
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
    function clusterSelectedNodeConnections() {
        var parentNode = nodeData.get(network.getSelection().nodes[0]);
        var clusterSize = 0;
        network.clusterByConnection(network.getSelection().nodes[0], {
            joinCondition: function (parentOptions, childOptions) {
                clusterSize += 1;
                return !network.isCluster(childOptions.id);
            },
            clusterNodeProperties: {
                label: parentNode.label,
                shape: 'diamond',
                mass: 3,
                group: 'cluster' + parentNode.group,
            }
        });
    }

    function getNodePaths(id, position) {
        $.ajax({
            method: 'get',
            url: '/graph/networkdata',
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
        rolesLabel += createLabel(edge.Roles[i]);
        if (i != edge.Roles.length - 1) {
            rolesLabel += ', ';
        }
    }
    $('#info-table').empty().append(
        '<tr><td>Source</td><td>' + createLabel(edge.SourceName) + '</td></tr>' +
        '<tr><td>Target</td><td>' + createLabel(edge.TargetName) + '</td></tr>' + 
        '<tr><td>Roles</td><td>' + rolesLabel + '</td></tr>'
    );
}