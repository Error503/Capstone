var totalPages = 0;
var currentPage = 0;

function searchStarted() {
    //$('#request-list-container').hide();
    //$('#preloader').show().addClass('active');
}

function searchCompleted() {
    //$('#preloader').hide().removeClass('active');
    //$('#request-list-container').show();
}

function searchSuccessful(response) {
    totalPages = response.TotalPages;
    currentPage = response.CurrentPage;
    updateDisplay(response.Requests);
    updateSytling();
}

function searchFailed(response) {
    console.error(response);
}

function updateDisplay(list) {
    $('#page-selection-box').val(currentPage);
    $('#total-pages').html(totalPages);
    $('#request-list').empty();
    for (var i = 0; i < list.length; i++) {
        $('#request-list').append('<a href="/admintools/viewrequest/' + list[i].NodeData.Id + '" class="collection-item">' +
            getElementBadge(list[i]) + '[' + getUpdateTypeString(list[i].RequestType) + '] ' + list[i].NodeData.CommonName + '</a>');
    }

    function getElementBadge(ele) {
        var str = '<span class="new badge"></span>';
        if (ele.Reviewed) {
            $(str).addClass(ele.Approved ? 'blue' : 'red').attr('data-badge-caption', ele.Approved ? 'approved' : 'rejected');
        }

        return str;
    }

    function getUpdateTypeString(type) {
        var str = "Unknown";
        if (type === 1) {
            str = "Addition";
        } else if (type === 2) {
            str = "Update";
        } else if (type === 3) {
            str = "Delete";
        }

        return str;
    }
}

function updateSytling() {
    if (currentPage === 1) {
        $('#previous-page-button').addClass('disabled');
    } else {
        $('#previous-page-button').removeClass('disabled');
    }

    if (currentPage === totalPages) {
        $('#next-page-button').addClass('disabled');
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