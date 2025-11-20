"use strict";

var KTDatatableList = function () {
    // Define shared variables
    var table;
    var datatable;
    var flatpickr;
    var minDate, maxDate;
    var indexLastColumn;
    var indexDateColumn;
    var indexStatusColumn;
    var includesDateColumn;
    var includesStatusColumn;

    // Private functions
    var initDatatable = function () {
        //Custom
        const element = document.querySelector('#hd_filterConfig');
        includesDateColumn = element.dataset.datecolumn;
        includesStatusColumn = element.dataset.statuscolumn;
        //get index of last column
        indexLastColumn = $(table).find('tr')[0].cells.length - 1;

        if (includesDateColumn == 'True' && includesStatusColumn == 'True') {
            indexDateColumn = $(table).find('tr')[0].cells.length - 2;
            indexStatusColumn = $(table).find('tr')[0].cells.length - 3;
        }
        else if (includesDateColumn == 'False' && includesStatusColumn == 'True') {
            indexStatusColumn = $(table).find('tr')[0].cells.length - 2;
            indexDateColumn = 0;
        }
        else if (includesDateColumn == 'True' && includesStatusColumn == 'False') {
            indexDateColumn = $(table).find('tr')[0].cells.length - 2;
            indexStatusColumn = 0;
        }
        else {
            indexDateColumn = 0;
            indexStatusColumn = 0;
        }

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'order': [],
            "pageLength": 10,
            //"lengthChange": true,
            'columnDefs': [
                //{ orderable: false, targets: 0 }, // Disable ordering on column 0 (checkbox)
                { orderable: false, targets: indexLastColumn }, // Disable ordering on last column (actions)                
            ]
        });

        // Re-init functions on datatable re-draws
        //datatable.on('draw', function () {
        //    handleDeleteRows();
        //});
    }

    // Init flatpickr --- more info :https://flatpickr.js.org/getting-started/
    var initFlatpickr = () => {

        const element = document.querySelector('#kt_date_range_flatpickr');
        flatpickr = $(element).flatpickr({
            altInput: true,
            altFormat: "d-M-Y",
            //altFormat: "d/m/Y",
            dateFormat: "d-M-Y",
            mode: "range",
            onChange: function (selectedDates, dateStr, instance) {
                handleFlatpickr(selectedDates, dateStr, instance);
            },
        });
    }

    //Handle flatpickr --- more info: https://flatpickr.js.org/events/
    var handleFlatpickr = (selectedDates, dateStr, instance) => {
        minDate = selectedDates[0] ? new Date(selectedDates[0]) : null;
        maxDate = selectedDates[1] ? new Date(selectedDates[1]) : null;

        //console.log(indexDateColumn, minDate, maxDate);
        // Datatable date filter --- more info: https://datatables.net/extensions/datetime/examples/integration/datatables.html
        // Custom filtering function which will search data in column four between two values
        $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
            var min = moment(minDate).format("L");
            var max = moment(maxDate).format("L");
            var date = moment(data[indexDateColumn]).format("L");

            if (
                (min === null && max === null) ||
                (min === null && date <= max) ||
                (min <= date && max === null) ||
                (min <= date && date <= max)
            ) {
                return true;
            }
            return false;
        });
        datatable.draw(); // Initial draw to apply filter to the loaded data
    }

    // Handle clear flatpickr
    var handleClearFlatpickr = () => {
        const clearButton = document.querySelector('#kt_date_range_flatpickr_clear');
        if (clearButton !== null) {
            clearButton.addEventListener('click', e => {
                flatpickr.clear();
                //datatable.search('').draw(); // Initial draw to apply filter to the loaded data
                //location.reload();
                handleSearchDatatable();
            });
        }
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = 'Export Options';
        var buttons = new $.fn.dataTable.Buttons(table, {
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
                }
            ]
        }).container().appendTo($('#kt_datatable_export_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_table_export_menu [data-kt-export]');
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
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = function () {
        const filterSearch = document.querySelector('[data-kt-datatable-table-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Handle status filter dropdown
    var handleStatusFilter = () => {
        const filterStatus = document.querySelector('[data-datatable-status-filter="status"]');
        $(filterStatus).on('change', e => {
            let value = e.target.value;
            if (value === 'all') {
                value = '';
            }
            datatable.column(indexStatusColumn).search(value).draw();
        });
    }

    return {
        // Public functions  
        init: function () {

            table = document.querySelector('#kt_datatable_records');

            if (!table) {
                return;
            }

            initDatatable();
            initFlatpickr();
            exportButtons();
            handleSearchDatatable();
            handleStatusFilter();
            handleClearFlatpickr();
        }
    }
}();

// On document ready
KTUtil.onDOMContentLoaded(function () {
    KTDatatableList.init();
});