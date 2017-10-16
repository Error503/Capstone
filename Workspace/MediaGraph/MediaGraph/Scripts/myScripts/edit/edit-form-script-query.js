var COMPANY_TYPE = 1, MEDIA_TYPE = 2, PERSON_TYPE = 3;
var id = null;
var otherNames = [];
var genres = [];
$(document).ready(function () {

    // If there is already form content, show and responed to that content
    if (document.forms['nodeForm']['ContentType'] != null) {
        $('#node-content-type').attr('disabled', 'disabled');
        $('#hidden-divider').show();
        $('#node-info').show();
        $('#add-relationship-button').removeClass('disabled');
        $('#submission-section').find('button').removeClass('disabled');
        materializeSetup();
        // Bind the model
        bindModel(document.forms['nodeForm']['ContentType'].value);
        // Get the JSON values from the inputs
        otherNames = JSON.parse(document.forms['nodeForm']['OtherNames'].value);
        var otherNameChips = [];
        for (var i = 0; i < otherNames.length; i++) {
            otherNameChips.push({ tag: otherNames[i] });
        }
        $('#other-names').material_chip({ data: otherNameChips });
        if (otherNameChips.length > 0) {
            $('#other-names').siblings('label').addClass('active');
        }
        if (document.forms['nodeForm']['ContentType'].value == 'Media') {
            genres = JSON.parse(document.forms['nodeForm']['Genres'].value);
            var genreChips = [];
            for (var x = 0; x < genres.length; x++) {
                genreChips.push({ tag: genres[x] });
            }
            $('#genre-chips').material_chip({ data: genreChips });
            if (genreChips.length > 0) {
                $('#genre-chips').siblings('label').addClass('active');
            }
        }
        relatedCompanies = JSON.parse(document.forms['nodeForm']['RelatedCompanies'].value);
        relatedMedia = JSON.parse(document.forms['nodeForm']['RelatedMedia'].value);
        relatedPeople = JSON.parse(document.forms['nodeForm']['RelatedPeople'].value);
        selectTab('Companies');
    }

    // Set up materialize things
    $('select').material_select();
    Materialize.updateTextFields();
    // Set up events
    $('#node-content-type').on('change', function (event) {
        if ($(this).val() !== "") {
            getNodeInformation($(this).val());
            $(this).siblings('input').attr('disabled', 'disabled');
            $('#add-relationship-button').removeClass('disabled');
            $('#submission-section').find('button').removeClass('disabled');
        }
    });
    $('#reset-button').on('click', function (event) {
        if (window.confirm('Are you sure you want to reset the form? All entered data will be lost.')) {
            $('#node-content-type').siblings('input').removeAttr('disabled');
            $('#hidden-divider').hide();
            $('#node-info').empty().hide();
            selectGroupItem(-1);
            $('#add-relationship-button').addClass('disabled');
            $('#submission-section').find('button').addClass('disabled');
            // Clear the model information
            relatedCompanies = [];
            relatedMedia = [];
            relatedPeople = [];
            relatedLinks = [];
            genres = [];
            otherNames = [];
            selectTab('Companies');
        }
    });

    bindListEvents();

    // ===== Requesting Functions =====
    function getNodeInformation(type) {
        $.ajax({
            method: 'GET',
            url: '/edit/getinformation?type=' + type,
            success: function (response) {
                $('#node-info').append(response);
                $('#node-info').show();
                $('#hidden-divider').show();

                materializeSetup();
                // Bind the model
                bindModel(type);
            },
            error: function (response) {
                console.error(response);
            },
        });
    }

    function bindModel(type) {
        if (type == MEDIA_TYPE || type === "Media") {
            console.log("MEDIA BIND");
            // Set up the genre chips
            $('#genre-chips').on('chip.add', function (e, chip) {
                genres.push(chip.tag);
                // Update the input field
                document.forms['nodeForm']['Genres'].value = JSON.stringify(genres);
            });
            $('#genre-chips').on('chip.delete', function (e, chip) {
                genres.splice(genres.indexOf(chip.tag), 1);
                document.forms['nodeForm']['Genres'].value = JSON.stringify(genres);
            });
        } else if (type === PERSON_TYPE || type == "Person") {
            $('#given-name').add('#family-name').on('change', function (event) {
                document.forms['nodeForm']['CommonName'] = document.forms['nodeForm']['GivenName'].value + ' ' + document.forms['nodeForm']['FamilyName'].value;
            });
        }

        // Get a reference to the id
        id = document.forms['nodeForm']['Id'].value;
        // Apply the 'active' class to all fields that have input 
        var inputs = $('#node-info').find('input[type="text"]');
        for (var i = 0; i < inputs.length; i++) {
            if ($(inputs[i]).val() !== null && $(inputs[i]).val() !== '') {
                $(inputs[i]).siblings('label').addClass('active');
            }
        }

        // Set up other-names chips
        $('#other-names').on('chip.add', function (e, chip) {
            otherNames.push(chip.tag);
            // Update the input field
            document.forms['nodeForm']['OtherNames'].value = JSON.stringify(otherNames);
        });
        $('#other-names').on('chip.delete', function (e, chip) {
            otherNames.splice(otherNames.indexOf(chip.tag), 1);
            // Update the input field
            document.forms['nodeForm']['OtherNames'].value = JSON.stringify(otherNames);
        });
    }
});

// ===== Helper Functions =====

// Sets up materialize plugin fields
function materializeSetup() {
    $('#node-info').find('select').material_select();
    $('#node-info').find('.chips').material_chip();
    $('.datepicker').pickadate({
        format: 'yyyy-mm-dd',
        selectMonths: true,
        selectYears: 30,
        closeOnSelect: true
    });
}

function validate() {
    var form = document.forms['nodeForm'];
    var valid = true;

    if (form['ContentType'].value !== PERSON_TYPE) {
        if (form['CommonName'].value === null || form['CommonName'] === '') {
            valid = false;
        }
    } else {
        if (form['FamilyName'].value === null || form['FamilyName'].value === '' ||
            form['GivenName'].value === null || form['GivenName'].value === '') {
            valid = false;
        }
    }

    if (form['ContentType'].value === COMPANY_TYPE || form['ContentType'].value === PERSON_TYPE) {
        var release = new Date(form['ReleaseDate'].value);
        var death = new Date(form['DeathDate'].value);
        // Death date cannot be before release date
        if (death < release) {
            valid = false;
        }
    }

    if (valid) {
        // Submit the form
        document.forms['nodeForm'].submit();
    }
}
