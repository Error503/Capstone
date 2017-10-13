var relationshipFormTarget = 'nodeForm';
var relatedCompanies = [];
var relatedMedia = [];
var relatedPeople = [];
var activeGroupArray = relatedCompanies;
var activeGroup = 'Companies';
var activeIndex = -1;
var ignoreNextSelection = false;

$(document).ready(function () {
    $('#relationship-chips').material_chip();
    $('#relationship-chips').on('chip.add', function (e, chip) {
        activeGroupArray[activeIndex].roles.push(chip.tag);
        document.forms[relationshipFormTarget]['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
    })
    .on('chip.delete', function (e, chip) {
        activeGroupArray[activeIndex].roles.splice(activeGroupArray[activeIndex].roles.indexOf(chip.tag), 1);
        document.forms[relationshipFormTarget]['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
    })

    $('#add-relationship-button').on('click', function (event) {
        // Add the relationship
        activeGroupArray.push(new Relationship(id));
        // Populate the list
        populateRelationshipList();
        // Update the active index
        selectGroupItem(activeGroupArray.length - 1);
    });

    $('#relationship-name-entry').on('change', function (event) {
        // Update the input field
        document.forms[relationshipFormTarget]['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
    })
    .on('keyup', function (event) {
        // Update the value
        activeGroupArray[activeIndex].targetName = $(this).val();
        // Update the field value
        $($('#relationship-list > li')[activeIndex]).children('span').html(getDisplayName(activeGroupArray[activeIndex]));
    });

    $('.relationship-tab').on('click', function (event) {
        activeGroup = $(this).attr('tab-value');
        $('#active-group-name').html(activeGroup);

        if (tab === 'Companies') {
            activeGroupArray = relatedCompanies;
        } else if (tab === 'Media') {
            activeGroupArray = relatedMedia;
        } else if (tab === 'People') {
            activeGroupArray = relatedPeople;
        }

        // Populate the list
        populateRelationshipList();
    });

    function selectGroupItem(index) {
        activeIndex = ignoreNextSelection ? -1 : index;
        ignoreNextSelection = false;

        if (activeIndex < 0) {
            disableRelationshipInputs();
            deactivateRelationshipInputs();
        } else {
            enableRelationshipInputs();
            // Update the active class
            Materialize.updateTextFields();
        }
        // Style the list
        styleRelationshipList();
    }

    function populateRelationshipList() {
        $('#relationship-list').empty();
        for (var i = 0; i < activeGroupArray.length; i++) {
            $('#relationship-list').append('<li class="collection-item" rel-index="' + i + '">' +
                '<span class="truncate relationship-label">' + getDisplayName(activeGroupArray[i].targetName) + '</span>' +
                '<a class="secondaray-content pointer-item black-text remove-relationship-button"><i class="material-icons">close</i></a></li>');
        }
        // Bind list events
        bindListEvents();
        // We just populated the list, so deselect the item
        selectGroupItem(-1);
    }

    function bindListEvents() {
        $('#relationship-list > li').on('click', function (event) {
            // Select the group item
            selectGroupItem(Number.parseInt($(this).attr('rel-index')));
        });
        $('.remove-relationship-button').on('click', function (event) {
            // Remove the item
            activeGroupArray.splice(Number.parseInt($(this).attr('rel-index')), 1);
            // Update the input field
            document.forms[relationshipFormTarget]['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
            // Ignore the next selection (effectively saying select -1)
            ignoreNextSelection = true;
            // Update the list
            populateRelationshipList();
        });
    }

    function getDisplayName(name) {
        return name !== null && name !== '' ? name : 'No name given';
    }
});

function deactivateRelationshipInputs() {
    $('#relationship-name-label').removeClass('active');
    $('#relationship-chips-label').removeClass('active');
}

function disableRelationshipInputs() {
    $('#relationship-name-entry').attr('disabled', 'disabled');
    $('#relationship-chips').material_chip({ data: [] });
    $('#relationship-chips').addClass('disabled');
    $('#relationship-chips').find('input').attr('disabled', 'disabled');
}

function enableRelationshipInputs() {
    $('#relationship-name-entry').removeAttr('disabled');

    // Update chips
    var chipData = [];
    for (var i = 0; i < activeGroupArray[activeIndex].length; i++) {
        chipData.push({ tag: activeGroupArray[activeIndex].roles[i] });
    }
    $('#relationship-chips').material_chip({ data: chipData });
    $('#relationship-chips').removeClass('disabled');
    $('#relationship-chips').find('input').removeAttr('disabled');
}

function styleRelationshipList() {
    // Get the list items
    var collection = $('.relationship-group > li');
    // Loop through the list items
    for (var i = 0; i < activeGroupArray.length; i++) {
        $(collection[i]).removeClass('light-green red accent-1');
        // If this is the active item
        if (i === activeIndex) {
            $(collection[x]).addClas('light-green accent-1');
        } else {
            // If the element is invalid,
            if (activeGroupArray[i].targetName === null || activeGroupArray[i].targetName === '') {
                $(collection[i]).addClass('red accent-1');
            }
        }
    }
}

function Relationship(source) {
    return {
        sourceId: source != null ? source : null,
        targetId: null,
        targetName: null,
        roles: []
    };
}