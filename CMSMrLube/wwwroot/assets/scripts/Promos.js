$(function () {
    $('#tblPromo').dataTable();
    //$('#tblPromo_length').hide();
    //$('#tblPromo_filter').hide();
    PromosDataTable('/Promos/PromosList');
    //$('#tblPromo_length').hide();
    //$('#tblPromo_filter').hide();
});

//$(document).ready(function () {

//    PromosDataTable('/Promos/PromosList');
//});

function PromosDataTable(url) {

    getPromosData(url);
    //$('#tblPromo_length').hide();
    //$('#tblPromo_filter').hide();
}


function getPromosData(url) {

    var table = $('#tblPromo').DataTable({
        "ajax": {
            "url": url,
            "type": "Post",
            "data": [],
            "datatype": "json",
        },
        autoWidth: false,
        "columns": [
            { "data": "itemId", "name": "itemId" },
            { "data": "title", "name": "title" },
            { "data": "frenchTitle", "name": "frenchTitle" },
            { "data": "status", "name": "status" },
            { "data": "Action", "name": "non" }
        ],
        "columnDefs": [
            //  { "width": "50px", "targets": [0, 1] }, { "width": "40px", "targets": [2] },
            {
                "targets": 4,
                "data": null,
                "orderable": false,
                "render": function (data, type, full, meta) {
                    //var editUrl = "'/Banner/Edit/" + full.guid + "'";
                    var viewUrl = "'/Promos/Details/" + full.guid + "'";
                    var editUrl = "'/Promos/Edit/" + full.guid + "'";
                    var deleteUrl = "'/Promos/Delete/" + full.guid + "'";

                    var actionUrl = '<div style="font-family: cursive;display: inline-flex; white-space:normal;overflow:hidden;"> \n'
                    actionUrl = actionUrl + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-primary" onclick="editPromosData(' + editUrl + ')" title = "Edit Promos Banner"><i class="pe-7s-pen btn-icon-wrapper"></i></button>'
                        + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-warning" onclick="viewPromosData(' + viewUrl + ')" title = "View Promos Banner"><i class="pe-7s-look btn-icon-wrapper"></i></button>'
                        + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-danger" onclick="deletePromosData(' + deleteUrl + ')" title = "Delete Promos Banner"><i class="pe-7s-trash btn-icon-wrapper"></i></button>'

                    '</div ></td > '

                    return actionUrl;
                }
            },
        ],

        "responsive": "true",
        //"serverSide": "true",
        "order": [0, "desc"],
        "destroy": "true",
        "searching": "true",
        "language": {
            //"processing": 'Processing- please wait' 
        }
    });
}

function AddNewItem() {
    var url = "/Promos/Create";
    $.ajax({
        type: "Get",
        url: url,
        async: false,
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');
            btnPublishToggle(false);
            formValidationRules();
            $('#date_expired').val('');
            //$(".date").datepicker({
            //    changeMonth: true,
            //    // dateFormat: DateFormat,
            //    autoclose: true
            //});
            $('#dvPromoUrls').hide();
        }
    });
}

function btnPublishToggle(flag) {
    if (flag == true)
        $('#btnPublish').prop('disabled', false);
    else if (flag == false)
        $('#btnPublish').prop('disabled', true);
}


function formValidationRules() {
    //jQuery.validator.addMethod("accept", function(value, element, param) {
    //    return value.match(new RegExp("/^[a-zA-Z0-9_\.%\+\-]+@@[a-zA-Z0-9\.\-]+\.[a-zA-Z]{2,}$/)");
    //}, 'please enter a valid email');
    $("#formPromo").validate({

        rules: {
            englishTitle: "required",
            frenchTitle: "required",
            date_expired: "required"
        },
        messages: {
            englishTitle: { required: "Please enter English Title" },
            frenchTitle: { required: "Please Enter French Title" },
            date_expired: { required: "Please Enter Date Expired" }
        },
        errorElement: "em",
        errorPlacement: function (error, element) {
            error.addClass("invalid-feedback");
            if (element.prop("type") === "checkbox") {
                error.insertAfter(element.next("label"));
            } else {
                error.insertAfter(element);
            }
        },
        highlight: function (element, errorClass, validClass) {
            $(element).addClass("is-invalid").removeClass("is-valid");
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).addClass("is-valid").removeClass("is-invalid");
        }
    });
}

