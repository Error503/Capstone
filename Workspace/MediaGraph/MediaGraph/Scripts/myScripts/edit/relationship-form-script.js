var relationshipFormTarget = 'nodeForm';
var allowEditing = true;
var relatedCompanies = [];
var relatedMedia = [];
var relatedPeople = [];
var activeGroupArray = relatedCompanies;
var activeGroup = 'Companies';
var activeIndex = -1;

function grabRelationshipInformation() {
    if (document.forms[relationshipFormTarget] != null) {
        // Get the information
        relatedCompanies = JSON.parse(document.forms[relationshipFormTarget]['RelatedCompanies'].value);
        relatedMedia = JSON.parse(document.forms[relationshipFormTarget]['RelatedMedia'].value);
        relatedPeople = JSON.parse(document.forms[relationshipFormTarget]['RelatedPeople'].value);
        activeGRoupArray = relatedCompanies;
        // Populate the list
        populateRelationshipList();
    }
}

$(document).ready(function () {

    grabRelationshipInformation();

    $('#relationship-chips').material_chip();
    $('#relatoinship-chips').find('input').attr('disabled', 'disabled');
    $('#relationship-chips').on('chip.add', function (e, chip) {
        chip.tag = chip.tag.trim().toLowerCase();
        if (activeGroupArray[activeIndex].Roles.indexOf(chip.tag) === -1) {
            activeGroupArray[activeIndex].Roles.push(chip.tag);
        }
        document.forms[relationshipFormTarget]['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
    })
    .on('chip.delete', function (e, chip) {
        activeGroupArray[activeIndex].Roles.splice(activeGroupArray[activeIndex].Roles.indexOf(chip.tag), 1);
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
        activeGroupArray[activeIndex].TargetName = $(this).val();
        // Update the field value
        $($('#relationship-list > li')[activeIndex]).children('span').html(getDisplayName(activeGroupArray[activeIndex].TargetName));
    });

    $('.relationship-tab').on('click', function (event) {
        selectTab($(this).attr('tab-value'));
    });
});

function selectTab(tab) {
    activeGroup = tab
    $('#active-group-name').html(activeGroup);

    if (activeGroup === 'Companies') {
        activeGroupArray = relatedCompanies;
    } else if (activeGroup === 'Media') {
        activeGroupArray = relatedMedia;
    } else if (activeGroup === 'People') {
        activeGroupArray = relatedPeople;
    }

    // Populate the list
    populateRelationshipList();
}


function selectGroupItem(index) {
    activeIndex = index;
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
            '<span class="truncate relationship-label">' + getDisplayName(activeGroupArray[i].TargetName) + '</span>' +
            '<a class="secondaray-content pointer-item black-text right remove-relationship-button"><i class="material-icons">close</i></a></li>');
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
        if (allowEditing) {
            // Remove the item
            activeGroupArray.splice(Number.parseInt($(this).attr('rel-index')), 1);
            // Update the input field
            document.forms[relationshipFormTarget]['Related' + activeGroup].value = JSON.stringify(activeGroupArray);
            // Ignore the next selection (effectively saying select -1)
            event.stopPropagation();
            // Update the list
            populateRelationshipList();
        }
    });
}

function getDisplayName(name) {
    return name !== null && name !== '' ? name : 'No name given';
}

function deactivateRelationshipInputs() {
    $('#relationship-name-label').removeClass('active');
    $('#relationship-chips-label').removeClass('active');
}

function disableRelationshipInputs() {
    // Update relationship information
    setRelationshipData();
    $('#relationship-chips').addClass('disabled');
    $('#relationship-chips').find('input').attr('disabled', 'disabled');
    $('#relationship-chips > .chip > i').hide();
    if (!allowEditing) {
        $('.remove-relationship-button').hide();
    }
}

function enableRelationshipInputs() {
    // Update relationship information
    setRelationshipData();
    // Only remove disabled styling if editing is allowed
    if (allowEditing) {
        $('.remove-relationship-button').show();

        if (activeIndex > -1) {
            $('#relationship-name-entry').removeAttr('disabled')
            $('#relationship-chips').removeClass('disabled');
            $('#relationship-chips').find('input').removeAttr('disabled');
            $('#relationship-chips > .chip > i').show();
        }
    } else {
        // Disabling of the chips input is still needed
        $('#relationship-chips').find('input').attr('disabled', 'disabled');
        $('#relationship-chips > .chip > i').hide();
    }
}

function setRelationshipData() {
    $('#relationship-name-entry').val(activeIndex > -1 ? activeGroupArray[activeIndex].TargetName : "");
    var relChipData = [];
    if (activeIndex > -1) {
        for (var i = 0; i < activeGroupArray[activeIndex].Roles.length; i++) {
            relChipData.push({ tag: activeGroupArray[activeIndex].Roles[i] });
        }
    }
    $('#relationship-chips').material_chip({ data: relChipData });
    if (relChipData.length > 0) {
        $('#relationship-chips').siblings('label').addClass('active');
    }
}

function styleRelationshipList() {
    // Get the list items
    var collection = $('#relationship-list > li');
    // Loop through the list items
    for (var i = 0; i < activeGroupArray.length; i++) {
        $(collection[i]).removeClass('light-green red accent-1');
        // If this is the active item
        if (i === activeIndex) {
            $(collection[i]).addClass('light-green accent-1');
        } else {
            // If the element is invalid,
            if (activeGroupArray[i].TargetName === null || activeGroupArray[i].TargetName === '') {
                $(collection[i]).addClass('red accent-1');
            }
        }
    }
}

function Relationship(source) {
    return {
        SourceId: source != null ? source : null,
        TargetId: null,
        TargetName: null,
        Roles: []
    };
}