$(document).ready(function () {
    // Don't allow users to press 'enter' and submit the form
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });

    $('select').material_select();
    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: 30,
        today: 'Today',
        clear: 'Clear',
        close: 'Ok',
        closeOnSelect: true
    });
    $('ul.tabs').tabs();
    $('.chips').material_chip();

    // Set up change events
    $('#content-type').on('change', function (event) {
        // Enable or disable the relationship addition buttons
        if (this.value === '') {
            $('#add-relationship-btn').addClass('disabled');
        } else {
            $('#add-relationship-btn').removeClass('disabled');
        }

        // Enable or disable the media tab
        if (this.value === 'media') {
            $('#media-info-tab').removeClass('disabled');
        } else {
            $('#media-info-tab').addClass('disabled');
        }
    });
});