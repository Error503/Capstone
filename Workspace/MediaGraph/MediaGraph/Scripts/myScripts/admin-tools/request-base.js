var totalPages = 0;
var currentPage = 0;

function updateSytling() {
    if (currentPage <= 1) {
        $('#previous-page-button').addClass('disabled');
    } else {
        $('#previous-page-button').removeClass('disabled');
    }

    if (currentPage >= totalPages) {
        $('#next-page-button').addClass('disabled');
        $('#page-selection-box').val(totalPages);
        currentPage = totalPages;
    } else {
        $('#next-page-button').removeClass('disabled');
    }
}

$(document).ready(function () {
    var isScreenInSmallState = false;
    // Check the screen size
    checkScreenSize();
    // Add an event handler for the window resizing
    $(window).on('resize', checkScreenSize);

    function checkScreenSize() {
        // Get the screen width
        var screenWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
        var isScreenSmall = screenWidth <= 600; // Materialize's setting for small screens
        // If the state has changed,
        if (isScreenSmall != isScreenInSmallState) {
            // If the screen is currently small,
            if (isScreenSmall) {
                isScreenInSmallState = true;
                var filterWrapper = $('#filter-wrapper'); // Get the filter content
                filterWrapper.children('h5').hide(); // Hide the title
                $('#filter-section').empty().append('<ul class="collapsible" data-collapsible="accordian">' +
                    '<li><div class="collapsible-header"><i class="material-icons">filter_list</i>Filter</div>' +
                    '<div class="collapsible-body">' + filterWrapper[0].outerHTML + '</div></li></ul>');
                $('.collapsible').collapsible();
                $('#hide-button').show();
            } else {
                isScreenInSmallState = false;
                var filterWrapper = $('#filter-wrapper'); // Get the filter content
                filterWrapper.children('h5').show(); // Show the title
                $('#filter-section').empty().append(filterWrapper[0].outerHTML);
                $('#hide-button').hide();
            }
        }
    }
    $('#filter-button').on('click', function (event) {
        // If the screen is in the small state,
        if (isScreenInSmallState) {
            // Close the collapsible
            $('.collapsible').collapsible('close', 0);
        }
    });
    $('#hide-button').on('click', function (event) {
        $('.collapsible').collapsible('close', 0);
    });
    $('select').material_select();
    $('.datepicker').pickadate({
        format: 'yyyy-mm-dd',
        selectMonths: true,
        selectYears: 10,
        closeOnSelect: true
    });
    currentPage = Number.parseInt($('#page-selection-box').val());
    totalPages = Number.parseInt($('#total-pages').html());

    updateSytling();

    $('#page-selection-box').on('change', function (event) {
        var input = $(this).val();
        if (input > 0 && input <= totalPages) {
            currentPage = input;
            document.forms['filterForm']['PageNumber'].value = input;
            sendForm();
        } else {
            $(this).value = currentPage;
        }
    });

    $('#previous-page-button').on('click', function (event) {
        currentPage -= 1;
        document.forms['filterForm']['PageNumber'].value = currentPage;
        sendForm();
    });
    $('#next-page-button').on('click', function (event) {
        currentPage += 1;
        document.forms['filterForm']['PageNumber'].value = currentPage;
        sendForm();
    });

    function sendForm() {
        $.ajax({
            method: $('#filterForm').attr('data-ajax-method'),
            url: $('#filterForm').attr('action'),
            data: $('#filterForm').serialize(),
            success: searchSuccessful,
            failure: function (response) { console.error(response); }
        });
    }
});