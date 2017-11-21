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
    var elementData = new vis.DataSet();
    var timeline = new vis.Timeline(document.getElementById(elementId), elementData, options != null ? options : default_options);
    var colorOption = 0;

    timeline.on('click', function (props) {
        if (props.what === 'item') {
            getNodeInformation(elementData.get(props.item).nid, { x: props.x, y: props.y - 50});
        }
    });
    timeline.on('contextmenu', function (props) {
        if (props.what === 'item') {
            generateContextMenu({ x: props.x, y: props.y - 50 }, [
                { text: 'Edit Node', link: '/edit/index/' + elementData.get(props.item).nid },
                { text: 'Flag for Deletion', link: 'javascript:flag()'}
            ]);
            // Select the item
            timeline.setSelection([props.item]);
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
        destroy: destroyTimeline,
        goToDate: goToDate,
        changeColorPalette: changeColorPalette,
        checkScreenSize: checkScreenSize
    };

    // ===== Functions =====

    function checkScreenSize(screenWidth) {
        // Nothing to check for now
    }

    function addNode(data) {
        var window = timeline.getWindow();
        var releaseDate = parseLongDateValue(data.ReleaseDate);
        var deathDate = parseLongDateValue(data.DeathDate);
        // If there is a release date,
        if (data.ReleaseDate > 0) {
            elementData.add({
                id: 'R:' + data.Id,
                content: createLabel(data.CommonName),
                start: releaseDate,
                type: 'point',
                className: 'type-' + data.DataType,
                nid: data.Id
            });
        }
        // If there is a death date
        if (data.DeathDate > 0) {
            elementData.add({
                id: 'D:' + data.Id,
                content: createLabel(data.CommonName),
                start: deathDate,
                type: 'point',
                className: 'death',
                nid: data.Id
            });
        }
    }
    function releaseLabel(nodeType) {
        var value = 'ERROR TYPE';
        if (nodeType === 1) {
            value = 'Founded';
        } else if (nodeType === 2) {
            value = 'Released';
        } else if (nodeType === 3) {
            value = 'Born';
        }
        return value;
    }

    function clearData() {
        elementData.clear();
    }

    function changeColorPalette(option) {

    }

    function destroyTimeline() {
        timeline.destroy();
    }

    function goToDate(longDate) {
        var asDate = parseLongDateValue(longDate);
        timeline.moveTo(asDate, { duration: 1000, easingFunction: 'easeInOutQuart' }, zoomIn);
    }
    function zoomIn() {
        var window = timeline.getWindow();
        // If the display is over the max display level,
        if (window.end.getTime() - window.start.getTime() > MAX_DISPLAY_LEVEL) {
            // Zoom in - callback function is recursive ending when the zoom is in display level
            timeline.zoomIn(1, { duration: 750, easingFunction: 'easeInOutQuart' }, zoomIn);
        }
    }


    function alterDate(date, delta) {
        return new Date(date.getTime() + (delta * 24 * 60 * 60 * 1000));
    }

    function getTimelineInformation(start, end) {
        $.ajax({
            method: 'get',
            url: '/graph/timelinedatarange',
            data: { start: start, end: end },
            success: function (response) {
                if (response.success) {
                    clearData(); // Clear the display
                    for (var i = 0; i < response.data.length; i++) {
                        addNode(response.data[i]);
                    }
                }
            },
            error: function (response) { console.error(response); }
        });
    }
}
