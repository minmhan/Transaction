$(document).ready(function () {
    $("#tblTransaction").dataTable({
        "processing": true,
        "serverSide": true,
        "filter": true,
        "lengthChange": false,
        "pageLength": 3000,
        "ajax": {
            "url": "/transaction",
            "type": "POST",
            "data": {
                status: function () {return $("#statusCode").val() }
            },
            "datatype": "json"
        },
        "columnDefs": [
        //{
        //    "targets": [0],
        //    "visible": false,
        //    "searchable": false
        //},
        {
            "targets": "_all",
            "className": "text-left"
        }
        ],
        "columns": [
            { "data": "id", "name": "id", "autoWidth": true },
            { "data": "payment", "name": "payment", "autoWidth": true },
            { "data": "status", "name": "status", "autoWidth": true }
        ]
    });
});

function showPayment(data, type, row) {
    return parseFloat(row.amount).toFixed(2) + " " + row.currencyCode;
}

