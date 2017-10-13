 var COMPANY_TYPE = 1, MEDIA_TYPE = 2, PERSON_TYPE = 3;
$(document).ready(function () {


    var id = null;
    var otherNames = [];
    var genres = [];
    var relatedLinks = [];
    var relatedCompanies = [];
    var relatedMedia = [];
    var relatedPeople = [];

    var activeGroup = 'Companies';
    var activeIndex = -1;
    var activeGroupArray = relatedCompanies;
    var ignoreNextSelection = false;

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
    $('#relationship-chips').material_chip();
    Materialize.updateTextFields();
    $('#relationship-chips').find('input').attr('disabled', 'disabled');
    // Wire up events to handle changes to the relationship chips   
    $('#relationship-chips').on('chip.add', function (e, chip) {
        activeGroupArray[activeIndex].roles.push(chip.tag);
        document.forms['nodeForm']['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
    });

    $('#relationship-chips').on('chip.delete', function (e, chip) {
        activeGroupArray[activeIndex].roles.splice(activeGroupArray[activeIndex].roles.indexOf(chip.tag), 1);
        document.forms['nodeForm']['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
    });

    // Set up events
    $('#node-content-type').on('change', function (event) {
        if ($(this).val() !== "") {
            getNodeInformation($(this).val());
            $(this).siblings('input').attr('disabled', 'disabled');
            $('#add-relationship-button').removeClass('disabled');
            $('#submission-section').find('button').removeClass('disabled');
        }
    });

    $('.tab').on('click', function (event) {
        selectTab($(this).attr('tab-value'));
    });

    $('#add-relationship-button').on('click', function (event) {
        // Add the relationship
        activeGroupArray.push(new Relationship(id));
        // Populate the list
        populateRelationshipList();
        // Update the active index
        selectGroupItem(activeGroupArray.length - 1);
    });

    $('#relationship-name-entry').on('keyup', function (event) {
        // Update the value
        activeGroupArray[activeIndex].targetName = $(this).val();
        // Update the field value
        $($('.relationship-group > li')[activeIndex]).children('span').html(getDisplayName(activeGroupArray[activeIndex].targetName));
    });
    $('#relationship-name-entry').on('change', function (event) {
        // Update the input field
        document.forms['nodeForm']['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
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

    function selectTab(tab) {
        activeGroup = tab;
        $('#active-group-name').html(tab);

        if (tab === 'Companies') {
            activeGroupArray = relatedCompanies;
        } else if (tab === 'Media') {
            activeGroupArray = relatedMedia;
        } else if (tab === 'People') {
            activeGroupArray = relatedPeople;
        }

        // Poplulate the list
        populateRelationshipList();
    }

    function selectGroupItem(index) {
        activeIndex = ignoreNextSelection ? -1 : index;
        ignoreNextSelection = false;

        if (activeIndex < 0) {
            disableRelationshipInputs();
            activeClassRemover();
        } else {
            enableRelationshipInputs();
            // Update whether the 'active' class is present
            if (activeGroupArray[activeIndex].roles.length === 0) {
                $('#relationship-chips-label').removeClass('active');
            } else {
                $('#relationship-chips-label').addClass('active');
            }
            if (activeGroupArray[activeIndex].targetName === null ||
                activeGroupArray[activeIndex].targetName === '') {
                $('#relationship-name-label').removeClass('active');
            } else {
                $('#relationship-name-label').addClass('active').val(activeGroup[activeIndex].targetName);
            }
        }
        // Style the relationship list
        styleRelationshipList();
    }

    function populateRelationshipList() {
        // Clear the list
        var list = $('.relationship-group');
        $(list).empty();
        for (var i = 0; i < activeGroupArray.length; i++) {
            $(list).append('<li class="collection-item" rel-index="' + i + '">' +
                '<span class="truncate relationship-label">' + getDisplayName(activeGroupArray[i].targetName) + '</span><a class="secondary-content pointer-item black-text remove-relationship-button"><i class="material-icons">close</i></a></li > ');
        }
        // Bind list events
        bindListEvents();
        // We just populated the list, so deselect the item
        selectGroupItem(-1);
    }

    function bindListEvents() {
        $('.relationship-group > li').on('click', function (event) {
            selectGroupItem(Number.parseInt($(this).attr('rel-index')));
        });

        $('.remove-relationship-button').on('click', function (event) {
            // Remove the item
            activeGroupArray.splice(Number.parseInt($(this).attr('rel-index')), 1);
            // Update the input field
            document.forms['nodeForm']['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
            // Ignore the next selection (effectively saying select index -1)
            ignoreNextSelection = true;
            // Update the list
            populateRelationshipList();
        });
    }

    function getDisplayName(name) {
        return name !== null && name !== '' ? name : 'No name given';
    }

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
        if (type === MEDIA_TYPE || type === "Media") {
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

    // ===== Helper Functions =====
    function activeClassRemover() {
        // Remove the 'active' class from the input fields
        $('#relationship-name-label').removeClass('active');
        $('#relationship-chips-label').removeClass('active');
    }

    // Disables the relationship entry fields
    function disableRelationshipInputs() {
        $('#relationship-name-entry').attr('disabled', 'disabled').val('');
        $('#relationship-chips').material_chip({ data: [] });
        $('#relationship-chips').addClass('disabled');
        $('#relationship-chips').find('input').attr('disabled', 'disabled');
    }

    // Enables the relationship entry fields
    function enableRelationshipInputs() {
        $('#relationship-name-entry').removeAttr('disabled').val(activeGroupArray[activeIndex].targetName);
        // Update chips
        var chipsData = [];
        for (var i = 0; i < activeGroupArray[activeIndex].roles.length; i++) {
            chipsData.push({ tag: activeGroupArray[activeIndex].roles[i] });
        }
        $('#relationship-chips').material_chip({ data: chipsData });
        $('#relationship-chips').removeClass('disabled');
        $('#relationship-chips').find('input').removeAttr('disabled');
    }

    // Styles the relationship list items
    function styleRelationshipList() {
        // Get the list items
        var collection = $('.relationship-group').children('li');
        // Loop through the group array
        for (var x = 0; x < activeGroupArray.length; x++) {
            $(collection[x]).removeClass('light-green red accent-1');
            // If this is the active item
            if (x === activeIndex) {
                $(collection[x]).addClass('light-green accent-1');
            } else {
                // If the element is invalid,
                if (activeGroupArray[x].targetName === null || activeGroupArray[x].targetName === '') {
                    $(collection[x]).addClass('red accent-1');
                }
            }
        }
    }

    // ===== Relationship Constructor =====

    function Relationship(source) {
        return {
            sourceId: source != null ? source : null,
            targetId: null,
            targetName: null,
            roles: []
        };
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
