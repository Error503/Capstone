$(document).ready(function () {
    $(".button-collapse").sideNav();
    $('.dropdown-button').dropdown();
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
            result = result.substring(0, trimLength - 3) + '...';
        }
    }
    return result;
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
                            autocompleteData[createLabel(response[i].Item1)] = '';
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
    console.log(options);
    // If there are place holder options,
    if (options.hasOwnProperty('placeholder') || options.hasOwnProperty('secondaryPlaceholder')) {
        // Set up the events
        $(element).on('chip.add', function (e, chip) {
            chipCount += 1; // Increment the chip count
            // If the chip count is now greater than 0,
            if (chipCount > 0) {
                // Set the value of the placeholder to the secondary placeholder,if present,
                if (options.hasOwnProperty('secondaryPlaceholder')) {
                    $(chipInput).attr('placeholder', options.secondaryPlaceholder)
                }
            }
        }).on('chip.delete', function (e, chip) {
            chipCount -= 1; // Decrement the chip count
            // IF the chip count is now 0,
            if (chipCount === 0) {
                // Set the value of the placeholder to the primary placeholder, if present,
                if (options.hasOwnProperty('placeholder')) {
                    $(chipInput).attr('placeholder', options.placeholder);
                }
            }
        });
    }
    // Set the initial placeholder
    if (options.hasOwnProperty('placeholder')) {
        $(chipInput).attr('placeholder', options.placeholder);
    }
}
