$(function () {
    $('#tblShopTire').dataTable();
    //$('#tblShopTire_length').hide();
    //$('#tblShopTire_filter').hide();
    ShopTireDataTable('/ShopTire/ShopTireList/');
    choosen();
    //$("li.search-field").on("click", function () {
    //    alert("The paragraph was clicked.");
    //    var content = $(this).find('span').text().trim();
    //    $("#storeNumchoosen").val(content);
    //});
});

//$(document).ready(function () {
//    choosen();
//    clickclose();
//    //$("#storeNumchoosenselect_chosen").on("click", function () {
//    //    alert("The paragraph was clicked.");
//    //    var content = $(this).find('span').text().trim();
//    //    $("#storeNumchoosen").val(content);
//    //});
//    //var jQuery_1_7_0 = $.noConflict(true);
//    //jQuery_1_7_0(document).ready(function () {
//    //    jQuery_1_7_0.validator.addMethod("storeNumchoosenRequired", function (value, element, param) {
//    //        return true;
//    //    });
//    //});
//    //$.validator.setDefaults({ ignore: ":hidden:not(.chosen-select)" })
//    //    ShopTireDataTable('/ShopTire/ShopTireList');
//});
//$(".js-source-states").select2();
//$(".js-source-states-2").select2();

function applySelect2() {
    debugger;
    $("#storeNum").select2({
        placeholder: "Select a Value",
        theme: "bootstrap4",
        allowClear: true
    });
}

function ShopTireDataTable(url) {

    getShopTireData(url);
    //$('#tblShopTire_length').hide();
    //$('#tblShopTire_filter').hide();
}

function toggleHintText() {
    //debugger;
    var option = document.getElementById("viewDevice").value;

    if (option == "Desktop") {


        document.getElementById("txtImageHint").innerText = "Please select an image of Resolution " + dwidthSize + " px x " + dheightSize + " px.";

    }
    else if (option == "Mobile") {

        document.getElementById("txtImageHint").innerText = "Please select an image of Resolution " + mwidthSize + " px x " + mheightSize + " px.";

    }
}

