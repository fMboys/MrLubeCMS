//$(document).ready(function () {
//    $('#bannerTable').DataTable();
//    $('#bannerTable_length').hide();
//    $('#bannerTable').hide();
//    bannerTable('/Banner/BannerList');
//});

//function BannerList() {
//    $('#bannerTable').DataTable();
//    $('#bannerTable_length').hide();
//    $('#bannerTable').hide();
//    bannerTable('/Banner/BannerList');
//}

//function bannerTable(url) {
//    getbanners(url);
//    $('#bannerTable_length').hide();
//    $('#bannerTable_filter').hide();

//}

//$('#bannerTable').DataTable({
//    pageLength: 10,
//    filter: true,
//    deferRender: true,
//    scrollY: 200,
//    scrollCollapse: true,
//    scroller: true
//})

function toggleHintText() {
    //debugger;
    

    var option = document.getElementById("viewSelect").value;

    if (option == "Desktop") {
        
            
            document.getElementById("txtImageHint").innerText = "Please select an image of Resolution " + dwidthSize + " px x " + dheightSize + " px.";
        
    }
    else if (option == "Mobile") {
        
            document.getElementById("txtImageHint").innerText = "Please select an image of Resolution " + mwidthSize + " px x " + mheightSize + " px.";
       
    }
}

//datatable column
var title = "";
var image = "";
var view = "";
var Status = "";
var ad_hyperlink = "";
var url = "/Banner/BannerList";
function getbanners(url) {


    $('#bannerTable').DataTable({
        "ajax": {
            "url": url,
            "type": "Post",
            "data": { title: title, image: image, view: view, Status: Status, ad_hyperlink: ad_hyperlink },
            "datatype": "json",
        },
        "columns": [

            { "data": "banner_id", "name": "banner_id" },
            { "data": "title", "name": "title" },
            { "data": "image", "name": "image" },
            { "data": "view", "name": "view" },
            { "data": "status", "name": "status" },
            { "data": "ad_hyperlink", "name": "ad_hyperlink" },
            { "data": "Action", "name": "non" }
        ],
        "columnDefs": [
            {
                "targets": 6,
                "data": null,
                "orderable": false,
                "render": function (data, type, full, meta) {

                    var viewUrl = "'/Banner/Detail/" + full.guid + "'";
                    var editUrl = "'/Banner/Edit/" + full.guid + "'";
                    var delUrl = "'/Banner/Delete/" + full.guid + "'";
                    var edit = '<div style="font-family: cursive;display: inline-flex; white-space:normal;overflow:hidden;"> \n'
                    edit = edit + '<button class="mb-2 mr-2 btn-icon btn-icon-only btn btn-primary" onclick="BannerEdit(' + full.guid + ')" title = "Edit Banner"><i class="pe-7s-pen btn-icon-wrapper"></i></button>'
                        + '<button class="mb-2 mr-2 btn-icon btn-icon-only btn btn-warning" onclick="BannerView(' + full.guid + ')" title = "View Banner"><i class="pe-7s-look btn-icon-wrapper"></i></button>'
                        + '<button class="mb-2 mr-2 btn-icon btn-icon-only btn btn-danger" onclick="BannerDel(' + full.guid + ')" title = "Delete Banner"><i class="pe-7s-trash btn-icon-wrapper"></i></button>'

                    //var editurl = '<div class="dropdown" style="text-align: center;"> \n'
                    //if (full.View) {
                    //    editurl = editurl + '<a href="/Banner/' + full.BannerId + '" class="view_banner" title="View Banner"> <span class="icon-wrapper icon-wrapper-alt rounded-circle"> <i class="pe-7s-look text-focus opacity-7 btn-icon-wrapper"> </i> </span> </a> \n'
                    //}
                    //if (full.Edit) {
                    //    editurl = editurl + '<a href = "javascript:" onclick="Banner(' + editUrl + ')" class="view_banner" title = "Edit Banner" > <span class="icon-wrapper icon-wrapper-alt rounded-circle"> <i class="text-focus opacity-7 btn-icon-wrapper pe-7s-pen"> </i> </span> </a > \n'
                    //}

                    '</div ></td > '


                    return edit;
                }
            },
        ],
        //"initComplete": function(data, json) {
        //    //$(".loader").hide();
        //},
        "responsive": "true",
        "serverSide": "true",
        "order": [0, "desc"],
        //"searchDelay": 350,
        //"filter": "true",
        /// "processing": "true",
        "destroy": "true",
        "searching": "false",
        "language": {
            //"processing": 'Processing- please wait'

        }
    });
}

function Clear() {
    $("#ddlBannerId").val("");
    $("#ddlBannerImage").val("");
    $("#BannerView").val("");
    $("#BannerStatus").val("");
    $("#BannerLanguage").val("");
}

