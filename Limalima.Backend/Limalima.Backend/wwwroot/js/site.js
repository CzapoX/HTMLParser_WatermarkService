// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


for (let i = 0; i < 10; i++) {
    $("#button-upload" + i).click(function (event) {
        let formData = new FormData($("#form-upload" + i).get(0));
        PostImage(event, formData);
    });
}

function PostImage(event, formData) {
    event.preventDefault();
    $.ajax({
        url: "Watermark/UploadAjax",
        enctype: 'multipart/form-data',
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        cache: false,
    }).done(function (response) {
        $("#button-add-photos").attr("disabled", false);
    }).fail(function () {
        alert('File not uploaded');
    })
}
