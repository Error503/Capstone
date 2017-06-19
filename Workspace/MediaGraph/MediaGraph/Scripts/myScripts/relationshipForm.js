var sourceType = -1;
var options = new Array();

function autosearch(field, text) {
    // If there is at least one character,
    if (text != null && text.length > 0) {
        // Run the search
        $.ajax({
            type: "GET",
            url: "api/autocomplete/searchasync?field=" + field + "&text=" + text,
        }).done(function (data) {
            console.log(data);
            options = data;
        });
    }
}

function addRelationship(groupId) {
    $.ajax({
        type: 'GET',
        url: 'reledit/relationshipentry?sourceType=' + sourceType + '&targetType=' + groupId,
        success: function (data) {
            var autobox = $(data).children('.rel-entry-content').first().children('input[id*="auto"]').attr('id');
            // Append the relationship entry to the DOM
            $('#rel-content-' + groupId).append(data);
            // Set up the autocomplete functionality on the new relationship entry
            setupAutocomplete(groupId, '#' + autobox);
        }
    })
}

function setupAutocomplete(groupId, entry) {
    // Add autocomplete functionality and an event to oninput
    $(entry).on('input', function () {
        autosearch(groupId, $(this).val());
        $(this).autocomplete({
            source: options,
            focus: function (event, ui) {
                event.preventDefault();
                $(this).val(ui.item.label);
            },
            select: function (event, ui) {
                event.preventDefault();
                $(this).siblings('input#targetId').val(ui.item.value);
                console.log($(entry).parent().siblings('.rel-entry-head').children('span.rel-entry-label'));
                $(entry).parent().siblings('.rel-entry-head').children('span.rel-entry-label').first().text(ui.item.label);
                $(this).val(ui.item.label);
            }
        });
    });
}

// Removes a single relationship
function removeRelationship(entry) {
    $(entry).parents('div.rel-entry').remove();
}

// Removes all relationships in a group
function clearRelationships(groupId) {
    $("#rel-content-" + groupId).empty();
}

$(function () {
    sourceType = $('#SourceType').val();
})