function savePromo() {
    if ($('#formPromo').valid()) {
        var url = "";
        var formData = new FormData($('#formPromo')[0]);
        url = $('#formPromo').attr('action');
    }

    $.ajax({
        type: "POST",
        url: url,
        data: formData,
        async: false,
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            if (data != null) {
                if (data.data.isSuccessfull === true && data.data.formMode === 'Create') {
                    toastr.success(data.data.message);
                    if (data.data.isEmpty === true)
                        btnPublishToggle(true);
                    else
                        btnPublishToggle(false);
                    //btnPublishToggle(true);
                    var engTitle = $("#englishTitle").val().replaceAll(' ', '-').toLowerCase();
                    engTitle = engTitle.replaceAll("'", '');
                    var staggingURL = $("#hdnStaggingDomain").val() + "en/promos/" + engTitle;
                    
                    var prodURL = $("#hdnProdDomain").val() + "en/promos/" + engTitle;
                    

                    $("#dvStagingPromoUrl").attr("href", staggingURL);
                    $("#dvStagingPromoUrl").text(staggingURL);

                    $("#dvProductionPromoUrl").attr("href", prodURL);
                    $("#dvProductionPromoUrl").text(prodURL);

                    var staggingFrURL = $("#hdnStaggingDomain").val() + "fr/promos/" + engTitle;
                    
                    var prodFrURL = $("#hdnProdDomain").val() + "fr/promos/" + engTitle;
                    

                    $("#dvStagingPromoFrUrl").attr("href", staggingFrURL);
                    $("#dvStagingPromoFrUrl").text(staggingFrURL);

                    $("#dvProductionPromoFrUrl").attr("href", prodFrURL);
                    $("#dvProductionPromoFrUrl").text(prodFrURL);

                    $('#dvPromoUrls').show();

                    window.open(staggingURL, "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');

                }
                else {
                    toastr.error(data.data.message);
                }
            }
            else {
                toastr.error(data.data.message);
            }
        },
        //error: function (exp) {
        //    alert(exp.responseText);
        //},
        cache: false,
        contentType: false,
        processData: false
    });
}

function editPromosData(url) {
    var arrUrl = url.split("/");
    var guid = arrUrl[3];
    $.ajax({
        type: "Get",
        url: url,
        data: { guid, guid },
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');
            btnPublishToggle(false);
            formValidationRules();

            var staggingURL = $("#hdnStaggingDomain").val() + "en/promos/" + $("#hdnurl_Key").val();
            var prodURL = $("#hdnProdDomain").val() + "en/promos/" + $("#hdnurl_Key").val();

            $("#dvStagingPromoUrl").attr("href", staggingURL);
            $("#dvStagingPromoUrl").text(staggingURL);

            $("#dvProductionPromoUrl").attr("href", prodURL);
            $("#dvProductionPromoUrl").text(prodURL);

            var staggingFrURL = $("#hdnStaggingDomain").val() + "fr/promos/" + $("#hdnurl_Key").val();
            var prodFrURL = $("#hdnProdDomain").val() + "fr/promos/" + $("#hdnurl_Key").val();

            $("#dvStagingPromoFrUrl").attr("href", staggingFrURL);
            $("#dvStagingPromoFrUrl").text(staggingFrURL);

            $("#dvProductionPromoFrUrl").attr("href", prodFrURL);
            $("#dvProductionPromoFrUrl").text(prodFrURL);

            $('#dvPromoUrls').show();
        }
    });
}


function updatePromo() {
    if ($("#formPromo").valid()) {
        var formData = new FormData($('#formPromo')[0]);
        var url = "/Promos/Edit";
        $.ajax({
            url: url,
            type: "POST",
            data: formData,
            async: false,
            success: function (data) {
                $.fn.modal.Constructor.prototype.enforceFocus = function () { };
                if (data != null) {
                    if (data.data.isUpdate === true && data.data.formMode === 'Edit') {
                        toastr.success(data.data.message);
                        if (data.data.isEmpty === true)
                            btnPublishToggle(true);
                        else
                            btnPublishToggle(false);

                        var staggingURL = $("#hdnStaggingDomain").val() + "en/promos/" + $("#hdnurl_Key").val();
                        window.open(staggingURL, "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');

                    }
                    else {
                        toastr.error(data.data.message);
                    }
                }
                else {
                    toastr.error(data.data.message);
                }
            },
            error: function (exp) {
                alert(exp.responseText);
            },
            cache: false,
            contentType: false,
            processData: false,
        });
    }
}

function viewPromosData(url) {

    var arrUrl = url.split("/");
    var guid = arrUrl[3];

    $.ajax({
        type: "Get",
        url: url,
        data: { guid, guid },
        success: function (data) {
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');
        }
    });
}

function deletePromosData(url) {

    var arrUrl = url.split("/");
    var guid = arrUrl[3];

    if (confirm("Do you want to delete the Promo?")) {
        this.click;
        $.ajax({
            type: "Get",
            url: url,
            data: { guid, guid },
            success: function (data) {
                if (data != null) {

                    if (data.data.isSuccessfull === true && data.data.formMode === "Delete") {
                        getPromosData('/Promos/PromosList');
                        toastr.success(data.data.message);
                    }
                    else {
                        toastr.error(data.data.message);
                    }
                }
                else {
                    // $("#dvErrorAuth").html("FE: Something went wrong");
                    toastr.error(data.data.message);
                }

            },
            error: function (exp) {
                alert(exp.responseText);
            }//,
            //cache: false,
            //contentType: false,
            //processData: false

        });
    }
    else {
        //alert("Cancel");
        toastr.error("Promo Deleted Query Cancel.");
    }
    event.preventDefault();
}

function closeModal() {
    $('#modalCMS').modal('hide');
    getPromosData('/Promos/PromosList');
}


function publishPromo() {
    var url = "/Promos/Publish";

    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            if (data != null) {
                if (data.data.isSuccessfull === true) {
                    toastr.success(data.data.message);
                    getPromosData('/Promos/PromosList');
                    //$('#modalCMS').modal('hide');
                }
                else {
                    toastr.error(data.data.message);
                }
            }
            else {
                toastr.error("Something went wrong.");
            }
        },
        error: function (exp) {
            alert(exp.responseText);
        }
    });
}
