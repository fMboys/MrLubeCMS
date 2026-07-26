$(function () {
    $('#tblPromoImage').dataTable();
    //$('#tblPromoImage_length').hide();
    //$('#tblPromoImage_filter').hide();
    PromoImageDataTable('/PromoImages/PromoImageList');
});

//$(document).ready(function () {

//    PromoImageDataTable('/PromoImages/PromoImageList');
//});

function PromoImageDataTable(url) {

    getPromoImageData(url);
    //$('#tblPromoImage_length').hide();
    //$('#tblPromoImage_filter').hide();
}

function getPromoImageData(url) {
    var storeNum = "";
    var title = "";
    var image = "";
    var viewDevice = "";
    var imageStatus = "";
    var language = "";
    var table = $('#tblPromoImage').DataTable({
        "ajax": {
            "url": url,
            "type": "Post",
            "data": { storeNum: storeNum, title: title, image: image, viewDevice: viewDevice, imageStatus: imageStatus, language: language },
            "datatype": "json",
        },
        autoWidth: false,
        "columns": [
            { "data": "title", "name": "title" },
            { "data": "image", "name": "image" },
            { "data": "view", "name": "view" },
            { "data": "page", "name": "page" },
            { "data": "status", "name": "status" },
            { "data": "language_id", "name": "language_id" },
            { "data": "Action", "name": "non" }
        ],
        "columnDefs": [{ "width": "50px", "targets": [0, 1] }, { "width": "40px", "targets": [2] },
        {
            "targets": [6], "data": null,
            "orderable": false,
        },
        {
            "targets": [5, 6],
            "data": null,
            "orderable": true,
            "render": function (data, type, full, meta) {
                if (meta.col == "5") {
                    if (data == "1")
                        return "English";
                    else if (data == "2")
                        return "French";
                    else
                        return "";
                }
                else {
                    var viewUrl = "'/PromoImages/Details/" + full.guid + "'";
                    var editUrl = "'/PromoImages/Edit/" + full.guid + "'";
                    var deleteUrl = "'/PromoImages/Delete/" + full.guid + "'";

                    var actionUrl = '<div style="font-family: cursive;display: inline-flex; white-space:normal;overflow:hidden;"> \n'
                    actionUrl = actionUrl + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-primary" onclick="editPromoImageData(' + editUrl + ')" title = "Edit Promo Image Banner"><i class="pe-7s-pen btn-icon-wrapper"></i></button>'
                        + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-warning" onclick="viewPromoImageData(' + viewUrl + ')" title = "View Promo Image Banner"><i class="pe-7s-look btn-icon-wrapper"></i></button>'
                        + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-danger" onclick="deletePromoImageData(' + deleteUrl + ')" title = "Delete Promo Image Banner"><i class="pe-7s-trash btn-icon-wrapper"></i></button>'

                    '</div ></td > '

                    return actionUrl;
                }
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

function viewPromoImageData(url) {
    var arrUrl = url.split("/");
    var guid = arrUrl[3];
    $.ajax({
        type: "Get",
        url: url,
        data: { guid: guid },
        success: function (data) {
            $('#modalCMS').html(data); //create this modal div in layout.
            $('#modalCMS').modal('show');

            //mask();
            //RentAdjustmentYearsAfterStart_OnChange();
            //dateObj.query(".date").datepicker({
            //    changeMonth: true,
            //    dateFormat: DateFormat,
            //    autoclose: true
            //});
            //dateObj.query(".Calendericon").click(function () {
            //    $(this).prev().focus();
            //});
        }
    });
}

function AddNewItem() {
    var url = "/PromoImages/Create";
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
            titleOnChange();
        }
    });
}

function editPromoImageData(url) {
    var arrUrl = url.split("/");
    var guid = arrUrl[3];
    $.ajax({
        type: "Get",
        url: url,
        data: { guid: guid },
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');
            btnPublishToggle(false);
            formValidationRules();
            titleOnChange();
        }
    });
}

function deletePromoImageData(url) {
    //$('#exampleModal').modal('show');
    //$('#exampleModal').on('shown.bs.modal', function () {
    //    alert("delete?");
    //})
    var arrUrl = url.split("/");
    var guid = arrUrl[3];
    if (confirm("Do you want to delete the Promo Image?")) {
        this.click;
        //alert("Ok");
        $.ajax({
            type: "Get",
            url: url,
            data: { guid: guid },
            success: function (data) {
                if (data != null) {

                    if (data.data.isSuccessfull === true && data.data.formMode === "Delete") {
                        getPromoImageData('/PromoImages/PromoImageList');
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
            },
            cache: false,
            contentType: false,
            processData: false

        });
    }
    else {
        //alert("Cancel");
        toastr.error("Promo Image Deleted Query Cancel.");
    }
    event.preventDefault();
}

function savePromoImage() {
    if ($('#formPromoImage').valid()) {
        var url = "";
        var formData = new FormData($('#formPromoImage')[0]);
        url = $('#formPromoImage').attr('action');

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
                    if (data.data.language === 'English') {
                        var staggingURL = $("#hdnStaggingDomain").val() + "en/promos/" + $('#url_key').val();
                        window.open(staggingURL, "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                    }
                    else {
                        var staggingURL = $("#hdnStaggingDomain").val() + "fr/promos/" + $('#url_key').val();
                        window.open(staggingURL, "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                    }
                    document.getElementById("btnSave").disabled = true;
                    if (data.data.isEmpty === true)
                        btnPublishToggle(true);
                    else
                        btnPublishToggle(false);
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

function updatePromoImage() {
    if ($("#formPromoImage").valid()) {
        var formData = new FormData($('#formPromoImage')[0]);
        var url = "/PromoImages/Edit";
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
                        if (data.data.language === 'English') {
                            var staggingURL = $("#hdnStaggingDomain").val() + "en/promos/" + $('#url_key').val();
                            window.open(staggingURL, "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }
                        else {
                            var staggingURL = $("#hdnStaggingDomain").val() + "fr/promos/" + $('#url_key').val();
                            window.open(staggingURL, "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }
                        if (data.data.isEmpty === true)
                            btnPublishToggle(true);
                        else
                            btnPublishToggle(false);
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

function publishPromoImage() {
    var url = "/PromoImages/Publish";

    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            if (data != null) {
                if (data.data.isSuccessfull === true) {
                    toastr.success(data.data.message);
                    getPromoImageData('/PromoImages/PromoImageList');
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

function btnPublishToggle(flag) {
    if (flag == true)
        $('#btnPublish').prop('disabled', false);
    else if (flag == false)
        $('#btnPublish').prop('disabled', true);
    //document.getElementById("btnPublish").disabled = true;
}

function closeModal() {
    $('#modalCMS').modal('hide');
    getPromoImageData('/PromoImages/PromoImageList');
}

function formValidationRules() {

    var imageFile = "";
    if (formModeCheck === "Edit") {
        imageFile = null;
    }
    else {
        imageFile = "required";
    }

    $("#formPromoImage").validate({

        rules: {
            imageFile: imageFile,
            storeNum: "required",
            title: "required",
            imageHyperLink: "required"
        },
        messages: {
            imageFile: { required: "Please Select the Image" },

            storeNum: { required: "Please Enter Store Number" },

            title: { required: "Please Enter Title" },

            imageHyperLink: { required: "Please Enter the Hyper Link" }
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

var clone = {};

function FileOnClick(event) {
    var fileElement = event.target;
    if (fileElement.value != "") {
        clone[fileElement.id] = $(fileElement).clone();
    }
}


function validateFileResolution(file, event) {

    var fileElement = event.target;
    if (fileElement.value == "") {
        clone[fileElement.id].insertBefore(fileElement);
        $(fileElement).remove();
    }

    //var fileName = file.files[0].name;
    var viewDevice = document.getElementById("viewDevice").value;
    var reader = new FileReader();

    //Read the contents of Image File.
    reader.readAsDataURL(file.files[0]);
    reader.onload = function (e) {

        //Initiate the JavaScript Image object.
        var image = new Image();

        //Set the Base64 string return from FileReader as source.
        image.src = e.target.result;

        //Validate the File Height and Width.
        image.onload = function () {
            var height = this.height;
            var width = this.width;

            var url = '/PromoImages/ValidateFileResolution';
            $.ajax({
                type: 'POST',
                url: url,
                async: false,
                data: { 'width': width, 'height': height, 'viewDevice': viewDevice },
                success: function (data) {
                    if (document.getElementById("btnSave") != null) {
                        document.getElementById("btnSave").disabled = false;
                    }
                    if (document.getElementById("btnUpdate") != null) {
                        document.getElementById("btnUpdate").disabled = false;
                    }
                    if (data.data.isSuccessful === true) {
                        toastr.success(data.data.message);
                        if (data.data.formMode === "Edit")
                            document.getElementById("btnUpdate").disabled = false;
                        else
                            document.getElementById("btnSave").disabled = false;
                    }
                    else if (data.data.isSuccessful === false) {
                        toastr.error(data.data.message);
                        if (data.data.formMode === "Edit")
                            document.getElementById("btnUpdate").disabled = true;
                        else
                            document.getElementById("btnSave").disabled = true;
                    }
                }
            });
        };
    };
}

function toggleHintText() {
    var option = document.getElementById("viewDevice").value;
    if (option == "Desktop") {

        document.getElementById("txtImageHint").innerText = "Please select an image of Resolution " + dwidthSize + " px x " + dheightSize + " px.";

    }
    else if (option == "Mobile") {

        document.getElementById("txtImageHint").innerText = "Please select an image of Resolution " + mwidthSize + " px x " + mheightSize + " px.";

    }
}

function titleOnChange() {
    if ($('#url_key').val() != "") {
        var option = document.getElementById("language").value;

        if (option == "English") {
            var staggingURL = $("#hdnStaggingDomain").val() + "en/promos/" + $('#url_key').val();
            var prodURL = $("#hdnProdDomain").val() + "en/promos/" + $('#url_key').val();

            $("#dvStagingPromoUrl").attr("href", staggingURL);
            $("#dvStagingPromoUrl").text(staggingURL);

            $("#dvProductionPromoUrl").attr("href", prodURL);
            $("#dvProductionPromoUrl").text(prodURL);
            $('#dvPromoUrls').show();
        }
        else if (option == "French") {
            var staggingURL = $("#hdnStaggingDomain").val() + "fr/promos/" + $('#url_key').val();
            var prodURL = $("#hdnProdDomain").val() + "fr/promos/" + $('#url_key').val();

            $("#dvStagingPromoUrl").attr("href", staggingURL);
            $("#dvStagingPromoUrl").text(staggingURL);

            $("#dvProductionPromoUrl").attr("href", prodURL);
            $("#dvProductionPromoUrl").text(prodURL);
            $('#dvPromoUrls').show();
        }

    }
    else {
        $('#dvPromoUrls').hide();
    }
}