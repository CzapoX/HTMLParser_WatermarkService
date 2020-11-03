// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


$("#button-upload0").click(function (event) {
    var formData = new FormData($("#form-upload0").get(0));
    PostImage(event, formData);
});

$("#button-upload1").click(function (event) {
    var formData = new FormData($("#form-upload1").get(0));
    PostImage(event, formData);
});

$("#button-upload2").click(function (event) {
    var formData = new FormData($("#form-upload2").get(0));
    PostImage(event, formData);
});

$("#button-upload3").click(function (event) {
    var formData = new FormData($("#form-upload3").get(0));
    PostImage(event, formData);
});

$("#button-upload4").click(function (event) {
    var formData = new FormData($("#form-upload4").get(0));
    PostImage(event, formData);
});

$("#button-upload5").click(function (event) {
    var formData = new FormData($("#form-upload5").get(0));
    PostImage(event, formData);
});

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
