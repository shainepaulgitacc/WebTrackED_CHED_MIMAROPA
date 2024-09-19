$(function () {
    $("#cancel").on("click", function () {
        $("#filename").text('');
        $("#submit").prop("disabled", true)
    })
    $("#docs-file").on("change", function () {

        const filenameSpan = $("#filename");
        if (this.files.length > 0) {
            if (this.files[0].name.length > 20) {
                filenameSpan.text(this.files[0].name.substring(0, 20) + "..");
            }
            else {
                filenameSpan.text(this.files[0].name);
            }
            $("#submitButton").prop("disabled", false);
        } else {
            filenameSpan.text('');
            $("#submitButton").prop("disabled", true);
        }
    });

    $('#drag-area').on('dragover', function (e) {
        e.preventDefault();
        $(this).addClass('dragover');
    });

    $('#drag-area').on('dragleave drop', function () {
        $(this).removeClass('dragover');
    });
    $('#drag-area').on('drop', function (e) {
        e.preventDefault();
        const file = e.originalEvent.dataTransfer.files[0];
        if (file && file.type === 'application/pdf') {
            $('#docs-file').prop("files", e.originalEvent.dataTransfer.files);
            $('#docs-file').trigger("change");
        } else {
            alert('Please drop a PDF file.');
        }
    });
    $('#submitButton').on('click', function (event) {
        var fileInput = $('#docs-file')[0];
        var file = fileInput.files[0];
        var size = file.size;
        if (file && file.size > 104857600) { 
            alert('The file size must be less than or equal to 50 MB.');
        } else {
            $('#uploadForm').submit();
        }
    });


});
