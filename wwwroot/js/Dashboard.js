function updateDateTime() {
    var now = new Date();
    var optionsDate = { day: '2-digit', month: 'short', year: 'numeric' };
    var formattedDate = now.toLocaleDateString('en-US', optionsDate).replace(',', '');

    // Custom formatting for the year
    var year = now.getFullYear().toString();
    formattedDate = formattedDate.replace(year, year.slice(-3)); // Replaces the year to 'yyy' format

    var optionsTime = { hour: '2-digit', minute: '2-digit', hour12: false };
    var formattedTime = now.toLocaleTimeString('en-US', optionsTime).replace(':', ':');

    var fullFormattedDateTime = formattedDate + ' ' + formattedTime;
    $('#realtime-date').text('Date Today ' + fullFormattedDateTime);
}
$(function () {

    updateDateTime(); // Initial call
    setInterval(updateDateTime, 1000); // Update every second
    $("#menu-toggle").on("click", function () {
        $("#sidebar").toggleClass("sidebar-hide");
        $("#sidebar").toggleClass("sidebar-show");
    })
    let menus = $(".menus");
    menus.each(function () {
        // Use .on() to attach the click event
        $(this).on("click", function () {
            if (!$(this).hasClass("active")) {
                menus.removeClass("active");
            }
            $(this).toggleClass("active");
        });

    });
    //data tables
    
    /*
    $('#table-data').DataTable({
        "order": [[5, "desc"]] // Order by the 6th column (index 5) in descending order
    });
    */
})

