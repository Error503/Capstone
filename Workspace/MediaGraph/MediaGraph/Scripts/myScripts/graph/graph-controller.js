var targetElementId = 'visualization-target';
var display;
var selectedType = 'network';
$(document).ready(function () {
    // Event handling to clear the context menus
    document.onclick = function (event) {
        $('#context-popup').removeClass('active').addClass('inactive');
        $('#info-popup').removeClass('active').addClass('inactive');
    };
    // Set up events for the visualization options
    var radioButtons = document.forms['visualization-options'].visualizationType;
    for (var i = 0; i < radioButtons.length; i++) {
        $(radioButtons[i]).on('click', function (event) {
            if ($(this).val() !== selectedType) {
                // Destroy the current visualization
                $('#visualization-target').empty().removeClass('network-display timeline-display');
                display.clear();
                if ($(this).val() === 'network') {
                    initializeNetworkDisplay();
                } else if ($(this).val() === 'timeline') {
                    initializeTimelineDisplay();
                }
            }
        });
    }
    $('#search-button').on('click', function (event) {
        getInformation();
    });
    // Set up autocomplete
    setupAutocomplete($('input.autocomplete'), true, autocompleteCallback);

    function autocompleteCallback(value) {
        document.forms['search-form']['id'].value = value.id;
        getInformation();
    }

    function initializeNetworkDisplay() {
        selectedType = 'network';
        display = new NetworkDisplay(targetElementId, null);
        $('#visualization-target').addClass('network-display');
    }
    function initializeTimelineDisplay() {
        selectedType = 'timeline';
        display = new TimelineDisplay(targetElementId, null, null);
        $('#visualization-target').addClass('timeline-display');
    }

    // Initialize the network display
    initializeNetworkDisplay();
});

// ===== Search Functions =====

function getInformation(position) {
    var formAction = selectedType === 'network' ? '/graph/networkdata' : '/graph/timelinedata';
    $.ajax({
        method: 'get',
        url: formAction,
        data: $('#search-form').serialize(),
        success: function (response) {
            if (selectedType === 'network') {
                updateNetwork(response, position);
            } else {
                updateTimeline(response);
            }
        },
        error: function (response) { console.log(response); }
    });
}

function updateNetwork(data, position) {
    if (data.Source != null) {
        display.addNode(data.Source, position);
        for (var i = 0; i < data.RelatedNodes.length; i++) {
            display.addNode(data.RelatedNodes[i], position);
            display.addEdge(data.Source.Id, data.RelatedNodes[i]);
        }
    }

    // TODO: after stabilizing, fit the view
}
function updateTimeline(data) {
    console.log("TIMELINE UPDATE NOT IMPLEMENTED");
}

// ===== Context Popup Methods =====

function displayContextMenu(nodeId, position, values) {
    $('#info-popup').hide();
    // Update the context options
    $('#edit-context-link').attr('href', '/edit/index/' + nodeId);
    $('#delete-context-link').attr('data-id', nodeId);
    // If this is the network display, show the cluster option
    if (selectedType == 'network') {
        $('#cluster-link').show();
    } else {
        $('#cluster-link').hide();
    }
    // Display the pop up
    $('#context-popup').removeClass('inactive').addClass('active').css({ left: position.x + 50, top: position.y + 50 });
}

function generateContextMenu(position, items) {
    $('#info-popup').removeClass('active').addClass('inactive'); // Hide the info popup
    $('#context-items').empty();
    // Create the context popup
    for (var i = 0; i < items.length; i++) {
        $('#context-items').append('<a href="' + items[i].link + '" class="collection-item">' + items[i].text + '</a>');
    }
    // Position the context popup
    $('#context-popup').removeClass('inactive').addClass('active').css({ left: position.x + 50, top: position.y + 50 });
}

function flag() {
    var selected = display.graphic.getSelection().nodes[0]; // Get the selected node.
    // Flag that node for deletion
    $.ajax({
        method: 'post',
        url: '/edit/flagdeletion',
        data: { id: selected },
        success: function (response) { createToast("Node flagged for deletion!", 2500); },
    });
}

function getNodeInformation(nodeId, position) {
    $.ajax({
        method: 'get',
        url: '/graph/getnodeinformation',
        data: { id: nodeId },
        success: function (response) {
            $('#info-popup').removeClass('inactive').addClass('active').css({ top: position.y + 50, left: position.x + 50 });
            var dateLabel;
            if (response.ContentType == 1) {
                dateLabel = "Date Founded";
            } else if (response.ContentType == 2) {
                dateLabel = "Release Date";
            } else if (response.ContentType == 3) {
                dateLabel = "Date of Birth";
            }
            $('#info-table').empty().append(
                '<tr><td>Name</td><td>' + createLabel(response.CommonName) + '</td></tr>' +
                '<tr><td>' + dateLabel + '</td><td>' + (response.ReleaseDate != null ? parseLongDateValue(response.ReleaseDate).toDateString() : "Unknown") + '</td></tr>'
            );
        },
        error: function (response) { console.error(response); }
    });
}