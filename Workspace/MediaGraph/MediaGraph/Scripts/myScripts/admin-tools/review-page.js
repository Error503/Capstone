$(document).ready(function () {
    var id;
    var otherNames;
    var genres;
    var relatedCompanies;
    var relatedMedia;
    var relatedPeople;

    // Set up materialize elements
    $('select').material_select();
    $('.datepicker').pickadate({
        format: 'yyyy-mm-dd',
        selectMonths: true,
        selectYears: 30,
        closeOnSelect: true
    });
    Materialize.updateTextFields();
    // Grab the data
    grabData();

    // Set up the events
    $('#edit-info-button').on('click', function (event) {
        console.log('edit info clicked');
    });

    function grabData() {
        var form = document.forms['nodeForm'];

        id = form['Id'].value;
        otherNames = JSON.parse(form['OtherNames'].value);
        var nameChips = getChipData(otherNames);
        $('#other-names').material_chip({ data: nameChips });

        if (form['ContentType'] === 'Media') {
            genres = JSON.parse(form['Genres'].value);
            var genreChips = getChipData(genres);
            $('#genre-chips').material_chip({ data: chipData });
        }
    }

    function getChipData(array) {
        var chips = [];
        for (var i = 0; i < array.length; i++) {
            chips.push({ tag: array[i] });
        }
        return chips;
    }
});