$(function () {
    connection.on('reviewerRealtime', function (title, description, notifType, date, redirectLink) {
        alert('Please reload the page');
    });

    // Function to check if both radio groups have a selected value
    function checkRadioSelection() {
        var reviewerSelected = $('.reviewer:checked').length > 0;
        var documentStatusSelected = $('.document-tracking-status:checked').length > 0;

        // Enable or disable the submit button based on selections
        $('#submitButton').prop('disabled', !(reviewerSelected && documentStatusSelected));
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
});