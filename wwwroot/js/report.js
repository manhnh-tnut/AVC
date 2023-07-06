"use strict";

$(function () {
    let from, to, min_hour = 8, min_count = 1000, group = 'day', data = [];
    let dataStore = new DevExpress.data.ArrayStore({
        key: "id",
        data: data
    });

    $("#formContainer").dxForm({
        colCount: 5,
        formData: { from: from, to: to },
        items: [{
            dataField: "from",
            editorType: "dxDateBox",
            editorOptions: {
                width: 200,
                type: "date",
                max: new Date(),
                value: new Date(),
                onContentReady: function (e) {
                    e.component.option("value", new Date());
                },
                onValueChanged: function (e) {
                    from = new Date(e.value).toISOString().split('T')[0];
                }
            }
        }, {
            dataField: "to",
            editorType: "dxDateBox",
            editorOptions: {
                width: 200,
                type: "date",
                max: new Date(),
                value: new Date(),
                onContentReady: function (e) {
                    e.component.option("value", new Date());
                },
                onValueChanged: function (e) {
                    to = new Date(e.value).toISOString().split('T')[0];
                }
            }
        }, {
            itemType: "button",
            buttonOptions: {
                text: "Search",
                stylingMode: "outlined",
                type: "success",
                icon: "search",
                onClick: function () {
                    $.post('report', {
                        '__RequestVerificationToken': token,
                        from: from,
                        to: to
                    }).done(function (response) {
                        console.log(response)
                        for (let index = 0; index < response.length; index++) {
                            if (data.length === 0 || data.find(i => i.id === response[index].id) === undefined) {
                                data.push(response[index])
                                dataStore.push([{ type: "insert", data: response[index] }]);
                            } else {
                                dataStore.push([{ type: "update", key: response[index].id, data: response[index] }]);
                            }

                            if (data.length > index && response.find(i => i.id === data[index].id) === null) {
                                data.pop(data[index])
                                dataStore.push([{ type: "remove", key: response[index].id, data: response[index] }]);
                            }
                        }
                        if ($("#gridContainer").dxDataGrid("instance") !== undefined) {
                            $("#gridContainer").dxDataGrid("instance").refresh();
                        }
                        if ($("#pivodGridContainer").dxPivotGrid("instance") !== undefined) {
                            $("#pivodGridContainer").dxPivotGrid("instance").option("dataSource.store", dataStore)
                        }
                    });
                }
            }
        }]
    });

    $('#tabPanelContainer').dxTabPanel({
        height: 'auto',
        swipeEnabled: true,
        animationEnabled: true,
        items: [{
            title: 'Thời gian vận hành',
        }, {
            title: 'Số lần cắt',
        }, {
            title: 'Lịch sử',
        }, {
            title: 'Tổng hợp',
        }],
        itemTemplate: function (itemData, itemIndex, itemElement) {
            switch (itemData.title) {
                case 'Thời gian vận hành':
                    $("<div id='hourChartContainer'>").dxChart({
                        palette: "Green Mist",
                        dataSource: {
                            store: dataStore,
                            reshapeOnPush: true
                        },
                        loadingIndicator: {
                            enabled: true
                        },
                        commonSeriesSettings: {
                            argumentField: "date",
                            valueField: "time",
                            type: "bar",
                            hoverMode: "allArgumentPoints",
                            selectionMode: "allArgumentPoints",
                            label: {
                                visible: true,
                                format: {
                                    type: "fixedPoint",
                                    precision: 0
                                }
                            }
                        },
                        seriesTemplate: {
                            nameField: "machineName"
                        },
                        valueAxis: [{
                            title: {
                                text: "Thời gian vận hành(Hour)",
                                font: {
                                    color: "#e91e63"
                                }
                            },
                            label: {
                                font: {
                                    color: "#e91e63"
                                }
                            },
                            name: "time",
                            position: "left",
                            constantLines: [{
                                value: min_hour,
                                color: "#e91e63",
                                dashStyle: "dash",
                                width: 2,
                                label: {
                                    text: "Thời gian vận hành tối thiểu"
                                },
                            }],
                            // }, {
                            //     title: {
                            //         text: "Count",
                            //         font: {
                            //             color: "#03a9f4"
                            //         }
                            //     },
                            //     label: {
                            //         font: {
                            //             color: "#03a9f4"
                            //         }
                            //     },
                            //     name: "count",
                            //     position: "right",
                            //     showZero: true,
                            //     valueMarginsEnabled: false
                        }],
                        tooltip: {
                            enabled: true,
                            customizeTooltip: function (arg) {
                                return {
                                    html: arg.seriesName === "Count" ? "" : "<div><div class='tooltip-header'>" +
                                        arg.argumentText + "</div>" +
                                        "<div class='tooltip-body'><div class='series-name'>" +
                                        arg.seriesName +
                                        ": </div><div class='value-text'>" +
                                        arg.valueText + "(" + arg.point.data.percent + "%)" +
                                        "</div></div></div>"
                                };
                            }
                        },
                        legend: {
                            verticalAlignment: "top",
                            horizontalAlignment: "right"
                        },
                        customizePoint: function (arg) {
                            if (this.value < min_hour) {
                                return { color: "#ff7c7c", hoverStyle: { color: "#ff7c7c" } };
                            }
                        },
                        customizeLabel: function (arg) {
                            // if (arg.seriesName === "Hour") {
                            return {
                                visible: true,
                                customizeText: function () {
                                    return arg.data.display;
                                }
                            };
                            // }
                        },
                    }).appendTo(itemElement)
                    break;
                case 'Số lần cắt':
                    $("<div id='countChartContainer'>").dxChart({
                        palette: "Green Mist",
                        dataSource: {
                            store: dataStore,
                            reshapeOnPush: true
                        },
                        loadingIndicator: {
                            enabled: true
                        },
                        commonSeriesSettings: {
                            argumentField: "date",
                            valueField: "count",
                            type: "bar",
                            hoverMode: "allArgumentPoints",
                            selectionMode: "allArgumentPoints",
                            label: {
                                visible: true,
                                format: {
                                    type: "fixedPoint",
                                    precision: 0
                                }
                            }
                        },
                        seriesTemplate: {
                            nameField: "machineName"
                        },
                        valueAxis: [{
                            title: {
                                text: "Số lần cắt",
                                font: {
                                    color: "#03a9f4"
                                }
                            },
                            label: {
                                font: {
                                    color: "#03a9f4"
                                }
                            },
                            name: "count",
                            position: "left",
                            showZero: true,
                            valueMarginsEnabled: false,
                            constantLines: [{
                                value: min_count,
                                color: "#03a9f4",
                                dashStyle: "dash",
                                width: 2,
                                label: {
                                    text: "Số lần cắt tối thiểu"
                                },
                            }],
                        }],
                        tooltip: {
                            enabled: true,
                            customizeTooltip: function (arg) {
                                return {
                                    html: arg.seriesName === "Count" ? "" : "<div><div class='tooltip-header'>" +
                                        arg.argumentText + "</div>" +
                                        "<div class='tooltip-body'><div class='series-name'>" +
                                        arg.seriesName +
                                        ": </div></div></div>"
                                };
                            }
                        },
                        legend: {
                            verticalAlignment: "top",
                            horizontalAlignment: "right"
                        },
                        customizePoint: function (arg) {
                            if (this.value < min_count) {
                                return { color: "#ff7c7c", hoverStyle: { color: "#ff7c7c" } };
                            }
                        },
                        customizeLabel: function (arg) {
                            if (arg.seriesName === "Hour") {
                                return {
                                    visible: true,
                                    customizeText: function () {
                                        return arg.data.display;
                                    }
                                };
                            }
                        },
                    }).appendTo(itemElement)
                    break;
                case 'Lịch sử':
                    $("<div id='gridContainer'>").dxDataGrid({
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
                        scrolling: {
                            mode: "virtual"
                        },
                        headerFilter: {
                            visible: true,
                            allowSearch: true
                        },
                        filterRow: {
                            visible: true,
                            applyFilter: "auto"
                        },
                        wordWrapEnabled: true,
                        export: {
                            enabled: true,
                            allowExportSelectedData: true
                        },
                        onExporting: function (e) {
                            var workbook = new ExcelJS.Workbook();
                            var worksheet = workbook.addWorksheet('report');
                            DevExpress.excelExporter.exportDataGrid({
                                component: e.component,
                                worksheet: worksheet,
                                autoFilterEnabled: true
                            }).then(function () {
                                workbook.xlsx.writeBuffer().then(function (buffer) {
                                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'report-' + (new Date().toLocaleDateString().replace(/\/|\-/g, '.')) + '.xlsx');
                                });
                            });
                            e.cancel = true;
                        },
                        columns: [{
                            caption: '#',
                            width: 50,
                            dataType: "number",
                            allowEditing: false,
                            cellTemplate: function (container, options) {
                                container.text(options.row.rowIndex + 1)
                            }
                        }, {
                            caption: "Tên máy",
                            dataField: "machineName",
                        }, {
                            caption: "Ngày",
                            dataField: "date",
                            dataType: "date",
                        }, {
                            caption: "Thời gian chạy tích lũy",
                            dataField: "display",
                            dataType: "time",
                            cellTemplate: function (container, options) {
                                container.text(options.data.display)
                                    .css("color", options.data.time < min_hour ? "red" : "black")
                            }
                        }, {
                            caption: "Số lần cắt sản phẩm",
                            dataField: "count",
                            dataType: "number",
                            cellTemplate: function (container, options) {
                                container.text(options.data.count)
                                    .css("color", options.data.count < min_count ? "red" : "black")
                            }
                        }],
                    }).appendTo(itemElement);
                    break;
                case 'Tổng hợp':
                    $("<div id='pivodChartContainer'>").dxChart({
                        commonSeriesSettings: {
                            type: "bar"
                        },
                        tooltip: {
                            enabled: true,
                            customizeTooltip: function (args) {
                                var valueText = (args.seriesName.indexOf("Total") != -1) ?
                                    Globalize.formatCurrency(args.originalValue,
                                        "USD", { maximumFractionDigits: 0 }) :
                                    args.originalValue;

                                return {
                                    html: args.seriesName + "<div class='currency'>"
                                        + valueText + "</div>"
                                };
                            }
                        },
                    }).appendTo(itemElement);

                    $("<div id='pivodGridContainer'>").dxPivotGrid({
                        dataSource: {
                            fields: [{
                                caption: "Tên máy",
                                dataField: "machineName",
                                area: "row",
                                sortBySummaryField: "Total"
                            }, {
                                caption: "Ngày",
                                dataField: "date",
                                groupInterval: group,
                                dataType: "date",
                                area: "column",
                                selector: function (data) {
                                    switch (group) {
                                        case 'day':
                                            return data.date;
                                        case 'week':
                                            return "Tuần " + data.week;
                                        case 'month':
                                            return "Tháng " + data.month;
                                        case 'quarter':
                                            return "Quý " + data.quarter;
                                        default:
                                            break;
                                    }
                                },
                            }, {
                                caption: "Tổng số lần cắt",
                                dataField: "count",
                                dataType: "number",
                                summaryType: "sum",
                                area: "data",
                            }, {
                                caption: "Tổng thời gian vận hành",
                                dataField: "time",
                                dataType: "number",
                                summaryType: "sum",
                                area: "data",
                            }],
                            store: dataStore,
                            reshapeOnPush: true
                        },
                        allowSortingBySummary: true,
                        showColumnGrandTotals: false,
                        showRowGrandTotals: false,
                        repaintChangesOnly: true,
                        showColumnTotals: false,
                        highlightChanges: true,
                        columnAutoWidth: true,
                        allowFiltering: true,
                        showRowTotals: false,
                        showBorders: true,
                        fieldChooser: {
                            enabled: true
                        },
                        searchPanel: {
                            visible: true
                        },
                        scrolling: {
                            mode: "virtual"
                        },
                        headerFilter: {
                            visible: true,
                            allowSearch: true
                        },
                        filterRow: {
                            visible: true,
                            applyFilter: "auto"
                        },
                        wordWrapEnabled: true,
                        export: {
                            enabled: true,
                            allowExportSelectedData: true
                        },
                        onExporting: function (e) {
                            var workbook = new ExcelJS.Workbook();
                            var worksheet = workbook.addWorksheet('report');
                            DevExpress.excelExporter.exportDataGrid({
                                component: e.component,
                                worksheet: worksheet,
                                autoFilterEnabled: true
                            }).then(function () {
                                workbook.xlsx.writeBuffer().then(function (buffer) {
                                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'report-' + (new Date().toLocaleDateString().replace(/\/|\-/g, '.')) + '.xlsx');
                                });
                            });
                            e.cancel = true;
                        }
                    }).appendTo(itemElement);

                    $("#pivodGridContainer").dxPivotGrid("instance").bindChart($("#pivodChartContainer").dxChart("instance"), {
                        dataFieldsDisplayMode: "splitPanes",
                        alternateDataFields: false
                    });
                    break;
                default:
                    break;
            }
        }
    });

    $("#min-hour").dxNumberBox({
        width: 200,
        type: "number",
        showClearButton: true,
        onContentReady: function (e) {
            e.component.option("value", min_hour);
        },
        onValueChanged: function (e) {
            min_hour = e.value
            if ($("#gridContainer").dxDataGrid("instance") !== undefined) {
                $("#gridContainer").dxDataGrid("instance").refresh();
            }
            if ($("#hourChartContainer").dxChart("instance") !== undefined) {
                $("#hourChartContainer").dxChart("instance").option("valueAxis[0].constantLines[0].value", min_hour)
                $("#hourChartContainer").dxChart("instance").refresh();
            }
        }
    });

    $("#min-count").dxNumberBox({
        width: 200,
        showClearButton: true,
        onContentReady: function (e) {
            e.component.option("value", min_count);
        },
        onValueChanged: function (e) {
            min_count = e.value
            if ($("#gridContainer").dxDataGrid("instance") !== undefined) {
                $("#gridContainer").dxDataGrid("instance").refresh();
            }
            if ($("#countChartContainer").dxChart("instance") !== undefined) {
                $("#countChartContainer").dxChart("instance").option("valueAxis[0].constantLines[0].value", min_count)
                $("#countChartContainer").dxChart("instance").refresh();
            }
        }
    });

    $("#groupInterval").dxSelectBox({
        dataSource: [{
            id: 'quarter',
            name: 'Qúy'
        }, {
            id: 'month',
            name: 'Tháng'
        }, {
            id: 'week',
            name: 'Tuần'
        }, {
            id: 'day',
            name: 'Ngày'
        }],
        valueExpr: "id",
        displayExpr: "name",
        value: 'day',
        onValueChanged: function (data) {
            group = data.value
            if ($("#pivodGridContainer").dxPivotGrid("instance") !== undefined) {
                $("#pivodGridContainer").dxPivotGrid("instance").option("dataSource.fields[1].groupInterval", group);
                $("#pivodGridContainer").dxPivotGrid("instance").repaint()
            }
        }
    });
});