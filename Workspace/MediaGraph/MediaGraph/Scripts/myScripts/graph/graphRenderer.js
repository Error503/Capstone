var svg;

var GRAPH_SETTINGS = {
    NODE_SLOT_WIDTH: 75, 
    REL_SLOT_WIDTH: 55,
    SLOT_HEIGHT: 25,
    END_MARK: null
};

var graph_nodes = [];
var all_nodes = null;
var graph_rels = [];
var all_rels = null;
var context_menu = null;

var test_data = {
    nodes: [
        {
            id: '00000000-0000-0000-0000-000000000000',
            type: 'video',
            title: 'The Original',
            date: '2013-02-15'
        },
        {
            id: '00000000-0000-0000-0000-000000000001',
            type: 'video',
            title: 'The Sequel',
            date: '2014-06-19',
        },
        {
            id: '00000000-0000-0000-0000-000000000011',
            type: 'game',
            title: 'The Game Adaptation',
            date: '2016-05-24'
        },
        {
            id: '00000000-0000-0000-0000-000000000002',
            type: 'game',
            title: 'A Different Game',
            date: '2015-04-23'
        }
    ],
    relationships: [
        {
            start: '00000000-0000-0000-0000-000000000001',
            end: '00000000-0000-0000-0000-000000000000',
            type: 'sequel',
            roles: []
        },
        {
            start: '00000000-0000-0000-0000-000000000011',
            end: '00000000-0000-0000-0000-000000000001',
            type: 'adaptation',
            roles: []
        },
        {
            start: '00000000-0000-0000-0000-000000000011',
            end: '00000000-0000-0000-0000-000000000000',
            type: 'adaptation',
            roles: []
        },
        {
            start: '00000000-0000-0000-0000-000000000002',
            end: '00000000-0000-0000-0000-000000000001',
            type: 'cast',
            roles: ['Voice Actor']
        },
        {
            start: '00000000-0000-0000-0000-000000000002',
            end: '00000000-0000-0000-0000-000000000011',
            type: 'cast',
            roles: ['Voice Actor']
        },
        {
            start: '00000000-0000-0000-0000-000000000002',
            end: '00000000-0000-0000-0000-000000000000',
            type: 'cast',
            roles: ['Voice Actor']
        }
    ]
};

// Check if SVG is supported
if (SVG.supported) {
    svg = SVG('graph');
    svg.id('svg-graph');
    all_nodes = svg.group().id('all-nodes');
    all_rels = svg.group().id('all-rels');
    // Create marker definitions
    GRAPH_SETTINGS.END_MARK = svg.marker(10, 10, function (add) {
        add.path('M 0 0 L 10 5 L 0 10 z');
        this.width(6);
        this.height(6);
        this.id('end-mark');
    });

    // Set up events
    setupEvents();
    // Render the data
    for (var i = 0; i < test_data.nodes.length; i++) {
        var n = makeNode(test_data.nodes[i]);
        graph_nodes.push(n);
        all_nodes.add(n);
    }
    // Render the relationships
    for (var i = 0; i < test_data.relationships.length; i++) {
        var r = makeRelationship(test_data.relationships[i]);
        graph_rels.push(r);
        all_rels.add(r);
    }
    all_rels.back();
    // Set the view box
    svg.viewbox(graph_nodes[0].children()[0].x() - 400, graph_nodes[0].children()[0].y() - 200, 800, 400);
} else {
    alert("SVG is not supported by your current browser!");
}

function setupEvents() {
    var mouseDown = false;
    svg.on('mousedown', function (event) {
        if(event.button === 0)
            mouseDown = true;
    });
    // Mouse move - pans the field of view
    svg.on('mousemove', function (event) {
        if (mouseDown) {
            var oldViewbox = svg.viewbox();
            svg.viewbox(oldViewbox.x - event.movementX, oldViewbox.y - event.movementY, oldViewbox.width, oldViewbox.height);
        }
    });
    svg.on('mouseup', function (event) {
        if(event.button === 0)
            mouseDown = false;
    });
}

