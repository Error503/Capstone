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
            beforeSend: searchStarted,
            complete: searchCompleted,
            success: searchSuccessful,
            failure: searchFailed
        });
    }
});