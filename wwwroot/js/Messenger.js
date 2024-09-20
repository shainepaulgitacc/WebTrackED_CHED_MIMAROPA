$(function () {


    var chatContainer = $('#chat-container');
    chatContainer.scrollTop(chatContainer.prop("scrollHeight"));


    // Event listener for input in the message content field
    $("#message-content-input").on("input", function () {
        $("#send-button").prop("disabled", true);
        let messageContent = $(this).val();
        if (messageContent) {
            // Enable the send button
            $("#send-button").prop("disabled", false).off("click").on("click", function () {
                let recipient = $("#recipient-input").val();
                let sender = $("#sender-input").val();
                let message = $("#message-content-input").val();

                // Invoke the sendMessage function on the messageConnect object
                messageConnect.invoke("sendMessage", sender, recipient, message)
                    .catch(err => console.error("SendMessage error:", err));

                // Hide the "no record" message
                $("#no-record-message").addClass("d-none").removeClass("d-flex");

                var totalMessage = $("#chat-container > div").length + 1;

                

                // Clear the value of the message input and disable the send button
               
                $("#send-button").prop("disabled", true);
            });
        }
    });

    messageConnect.on('sender', function (newMessageId) {
        let messageContent = $("#message-content-input").val();
        var yMessage = `<div class="d-flex align-items-center gap-2 justify-content-end mb-3">
                                     <input class="message-id" type="hidden" value="${newMessageId}" />
                                    <i class="delete-message fa-solid fa-trash-can text-body-tertiary clickable"></i>
                                    <p class="bg-primary text-white rounded-3 m-0 p-2">${messageContent}</p>
                                </div>`;
        $("#chat-container").append(yMessage);
        $("#message-content-input").val(null);
        $.ajax({
            type: "GET",
            url: "/Application/Messenger?handler=RecipientsListSender",
            dataType: "json",
            success: function (result) {
                $("#user-list").empty();
                result.forEach(function (e) {
                    var currentRecipientPage = e.recipientId == $("#recipient-id-page").val() ? "border-primary border-4" : "border-secondary";
                    var hasUnviewedBadge = e.hasUnviewed ? '<span class="p-1 rounded-circle bg-danger"></span>' : '';

                    // Convert messageLatestDate to a Date object
                    var messageDate = new Date(e.messageLatestDate);
                    var formattedDate = messageDate.toLocaleDateString('en-GB', {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric'
                    });
                    var formattedTime = messageDate.toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit'
                    }).toLowerCase();

                    let recipientProfile = e.recipientProfile != null ? `/ProfilePicture/${e.recipientProfile}`:'/image/NoProfile.jfif';

                    var listRecipients =
                `<li class="${currentRecipientPage} user-item border border-opacity-50 d-flex justify-content-between align-items-center p-3 rounded-4 shadow-sm bg-white mb-3">
                    <a href="/Application/Messenger/Index?rId=${e.recipientId}" class="d-lg-flex d-none gap-2 clickable text-decoration-none">
                        <img src="${recipientProfile}" width="50" height="50" class="rounded-circle" />
                        <div>
                            <h6 class="p-0 mb-0 text-dark">${e.recipientFullName}</h6>
                            <span class="text-secondary">${formattedDate} ${formattedTime}</span>
                        </div>
                    </a>
                     <a href="/Application/Messenger/ChatPage?rId=${e.recipientId}" class="d-lg-none d-flex gap-2 clickable text-decoration-none">
                            <img src="${recipientProfile}" width="50" height="50" class="rounded-circle" />
                            <div>
                                <h6 class="p-0 mb-0 text-dark">${e.recipientFullName}</h6>
                                <span class="text-secondary">${formattedDate} ${formattedTime}</span>
                            </div>
                        </a>
                    ${hasUnviewedBadge}
                </li>`;

                    // Append the generated list item to the container
                    $("#user-list").append(listRecipients);
                });
            },
            error: function (xhr, status, error) {
                console.error("Error fetching recipients:", error);
            }
        });



    })


    // Event listener for receiving messages
    messageConnect.on('receiveMessage', function (senderId, recipientId, messageContent, profileName,newMessageId) {
        // Append the sent message to the chat container

        let now = new Date();
        let formattedDate = now.toDateString() === new Date().toDateString()
            ? now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
            : now.toLocaleDateString('en-GB') + ' ' + now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }).toLowerCase();

        let profilePicture = profileName != null ? `/ProfilePicture/${profileName}` : "/image/NoProfile.jfif";

        var rMessage = ` <div class="r-message d-flex">
                            <div class="d-flex flex-column align-items-center" style="width:20%;">
                                <img src="${profilePicture}" width="40" height="40" class="rounded-circle" />
                                <p style="font-size: 12px">${formattedDate}</p>
                            </div>
                            <div style="width: 80%">
                                <input class="r-message-id" value="${newMessageId}" type="hidden"/>
                                <p class="message-content bg-secondary-subtle rounded-3 p-2 d-inline-block">${messageContent}</p>
                            </div>
                        </div>`;

        let v = $("#recipient-id-page").val();
        let v2 = senderId;
        if (v === v2) {
            $("#chat-container").append(rMessage);
            // Hide the "no record" message
            $("#no-record-message").addClass("d-none").removeClass("d-flex");
        }
    });

   
    messageConnect.on("recipients", function (recipientId) {
        $.ajax({
            type: "GET",
            url: "/Application/Messenger?handler=RecipientsListRecipient&recipientId=" + recipientId,
            dataType: "json",
            success: function (result) {
                $("#user-list").empty();
                result.forEach(function (e) {
                    var currentRecipientPage = e.recipientId == $("#recipient-id-page").val() ? "border-primary border-4" : "border-secondary";
                    var hasUnviewedBadge = e.hasUnviewed ? '<span class="p-1 rounded-circle bg-danger"></span>' : '';

                    // Convert messageLatestDate to a Date object
                    var messageDate = new Date(e.messageLatestDate);
                    var formattedDate = messageDate.toLocaleDateString('en-GB', {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric'
                    });
                    var formattedTime = messageDate.toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit'
                    }).toLowerCase();

                    let recipientProfile = e.recipientProfile != null ? `/ProfilePicture/${e.recipientProfile}` : '/image/NoProfile.jfif';

                    var listRecipients =
                        `<li class="${currentRecipientPage} user-item border border-opacity-50 d-flex justify-content-between align-items-center p-3 rounded-4 shadow-sm bg-white mb-3">
                    <a href="/Application/Messenger/Index?rId=${e.recipientId}" class="d-lg-flex d-none gap-2 clickable text-decoration-none">
                        <img src="${recipientProfile}" width="50" height="50" class="rounded-circle" />
                        <div>
                            <h6 class="p-0 mb-0 text-dark">${e.recipientFullName}</h6>
                            <span class="text-secondary">${formattedDate} ${formattedTime}</span>
                        </div>
                    </a>
                     <a href="/Application/Messenger/ChatPage?rId=${e.recipientId}" class="d-lg-none d-flex gap-2 clickable text-decoration-none">
                            <img src="${recipientProfile}" width="50" height="50" class="rounded-circle" />
                            <div>
                                <h6 class="p-0 mb-0 text-dark">${e.recipientFullName}</h6>
                                <span class="text-secondary">${formattedDate} ${formattedTime}</span>
                            </div>
                        </a>
                    ${hasUnviewedBadge}
                </li>`;

                    // Append the generated list item to the container
                    $("#user-list").append(listRecipients);
                });
            },
            error: function (xhr, status, error) {
                console.error("Error fetching recipients:", error);
            }
        });
    });

    $('#search-input').on('input', function () {
        let searchQuery = $(this).val().toLowerCase();

        $('#user-list .user-item').each(function () {
            let userName = $(this).find('h6').text().toLowerCase();

            if (userName.includes(searchQuery)) {
                $(this).removeClass("d-none");
            } else {
                $(this).addClass("d-none");
            }
        });
    });

    $("#chat-container").on("click", ".delete-message", function () {
        let messageId = $(this).siblings(".message-id").val();
        let index = $(this).parent().index();
        messageConnect.invoke("deleteMessage",messageId, index);
        $(this).parent().replaceWith(` <div class="d-flex align-items-center gap-2 justify-content-end  mb-3">
                                    <i class="bg-light border border-1 border-opacity-25 text-secondary rounded-3 m-0 p-2 fw-light">Deleted Message</i>
                                </div>`);
        if ($("#chat-container").children().length === 0) {
            $("#chat-container").append(`<div id="no-record-message" class="h-100 d-flex align-items-center justify-content-center">
                        <h2 class="text-body-secondary">No chat record</h2>
                    </div>`);
        }
    });

   
    messageConnect.on("removeMessage", function (senderId, messageId) {
        let v = $("#recipient-id-page").val();
        let v2 = senderId;
        if (v === v2) {
            $("#chat-container > .r-message").each(function () {
                if (parseInt($(this).find(".r-message-id").val()) == messageId) {
                    $(this).find(".message-content").replaceWith(`<p class="bg-light border border-1 border-opacity-25 text-secondary rounded-3 p-2 d-inline-block fw-light">Deleted Message</p>`);
                }
                
            })
          
            if ($("#chat-container").children().length === 0) {
                $("#chat-container").append(`<div id="no-record-message" class="h-100 d-flex align-items-center justify-content-center">
                        <h2 class="text-body-secondary">No chat record</h2>
                    </div>`);
            }
        }
    });
});
