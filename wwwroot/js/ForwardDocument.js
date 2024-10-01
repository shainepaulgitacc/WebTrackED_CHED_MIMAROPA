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
});