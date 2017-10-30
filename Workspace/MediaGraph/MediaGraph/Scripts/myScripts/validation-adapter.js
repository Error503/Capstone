$(document).ready(function () {
    runValidationAdapter();
});

function runValidationAdapter() {
    $('input[data-val-length-min').each(function (index, element) {
        var length = Number.parseInt($(this).attr('data-val-length-min'));
        $(this).attr('minlength', length).removeAttr('data-val-length-min');
    });
    $('input[data-val-length-max').each(function (index, element) {
        var length = Number.parseInt($(this).attr('data-val-length-max'));
        $(this).attr('maxlength', length).removeAttr('data-val-length-max');
    });
    $('input[data-val-required]').each(function (index, element) {
        $(this).attr('required', '').attr('aria-required', 'true').removeAttr('data-val-required');
    });
    $('input[data-val-equalto]').each(function (index, element) {
        var equalToWhat = '#' + $(this).attr('data-val-equalto-other').substring(2);
        $(this).attr('equalTo', equalToWhat).attr('data-msg-equalto', $(this).attr('data-val-equalto')).removeAttr('data-val-equalto-other data-val-equalto');
    });
    $('input[data-val-remote').each(function (index, element) {
        $(this).attr('remote', $(this).attr('data-val-remote-url')).attr('data-msg-remote', 'That username is already taken')
            .removeAttr('data-val-remote data-val-remote-url data-remote-additonalfields data-remote-type');
    });
    $.validator.setDefaults({
        onkeyup: false,
        errorClass: 'invalid',
        validClass: 'valid',
        errorPlacement: function (error, element) {
            console.log(element[0]);
            $('label[for="' + element[0].id + '"]').attr('data-error', error.contents().text());
        }
    });
    $('form').validate();
}
