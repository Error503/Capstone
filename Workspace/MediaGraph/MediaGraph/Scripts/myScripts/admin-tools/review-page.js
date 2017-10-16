var id, otherNames, genres;

$(document).ready(function () {

    // Set up materialize elements
    $('select').material_select();
    $('.datepicker').pickadate({
        format: 'yyyy-mm-dd',
        selectMonths: true,
        selectYears: 30,
        closeOnSelect: true
    });
    // Grab the data
    grabData();
    // Disable the input fields
    disableInputFields();

    // Set up the events
    $('#edit-info-button').on('click', function (event) {
        if (allowEditing) {
            disableInputFields();
        } else {
            enableInputFields();
        }
    });
    $('#approve-button').on('click', function (event) {
        document.forms['nodeForm']['Approved'].value = true;
        $('#nodeForm').find('input.datepicker').removeAttr('disabled');
        $('#nodeForm').find('select').siblings('input').removeAttr('disabled');
        document.forms['nodeForm'].submit();
    });
    $('#reject-button').on('click', function (event) {
        document.forms['nodeForm']['Approved'].value = false;
        $('#nodeForm').find('input.datepicker').removeAttr('disabled');
        $('#nodeForm').find('select').siblings('input').removeAttr('disabled');
        document.forms['nodeForm'].submit();
    });

    // Set up chip events
    $('#other-names').on('chip.add', function (e, chip) {
        otherNames.push(chip.tag);
        document.forms['nodeForm']['OtherNames'].value = JSON.stringify(otherNames);
    }).on('chip.delete', function (e, chip) {
        otherNames.splice(otherNames.indexOf(chip.tag), 1);
        document.forms['nodeForm']['OtherNames'].value = JSON.stringify(otherNames);
    });
    if (document.forms['nodeForm']['ContentType'] === 'Media') {
        $('#genre-chips').on('chip.add', function (e, chip) {
            genres.push(chip.tag);
            document.forms['nodeForm']['Genres'].value = JSON.stringify(genres);
        }).on('chip.delete', function (e, chip) {
            genres.splice(genres.indexOf(chip.tag), 1);
            document.forms['nodeForm']['Genres'].value = JSON.stringify(genres);
        });
    }

    function grabData() {
        var form = document.forms['nodeForm'];

        id = form['Id'].value;
        otherNames = JSON.parse(form['OtherNames'].value);
        var nameChips = getChipData(otherNames);
        $('#other-names').material_chip({ data: nameChips });
        if (nameChips.length > 0) {
            $('#other-names').siblings('label').addClass('active');
        }

        if (form['ContentType'] === 'Media') {
            genres = JSON.parse(form['Genres'].value);
            var genreChips = getChipData(genres);
            $('#genre-chips').material_chip({ data: chipData });
            if (genreChips.length > 0) {
                $('#genre-chips').siblings('label').addClass('active');
            }
        }
    }

    function getChipData(array) {
        var chips = [];
        for (var i = 0; i < array.length; i++) {
            chips.push({ tag: array[i] });
        }
        return chips;
    }

    function disableInputFields() {
        $('#nodeForm').find('input[name]').attr('readonly', 'readonly'); 
        $('#nodeForm').find('select').siblings('input').attr('disabled', 'disabled');
        $('#nodeForm').find('input.datepicker').attr('disabled', 'disabled');
        allowEditing = false;
        disableRelationshipInputs();

        $('#genre-chips').addClass('disabled');
        $('#genre-chips > input').attr('readonly', 'readonly');
        $('#genre-chips > .chip > i').hide();
        $('#other-names').addClass('disabled');
        $('#other-names > input').attr('readonly', 'readonly');
        $('#other-names > .chip > i').hide();
    }

    function enableInputFields() {
        $('#nodeForm').find('input[name]').removeAttr('disabled');
        $('#nodeForm').find('select').siblings('input').removeAttr('disabled');
        $('#nodeForm').find('input.datepicker').removeAttr('disabled');
        allowEditing = true;
        enableRelationshipInputs();

        $('#genre-chips').removeClass('disabled');
        $('#genre-chips > input').removeAttr('readonly');
        $('#genre-chips > .chip > i').show();
        $('#other-names').removeClass('disabled');
        $('#other-names > input').removeAttr('readonly');
        $('#other-names > .chip > i').show();
    }
});