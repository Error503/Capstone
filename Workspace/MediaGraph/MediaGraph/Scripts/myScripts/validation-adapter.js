$(document).ready(function () {
    $.validator.setDefaults({
        //onkeyup: false,
        errorClass: 'invalid',
        validClass: 'valid',
        errorPlacement: function (error, element) {
            $('label[for="' + element[0].id + '"]').attr('data-error', error.contents().text());
        },
        ignore: '.chips input',
    });
});