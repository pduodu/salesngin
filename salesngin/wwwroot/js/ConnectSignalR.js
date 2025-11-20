"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/NotificationHub").build();
connection.start();
connection.on("UpdateDashboardUI", function (message) {
    GetDashboardUI();
});
