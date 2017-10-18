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

var display;
$(document).ready(function () {
    display = new NetworkDisplay('graph', custom_options);

    display.network.on('click', function (props) {
        if (props.nodes.length > 0) {
            getSingleInformation(props.nodes[0]);
        }
    });
    display.network.on('doubleClick', function (props) {
        if (props.nodes.length > 0) {
            getInformation(props.nodes[0], props.event.center);
        }
    });
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
    if (response.ReleaseDate != null) {
        var dateTicks = Number.parseInt(response.ReleaseDate.substring(6, response.ReleaseDate.length - 1));
        $('#release-date-value').html((new Date(dateTicks)).toDateString());
    } else {
        $('#release-date-value').html("Unknown");
    }
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