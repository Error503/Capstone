var custom_options = {
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
        navigationButtons: true
    }
};  

var targetElementId = 'visualization-target';
var timelineRefreshDelay = 500;
var display;
$(document).ready(function () {
    // Set up events for the visualization options
    var radioButtons = document.forms['visualization-options'].visualizationType;
    var selectedType = 'network';
    for (var i = 0; i < radioButtons.length; i++) {
        $(radioButtons[i]).on('click', function (event) {
            if ($(this).val() !== selectedType) {
                // Change the display type
                selectedType = $(this).val();
                changeVisualization();
            }
        });
    }
    $('#delete-context-link').on('click', function (response) {
        $.ajax({
            method: 'post',
            url: '/edit/flagdeletion',
            data: { id: $('#delete-context-link').attr('data-id') },
            success: function (response) {
                Materialize.toast("Flagged for deletion!", 3000);
            },
            error: searchFailed
        });
    });

    function changeVisualization() {
        // Destroy the current visualization
        $('#visualization-target').empty().removeClass('network-display timeline-display');
        display.clear();
        if (selectedType === 'network') {
            initializeNetworkDisplay();
        } else if(selectedType === 'timeline') {
            initializeTimelineDisplay();
        }
    }

    function initializeNetworkDisplay() {
        display = new NetworkDisplay(targetElementId, custom_options);
        document.onclick = function (event) {
            $('#context-popup').removeClass('active').addClass('inactive');
        };
        display.graphic.on('selectNode', function (props) {
            if (props.nodes.length > 0) {
                getSingleInformation(props.nodes[0]);
            }
        });
        display.graphic.on('selectEdge', function (props) {
            console.log(props);
        });
        display.graphic.on('doubleClick', function (props) {
            if (props.nodes.length > 0) {
                getInformation(props.nodes[0], props.event.center);
            }
        });
        display.graphic.on('oncontext', function (props) {
            if (display.graphic.getNodeAt(props.pointer.DOM) != null) {
                var selected = display.graphic.getNodeAt(props.pointer.DOM); // Get the node at the point
                console.log(selected);
                display.graphic.selectNodes([selected]); // Select the node
                // Update the context options
                $('#edit-context-link').attr('href', '/edit/index/' + selected);
                console.log($('#edit-context-link'));
                $('#delete-context-link').attr('data-id', selected);
                // Display the pop up
                $('#context-popup').removeClass('inactive').addClass('inactive active').css({ left: props.pointer.DOM.x + 50, top: props.pointer.DOM.y + 50 });
            }
            // Prevent the default functionality
            props.event.preventDefault();
        });
        $('#visualization-target').addClass('network-display');
    }
    function initializeTimelineDisplay() {
        display = new TimelineDisplay(targetElementId, null, null);
        document.onclick = null;
        // Set up event handling
        display.graphic.on('click', function (props) {
            console.log(props);
        });
        display.graphic.on('rangechanged', function (props) {
            console.log(props);
        });
        $('#visualization-target').addClass('timeline-display');
    }

    // Initialize the network display
    initializeNetworkDisplay();
});

function getSingleInformation(id) {
    $.ajax({
        method: 'get',
        url: '/graph/getnodeinformation',
        data: { id: id },
        success: populateSelectedInformation,
        error: searchFailed
    });
}

function populateSelectedInformation(response) {
    $('#node-name-value').html(capitalizeLabel(response.CommonName));
    if (response.ContentType == 1) {
        $('#release-date-label').html("Date Founded:");
    } else if (response.ContentType == 2) {
        $('#release-date-label').html("Release Date:");
    } else if (response.ContentType == 3) {
        $('#release-date-label').html("Date of Birth:");
    }
    $('#release-date-value').html(response.ReleaseDate != null ? parseLongDateValue(response.ReleaseDate).toDateString() : "Unknown");
}

function getInformation(id, position) {
    $.ajax({
        method: 'get',
        url: $('#search-form').attr('action'),
        data: { searchText: null, id: id },
        success: function (response) {
            updateInformation(response, position);
        },
        error: searchFailed
    });
}

function updateInformation(data, position) {
    if (data.Source != null) {
        display.addNode(data.Source, position);
        for (var i = 0; i < data.RelatedNodes.length; i++) {
            display.addNode(data.RelatedNodes[i], position);
            display.addEdge(data.Source.Id, data.RelatedNodes[i]);
        }
    }
}

function searchSuccessful(response) {
    updateInformation(response, null);
}

function searchFailed(response) {
    console.error(response);
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

function parseLongDateValue(date) {
    var upperConst = 10000;
    var lowerConst = 100;
    return new Date(Math.floor(date / upperConst), Math.floor(((date % upperConst) / lowerConst) - 1), Math.floor(date % lowerConst));
}