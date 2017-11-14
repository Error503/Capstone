$(document).ready(function () {
    $(".button-collapse").sideNav();
    $('.dropdown-button').dropdown();
    $('.modal').modal();
});

// Creates a captialized label out of the given string that fits within the given length
var LABEL_LENGTH = 15;
function createLabel(value, length) {
    var trimLength = length != null ? length : -1
    var result = value;
    // Capitalize the label
    if (result != null) {
        result = result.replace(/\s(\S)/g, function (substring, args) {
            return substring.toUpperCase();
        });
        result = result.replace(/^(\S)/g, function (substring, args) {
            return substring.toUpperCase();
        });
        // Trim the label to length
        if (trimLength > 0 && result.length > trimLength) {
            result = truncateLabel(result, trimLength);
        }
    }
    return result;
}
function truncateLabel(value, length) {
    return (length != null && value.length > length) ? value.substring(0, length - 3) + '...' : value;
}

// Parses the given long value as a date
function parseLongDateValue(date) {
    if (date == null || date <= 0) {
        return new Date(1, 0, 1);
    }
    var upperConst = 10000;
    var lowerConst = 100;
    return new Date(Math.floor(date / upperConst), Math.floor(((date % upperConst) / lowerConst) - 1), Math.floor(date % lowerConst));
}

// Creates a toast message with default settings
function createToast(msg, dur) {
    Materialize.toast(msg, dur);
}

// Initializes autocomplete functionality on the given element
function setupAutocomplete(element, useClearing, callback) {
    var autocompleteData = {};
    var autocompleteStorage = {};
    var lastSearchText = null;
    var minimumLength = 3;
    var wasCompleted = false;
    $(element).on('keyup', function (event) {
        var value = $(this).val().toLowerCase().trim();
        // If the length of the search text is at least the minimum length AND
        // There is no previous search OR the current text is shorter than the last search
        if (value.length >= minimumLength) {
            if (lastSearchText == null || value.length < lastSearchText.length) {
                // Update the search value
                lastSearchText = value;
                // Perform the search
                $.ajax({
                    method: 'get',
                    url: '/autocomplete/index',
                    data: { t: value },
                    success: function (response) {
                        autocompleteData = {};
                        autocompleteStorage = {};
                        for (var i = 0; i < response.length; i++) {
                            autocompleteData[truncateLabel(response[i].Item1)] = '';
                            autocompleteStorage[response[i].Item1.toLowerCase()] = response[i].Item2;
                        }
                        $(element).autocomplete({
                            data: autocompleteData,
                            limit: 20,
                            onAutocomplete: function (selection) {
                                wasCompleted = true;
                                callback({ value: selection, id: autocompleteStorage[selection.toLowerCase()] });
                            },
                            minLength: minimumLength - 1
                        })
                    },
                    error: function (response) { console.error(response); }
                });
            }
        } else {
            wasCompleted = false;
            autocompleteData = {};
            autocompleteStorage = {};
            lastSearchText = null;
        }
    }).on('focus', function (event) {
        if (useClearing && wasCompleted) {
            $(this).val(''); // Clear the search box
            wasCompleted = false;
        }
    });
}

// Creates a materialize chip whose placeholders won't get mixed up
function customChips(element, options, data) {
    $(element).material_chip({ data: data != null ? data : [] }); // Create the chips input
    var chipInput = $(element).find('input'); // Get the chips input
    var chipCount = 0;
    // If there is tooltip settings
    if (options.hasOwnProperty('tooltip')) {
        chipInput.addClass('tooltipped').tooltip(options.tooltip);
    }

    // Chip auto capture
    chipInput.on('blur', function (event) {
        var val = $(this).val();
        // If there is content in the input field,
        if (val.length > 0) {
            chipInput.tooltip('remove');
            // Get the exsiting chip data
            var existing = $(element).material_chip('data');
            // If the new value is not in the existing chips
            var found = false;
            for (var i = 0; i < existing.length && !found; i++) {
                found = existing[i].tag == val;
            }
            // If the value was not found,
            if (!found) {
                existing.push({ tag: val }); // Add the value
                // Update the chip input
                customChips(element, options, existing);
            } else {
                $(this).val(''); // Clear the value
            }
        }
    });

    // Set up the events on the chips input
    $(element).on('chip.add', function (e, chip) {
        chipCount += 1; // Increment the chip count
        // If the chip count is now greater than 0,
        if (chipCount > 0) {
            // Set the value of the placeholder to the secondary placeholder,if present,
            if (options.hasOwnProperty('secondaryPlaceholder')) {
                chipInput.attr('placeholder', options.secondaryPlaceholder)
            }
        }
    }).on('chip.delete', function (e, chip) {
        chipCount -= 1; // Decrement the chip count
        // IF the chip count is now 0,
        if (chipCount === 0) {
            // Set the value of the placeholder to the primary placeholder, if present,
            if (options.hasOwnProperty('placeholder')) {
                chipInput.attr('placeholder', options.placeholder);
            }
        }
    });
    // This will prevent the label from being placed into the input field
    $(chipInput).on('blur', function (event) {
        if (chipCount == 0) {
            $('label[for="' + element.substring(1) + '"]').addClass('active');
        }
    });
    // Set the initial placeholder
    if (data != null && data.length > 0 && options.hasOwnProperty('secondaryPlaceholder')) {
        chipInput.attr('placeholder', options.secondaryPlaceholder);
    } else if (options.hasOwnProperty('placeholder')) {
        chipInput.attr('placeholder', options.placeholder);
    }
}
