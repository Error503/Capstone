function searchExistingNames(input) {

}

$(document).ready(function () {
    // Add change listeners to the content type and name fields
    $('#Franchise').attr('disabled', 'disabled');
    $('#ContentType').first().change(function () {
        console.log(this.value);
        var MEDIA_VALUES = 0x78; // 0111 1000
        // Update the date label
        $('#date-label').text(this.value == 1 ? "Date of Birth" : this.value == 2 ? "Date Founded" : "Release Date");
        // Update the display of the media information panel
        if ((this.value & MEDIA_VALUES) == this.value) {
            // Value is a media value - show the media information section
            $('#Franchise').removeAttr('disabled');
        } else {
            // Not a media value - hide the media information
            //$('#media-information-section').hide();
            $('#Franchise').attr('disabled', 'disabled');
        }
    });
});
