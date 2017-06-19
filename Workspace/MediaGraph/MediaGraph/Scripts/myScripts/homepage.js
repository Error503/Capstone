function searchButtonClicked() {
    $.ajax({
        url: 'search',
        type: 'GET',
        success: function (datax) {
            setToResponseSection(datax);
        }
    });
}

function createButtonClicked() {
    setToResponseSection("Create button clicked");
}

function setToResponseSection(data) {
    console.log(data);
    // Get the item
    var responseHolder = $('#response-section');
    // Remove existing data and append the new data
    responseHolder.empty().append(data);
}