$(function () {
    connection.on('reviewerRealtime', function (title, description, notifType, date, redirectLink){
        alert('Please reload the page');
    });
    function toggleButton() {
        if ($('.reviewer:checked').length > 0) {
            $('#btn-procceed').prop('disabled', false);
        } else {
            $('#btn-procceed').prop('disabled', true);
        }
    }

    // Initial check on page load
    toggleButton();

    // Check when any checkbox is clicked
    $('.reviewer').on('change', function () {
        toggleButton();
    });



    $("#btn-procceed").on("click", function () {
        let selectedId = [];
        $(".procedure-input").each(function () {
            if ($(this).is(":checked")) {
                selectedId.push($(this).val());
            }
        })
        let joinedIds = selectedId.join(",");
        $("#selected-ids").val(joinedIds);



        let reviewerId = [];
        $(".reviewer").each(function () {
            if ($(this).is(":checked")) {
                reviewerId.push($(this).val());
            }
        })
        let reviewerJoinedIds = reviewerId.join(",");
        $("#new-reviewer").val(reviewerJoinedIds);

        $("#form-procceed").submit();
    })

    $(".procedure-input").on("change", function () {
        if ($('.procedure-input:checked').length === $('.procedure-input').length) {
            $('#btn-done').removeClass("d-none");
            $('#btn-procceed').addClass("d-none");
            $('input[name=reviewer]').prop('disabled', true);
        }
        else {
            $('#btn-done').addClass("d-none");
            $('#btn-procceed').removeClass("d-none");
            $('input[name=reviewer]').prop('disabled', false);
        }
    })
})