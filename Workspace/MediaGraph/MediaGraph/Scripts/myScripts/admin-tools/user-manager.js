function searchSuccessful(response) {
    currentPage = response.CurrentPage;
    totalPages = response.TotalPages == 0 ? 1 : response.TotalPages;
    updateDisplay(response.Users);
    updateSytling();
}

function updateUser(user, role) {
    $.ajax({
        method: 'POST',
        url: '/admintools/updateUser',
        data: { id: user, role: role },
        success: function () {
            Materialize.toast('User updated!', 2250);
        },
        error: function (response) { console.error(response); }
    });
}

function updateDisplay(list) {
    $('#user-list').empty();

    if (list.length > 0) {
        // Populate the list
        for (var i = 0; i < list.length; i++) {
            $('#user-list').append('<li class="collection-item valign-wrapper">' +
                '<div class="col s5">' + list[i].Username + '</div>' +
                '<div class="col s3 input-field">' + getSelectList(list[i]) + '</div>' +
                '<div class="col s2"><button type="button" data-user="' + list[i].Id + '" class="waves-effect waves-blue btn-flat update-button"><i class="material-icons">edit</i></button></div>' +
                '<div class="col s2"><button type="button" data-user="' + list[i].Id + '" class="waves-effect waves-red btn-flat modal-trigger" data-target="confirm-delete-modal"><i class="material-icons">delete</i></button></div>' +
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

    // Initialize the materialize select fields and set up events
    updateFunctions();
}

function updateFunctions() {
    $('select').material_select();
    $('.update-button').on('click', function (event) {
        var userId = $(this).attr('data-user');
        var role = $('#user-role-' + userId).val();
        updateUser(userId, role);
    });
}

$(document).ready(function () {
    var selectedUserTrigger = null;
    $('.modal').modal({
        dismissible: false,
        ready: function (modal, trigger) {
            selectedUserTrigger = trigger;
        }, 
        complete: function () { selectedUserTrigger = null; }
    });
    $('#confirm-delete-button', '.modal').on('click', function (event) {
        if (selectedUserTrigger != null) {
            $.ajax({
                method: 'delete',
                url: '/admintools/deleteuser',
                data: { id: $(selectedUserTrigger).attr('data-user') },
                success: function (response) {
                    if (response.success) {
                        // Remove the element from the page
                        $(selectedUserTrigger).parents('li.collection-item').remove();
                        // Create a toast to inform the user
                        createToast("The user has been deleted.", 2500);
                    }
                },
                error: function (response) {
                    console.error(response);
                }
            });
        }
    });
    // Initialize select dropdowns and set up events
    updateFunctions();
});