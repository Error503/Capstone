$(function () {
    // Add change listeners to the content type and name fields
    $('#SimpleType').change(function () {
        const MEDIA_VALUES = 0x3C; // 0011 1100
        // Update the date label
        $('#date-label').val(this.value === 1 ? "Date of Birth" : this.value === 2 ? "Date Founded" : "Release Date");
        // Update the display of the media information panel
        if ((this.value & MEDIA_VALUES) === this.value) {
            // Value is a media value - show the media information section
            $('#media-information-section').show();
        } else {
            // Not a media value - hide the media information
            $('#media-information-section').hide();
        }
    });
});
