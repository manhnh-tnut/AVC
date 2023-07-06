"use strict";

$(async () => {
    let data = []
    let machine = null
    let dataStore = new DevExpress.data.ArrayStore({
        key: "id",
        data: data
    });
    let machineStore = new DevExpress.data.ArrayStore({
        key: "name",
        data: data
    });
    dataStore.load = function () {
        var deferred = $.Deferred();
        $.get('summaries').done(function (response) {
            if (machine === null) {
                data = response
                deferred.resolve(data);
            } else {
                data = response.filter(i => i.name === machine)
                deferred.resolve(data);
            }
        });
        return deferred.promise();
    }

    machineStore.load = function () {
        var deferred = $.Deferred();
        $.get('machines').done(function (response) {
            deferred.resolve(response);
        });
        return deferred.promise();
    }
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/live")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.onreconnecting(error => {
        var li = document.createElement("li");
        li.textContent = `Connection lost due to error "${error}". Reconnecting.`;
        document.getElementById("messagesList").prepend(li);
    });
    connection.onreconnected(connectionId => {
        var li = document.createElement("li");
        li.textContent = `Connection reestablished. Connected with connectionId "${connectionId}".`;
        document.getElementById("messagesList").prepend(li);
    });

    connection.onclose(error => {
        var li = document.createElement("li");
        li.textContent = `Connection closed due to error "${error}". Try refreshing this page to restart the connection.`;
        document.getElementById("messagesList").prepend(li);
        // start();
    });

    async function start() {
        await connection.start().then(function () {
            DevExpress.ui.notify('Connected', 'success', 600);
        }).catch(function (err) {
            return console.error(err.toString());
        });
    };

    connection.on("Log", function (log) {
        console.log(log);
    });

    connection.on("Summaries", function (response) {
        for (let index = 0; index < response.length; index++) {
            if (machine !== null && response[index].name !== machine) {
                if (response.find(i => i.id === data[index].id) === null) {
                    data.pop(data[index])
                    dataStore.push([{ type: "remove", key: response[index].id, data: response[index] }]);
                }
                continue;
            }
            if (data.find(i => i.id === response[index].id) === null) {
                data.push(response[index])
                dataStore.push([{ type: "insert", data: response[index] }]);
            } else {
                dataStore.push([{ type: "update", key: response[index].id, data: response[index] }]);
            }

            if (response.find(i => i.id === data[index].id) === null) {
                data.pop(data[index])
                dataStore.push([{ type: "remove", key: response[index].id, data: response[index] }]);
            }
        }
        $("#gridContainer").dxDataGrid("instance").refresh();
    });

    $("#gridContainer").dxDataGrid({
        dataSource: {
            store: dataStore,
            reshapeOnPush: true
        },
        repaintChangesOnly: true,
        highlightChanges: true,
        columnAutoWidth: true,
        showBorders: true,
        searchPanel: {
            visible: true
        },
        loadPanel: {
            enabled: false
        },
        headerFilter: {
            visible: true,
            allowSearch: true
        },
        wordWrapEnabled: true,
        columns: [{
            caption: '#',
            width: 50,
            dataType: "number",
            allowEditing: false,
            cellTemplate: function (container, options) {
                container.text(options.row.rowIndex + 1)
            }
        }, {
            caption: "IP",
            dataField: "machine.ip",
        }, {
            caption: "Tên máy",
            dataField: "machine.name",
        }, {
            caption: "Trạng thái",
            dataField: "machine.status",
            dataType: "boolean",
            cellTemplate: function (container, options) {
                container.text(options.data.machine.status === true ? "ON" : "OFF")
                    .css("color", "white")
                    .css("background-color", options.data.machine.status === true ? "green" : "gray")
            }
        }, {
            caption: "Thời gian chạy tích lũy",
            dataField: "display",
            dataType: "time",
        }, {
            caption: "Số lần cắt sản phẩm",
            dataField: "count",
            dataType: "number",
        }],
        masterDetail: {
            enabled: true,
            template: function (container, options) {
                $("<div>")
                    .addClass("master-detail-caption")
                    .text("Logs")
                    .appendTo(container);

                $("<div>").dxDataGrid({
                    dataSource: new DevExpress.data.CustomStore({
                        key: "id",
                        load: function () {
                            var deferred = $.Deferred();
                            $.post('logs', {
                                '__RequestVerificationToken': token,
                                name: options.data.machine.name
                            }).done(function (response) {
                                deferred.resolve(response);
                            });
                            return deferred.promise();
                        }
                    }),
                    loadPanel: {
                        enabled: false
                    },
                    columns: [{
                        caption: "Trạng thái",
                        dataField: "gpio.value",
                        dataType: "boolean",
                        cellTemplate: function (container, options) {
                            container.text(options.data.gpio.value === 0 ? "ON" : "OFF")
                                .css("color", "white")
                                .css("background-color", options.data.gpio.value === 0 ? "green" : "gray")
                        }
                    }, {
                        caption: "Time update",
                        dataField: "display",
                    }],
                }).appendTo(container);
            }
        }
    }).dxDataGrid("instance");

    await start();
});