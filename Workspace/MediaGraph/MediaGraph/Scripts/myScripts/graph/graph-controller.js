var targetElementId = 'visualization-target';
var display;
$(document).ready(function () {
    // Event handling to clear the context menus
    document.onclick = function (event) {
        $('#context-popup').removeClass('active').addClass('inactive');
        $('#info-popup').removeClass('active').addClass('inactive');
    };
    // Set up events for the visualization options
    var radioButtons = document.forms['visualization-options'].visualizationType;
    var selectedType = 'network';
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
    var lastSearchValue = null;
    var autocompleteStorage = null;
    var autocompleteData = {};
    $('#autocomplete-field').on('keyup', function (event) {
        var value = $(this).val().trim();
        // If the lenghth of the trimmed text is greater than 3 characters
        if (value.length >= 3) {
            // If there is not an existing entry OR the current search text is shorter than the existing search
            // OR the search text is 3 characters longer than the existing search texts
            if (lastSearchValue == null || value.length < lastSearchValue.length || (value.length - lastSearchValue) >= 3) {
                // Run the autocomplete function
                $.ajax({
                    method: 'get',
                    url: '/graph/searchfornodes',
                    data: { text: value.toLowerCase().trim() },
                    success: function (response) {
                        lastSearchValue = value;
                        autocompleteStorage = {};
                        autocompleteData = {};
                        for (var i = 0; i < response.length; i++) {
                            autocompleteData[capitalizeLabel(response[i].Item1)] = '';
                            autocompleteStorage[response[i].Item1] = response[i].Item2;
                        }
                        console.log(autocompleteData);
                        $('input.autocomplete').autocomplete({
                            data: autocompleteData,
                            //limit: 20, // The number of results to display, defaults to infinity,
                            onAutocomplete: function (val) {
                                getInformation(autocompleteStorage[val.toLowerCase()], null);
                            },
                            minLength: 3, // Minimum length before autocomplete function begins
                        });
                    },
                    error: function (response) { console.error(response); }
                })
            }
        } else {
            // Clear the existing data
            lastSearchValue = null;
            autocompleteData = [];
        }
    });

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

// ===== Helper Functions =====

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

// ===== Context Popup Methods =====

function displayContextMenu(nodeId, position) {
    // Update the context options
    $('#edit-context-link').attr('href', '/edit/index/' + nodeId);
    $('#delete-context-link').attr('data-id', nodeId);
    // Display the pop up
    $('#context-popup').removeClass('inactive').addClass('active').css({ left: position.x + 50, top: position.y + 50 });
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
                '<tr><td>Name</td><td>' + capitalizeLabel(response.CommonName) + '</td></tr>' +
                '<tr><td>' + dateLabel + '</td><td>' + (response.ReleaseDate != null ? parseLongDateValue(response.ReleaseDate).toDateString() : "Unknown") + '</td></tr>'
            );
        },
        error: searchFailed
    })
}