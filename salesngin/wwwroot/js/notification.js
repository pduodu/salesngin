"use strict";
var hubUrl = "/notificationHub";
var connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveOrder", function (message) {

    $.ajax({
        type: "GET",
        url: '/Order/GetOrdersPV',
        //url: "@Url.Action('GetNewOrdersPV', 'Order')",
        contentType: 'application/json; charset=utf=8',
        data: { status: 'New' },
        success: function (response) {
            $("#div_notification_content").empty();
            $("#div_notification_content").html(response);
            $("#div_notification_popup").modal("show");
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alert(err.Message);
        }
    });
});

connection.on("OrderComplete", function (message) {
    $.ajax({
        type: "GET",
        url: '/Order/GetOrdersPV',
        contentType: 'application/json; charset=utf=8',
        data: { status: 'Ready' },
        success: function (response) {
            $("#div_notification_content").empty();
            $("#div_notification_content").html(response);
            $("#div_notification_popup").modal("show");
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alert(err.Message);
        }
    });
});

connection.on("ReceiveMessage", function (message) {
    $.ajax({
        type: "GET",
        url: '/Order/GetChatPartialView',
        contentType: 'application/json; charset=utf=8',
        data: { id: message },
        success: function (data) {
            var chatBox = document.getElementById("div_chat_container");
            $("#div_chat_container").empty();
            $("#div_chat_container").html(data);
            chatBox.scrollTop = parseInt(chatBox.scrollHeight);
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alert(err.Message);
        }
    });
    
});

connection.on("RefreshStatusBar", function (message) {
    $.ajax({
        type: "GET",
        url: '/Order/GetStatusBarPartialView',
        contentType: 'application/json; charset=utf=8',
        data: { id: message },
        success: function (data) {
            //$("#divOrderDetailsPanel").empty();
            $("#divOrderStatusPanel").html(data);
            //location.reload();
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alert(err.Message);
        }
    });

    //var url = Configuration["ProductionServerUrl"];

    //fetch("/Order/GetStatusBarPartialView")
    //    .then(response => response.text())
    //    .then(html => {
    //        document.getElementById("container").innerHTML = html;
    //    })
    //    .catch(error => console.error("Unable to get partial view.", error));

});

//connection.start(); 
connection.start()
    .then(function () {
        console.log("Connected to Notification Hub");
    })
    .catch(function (err) {
        console.error(err.toString());
    });


//connection.on("refreshTableRecords", function () {
//    $('#kt_datatable_records').DataTable().ajax.reload();
//});



//connection.on("ReceiveOrder", function (message) {
//    // Handle the received order notification
//    displayOrderNotification(message);
//});

//connection.start()
//    .then(function () {
//        // Connection is established
//    })
//    .catch(function (err) {
//        console.error(err.toString());
//    });





//function displayOrderNotification(message) {
//    // Create a new notification element
//    var notificationElement = document.createElement("div");
//    notificationElement.innerHTML = message;

//    // Add the notification to the container
//    document.getElementById("orderNotifications").appendChild(notificationElement);
//}


