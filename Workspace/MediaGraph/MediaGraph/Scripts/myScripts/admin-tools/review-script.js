$(document).ready(function () {
    var isEditing;
    $('#reject-button').on('click', function (event) {
        document.forms['node-form']['Notes'].value = $('#notes-input').val();
        document.forms['node-form']['Approved'].value = false;
        enableInputs();
        $('#node-form').submit();
    });
    $('#approve-button').on('click', function (event) {
        enableInputs();
        document.forms['node-form']['Notes'].value = $('#notes-input').val();
        document.forms['node-form']['Approved'].value = true;
        $('#node-form').submit();
    });
    $('#edit-info-button').on('click', function (event) {
        if (isEditing) {
            disableInputs();
        } else {
            enableInputs();
        }
    });

    // For now, disable validation of the node form
    $('#node-form').find('input').on('blur', function (event) {
        $(this).removeClass('invalid valid');
    });

    disableInputs();

    function disableInputs() {
        $('#node-form').find('input:not([hidden])').attr('disabled', 'disabled');
        $('#node-form').find('select').attr('disabled', 'disabled').material_select();
        $('#relationship-form').find('input:not([hidden])').attr('disabled', 'disabled');
        $('.chips').addClass('disabled');
        $('#relationship-form').find('select').attr('disabled', 'disabled').material_select();
        isEditing = false;
    }

    function enableInputs() {
        $('#node-form').find('input:not([hidden])').removeAttr('disabled');
        $('#node-form').find('select').removeAttr('disabled').material_select();
        $('#relationship-form').find('input:not([hidden])').removeAttr('disabled');
        $('.chips').removeClass('disabled');
        $('#relationship-form').find('select').removeAttr('disabled').material_select();
        isEditing = true;
    }
});