$(function () {
    // Get the PDF file URL from the server
    //var pdfUrl = '@Url.Content("~/Documents/" + Model.DocumentAttachment.DocumentAttachment.FileName)';

    // Initialize PDF.js
    /*pdfjsLib.getDocument(pdfUrl).promise.then(function (pdf) {
        var totalPages = pdf.numPages;
        var container = document.getElementById('pdf-pages-container');

        // Loop through each page and render it as an image
        for (var pageNumber = 1; pageNumber <= totalPages; pageNumber++) {
            pdf.getPage(pageNumber).then(function (page) {
                var scale = 1.5;
                var viewport = page.getViewport({ scale: scale });

                // Prepare canvas for rendering
                var canvas = document.createElement('canvas');
                var context = canvas.getContext('2d');
                canvas.height = viewport.height;
                canvas.width = viewport.width;

                // Render PDF page into canvas
                var renderContext = {
                    canvasContext: context,
                    viewport: viewport
                };
                page.render(renderContext).promise.then(function () {
                    // Convert canvas to base64 image
                    var image = canvas.toDataURL('image/jpeg');

                    // Create image element and append it to the container
                    var imgElement = document.createElement('img');
                    imgElement.src = image;
                    container.appendChild(imgElement);
                });
            });
        }
    });
    */
    $("#new-docs").on("change", function () {
        $("#form-new-docs").submit();
    });

    $("#prioritization").on("change", function () {
        $("#form-prioritization").submit();
    })


    $('#new-docs').on('change', function (e) {
        // Disable the button and show the spinner and loading text
        // $("#form-pass").submit();
        $(window).on('beforeunload', function () {
            $(this).prop('disabled', true);
            $('.loading-spinner').removeClass('d-none');
            $('.loading-text').removeClass('d-none');
            $('#change-document i').addClass('d-none');
        })
    });

    /*
    $('#form-new-docs').on('submit', function (event) {
        var fileInput = $('#new-docs')[0];
        var file = fileInput.files[0];
        if (file && file.size > 52428800) { // 50 MB in bytes
            alert('The file size must be less than 50 MB.');
            event.preventDefault(); // Prevent the form from submitting
        }
    });
    */
    
});