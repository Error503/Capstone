var targetElementId = 'visualization-target';
var display;
var selectedType = 'network';
$(document).ready(function () {
    $('#create-node-modal').modal();
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
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            getInformation();
            event.preventDefault();
            return false;
        }
    });
    $('#autocomplete-field').on('focus', function (event) {
        $('input[name="id"]').val('');
    });
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
            if (response.success) {
                if (selectedType === 'network') {
                    updateNetwork(response.data, position);
                } else {
                    updateTimeline(response.data);
                }
            } else {
                console.log("MODAL");
                $('#create-node-modal').modal('open');
            }
        },
        error: function (response) { console.log(response); }
    });
}

function closeModal() {
    $('#create-node-modal').modal('close');
}

function updateNetwork(data, position) {
    if (data.Source != null) {
        display.addNode(data.Source, position);
        for (var i = 0; i < data.RelatedNodes.length; i++) {
            display.addNode(data.RelatedNodes[i], position);
            display.addEdge(data.Source.Id, data.RelatedNodes[i ]);
        }
    }
}
function updateTimeline(data) {
    if (data != null) {
        // Adjust the window the the release date of the node
        display.goToDate(data.ReleaseDate);
    }
}

// ===== Context Popup Methods =====

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
    var selected;
    // If we are on the network display,
    if (selectedType === 'network') {
        // Get the selected node
        selected = display.graphic.getSelection().nodes[0];
    } else {
        // Get the selected timeline item
        selected = display.elements.get(display.graphic.getSelection())[0].nid;
    }
    // Flag that node for deletion
    $.ajax({
        method: 'post',
        url: '/edit/flagdeletion',
        data: { id: selected },
        success: function (response, status, xhr) {
            var parsed = JSON.parse(xhr.getResponseHeader('X-Responded-JSON')); 
            if (parsed == null) {
                createToast('Node flagged for deletion!', 2500);
            }
        }
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