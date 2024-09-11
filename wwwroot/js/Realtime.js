var connection = new signalR.HubConnectionBuilder().withUrl('/notification').build();
connection.start().then(function () {
    console.log("Successfully connected to the notification hub");
})
    .catch(function (error) {
        return console.error(error.ToString());
    })


var messageConnect = new signalR.HubConnectionBuilder().withUrl('/message').build();
messageConnect.start().then(function () { 
    console.log("message succesfully connected");
})
    .catch(function (error) {
        return console.error(error.ToString());
    })


//we do the coding in javascript
$(function () {
    connection.on('receiveNotification', function (title, description, notifType, date, redirectLink) {

        var bgColor = (notifType === 'Registration') ? 'bg-info' : 'bg-warning';
        var icon = (notifType === 'Registration') ? 'fa-user' : 'fa-file';

        if ($("#notif-list").children().length === 2) {
            // Create the new li element
            var newListItem = $(`<li class="dropdown-item text-center p-3">
							<a href="/Application/NotificationManagement" class="text-decoration-none text-body-tertiary clickable">View notification in detail</a>
						   </li>`);

            // Replace the existing child with the new li element
            $("#notif-list").children().last().replaceWith(newListItem);
        }
        $("#notif-icon").append(`<span class="p-1 rounded-circle bg-danger position-absolute" style="top: -2px; right: -5px"></span>`);

        $('<li class="dropdown-item d-flex gap-3 align-items-center p-3 border-bottom border-opacity-75">' +
            '<div class="' + bgColor + ' rounded-circle d-flex align-items-center justify-content-center p-3 text-white">' +
            '<i class="fa-solid ' + icon + '"></i>' +
            '</div>' +
            '<div>' +
            '<span class="d-block text-body-tertiary">' + date + '</span>' +
            '<h6>' + description + '</h6>' +
            '</div>' +
            '</li>').insertAfter($("#notif-list").children().eq(0));

        var listItems = $("#notif-list > li").not(':first').not(':last');

        // Check if there are more than 5 <li> elements
        if (listItems.length > 5) {
            // Keep the first 5 and remove the rest
            listItems.slice(5).remove();
        }
        // $("#notif-list > li").not(':first').not(':last').slice(5).remove()
        $("body").append(`<h5 class="fw-bolder bg-warning d-inline px-3 py-2 rounded-2 z-4 position-fixed" style="top: 70px;left: 50%;">Reload the page...</h5>`);
    })

    var listItems = $("#notif-list > li").not(':first').not(':last');
    // Check if there are more than 5 <li> elements
    if (listItems.length > 5) {
        // Keep the first 5 and remove the rest
        listItems.slice(5).remove();
    }
    messageConnect.on('countMessage', function (value) {
        $("#message-menu").append(` <span class="fw-bolder text-white bg-danger py-1 px-2 rounded-pill position-absolute" style="top: -1px; right: -2px;">${value}</span>`)
    });

    //message count
   
})

