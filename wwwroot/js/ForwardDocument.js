$(function () {

    // Function to check if both radio groups have a selected value
    function checkRadioSelection() {
        var reviewerSelected = $('.reviewer:checked').length > 0;
        var documentStatusSelected = $('.document-tracking-status:checked').length > 0;

        // Check if the radio button with value 0 is selected
        var trackingStatusIsZero = $('.document-tracking-status:checked').val() == "0";

        // Disable or enable the submit button based on selections, also ensuring that value 0 doesn't trigger submit unless a checkbox is selected
        $('#submitButton').prop('disabled', !(reviewerSelected && documentStatusSelected && !trackingStatusIsZero));
    }

    // Call the function on page load to ensure the button is correctly set
    checkRadioSelection();

    // Attach event listeners to radio buttons and checkboxes
    $('.reviewer').change(checkRadioSelection);
    $('.document-tracking-status').change(checkRadioSelection);


    // Function to update the hidden input field with selected checkboxes
    function updateSelectedReviewers() {
        var selectedReviewers = [];

        // Iterate over each checked checkbox and get its value
        $('.reviewer:checked').each(function () {
            selectedReviewers.push($(this).val());
        });

        // Join the selected values with commas and set to hidden input field
        $('#new-reviewer').val(selectedReviewers.join(','));
    }

    // Add change event listener to checkboxes
    $('.reviewer').on('change', function () {
        updateSelectedReviewers();
    });

    // Initial update in case some checkboxes are pre-checked
    updateSelectedReviewers();


    // Disable/Enable reviewers based on the selected radio button value
    $('.document-tracking-status').change(function () {
        var trackingStatusValue = $(this).val();

        if (trackingStatusValue == "3") {
            // Disable all reviewers except those with class "Records Office"
            $('.reviewer').prop('disabled', true); // Disable all
            $('.reviewer.Records\\ Office').prop('disabled', false); // Enable only "Records Office"
        } else if (trackingStatusValue == "0") {
            // If the radio button with value 0 is selected, do nothing special, just check if reviewers are selected
            checkRadioSelection();
        } else {
            // Re-enable all reviewers when the radio button with value other than 3 is selected
            $('.reviewer').prop('disabled', false);
        }

        // Re-check button enablement logic
        checkRadioSelection();
    });

    // Connection to handle real-time reviewer update
    connection.on('reviewerRealtime', function () {
        location.reload();
    });
});
