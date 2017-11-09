
function searchStarted() {

}

function searchCompleted() {

}

function searchSuccessful(response) {
    currentPage = response.CurrentPage;
    totalPages = response.TotalPages == 0 ? 1 : response.TotalPages;
    updateDisplay(response.Users);
    updateSytling();
}

function searchFailed(response) {
    console.error(response);
}

function materializeSetup() {
    $('select').material_select();
}

function updateUser(user, role) {
    $.ajax({
        method: 'POST',
        url: '/admintools/updateUser',
        data: { id: user, role: role },
        success: function () {
            Materialize.toast('User updated!', 2250);
        },
        error: searchFailed
    });
}

function updateDisplay(list) {
    $('#user-list').empty();

    if (list.length > 0) {
        // Populate the list
        for (var i = 0; i < list.length; i++) {
            $('#user-list').append('<li class="collection-item valign-wrapper">' +
                '<div class="col s4">' + list[i].Username + '</div>' +
                '<div class="col s4 input-field">' + getSelectList(list[i]) + '</div>' +
                '<div class="col s4"><button type="button" data-user="' + list[i].Id + '" class="waves-effect waves-blue btn-flat update-button">Update</button></div>' +
                '</li>');
        }
    } else {
        $('#user-list').append('<div class="center-align">No Results</div>');
    }

    function getSelectList(user) {
        var input = '<select id="user-role-' + user.Id + '">' +
            '<option value="0"' + (user.Role === "0" ? "selected" : "") + '>Member</option>' +
            '<option value="1"' + (user.Role === "1" ? "selected" : "") + '>Admin</option>' +
            '<option value="2"' + (user.Role === "2" ? "selected" : "") + '>Staff</option>' +
            '</select>';
        return input;
    }

    $('select').material_select();
    $('.update-button').on('click', function (event) {
        var userId = $(this).attr('data-user');
        var role = $('#user-role-' + userId).val();
        updateUser(userId, role);
    });
}

$(document).ready(function () {
    $('.update-button').on('click', function (event) {
        var userId = $(this).attr('data-user');
        var role = $('#user-role-' + userId).val();
        updateUser(userId, role);
    });
});