function btnPublish() {

    //var formData = new FormData($('#bannerupload')[0]);
    var url = "/Banner/Publish";
    //var img = "imgdata.BannerImage";
    $.ajax({
        type: "Get",
        url: url,
        //data: formData,
        success: function (data) {
            if (data != null) {
                if (data.data.isSuccessfull === true) {

                    toastr.success(data.data.message);
                    $('#modalCMS').modal('hide');
                    getBannerList();
                }
                else {
                    toastr.error(data.data.message);
                }
            }
            else {
                // $("#dvErrorAuth").html("FE: Something went wrong");
            }
        },
        error: function (exp) {
            alert(exp.responseText);
        }
    });
}

function NewBanner() {
    var url = "/Banner/Create";
    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            $('#modalCMS').html(data);
            $('#modalCMS').modal('show');
            publishDisableButton();

            //mask();
            //$('select').each(function () {
            //    if ($(this).hasClass('js-source-states')) {
            //        $(this).select2();
            //    }
            //});
            ValidateRules();
        }
    });
}

function saveBanner() {

    if ($('#bannerupload').valid()) {
        var url = "";
        var formData = new FormData($('#bannerupload')[0]);
        var urlEdit = $('#bannerupload').attr('action');
        var formMode = ('#formMode').value;
        var lang = document.getElementById("languageSelect").value;
        if (formMode === "Edit") {
            url = "/Banner/Edit" + formData;
        }
        else {
            url = $('#bannerupload').attr('action');
        }
        $.ajax({
            type: "POST",
            url: url,
            data: formData,
            success: function (data) {
                $.fn.modal.Constructor.prototype.enforceFocus = function () { };
                if (data.data != null) {
                    if (data.data.isSuccessfull === true && data.data.formMode === "Create") {
                        if (lang === "English") {
                            window.open($("#hdnStaggingDomain").val(), "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }
                        else {
                            window.open($("#hdnStaggingDomain").val() + "fr", "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }

                        toastr.success(data.data.message);
                        if (data.data.isEmpty === true) { publishEnableButton(); }

                        else { publishDisableButton(); }
                        //getBannerList();
                        //data.bannerimgdata = 'TempData["mainimg"]';
                        //window.open('https://mldev-01-staging.azurewebsites.net/', 'popup', 'width=600,height=600');
                    }
                    else if (data.data.formMode === "Edit" && data.data.isSuccessfull === true) {
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
}

function updateBanner() {

    if ($('#bannerupload').valid()) {

        var url = "";
        var formData = new FormData($('#bannerupload')[0]);
        //var urlEdit = $('#bannerupload').attr('action');
        var formMode = ('#formMode').value;
        var lang = document.getElementById("languageSelect").value;

        url = "/Banner/Edit";

        $.ajax({
            type: "POST",
            url: url,
            data: formData,
            success: function (data) {
                $.fn.modal.Constructor.prototype.enforceFocus = function () { };
                if (data != null) {
                    if (data.isSuccessfull === true && data.formMode === "Edit") {
                        toastr.success(data.message);
                        if (lang === "English") {
                            window.open($("#hdnStaggingDomain").val(), "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }
                        else {
                            window.open($("#hdnStaggingDomain").val() + "fr", "", 'scrollbars=yes,toolbar=yes,menubar=yes,width=700,height=800');
                        }

                        if (data.isEmpty === true) { publishEnableButton(); }

                        else { publishDisableButton(); }

                    }
                    //else if (data.formMode === "Create" && data.isSuccessfull === true) {
                    //    toastr.success(data.message);
                    //}
                    else {
                        toastr.error(data.message);
                    }
                }
                else {
                    // $("#dvErrorAuth").html("FE: Something went wrong");
                    toastr.error(data.message);
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
}


function closeModal() {
    $('#modalCMS').modal('hide');
    getBannerList();
}

function newPop(data) {
    var data = data;
    window.open($("#hdnStaggingDomain").val(), "popupWindow", "width=600,height=600,scrollbars=yes");
}

function publishHideButton() {
    $("#publish").hide();
}

function publishShowButton() {
    $("#publish").show();
}

function publishDisableButton() {
    $("#publish").prop("disabled", true);
}

function publishEnableButton() {
    $("#publish").prop("disabled", false);
}

function BannerEdit(id) {
    var url = id;
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
            publishDisableButton();
            ValidateRules();
        }
    });
}

function BannerView(id) {
    var url = id;
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
            publishDisableButton();
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
    var viewDevice = document.getElementById("viewSelect").value;
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
            /*var formMode = @'Html.Raw(Json.Encode(ViewBag.Title)';*/

            var url = '/Banner/ValidateFileResolution';
            $.ajax({
                type: 'POST',
                url: url,
                async: false,
                data: { 'width': width, 'height': height, 'viewDevice': viewDevice },
                success: function (data) {
                    if (data.data.isSuccessful === true) {
                        toastr.success(data.data.message);
                        if (formModeCheck === "Edit") {
                            document.getElementById("UpdateBanner").disabled = false;
                        }
                        else {
                            document.getElementById("SaveBanner").disabled = false;
                        }

                    }
                    else if (data.data.isSuccessful === false) {
                        toastr.error(data.data.message);
                        if (formModeCheck === "Edit") {
                            document.getElementById("UpdateBanner").disabled = true;
                        }
                        else {
                            document.getElementById("SaveBanner").disabled = true;
                        }
                    }
                }
            });
        };
    };
}


