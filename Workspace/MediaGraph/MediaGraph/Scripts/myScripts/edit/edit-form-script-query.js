function contentTypeChanged() {
    console.log("EVENT");
    $.ajax({
        method: "GET",
        url: '/edit/getinformation?type' + this.value
    })
}