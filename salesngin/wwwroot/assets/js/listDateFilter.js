"use strict";

var KTDatatableListDateFilter = function () {
    // Define shared variables
    var table;
    var datatable;
    var flatpickr;
    var minDate, maxDate;


    // Private functions
    var initDatatable = function () {
        // Set date data order
        //const tableRows = table.querySelectorAll('tbody tr');

        //tableRows.forEach(row => {
        //    const dateRow = row.querySelectorAll('td');
        //    const realDate = moment(dateRow[5].innerHTML, "DD MMM YYYY, LT").format(); // select date from 4th column in table
        //    dateRow[5].setAttribute('data-order', realDate);
        //});

        //get index of last column
        var indexLastColumn = $(table).find('tr')[0].cells.length - 1;

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'order': [],
            //"pageLength": 10,
            //"lengthChange": true,
            'columnDefs': [
                //{ orderable: false, targets: 0 }, // Disable ordering on column 0 (checkbox)
                { orderable: false, targets: indexLastColumn }, // Disable ordering on last column (actions)                
            ]
        });
    }

    // Init flatpickr --- more info :https://flatpickr.js.org/getting-started/
    var initFlatpickr = () => {
        const element = document.querySelector('#kt_date_range_flatpickr');
        flatpickr = $(element).flatpickr({
            altInput: true,
            altFormat: "d/m/Y",
            dateFormat: "Y-m-d",
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

        // Datatable date filter --- more info: https://datatables.net/extensions/datetime/examples/integration/datatables.html
        // Custom filtering function which will search data in column four between two values
        $.fn.dataTable.ext.search.push(
            function (settings, data, dataIndex) {
                var min = minDate;
                var max = maxDate;
                var dateAdded = new Date(moment($(data[5]).text(), 'DD/MM/YYYY'));
                var dateModified = new Date(moment($(data[6]).text(), 'DD/MM/YYYY'));

                if (
                    (min === null && max === null) ||
                    (min === null && max >= dateModified) ||
                    (min <= dateAdded && max === null) ||
                    (min <= dateAdded && max >= dateModified)
                ) {
                    return true;
                }
                return false;
            }
        );
        datatable.draw();
    }

    // Handle clear flatpickr
    var handleClearFlatpickr = () => {
        const clearButton = document.querySelector('#kt_date_range_flatpickr_clear');
        clearButton.addEventListener('click', e => {
            flatpickr.clear();
        });
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

    return {
        // Public functions  
        init: function () {
            table = document.getElementById('kt_datatable_records');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            initFlatpickr();
            handleSearchDatatable();
            //handleStatusFilter();
            handleClearFlatpickr();
        }
    }
}();

// On document ready
KTUtil.onDOMContentLoaded(function () {
    KTDatatableList.init();
});