"use strict";

var KTAppTableListing = (function () {
    var table,
        initialData,  // Variable to store the initial data
        dateRangePickerInstance, // Variable to store Flatpickr instance
        dateRangePicker,
        searchInput,
        statusFilter,
        clearDateRangeButton,
        exportButtonsContainerId,
        exportMenuId;

    var initializeDataTable = function (tableId, dateRangeId, searchInputId, statusFilterId, sumColumns, dateColumnIdentifier, statusColumnIdentifier, clearDateRangeButtonId, exportButtonsContainer, exportMenu) {
        table = document.querySelector("#" + tableId);
        dateRangePicker = document.querySelector("#" + dateRangeId);
        searchInput = document.querySelector("#" + searchInputId);
        statusFilter = document.querySelector("#" + statusFilterId);
        clearDateRangeButton = document.querySelector("#" + clearDateRangeButtonId);
        exportButtonsContainerId = exportButtonsContainer;
        exportMenuId = exportMenu;

        if (!table) {
            console.error("Table element not found");
            return;
        }

        //if (!dateRangePicker || !searchInput) {
        //    console.error("Date range input or search input not found");
        //    return;
        //}

        // DataTable initialization
        var dataTable = $(table).DataTable({
            info: false,
            order: [],
            pageLength: 10,
            //lengthChange : true,
            //dom: 'Bfrtip', // Add export buttons to the DataTable
            //buttons: [
            //    'copyHtml5',
            //    'excelHtml5',
            //    'csvHtml5',
            //    'pdfHtml5'
            //],
            columnDefs: [
                { orderable: false, targets: -1 }, // Set the last column as not orderable
            ],
        });

        // Capture the initial data after DataTable is populated
        initialData = dataTable.rows().data().toArray();
        // Find column index based on dateColumnIdentifier
        var dateColumnIndex = findColumnIndex(dataTable, dateColumnIdentifier);
        //console.log("Date Column Index : ", dateColumnIndex);

        // Find column index based on statusColumnIdentifier
        var statusColumnIndex = findColumnIndex(dataTable, statusColumnIdentifier);
        //console.log("Status Column Index : ", statusColumnIndex);

        if (dateRangePicker) {


            // Custom search by date range
            var filterDateRange = function (dataTable, start, end) {
                //var startDate = start ? new Date(start) : null;
                //var endDate = end ? new Date(end) : null;
                //startDate = moment(startDate).format("L");
                //endDate = moment(endDate).format("L");

                //var startDate = start ? moment(start).format("L") : null;
                //var endDate = end ? moment(end).format("L") : null;

                var startDate = start ? moment(start).format("DD-MMM-YYYY") : null;
                var endDate = end ? moment(end).format("DD-MMM-YYYY") : null;

                console.log(startDate, endDate);
                $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
                    //var orderDate = new Date(moment($(data[dateColumnIndex]).text(), "DD/MM/YYYY"));
                    //var orderDate = new Date(moment($(data[dateColumnIndex]).text()).format("L"));
                    //var orderDate = moment(data[dateColumnIndex]).format("L");
                    //var orderDate = moment($(data[dateColumnIndex]).text()).format("L");
                    var odate = data[dateColumnIndex];
                    var orderDate = odate ? moment(odate).format("DD-MMM-YYYY") : null;
                    //var orderDate = odate ? moment(odate).format("L") : null;

                    console.log(orderDate);
                    return (startDate === null && endDate === null) ||
                        (startDate === null && endDate >= orderDate) ||
                        (startDate <= orderDate && endDate === null) ||
                        (startDate <= orderDate && endDate >= orderDate);

                });

                dataTable.draw();
            };

            // Initialize Flatpickr and store the instance in a variable
            dateRangePickerInstance = $(dateRangePicker).flatpickr({
                altInput: true,
                altFormat: "d-M-Y",
                dateFormat: "d-M-Y",
                mode: "range",
                onChange: function (selectedDates, dateStr, instance) {
                    var start = selectedDates[0];
                    var end = selectedDates[1];
                    filterDateRange(dataTable, start, end);
                },
            });

            // Assign clearDateRange function to the clear button
            if (clearDateRangeButton && document.querySelector("#" + clearDateRangeButtonId)) {
                clearDateRangeButton.addEventListener('click', function () {
                    clearDateRange(dataTable);
                });
            }

        }

        //Search functionality
        $(searchInput).on("keyup", function (e) {
            dataTable.search(e.target.value).draw();
        });

        // Status filter        
        if (statusFilter) {
            $(statusFilter).on("change", function (e) {
                var selectedStatus = e.target.value;
                selectedStatus = (selectedStatus === "all") ? "" : selectedStatus;
                dataTable.column(statusColumnIndex).search(selectedStatus).draw();
            });
        }

        // Sum specified columns
        if (sumColumns && sumColumns.length > 0) {
            sumColumns.forEach(column => {
                updateColumnSum(dataTable, column);
            });
        }

        // Custom export buttons
        if (exportButtonsContainerId && document.querySelector("#" + exportButtonsContainerId)) {
            exportButtons(dataTable);
        }


    };

    var findColumnIndex = function (dataTable, identifier) {
        var columnIndex = -1;
        $(table).find("thead tr th").each(function (index) {
            if ($(this).text().toLowerCase().includes(identifier.toLowerCase())) {
                columnIndex = index;
                return false; // Break the loop
            }
        });
        return columnIndex;
    };

    var updateColumnSum = function (dataTable, columnIndex) {
        var sum = 0;
        dataTable.rows({ search: "applied" }).data().each(function (row) {
            sum += parseFloat(row[columnIndex]);
        });

        dataTable.column(columnIndex).footer().innerHTML = sum;
    };

    var exportButtons = function (dataTable) {
        const documentTitle = 'Export Options';
        var buttons = new $.fn.dataTable.Buttons(dataTable, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle
                },
                {
                    extend: 'print',
                    text: 'Print all records',
                    exportOptions: {
                        modifier: {
                            page: 'all'
                        }
                    }
                }
            ]
        }).container().appendTo($('#' + exportButtonsContainerId));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#' + exportMenuId + ' [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    };

    var clearDateRange = function (dataTable) {
        // Call clear method on the stored Flatpickr instance
        dateRangePickerInstance.clear();
        // Remove the date range filter from DataTable
        //$.fn.dataTable.ext.search.pop();
        $.fn.dataTable.ext.search = [];
        //console.log("Cleared date range filter. Reloading records...");
        //dataTable.search('').draw();
        // Reload the initial records
        dataTable.clear().rows.add(initialData).draw();
        // Redraw the DataTable to reload all initial records
        dataTable.search('').draw();
        //dataTable.search('').draw();
        //console.log("Records reloaded.");
    };

    return {
        init: function (tableId, dateRangeId, searchInputId, statusFilterId, sumColumns, dateColumnIdentifier, statusColumnIdentifier, clearDateRangeButtonId, exportButtonsContainer, exportMenu) {

            initializeDataTable(tableId, dateRangeId, searchInputId, statusFilterId, sumColumns, dateColumnIdentifier, statusColumnIdentifier, clearDateRangeButtonId, exportButtonsContainer, exportMenu);
        },
    };
})();

