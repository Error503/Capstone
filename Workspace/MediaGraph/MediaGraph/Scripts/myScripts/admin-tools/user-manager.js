function displayPreLoader() {
    $('#user-table').hide();
    $('#preloader').show();
}

function hidePreloader() {
    $("#preloader").hide();
    $("#user-table").show();
}

function userPageReceieved(response) {
    // Replace the content
    $("#user-list").append(response);
    // Initialize all select fields
    $('select').material_select();
}

function errorReceived(response) {
    console.log(response);
    alert("Error retrieving user page");
}

function updateUser(userId) {
    // Get the roles
    var roles = null;
    $.ajax({
        method: "POST",
        url: "/admintools/updateuser",
        data: { userId: userId, role: document.forms['user-form-' + userId]['roles'].value },
        success: function (response) {
            console.log("User updated");
        },
        error: function (response) {
            console.log(response);
            alert("Error while updating user");
        }
    });
}

$(document).ready(function () {
    $.ajax({
        method: "GET",
        url: "/admintools/userpage?username=&useremail=&page=1&resultsperpage=25",
        beforeSend: displayPreLoader,
        complete: hidePreloader,
        success: userPageReceieved,
        failure: errorReceived,
    });
})