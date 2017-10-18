function TimelineDisplay(elementId, initialDate, options) {
    var current_date = new Date();
    var usingDate = (initialDate != null ? initialDate : current_date);
    var default_options = {
        //start: new Date(usingDate.getFullYear(), usingDate.getDate() <= 3 ? usingDate.getMonth() - 1 : usingDate.getMonth(), usingDate.getDate() - 3), // Start position of the initial view 
        //end: new Date(usingDate.getFullYear(), usingDate.get, // End position of the initial panel
        max: current_date.setFullYear(current_date.getFullYear() + 2), // Set the max point of the display to 2 years in the future
        zoomMin: 7 * 24 * 60 * 60 * 1000, // Set the minimum (closest) zoom level (7 days)
        zoomMax: 10 * 365 * 24 * 60 * 60 * 1000, // Set the maximum (farthest) zoom level (10 years)
        height: '100%', // Manually set the time to prevent the timeline from growing during resizing
    };
    var elementData = new vis.DataSet([{ id: 1, content: "Test item", start: '2017-10-18', type: 'box' }]);
    return {
        elements: elementData,
        graphic: new vis.Timeline(document.getElementById(elementId), elementData, options != null ? options : default_options),
        // Functions
        addNode: addNode,
        clear: clearData,
        destroy: destroyTimeline
    }

    function addNode(data) {
        // If the node is not already present
        if (this.elements.get(data.Id) == null) {
            // Add the node to the element data
            this.elements.add(createNodeFor(data));
        }
    }
    function createNodeFor(data) {
        var result = { id: data.Id, content: capitalizeLabel(data.CommonName), start: new Date(data.ReleaseDate) };

        if (data.DataType == 2) {
            // Media 
            result.type = 'point';
        } else {
            // If there is a death date
            if (data.DeathDate != null) {
                // Set the display type to 'range'
                result.end = new Date(data.DeathDate);
                result.type = 'range';
            } else {
                result.type = 'box';
            }
        }

        return result;
    }

    function clearData() {
        this.elements.clear();
    }

    function destroyTimeline() {
        this.graphic.destroy();
    }
}