$(document).ready(function () {
    var model = new BasicNode();

    var activeGroup = 'Companies';
    var activeGroupArray = model.relatedCompanies;
    var activeIndex = -1;
    var ignoreNextSelection = false;

    // Set up materialize things
    $('select').material_select();
    $('#relationship-chips').material_chip();
    $('#relationship-chips').find('input').attr('disabled', 'disabled');
    // Wire up events to handle changes to the relationship chips   
    $('#relationship-chips').on('chip.add', function (e, chip) {
        activeGroupArray[activeIndex].roles.push(chip.tag);
    });

    $('#relationship-chips').on('chip.delete', function (e, chip) {
        activeGroupArray[activeIndex].roles.splice(activeGroupArray[activeIndex].roles.indexOf(chip.tag), 1);
    });

    // Set up events
    $('#node-content-type').on('change', function (event) {
        var value = $(this).val();
        if (value === 'company' || value === 'media' || value === 'person') {
            model.contentType = value;
            getNodeInformation(value);
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
        activeGroupArray.push(new Relationship(model.id));
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
        // Update the styling

    });

    $('#reset-button').on('click', function (event) {
        if (window.confirm('Are you sure you want to reset the form? All entered data will be lost.')) {
            $('#node-content-type').siblings('input').removeAttr('disabled');
            $('#hidden-divider').hide();
            $('#type-specific-info').empty().hide();
            selectGroupItem(-1);
            $('#add-relationship-button').addClass('disabled');
            $('#submission-section').find('button').addClass('disabled');
            model = new BasicNode(model);
        }
    });

    $('#submit-button').on('click', function (event) {
        console.log(model);
    });

    bindListEvents();

    function selectTab(tab) {
        activeGroup = tab;
        $('#active-group-name').html(tab);

        if (tab === 'Companies') {
            activeGroupArray = model.relatedCompanies;
        } else if (tab === 'Media') {
            activeGroupArray = model.relatedMedia;
        } else if (tab === 'People') {
            activeGroupArray = model.relatedPeople;
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
                $('#type-specific-info').empty().append(response);
                $('#type-specific-info').show();
                $('#hidden-divider').show();

                materializeSetup();
                // Create the model
                if (type === 'company') {
                    model = new CompanyNode(model);
                } else if (type === 'media') {
                    model = new MediaNode(model);
                } else if (type === 'person') {
                    model = new PersonNode(model);
                }
                // Bind the model
                bindModel(type);
            },
            error: function (response) {
                console.error(response);
            },
        });
    }

    function bindModel(type) {
        var bindings = $('#type-specific-info').find('input[model]');
        for (var i = 0; i < bindings.length; i++) {
            $(bindings[i]).on('change', function (event) {
                model[$(this).attr('model')] = $(this).val();
            });
        }

        if (type === 'media') {
            $('#media-type').on('change', function (event) {
                model.mediaType = $(this).val();
            });
            // Set up the genre chips
            $('#genre-chips').on('chip.add', function (e, chip) {
                model.genres.push(chip.tag);
            });
            $('#genre-chpis').on('chip.delete', function (e, chip) {
                model.genres.splice(model.genres.indexOf(chip.tag), 1);
            });
        } else if (type === 'person') {
            // Set up the status box
            $('#person-status').on('change', function (event) {
                model.status = $(this).val();
            });
        }

        // Set up chips
        $('#other-names').on('chip.add', function (e, chip) {
            model.otherNames.push(chip.tag);
            console.log(model.otherNames);
        });
        $('#other-names').on('chip.delete', function (e, chip) {
            model.otherNames.splice(model.otherNames.indexOf(chip.tag), 1);
            console.log(model.otherNames);
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

    // ===== Constructors =====
    function BasicNode(definition) {
        return {
            id: definition ? definition.id : null,
            contentType: definition ? definition.contentType : null,
            commonName: definition ? definition.commonName : null,
            otherNames: definition ? definition.otherNames : [],
            releaseDate: definition ? definition.releaseDate : null,
            relatedLinks: definition ? definition.relatedLinks : [],
            relatedCompanies: definition ? definition.relatedCompanies : [],
            relatedMedia: definition ? definition.relatedMedia : [],
            relatedPeople: definition ? definition.relatedPeople : []
        };
    }

    function CompanyNode(basic) {
        var obj = new BasicNode(basic);
        obj.deathDate = null;

        return obj;
    }

    function MediaNode(basic) {
        var obj = new BasicNode(basic);
        obj.mediaType = null;
        obj.franchiseName = null;
        obj.genres = [];
        //media.platforms = null;
        //media.regionalReleaseDates = null;

        return obj;
    }

    function PersonNode(basic) {
        var obj = new BasicNode(basic);
        obj.givenName = null;
        obj.familyName = null;
        obj.deathDate = null;
        obj.status = null;
        //obj.nationality = null;

        return obj;
    }

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
    $('#type-specific-info').find('select').material_select();
    $('#type-specific-info').find('.chips').material_chip();
    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: 30,
        closeOnSelect: true
    });
}
