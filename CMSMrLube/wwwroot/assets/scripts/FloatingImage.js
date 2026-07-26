$(function () {
    $('#tblFloatingImage').dataTable();
    FloatingImageDataTable('/FloatingImage/GetFloatingImagesList');
});

//$(document).ready(function () {

//    FloatingImageDataTable('/FloatingImage/GetFloatingImagesList');
//});

////Check-Uncheck for SelectAll checkbox - not in use or checked
$(".cbxMark").click(function () {
    //if (!this.checked) {
    //    $("#selectAll").attr('checked', false);
    //}
    //else
    if ($(".cbxMark").length == $(".cbxMark:checked").length) {
        $("#selectAll").attr('checked', true);
    }
    else if ($(".cbxMark").length != $(".cbxMark:checked").length) {
        $("#selectAll").attr('checked', false);
    }
});

//Check-Uncheck for SelectAll checkbox
function syncSelectAll() {
    if ($(".cbxMark").length == $(".cbxMark:checked").length) {
        document.getElementById("selectAll").checked = true;
        //$("#selectAll").attr('checked', true);
    }
    else if ($(".cbxMark").length != $(".cbxMark:checked").length) {
        document.getElementById("selectAll").checked = false;
        //$("#selectAll").attr('checked', false);
    }
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

//Check-Uncheck all Checkboxs using SelectAll checkbox
function toggleAll(elem) {
    if (elem.checked) {

        $(".cbxMark").attr('checked', true);

        //Unchecked the disabled checkboxes
        $("input[type=checkbox]").each(function () {
            if ($(this).prop('disabled'))
                $(this).attr('checked', false);
        });

        //$('.cbxMark').each(function () {
        //    document.getElementById("cbxPage").checked = !document.getElementById("cbxPage").checked;
        //});
    }
    else {
        $(".cbxMark").attr('checked', false);
    }
}

function checkActiveImagePages() {
    var index = document.getElementById("imageStatus").selectedIndex;

    if (index == 0) {
        var selectedPages = '';
        var ddlViewDevice = document.getElementById("viewDevice");
        var device = ddlViewDevice.value;
        var ddlLanguage = document.getElementById("language");
        var language = ddlLanguage.value;

        $('#listPages input:checked').each(function () {
            if (selectedPages == "") {
                selectedPages = $(this).val();
            }
            else {
                selectedPages += "," + $(this).val();
            }
        });

        if (selectedPages == "") {
            toastr.error("Please select atleast one page for floating image.");
            return;
        }

        var url = '/FloatingImage/CheckActiveImagePages';
        $.ajax({
            type: 'POST',
            url: url,
            async: false,
            data: { 'selectedPages': selectedPages, 'viewDevice': device, 'language': language },
            success: function (data) {
                if (data.data.isSuccessful === false) {
                    toastr.error(data.data.message);
                    if (document.getElementById("btnUpdate") != null)
                        document.getElementById("btnUpdate").disabled = true;
                    if (document.getElementById("btnSave") != null)
                        document.getElementById("btnSave").disabled = true;
                }
                else if (data.data.isSuccessful === true) {
                    toastr.success(data.data.message);
                    if (document.getElementById("btnUpdate") != null)
                        document.getElementById("btnUpdate").disabled = false;
                    if (document.getElementById("btnSave") != null)
                        document.getElementById("btnSave").disabled = false;
                }
            }
        });
    }
    else if (index == 1) {
        if (document.getElementById("btnUpdate") != null)
            document.getElementById("btnUpdate").disabled = false;
        if (document.getElementById("btnSave") != null)
            document.getElementById("btnSave").disabled = false;
        toastr.warning("You're about to deactive the image for selected pages.");
        return;
    }
}

function FloatingImageDataTable(url) {
    getFloatingImagesData(url);
    //$('#tblFloatingImage_length').hide();
    //$('#tblFloatingImage_filter').hide();
}

function closeModal() {
    $('#modalCMS').modal('hide');
    getFloatingImagesData('/FloatingImage/GetFloatingImagesList');
}

function getFloatingImagesData(url) {
    //debugger;
    var imageName = "";
    var title = "";
    var viewDevice = "";
    var language = "";
    var imageStatus = "";

    var table = $('#tblFloatingImage').DataTable({
        "ajax": {
            "url": url,
            "type": "POST",
            "data": { title: title, image: imageName, viewDevice: viewDevice, language: language, imageStatus: imageStatus },
            "datatype": "json"
        },
        "columns": [
            { "data": "title", "name": "title", "width": "20%" },
            { "data": "image", "name": "image", "width": "25%" },
            { "data": "view", "name": "view", "width": "10%" },
            { "data": "language_id", "name": "language_id", "width": "15%" },
            { "data": "status", "name": "status", "width": "10%" },
            { "data": "Action", "name": "non", "width": "15%" }
        ],
        "columnDefs": [
            {
                "targets": 3, "data": null, "orderable": true,
                "render": function (data, meta) {
                    if (data == "1")
                        return "English";
                    else if (data == "2")
                        return "French";
                    else return "No Data";
                }
            },
            {
                "targets": 5,
                "data": null,
                "orderable": false,
                "render": function (data, type, full, meta) {
                    var viewUrl = "'/FloatingImage/Details/" + full.id + "'";
                    var editUrl = "'/FloatingImage/Edit/" + full.id + "'";
                    var deleteUrl = "'/FloatingImage/Delete/" + full.id + "'";
                    //var viewUrl = "'/FloatingImage/Details/" + full.guid + "'";
                    //var editUrl = "'/FloatingImage/Edit/" + full.guid + "'";
                    //var deleteUrl = "'/FloatingImage/Delete/" + full.guid + "'";

                    var actionUrl = '<div style="font-family: cursive;display: inline-flex; white-space:normal;overflow:hidden;"> \n'
                    actionUrl = actionUrl + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-primary" onclick="editFloatingImageData(' + editUrl + ')" title = "Edit Floating Image"><i class="pe-7s-pen btn-icon-wrapper"></i></button>'
                        + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-warning" onclick="viewFloatingImageDetails(' + viewUrl + ')" title = "View Floating Image"><i class="pe-7s-look btn-icon-wrapper"></i></button>'
                        + '<button class="mb-0 mr-2 btn-icon btn-icon-only btn btn-danger" onclick="deleteFloatingImage(' + deleteUrl + ')" title = "Delete Floating Image"><i class="pe-7s-trash btn-icon-wrapper"></i></button>'

                    '</div ></td > '

                    return actionUrl;
                }
            }
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

function AddFloatingImage(lang, view) {

    var url = "/FloatingImage/Create/";
    $.ajax({
        type: 'GET',
        url: url,
        data: { lang: lang, view: view },
        async: false,
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');
            btnPublishToggle(false);
            formValidation();
            //Todo check validation for checkboxs
            //if ($('div.checkbox-group.required :checkbox:checked').length <= 0) {
            //    document.getElementById("lblCbxValidate").style.display = 'block';
            //    document.getElementById("lblCbxValidate").innerText = 'Please select Page(s) for Floating Image.';
            //}
            //else if ($('div.checkbox-group.required :checkbox:checked').length > 0)
            //    document.getElementById("lblCbxValidate").style.display = 'none';
        }
    });
}

function editFloatingImageData(url) {
    var url = url;
    //var arrUrl = url.split('/');
    //var guid = arrUrl[3];
    $.ajax({
        type: "Get",
        url: url,
        //data: { guid: guid },
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');
            btnPublishToggle(false);
            formValidation();
        }
    });
}

function viewFloatingImageDetails(url) {
    var url = url;
    //var arrUrl = url.split('/');
    //var guid = arrUrl[3];
    $.ajax({
        type: "GET",
        url: url,
        //data: { guid: guid },
        success: function (data) {
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');
        }
    });
}

function deleteFloatingImage(url) {
    //$('#exampleModal').modal('show');
    //$('#exampleModal').on('shown.bs.modal', function () {
    //    alert("delete?");
    //})
    var url = url;

    if (confirm("Do you want to delete the Floating Image?")) {
        this.click;

        //var arrUrl = url.split('/');
        //var guid = arrUrl[3];
        $.ajax({
            type: "Get",
            url: url,
            //data: { guid: guid },
            success: function (data) {
                if (data != null) {
                    if (data.data.isSuccessfull === true) {
                        getFloatingImagesData('/FloatingImage/GetFloatingImagesList');
                        toastr.success(data.data.message);
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
            }
        });
    }
    else {
        //alert("Cancel");
        toastr.error("Floating Image Delete Query Cancel.");
    }
    event.preventDefault();
}

function saveFloatingImage() {
    if ($('#formFloatingImage').valid()) {
        var selectedPages = '';

        $('#listPages input:checked').each(function () {
            if (selectedPages == "") {
                selectedPages = $(this).val();
            }
            else {
                selectedPages += "," + $(this).val();
            }
        });

        if (selectedPages == "") {
            toastr.error("Please select atleast one page for floating image.");
            return;
        }

        $('#selectedPages').val(selectedPages);
        var formData = new FormData($('#formFloatingImage')[0]);

        var url = $('#formFloatingImage').attr('action');
        $.ajax({
            url: url,
            type: "POST",
            data: formData,
            async: false,
            success: function (data) {
                if (data.data.isSuccessful === true) {
                    toastr.success(data.data.message);
                    if (data.data.language === "English") {
                        window.open($("#hdnStaggingDomain").val(), "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                    }
                    else {
                        window.open($("#hdnStaggingDomain").val()+"fr", "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                    }
                    document.getElementById("btnSave").disabled = true;
                    FloatingImageDataTable('/FloatingImage/GetFloatingImagesList');//Refresh datatable list
                    if (data.data.isEmpty === true)
                        btnPublishToggle(true);
                    else
                        btnPublishToggle(false);
                    //ResetValidateForm();
                    //StoreTable('/store/StoreList')
                }
                else if (data.data.isSuccessful === false) {
                    toastr.error(data.data.message);
                    //NothingChanged();
                    //ResetValidateForm();
                }
                else {
                    toastr.error(data.data.message);
                    //$('#StoreMode').html(data);
                    //$('#StoreMode').modal('show');
                    //mask();
                }
            },
            cache: false,
            contentType: false,
            processData: false
        });
    }
    else {
        toastr.error("Please fill all required fields.");
    }
}

function updateFloatingImage() {
    if ($("#formFloatingImage").valid()) {
        var selectedPages = '';

        $('#listPages input:checked').each(function () {
            if (selectedPages == "") {
                selectedPages = $(this).val();
            }
            else {
                selectedPages += "," + $(this).val();
            }
        });

        if (selectedPages == "") {
            toastr.error("Please select atleast one page for floating image.");
            return;
        }

        $('#selectedPages').val(selectedPages);

        var formData = new FormData($('#formFloatingImage')[0]);
        var url = "/FloatingImage/Edit";

        $.ajax({
            url: url,
            type: "POST",
            data: formData,
            async: false,
            success: function (data) {
                //$.fn.modal.Constructor.prototype.enforceFocus = function () { };
                if (data != null) {
                    if (data.data.isUpdate === true) {
                        toastr.success(data.data.message);
                        if (data.data.language === "English") {
                            window.open($("#hdnStaggingDomain").val(), "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }
                        else {
                            window.open($("#hdnStaggingDomain").val() +"fr", "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
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

function publishFloatingImage() {
    var url = "/FloatingImage/Publish";

    $.ajax({
        type: "get",
        url: url,
        success: function (data) {
            if (data != null) {
                if (data.data.isSuccessfull === true) {
                    toastr.success(data.data.message);
                    getFloatingImagesData('/FloatingImage/GetFloatingImagesList');
                    $('#modalCMS').modal('hide');
                    //btnPublishToggle(false);
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
    //debugger;
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

            var url = '/FloatingImage/ValidateFileResolution';
            $.ajax({
                type: 'POST',
                url: url,
                async: false,
                data: { 'width': width, 'height': height, 'viewDevice': viewDevice },
                success: function (data) {
                    if (data.data.isSuccessful === true) {
                        toastr.success(data.data.message);
                        if (document.getElementById("btnSave") != null)
                            document.getElementById("btnSave").disabled = false;
                        else
                            document.getElementById("btnUpdate").disabled = false;
                    }
                    else if (data.data.isSuccessful === false) {
                        toastr.error(data.data.message);
                        if (document.getElementById("btnSave") != null)
                            document.getElementById("btnSave").disabled = true;
                        else
                            document.getElementById("btnUpdate").disabled = true;
                    }
                }
            });
        };
    };
}

function btnPublishToggle(flag) {
    if (flag == true)
        $('#btnPublish').prop('disabled', false);
    else if (flag == false)
        $('#btnPublish').prop('disabled', true);
}

//TODO
function formValidation() {
    var imageFile = "";
    if (formModeCheck === "Edit") {
        imageFile = null;
    }
    else {
        imageFile = "required";
    }
    //jQuery.validator.addMethod("accept", function(value, element, param) {
    //    return value.match(new RegExp("/^[a-zA-Z0-9_\.%\+\-]+@@[a-zA-Z0-9\.\-]+\.[a-zA-Z]{2,}$/)");
    //}, 'please enter a valid email');
    $("#formFloatingImage").validate({

        ignore: [],  // ignore NOTHING
        rules: {
            imageFile: imageFile,
            titleImage: "required",
            imageHyperLink: "required"
        },
        messages: {
            imageFile: { required: "Please Select the Image" },

            titleImage: { required: "Please Enter Image Title" },

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