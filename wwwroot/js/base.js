$(function () {
    $('#submitButton').on('click', function (e) {
        // Disable the button and show the spinner and loading text
       // $("#form-pass").submit();
        $(window).on('beforeunload', function () {
            $(this).prop('disabled', true);
            $('.loading-spinner').removeClass('d-none');
            $('.loading-text').removeClass('d-none');
            $('.button-text').addClass('d-none d-lg-none d-md-none d-sm-none');
            $('#submitButton i').addClass('d-none');
        })
    });

    $("#alert-close").on("click", function () {
        $("#alert").removeClass("active-alert");
    })


});