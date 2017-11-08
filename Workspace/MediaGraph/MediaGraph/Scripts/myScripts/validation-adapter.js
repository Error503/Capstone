$(document).ready(function () {
    $.validator.setDefaults({
        errorClass: 'invalid',
        validClass: 'valid',
        errorPlacement: function (error, element) {
            $('label[for="' + element[0].id + '"]').attr('data-error', error.contents().text());
        },
        ignore: '.chips input',
    });
    $.validator.addMethod("passwordLower", function (value, element) {
        return value.match(/^(?=.*[a-z])/) != null;
    }, 'Password must contain at least one lower case letter.');
    $.validator.addMethod('passwordUpper', function (value, element) {
        return value.match(/^(?=.*[A-Z])/) != null;
    }, 'Password must contain at least one upper case letter.');
    var v;
    if ($('#register-form')[0] != null) {
        v = registrationValidation();
    } else if ($('#update-password-form')[0] != null) {
        v = passwordValidation();
    } 
    // Find the labels with an existing validation error
    $('label[for][data-error]').each(function (index, elem) {
        $('input[name="' + $(elem).for + '"]').addClass('invalid');
    });
});

function registrationValidation() {
    return $('#register-form').validate({
        submitHandler: function (form) {
            form.submit();
        },
        invalidHandler: function (event, validator) {
            validator.focusInvalid();
        },
        onkeyup: function (element, event) { 
            // This causes the user name field to be validated on change
            if (element.id != 'Username') {
                this.element(element);
            }
        }, 
        rules: {
            Email: {
                required: true,
                email: true
            },
            Username: {
                required: true, 
                minlength: 3,
                remote: {
                    url: '/account/doesusernameexist',
                    type: 'get'
                }
            },
            Password: {
                required: true,
                minlength: 6,
                passwordLower: true,
                passwordUpper: true
            },
            ConfirmPassword: {
                required: true,
                equalTo: '#Password'
            }
        },
        messages: {
            Email: {
                required: "You must provide a valid email address.",
                email: "Provided value is not an email address."
            },
            Username: {
                required: "You must provide a username.",
                minlength: "Usernames must be at least 3 characters long.",
                remote: "That user name is already taken"
            },
            Password: {
                minlength: "Password must be at least 6 characters long."
            },
            ConfirmPassword: {
                equalTo: "Passwords do not match"
            }
        }
    });
}

function passwordValidation() {
    return $('#update-password-form').validate({
        invalidHandler: function (event, validator) {
            validator.focusInvalid();
        },
        rules: {
            OldPassword: {
                required: true
            },
            NewPassword: {
                required: true,
                minlength: 6,
                passwordLower: true,
                passwordUpper: true
            },
            ConfirmPassword: {
                required: true,
                equalTo: '#NewPassword'
            }
        },
        messages: {
            NewPassword: {
                minlength: 'Password must be at least 6 characters long'
            },
            ConfirmPassword: {
                equalTo: 'Passwords do not match'
            }
        }
    });
}