$(document).ready(function () {
    $(".button-collapse").sideNav();
    $('.chips').material_chip();
    $('.chips').on('chip.add', chipAdded);
    $('.chips').on('chip.delete', chipRemoved);
    $('.chips-placeholder').material_chip({
        secondaryPlaceholder: 'Add a name',
    });

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: 15,
        today: 'Today',
        clear: 'Clear',
        close: 'Ok',
        closeOnSelect: false
    });
    $('select').material_select();
});

function chipAdded(e, chip) {
    console.log("chip added");
}

function chipRemoved(e, chip) {
    console.log("Chip removed");
}