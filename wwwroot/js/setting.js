"use strict";
let types = []
$.get('types').done(function (response) {
    types = response;
});
$(function () {
    $(function () {
        $("#machineContainer").dxDataGrid({
            dataSource: new DevExpress.data.CustomStore({
                key: "id",
                load: function () {
                    var deferred = $.Deferred();
                    $.get('machines').done(function (response) {
                        deferred.resolve(response);
                    });
                    return deferred.promise();
                },
                insert: function (values) {
                    $.ajax({
                        type: "POST",
                        url: "create-machine",
                        data: {
                            '__RequestVerificationToken': token,
                            values: JSON.stringify(values)
                        },
                        success: function () {
                            $("#machineContainer").dxDataGrid("instance").refresh();
                            DevExpress.ui.notify('Done', 'success', 600);
                        },
                        error: function (xhr, ajaxOptions, thrownError) {
                            DevExpress.ui.notify(thrownError + "\r\n" + xhr.statusText + "\r\n" + xhr.responseText, 'error', 600);
                        }
                    });
                },
                update: function (key, values) {
                    $.ajax({
                        type: "PUT",
                        url: "update-machine",
                        data: {
                            '__RequestVerificationToken': token,
                            key: key,
                            values: JSON.stringify(values)
                        },
                        success: function () {
                            $("#machineContainer").dxDataGrid("instance").refresh();
                            DevExpress.ui.notify('Done', 'success', 600);
                        },
                        error: function (xhr, ajaxOptions, thrownError) {
                            DevExpress.ui.notify(thrownError + "\r\n" + xhr.statusText + "\r\n" + xhr.responseText, 'error', 600);
                        }
                    });
                },
                remove: function (key) {
                    $.ajax({
                        type: "DELETE",
                        url: "delete-machine",
                        data: {
                            '__RequestVerificationToken': token,
                            key: key
                        },
                        success: function () {
                            $("#machineContainer").dxDataGrid("instance").refresh();
                            DevExpress.ui.notify('Done', 'success', 600);
                        },
                        error: function (xhr, ajaxOptions, thrownError) {
                            DevExpress.ui.notify(thrownError + "\r\n" + xhr.statusText + "\r\n" + xhr.responseText, 'error', 600);
                        }
                    });
                }
            }),
            repaintChangesOnly: true,
            showBorders: true,
            searchPanel: {
                visible: true
            },
            headerFilter: {
                visible: true
            },
            editing: {
                refreshMode: "reshape",
                allowAdding: true,
                allowUpdating: true,
                allowDeleting: true,
                useIcons: true
            },
            scrolling: {
                mode: "virtual"
            },
            columns: [{
                caption: "Tên máy",
                dataField: "name",
                validationRules: [{ type: "required" }]
            }, {
                caption: "IP",
                dataField: "ip",
                validationRules: [{ type: "required" }, {
                    type: "pattern",
                    message: 'Your IP must have "xxx-xxx-xxx" format!',
                    pattern: /^((25[0-5]|(2[0-4]|1[0-9]|[1-9]|)[0-9])(\.(?!$)|$)){4}$/i
                }]
            }, {
                caption: "Trạng thái",
                dataField: "status",
                dataType: "boolean"
            }],
            deferRendering: false,
            masterDetail: {
                enabled: true,
                template: function (container, options) {
                    $("<div>")
                        .addClass("master-detail-caption")
                        .text("GPIO Setting")
                        .appendTo(container);

                    $("<div id='gpioContainer'>").dxDataGrid({
                        dataSource: new DevExpress.data.CustomStore({
                            key: "port",
                            load: function () {
                                var deferred = $.Deferred();
                                deferred.resolve(options.data.gpio);
                                return deferred.promise();
                            },
                            insert: function (values) {
                                if (options.data.gpio === null) {
                                    options.data.gpio = []
                                }
                                options.data.gpio.push(values)
                                $.ajax({
                                    type: "PUT",
                                    url: "update-machine",
                                    data: {
                                        '__RequestVerificationToken': token,
                                        key: options.data.id,
                                        values: JSON.stringify(options.data)
                                    },
                                    success: function () {
                                        $("#gpioContainer").dxDataGrid("instance").refresh();
                                        DevExpress.ui.notify('Done', 'success', 600);
                                    },
                                    error: function (xhr, ajaxOptions, thrownError) {
                                        DevExpress.ui.notify(thrownError + "\r\n" + xhr.statusText + "\r\n" + xhr.responseText, 'error', 600);
                                    }
                                });
                            },
                            remove: function (key) {
                                options.data.gpio = options.data.gpio.filter(i => i.port !== key)
                                $.ajax({
                                    type: "PUT",
                                    url: "update-machine",
                                    data: {
                                        '__RequestVerificationToken': token,
                                        key: options.data.id,
                                        values: JSON.stringify(options.data)
                                    },
                                    success: function () {
                                        $("#gpioContainer").dxDataGrid("instance").refresh();
                                        DevExpress.ui.notify('Done', 'success', 600);
                                    },
                                    error: function (xhr, ajaxOptions, thrownError) {
                                        DevExpress.ui.notify(thrownError + "\r\n" + xhr.statusText + "\r\n" + xhr.responseText, 'error', 600);
                                    }
                                });
                            }
                        }),
                        repaintChangesOnly: true,
                        columnAutoWidth: true,
                        showBorders: true,
                        editing: {
                            refreshMode: "reshape",
                            allowAdding: true,
                            allowUpdating: false,
                            allowDeleting: true,
                            useIcons: true
                        },
                        scrolling: {
                            mode: "virtual"
                        },
                        columns: [{
                            caption: "Tên chân",
                            dataField: "name",
                        }, {
                            caption: "Số chân",
                            dataField: "port",
                            dataType: "number",
                            validationRules: [{ type: "required" }]
                        }, {
                            caption: "Loại chân",
                            dataField: "type",
                            validationRules: [{ type: "required" }],
                            cellTemplate: function (container, options) {
                                if (options.data.type === undefined || options.data.type === null) {
                                    return;
                                }
                                container.html(types.find(i => i.id === options.data.type).name)
                            },
                            lookup: {
                                dataSource: types,
                                valueExpr: "id",
                                displayExpr: "name"
                            }
                        }],
                    }).appendTo(container);
                }
            }
        });
    });
});