$(document).ready(function () {
    $("#tblTransactionErrorLog").dataTable({
        "processing": true,
        "serverSide": true,
        "filter": true,
        "lengthChange": false,
        "pageLength": 3000,
        "order": [[0, "desc"]],
        "ajax": {
            "url": "/transactionErrorLog",
            "type": "POST",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [0],
                "searchable": false
            },
            {
                "targets": "_all",
                "className": "text-left"
            }
            //{ "orderable": false, "targets": [1] }
        ],
        "columns": [
            { "data": "id", "name": "id", "autoWidth": true },
            { "data": "uploadedDate", "name": "uploadedDate", "autoWidth": true, render: showDate },
            { "data": "fileName", "name": "fileName", "autoWidth": true, render: showFile },
            { "data": "transactionId", "name": "transactionId", "autoWidth": true },
            { "data": "amount", "name": "amount", "autoWidth": true },
            { "data": "currencyCode", "name": "currencyCode", "autoWidth": true },
            { "data": "dateTime", "name": "dateTime", "autoWidth": true },
            { "data": "status", "name": "status", "autoWidth": true },
            { "data": "error", "name": "error", "autoWidth": true}
        ]
    });
});


function showDate(data) {
    date = moment(data, "YYYY-MM-DD");
    if (date.isValid())
        return date.format("MMM DD, YYYY");
    else
        return "";
}

function showFile(data) {
    if (data) {
        return "<label title='"+ data + "' style='cursor:pointer;'>" + data.substring(0, 15) + "...</label>";
    }
    else return "";
}
