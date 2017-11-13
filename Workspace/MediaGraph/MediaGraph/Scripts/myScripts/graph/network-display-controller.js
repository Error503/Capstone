function NetworkDisplay(elementId, options) {
    var default_options = {
        autoResize: true,
        height: '100%',
        width: '100%',
        clickToUse: false,
        groups: {
            // Full color
            "pal0-1": { color: { background: '#63CE4B', border: '#53AD3F' } },
            "pal0-2": { color: { background: '#CD7ED1', border: '#8C568F' } },
            "pal0-3": { color: { background: '#5BC5D9', border: '#35747F' } },
            // Colorblind option 1 - Deutranopia
            "pal1-1": { color: { background: '#63CE4B', border: '#000000' } },
            "pal1-2": { color: { background: '#7523ff', border: '#000000' } },
            "pal1-3": { color: { background: '#3578ba', border: '#000000' } },
            // Colorblind option 2 - Protanopia
            "pal2-1": { color: { background: '#63CE4B', border: '#000000' } },
            "pal2-2": { color: { background: '#7523ff', border: '#000000' } },
            "pal2-3": { color: { background: '#3578BA', border: '#000000' } },
            // Colorblind option 3 - Tritanopia
            "pal2-1": { color: { background: '#63CE4B', border: '#000000' } },
            "pal2-2": { color: { background: '#7523ff', border: '#000000' } },
            "pal2-3": { color: { background: '#3578BA', border: '#000000' } },
            // Color blind option 3 - Grayscale
            "pal4-1": { color: { background: '#E5E5E5', border: '#7F7F7F' }, shape: 'square' },
            "pal4-2": { color: { background: '#404040', border: '#7F7F7F' }, shape: 'dot' },
            "pal4-3": { color: { background: '#AAAAAA', border: '#7F7F7F' }, shape: 'triangle' }
        },
        interaction: {
            tooltipDelay: 100,
            navigationButtons: window.innerWidth > 600,
            selectConnectedEdges: false
        }
    };
    var nodeData = new vis.DataSet();
    var edgeData = new vis.DataSet();
    var storedOptions = options || default_options;
    var network = new vis.Network(document.getElementById(elementId), { nodes: nodeData, edges: edgeData }, storedOptions);
    var colorPalette = 0;

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
                console.log(props.nodes[0]);
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
        if (nodeData.get(id).group.match(/(\d)$/)[1] == "2") {
            // Add the cluster option
            options.unshift({ text: 'Cluster Connections', link: 'javascript:display.clusterConnections();' });
        }

        return options;
    }
    // Setup an event to handle screen resizing
    var isScreenInSmallState = false;
    // Check the screen size
    checkScreenSize();
    // Add an event handler for the window resizing
    $(window).on('resize', checkScreenSize);

    function checkScreenSize() {
        // Get the screen width
        var screenWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
        var isScreenSmall = screenWidth <= 992; // Materialize's setting for large screens
        // If the state has changed,
        if (isScreenSmall != isScreenInSmallState) {
            // If the screen is currently small,
            if (isScreenSmall) {
                isScreenInSmallState = true;
                storedOptions.interaction.navigationButtons = false;
                storedOptions.clickToUse = true;
            } else {
                isScreenInSmallState = false;
                storedOptions.interaction.navigationButtons = true;
                storedOptions.clickToUse = false;
            }
            network.setOptions(storedOptions);
        }
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
        changeColorPalette: changeColorPalette
    };

    function addNode(data, position) {
        if (nodeData.get(data.Id) == null) {
            nodeData.add({
                id: data.Id,
                group: 'pal' + colorPalette + '-' + data.DataType,
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
        this.graphic.destroy();
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
                group: parentNode.group,
            }
        });
    }

    // Changes the colorblind color options
    function changeColorPalette(option) {
        colorPalette = option;   // Update the color palette option
        console.log(colorPalette);
        var nodes = nodeData.get(); // Get all of the nodes
        // Update all of the nodes in the graph to the new palette
        for (var i = 0; i < nodes.length; i++) {
            // Change the node's group
            nodes[i].group = 'pal' + colorPalette + '-' + nodes[i].group.match(/(\d)$/)[1];
            // Update the node
            nodeData.update(nodes[i]);
        }
    }

    function getNodePaths(id, position) {
        $.ajax({
            method: 'get',
            url: '/graph/networkdata',
            data: { searchText: null, id: id },
            success: function (response) {
                if (response.success) {
                    addNode(response.data.Source, position); // Add the source node
                    // Add the related nodes and their edges
                    for (var i = 0; i < response.data.RelatedNodes.length; i++) {
                        addNode(response.data.RelatedNodes[i], position);
                        addEdge(response.data.Source.Id, response.data.RelatedNodes[i]);
                    }
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