function makeNode(nodeData) {
    // Make a group to house the node components
    var node_group = svg.group();
    // Get the position of the node
    var node_position = positionNode(nodeData.date);
    // Add the node to the group
    node_group.add(svg.rect(GRAPH_SETTINGS.NODE_SLOT_WIDTH, GRAPH_SETTINGS.SLOT_HEIGHT).radius(5).addClass('node').addClass('node-' + nodeData.type).move(node_position.x, node_position.y));
    //node_group.add(svg.circle(GRAPH_SETTINGS.NODE_SLOT_WIDTH).addClass('node').addClass('node-' + nodeData.type).move(node_position.x, node_position.y));
    // Bind JSON data to the node
    node_group.data('id', nodeData.id, true);
    // Add text
    node_group.add(svg.text(function (add) {
        add.tspan(nodeData.title).attr({ 'textLength': GRAPH_SETTINGS.NODE_SLOT_WIDTH - 5, 'lengthAdjust': 'spacingAndGlyphs' });
        add.tspan(nodeData.date).newLine();
    }).move(node_position.x + (GRAPH_SETTINGS.NODE_SLOT_WIDTH / 2), node_position.y - 2.25).font({ 'size': 10, 'anchor': 'middle' }));

    // Add a click listener
    node_group.click(nodeClicked);

    return node_group;
}

function positionNode(date) {
    var dateObj = new Date(date);
    return {
        x: (dateObj.getUTCFullYear() - 1950) * (GRAPH_SETTINGS.NODE_SLOT_WIDTH + GRAPH_SETTINGS.REL_SLOT_WIDTH),
        y: dateObj.getUTCMonth() * (GRAPH_SETTINGS.SLOT_HEIGHT + GRAPH_SETTINGS.REL_SLOT_WIDTH)
    };
}

// ===== NODE EVENTS =====
function nodeClicked(event) {
    console.log("element clicked");
    svg.rect(this.x() + GRAPH_SETTINGS.NODE_SLOT_WIDTH, this.y(), 100, 200).stroke('#000');
}

// ===== RELATIONSHIP FUNCTIONS =====
// TODO: Layer relationships and nodes together. Make relationships appear behind nodes

function makeRelationship(relData) {
    // Find the start and end nodes in the graph_nodes list
    var startNode = null, endNode = null;

    for (var i = 0; i < graph_nodes.length && (startNode === null || endNode === null); i++) {
        if (graph_nodes[i].data('id') === relData.start)
            startNode = graph_nodes[i].children()[0];
        if (graph_nodes[i].data('id') === relData.end)
            endNode = graph_nodes[i].children()[0];
    }
    // Get the connetion anchors for the points
    var anchors = getRelationshipAnchors(startNode, endNode);
    // Create a path between the points
    var path_group = svg.group();
    // Bind JSON data
    path_group.data('start', relData.start, true);
    path_group.data('end', relData.end, true);
    // TODO: Relationship curves
    var path = svg.path('M' + anchors.startAnchor.x + ' ' + anchors.startAnchor.y + ' L' + anchors.endAnchor.x + ' ' + anchors.endAnchor.y).addClass('relationship').addClass('relationship-' + relData.type);

    // Add the path
    path_group.add(path);

    return path_group;
}

function getRelationshipAnchors(start, end) {
    var anchors = {
        startAnchor: { x: 0, y: start.y() + (GRAPH_SETTINGS.SLOT_HEIGHT / 2) },
        endAnchor: { x: 0, y: end.y() + (GRAPH_SETTINGS.SLOT_HEIGHT / 2) }
    };

    // TODO: Put in check  steep slopes so that that relationships with steep slope anchor to the tope of the 
    if (start.x() > end.x()) {
        anchors.startAnchor.x = start.x();
        anchors.endAnchor.x = end.x() + GRAPH_SETTINGS.NODE_SLOT_WIDTH;
    } else if (start.x() === end.x()) {
        anchors.startAnchor.x = start.x() + (GRAPH_SETTINGS.NODE_SLOT_WIDTH / 2);
        anchors.endAnchor.x = end.x() + (GRAPH_SETTINGS.NODE_SLOT_WIDTH / 2);

        if (start.y() > end.y()) {
            anchors.startAnchor.y = start.y();
            anchors.endAnchor.y = end.y() + GRAPH_SETTINGS.SLOT_HEIGHT;
        } else if(start.y() === end.y()) {
            console.log("Two nodes have the same position!!!! " + start.data('id') + ", " + start.data('id'));
        } else {
            anchors.startAnchor.y = start.y() + GRAPH_SETTINGS.SLOT_HEIGHT;
            anchors.endAnchor.y = end.y();
        }

    } else {
        anchors.startAnchor.x = start.x() + GRAPH_SETTINGS.NODE_SLOT_WIDTH;
        anchors.endAnchor.x = end.x();
    }

    return anchors;
}