function getShopTireData(url) {
    var stores = "";
    var title = "";
    var imageName = "";
    var viewDevice = "";
    var imageStatus = "";
    var languageId = "";
    //debugger;
    var table = $('#tblShopTire').DataTable({
        "ajax": {
            "url": url,
            "type": "Post",
            "data": { stores: stores, title: title, imageName: imageName, viewDevice: viewDevice, imageStatus: imageStatus, languageId: languageId },
            "datatype": "json",
        },
        autoWidth: false,
        "columns": [
            { "data": "stores", "name": "stores", "width": "15%" },
            { "data": "title", "name": "title", "width": "15%" },
            { "data": "imageName", "name": "imageName", "width": "15%" },
            { "data": "viewDevice", "name": "viewDevice", "width": "15%" },
            { "data": "imageStatus", "name": "imageStatus", "width": "15%" },
            { "data": "languageId", "name": "languageId", "width": "15%" },
            { "data": "Action", "name": "non", "width": "10%" }
        ],
        "columnDefs": [{
            "targets": 5, "data": null, "orderable": true,
            "render": function (data, meta) {
                if (data == "1")
                    return "English";
                else if (data == "2")
                    return "French";
                else return "No Data";
            }
        },
        {
            "targets": 6,
            "data": null,
            "orderable": false,
            "render": function (data, type, full, meta) {
                var viewUrl = "'/ShopTire/Details/" + full.guid + "'";
                var editUrl = "'/ShopTire/Edit/" + full.guid + "'";
                var deleteUrl = "'/ShopTire/Delete/" + full.guid + "'";

                var actionUrl = '<div style="font-family: cursive;display: inline-flex; white-space:normal;overflow:hidden;"> \n'
                actionUrl = actionUrl + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-primary" onclick="editShopTireData(' + editUrl + ')" title = "Edit ShopTire Banner"><i class="pe-7s-pen btn-icon-wrapper"></i></button>'
                    + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-warning" onclick="viewShopTireData(' + viewUrl + ')" title = "View ShopTire Banner"><i class="pe-7s-look btn-icon-wrapper"></i></button>'
                    + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-danger" onclick="deleteShopTireData(' + deleteUrl + ')" title = "Delete ShopTire Banner"><i class="pe-7s-trash btn-icon-wrapper"></i></button>'

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

function viewShopTireData(url) {
    var url = url;
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

function AddShopTireImage(lang, view) {
    var url = "/ShopTire/Create";
    $.ajax({
        type: "Get",
        url: url,
        data: { lang: lang, view: view },
        async: false,
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');


            choosen();
            
            $("#storeNumchoosenselect_chosen").on("click", function () {
                //var content = $(this).find('span').text().trim();
                var content = $("li.search-choice").find('span').text().trim();
                if (content === null || content === "") {
                    //var display = getElementById("id").style.display = null;
                    $("#storeNumError").css('display', 'block');
                    $("#storeNumError").val('message');
                }
                else {
                    $("#storeNumError").css('display', 'none');
                }
                //alert("The element was clicked.");
                //clickclose();
                var selectvalue = content;
                //clickclose();
            });

            $("a.search-choice-close").on("click", function () {
                //var content = $(this).find('span').text().trim();
                var content = $("li.search-choice").find('span').text().trim();
                if (content === null || content === "") {
                    //var display = getElementById("id").style.display = null;
                    $("#storeNumError").css('display', 'block');
                    $("#storeNumError").val('message');
                }
                else {
                    $("#storeNumError").css('display', 'none');
                }
                //alert("The element was clicked.");
                var selectvalue = content;
            });

            btnPublishToggle(false);
            formValidationRules();
        }
    });
}


function clickclose() {
    $("search-choice-close").on("click", function () {
        //var content = $(this).find('span').text().trim();
        var content = $("li.search-choice").find('span').text().trim();
        if (content === null || content === "") {
            //var display = getElementById("id").style.display = null;
            $("#storeNumError").css('display', 'block');
            $("#storeNumError").val('message');
        }
        else {
            $("#storeNumError").css('display', 'none');
        }
        //alert("The element was clicked.");
        var selectvalue = content;
    });
}

function editShopTireData(url) {
    var url = url;
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
            choosen();
            btnPublishToggle(false);
            formValidationRules();
        }
    });
}

function deleteShopTireData(url) {
    //$('#exampleModal').modal('show');
    //$('#exampleModal').on('shown.bs.modal', function () {
    //    alert("delete?");
    //})
    var url = url;
    var arrUrl = url.split("/");
    var guid = arrUrl[3];
    if (confirm("Do you want to delete the ShopTire Banner?")) {
        this.click;
        //alert("Ok");
        $.ajax({
            type: "Get",
            url: url,
            data: { guid: guid },
            success: function (data) {
                if (data != null) {

                    if (data.data.isSuccessfull === true && data.data.formMode === "Delete") {
                        getShopTireData('/ShopTire/ShopTireList');
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
        toastr.error("ShopTire Banner Deleted Query Cancel.");
    }
    event.preventDefault();
}

//var storeData = new "";

function saveShopTire() {

    //listsearch(storeData);
    //formValidationRules();

    var content = $("li.search-choice").find('span').text().trim();
    var errmsg = "Please Select the Store";
    if (content === null || content === "") {
        //var display = getElementById("id").style.display = null;
        $("#storeNumError").css('display', 'block');
        $("#storeNumError").val(errmsg);
    }
    else {
        $("#storeNumError").css('display', 'none');
    }

    if ($('#formShopTire').valid() && (content != null || content != "")) {
        var url = "";
        var formData = new FormData($('#formShopTire')[0]);
        //var urlEdit = $('#formShopTire').attr('action');
        //var formMode = $('#formMode').val();

        //if (formMode === "Edit") {
        //    url = "/ShopTire/Edit" + formData;
        //}
        //else {
        url = $('#formShopTire').attr('action');
        var lang = document.getElementById("language").value;
        //}


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
                        if (lang === "English") {
                            window.open($("#hdnStaggingDomain").val(), "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }
                        else {
                            window.open($("#hdnStaggingDomain").val() + "fr", "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }
                        //document.getElementById("btnSave").disabled = true;
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
}

function updateShopTire() {
    if ($("#formShopTire").valid()) {
        var formData = new FormData($('#formShopTire')[0]);
        //var url = $('#formShopTire').attr('action');
        var url = "/ShopTire/Edit";
        var lang = document.getElementById("language").value;
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
                        if (lang === "English") {
                            window.open($("#hdnStaggingDomain").val(), "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }
                        else {
                            window.open($("#hdnStaggingDomain").val() + "fr", "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
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

function publishShopTire() {
    var url = "/ShopTire/Publish";

    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            if (data != null) {
                if (data.data.isSuccessfull === true) {
                    toastr.success(data.data.message);
                    getShopTireData('/ShopTire/ShopTireList');
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
    var url = '/ShopTire/ShopTireList';
    $('#modalCMS').modal('hide');
    getShopTireData(url);
}

function formValidationRules() {

    var imageFile = "";
    var storeNumchoosen = "";
    var storeselect = "";
    var storeData = $("li.search-choice").find('span').text().trim();
    var storeNumchoosen = $("#storeNumchoosen").val(storeData);
    //datastore.addClass("is-valid").removeClass("is-invalid");

    if (formModeCheck === "Edit") {
        imageFile = null;
    }
    else {
        imageFile = "required";
    }

    if (storeData === "") {
        storeNumchoosen = "required";

    }
    else {
        storeNumchoosen = null;
        //var datastore = $("#storeNumchoosen").val(storeData);
        //if (datastore != "") {
        //    $("#storeNumchoosen").addClass("is-valid").removeClass("is-invalid");
        //    if ($('#storeNumchoosen') != "") {
        //        $.validator.setDefaults({ ignore: "#storeNumchoosen" });
        //        $("#storeNumchoosen").addClass("is-valid").removeClass("is-invalid");
        //        //$("#formShopTire").valid() == true
        //    }


        //}

    }
    //jQuery.validator.addMethod("storeNumchoosenRequired", function (value, element, param) {
    //    return true;
    //});
    //jQuery.validator.addMethod("accept", function(value, element, param) {
    //    return value.match(new RegExp("/^[a-zA-Z0-9_\.%\+\-]+@@[a-zA-Z0-9\.\-]+\.[a-zA-Z]{2,}$/)");
    //}, 'please enter a valid email');
    //jQuery.validator.addMethod("match", function (data, element) {
    //    if (data.IsStoreExist == true) {
    //        return false;
    //    }
    //    else {

    //        return true;
    //    }

    //}, "Please enter a valid email address.");
    // $.validator.setDefaults({ ignore: ":hidden:not(select)" });




    $("#formShopTire").validate({

        rules: {
            imageFile: imageFile,
            //store_num: "required",
            //storeNumchoosen: "required",
            // storeNumchoosenselect: "required",
            //storeNum: "required",
            storeTitle: "required",
            imageHyperLink: "required"
        },
        messages: {
            imageFile: { required: "Please Select the Image" },

            //store_num: {
            //    required: "Please select a Store Number"
            //},

            //  storeNumchoosenselect: {
            //      required: "Please select Store Numbers"
            //  },

            //storeNum: {
            //    required: "Please select a Store Number"
            //},

            storeTitle: { required: "Please Enter Store Title" },

            imageHyperLink: { required: "Please Enter the Hyper Link" }
        },
        errorElement: "em",
        errorPlacement: function (error, element) {
            error.addClass("invalid-feedback");
            if (element.prop("type") === "checkbox") {
                error.insertAfter(element.next("label"));
                if (element.prop("type") === "multiselect") {
                    error.insertAfter(element.next("label"));
                }
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
    //debugger;

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

            var url = '/ShopTire/ValidateFileResolution';
            $.ajax({
                type: 'POST',
                url: url,
                async: false,
                data: { 'width': width, 'height': height, 'viewDevice': viewDevice },
                success: function (data) {
                    if (data.data.isSuccessful === true) {
                        toastr.success(data.data.message);
                        if (formModeCheck === "Edit") {
                            document.getElementById("btnUpdate").disabled = false;
                        }
                        else {
                            document.getElementById("btnSave").disabled = false;
                        }

                    }
                    else if (data.data.isSuccessful === false) {
                        toastr.error(data.data.message);
                        if (formModeCheck === "Edit") {
                            document.getElementById("btnUpdate").disabled = true;
                        }
                        else {
                            document.getElementById("btnSave").disabled = true;
                        }

                    }
                }
            });
        };
    };
}


function choosen() {
    $("#storeNumchoosenselect").chosen({
        width: "100%",

        display: "block",
        width: "100%",
        height: "calc(2.25rem + 2px)",
        padding: "0.375rem 0.75rem",
        fontsize: "1rem",
        fontweight: "400",
        lineheight: "1.5",
        color: "#495057",
        backgroundcolor: "#fff",
        backgroundclip: "padding-box",
        border: "1px solid #ced4da",
        borderradius: "0.25rem"

    });
}

$("li.search-field").on("click", function () {
    var content = $(this).find('span').text().trim();
    $("#storeNumchoosen").val(content);
});

function listsearch(storeData) {

    var storeData = $("li.search-choice").find('span').text().trim();
    $("#storeNumchoosen").val(storeData);
}

$("li.search-choice").on("click", function () {
    var content = $(this).find('span').text().trim();
    $("#storeNumchoosen").val(content);
});

