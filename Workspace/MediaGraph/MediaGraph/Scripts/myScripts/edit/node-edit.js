$(document).ready(function () {
    var inputs = $('input');
    for (var i = 0; i < inputs.length; i++) {
        if ($(inputs[i]).val() !== null && $(inputs[i]).val() !== '') {
            $(inputs[i]).siblings('label').addClass('active');
        }
    }

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
});

function materializeSetup() {
    $('#type-specific-info').find('select').material_select();
    $('#type-specific-info').find('chips').material_chip();
}