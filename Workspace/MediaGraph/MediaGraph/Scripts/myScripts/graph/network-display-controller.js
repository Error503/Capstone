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
    };
    var nodeData = new vis.DataSet();
    var edgeData = new vis.DataSet();
    return {
        nodes: nodeData,
        edges: edgeData,
        graphic: new vis.Network(document.getElementById(elementId), { nodes: nodeData, edges: edgeData }, options != null ? options : default_options),
        // Functions
        addNode: addNode,
        addEdge: addEdge,
        clear: clearData
    };

    function addNode(data, position) {
        if (this.nodes.get(data.Id) == null) {
            var characterLimit = 15;
            var capitalizedLabel = capitalizeLabel(data.CommonName, data.DataType);
            this.nodes.add({
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
        var existingEdges = this.edges.get({
            filter: function (item) {
                return (item.from == sourceId && item.to == target.Id) || (item.from == target.Id && item.to == sourceId);
            }
        });
        if (existingEdges.length === 0) {
            var capitalizedLabel = capitalizeLabel(target.Roles[0], target.DataType);
            this.edges.add({
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
}