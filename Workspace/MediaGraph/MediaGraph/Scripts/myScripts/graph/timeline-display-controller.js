function TimelineDisplay(elementId, initialDate, options) {
    var UPDATE_DELAY = 200;
    var MAX_DISPLAY_LEVEL = 90 * 24 * 60 * 60 * 1000;
    var currentTimelineTask = -1;
    var current_date = new Date();
    var usingDate = (initialDate != null ? initialDate : current_date);
    var default_options = {
        start: alterDate(usingDate, -3), // Start position of the initial view 
        end: alterDate(usingDate, 3),
        max: current_date.setFullYear(current_date.getFullYear() + 2), // Set the max point of the display to 2 years in the future
        zoomMin: 7 * 24 * 60 * 60 * 1000, // Set the minimum (closest) zoom level (7 days)
        zoomMax: 10 * 365 * 24 * 60 * 60 * 1000, // Set the maximum (farthest) zoom level (10 years)
        width: '99%',   // For some reason 100% causes an infinite loop warning
        height: '100%', // Manually set the time to prevent the timeline from growing during resizing
    };
    var elementData = new vis.DataSet();//([{ id: 1, content: "Test item", start: '2017-10-18', type: 'box' }]);
    var timeline = new vis.Timeline(document.getElementById(elementId), elementData, options != null ? options : default_options);

    timeline.on('click', function (props) {
        if (props.what === 'item') {
            getNodeInformation(props.item, { x: props.x, y: props.y - 50});
        }
    });
    timeline.on('contextmenu', function (props) {
        if (props.what === 'item') {
            displayContextMenu(props.item, { x: props.x, y: props.y - 50 });
        }
        // Prevent the default functionality
        props.event.preventDefault();
    });
    timeline.on('rangechange', function (props) {
        // If there is currently a timeline task,
        if (currentTimelineTask != -1) {
            clearTimeout(currentTimelineTask); // Cancel the task
            currentTimelineTask = -1;
        }
    });
    timeline.on('rangechanged', function (props) {
        if (isWithinRange(props.start, props.end)) {
            // Schedule a task to complete the refresh
            currentTimelineTask = setTimeout(function () {
                getTimelineInformation(stringifyDate(props.start), stringifyDate(props.end));
                currentTimelineTask = -1;
            }, UPDATE_DELAY);
        }
    });

    function isWithinRange(start, end) {
        var startDate = new Date(start);
        var endDate = new Date(end);
        return (endDate.getTime() - startDate.getTime()) < MAX_DISPLAY_LEVEL;
    }
    function stringifyDate(value) {
        var date = new Date(value);
        return date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate();
    }

    return {
        elements: elementData,
        graphic: timeline,
        // Functions
        addNode: addNode,
        clear: clearData,
        destroy: destroyTimeline
    };

    // ===== Functions =====

    function addNode(data) {
        // If the node is not already present
        if (elementData.get(data.Id) == null) {
            // Add the node to the element data
            elementData.add(createNodeFor(data));
        }
    }
    function createNodeFor(data) {
        return {
            id: data.Id,
            content: capitalizeLabel(data.CommonName),
            start: parseLongDateValue(data.ReleaseDate),
            type: 'point',
            className: 'type-' + data.DataType
        };
    }

    function clearData() {
        elementData.clear();
    }

    function destroyTimeline() {
        timeline.destroy();
    }

    function alterDate(date, delta) {
        return new Date(date.getTime() - (delta * 24 * 60 * 60 * 1000));
    }

    function getTimelineInformation(start, end) {
        $.ajax({
            method: 'get',
            url: '/graph/gettimelineinformation',
            data: { start: start, end: end },
            success: function (response) {
                clearData(); // Clear the display
                for (var i = 0; i < response.length; i++) {
                    addNode(response[i]);
                }
            },
            error: function (response) { console.error(response); }
        });
    }
}