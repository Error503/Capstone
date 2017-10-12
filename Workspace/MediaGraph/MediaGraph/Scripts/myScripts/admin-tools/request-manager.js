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