$(function () {

    // Function to check if both radio groups have a selected value
    function checkRadioSelection() {
        var reviewerSelected = $('.reviewer:checked').length > 0;
        var documentStatusSelected = $('.document-tracking-status:checked').length > 0;

        console.log(documentStatusSelected);
        console.log(reviewerSelected);

        // Disable or enable the submit button based on selections, also ensuring that value 0 doesn't trigger submit unless a checkbox is selected
        $('#submitButton').prop('disabled', !(reviewerSelected && documentStatusSelected));
    }
    function updateSelectedReviewers() {
        var selectedReviewers = [];

        // Iterate over each checked checkbox and get its value
        $('.reviewer:checked').each(function () {
            selectedReviewers.push($(this).val());
        });

        // Join the selected values with commas and set to hidden input field
        $('#new-reviewer').val(selectedReviewers.join(','));
    }
    $('.reviewer').on('change', function () {
        checkRadioSelection();
        updateSelectedReviewers();
    });
    // Disable/Enable reviewers based on the selected radio button value
    $('.document-tracking-status').change(function () {

        $('.reviewer').prop('checked', false);
        var trackingStatusValue = $(this).val();
       
        if (trackingStatusValue == "PreparingRelease") {
            let firstDesignationName = $("#first-designation");
            // Disable all reviewers except those with class "Records Office"
            $('.reviewer').prop('disabled', true); // Disable all
            $(firstDesignationName).prop('disabled', false); // Enable only "Records Office"
            $(firstDesignationName).prop('checked', true);
        } else if (trackingStatusValue == "ToReceived") {
            $('.reviewer').prop('disabled', false); // Disable all          
         
        }
        checkRadioSelection();
        updateSelectedReviewers();
    });

    // Connection to handle real-time reviewer update
    connection.on('reviewerRealtime', function () {
        location.reload();
    });
});
