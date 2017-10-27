$.validator.setDefaults({
    errorClass: 'invalid',
    validClass: 'valid',
    errorPlacement: function (error, element) {
        $(element).siblings('label').attr('data-error', error.contents().text());
    }
});
$('form').validate();