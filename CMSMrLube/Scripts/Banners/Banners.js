$(document).ready(function () {
    $("#bannerupload").validate({
        rules: {
            bannerFile: "required",
            BannerTitle: "required",
            ImageName: "required",
            statusSelect: "required"
        },
        messages: {
            bannerFile: "Please upload your File",
            BannerTitle: "Please enter your Title",

            agree: "Please select Status"
        },
        errorElement: "em",
        errorPlacement: function (t, e) {
            t.addClass("invalid-feedback"), "checkbox" === e.prop("type") ? t.insertAfter(e.next("label")) : t.insertAfter(e),
                t.addClass("invalid-feedback"), "file" === e.prop("type") ? t.insertAfter(e.next("label")) : t.insertAfter(e)
        },
        //highlight: function(e, i, n) {
        //    t(e).addClass("is-invalid").removeClass("is-valid")
        //},
        //unhighlight: function(e, i, n) {
        //    t(e).addClass("is-valid").removeClass("is-invalid")
        //}
    })

    //$('#bannerTable').DataTable({
    //    pageLength: 10,
    //    filter: true,
    //    deferRender: true,
    //    scrollY: 200,
    //    scrollCollapse: true,
    //    scroller: true
    //})

    //datatable column

    var Status = $("#Status").val();
    var View = $("#Views").val();
    var LastUser = $("#LastUser").val();

    var table = $('#bannerTable').DataTable({
        "ajax": {
            "url": "Home/BannerList",
            "type": "get",
            "data": { Title: Title, Image: Image, View: View, Status: Status },
            "datatype": "json",
        },
        "columns": [

            { "data": "Title", "name": "Title" },
            { "data": "Image", "name": "Image" },
            { "data": "View", "name": "View" },
            { "data": "Status", "name": "Status" },
            { "data": "HyperLink", "name": "HyperLink" },
            { "data": "Action", "name": "non" }
        ],
        "columnDefs": [
            {
                "targets": 5,
                "data": null,
                "orderable": true,
                "render": function (data, type, full, meta) {
                    var viewUrl = "'/Home/Detail/" + full.BannerId + "'";
                    var editUrl = "'/Home/Edit/" + full.BannerId + "'";


                    //var CloseStoreUrl = "'/Store/CloseStore/" + full.StoreNumber + "'";

                    var editurl = '<div class="dropdown" style="text-align: center;"> \n'
                    if (full.View) {
                        editurl = editurl + '<a href="/Home/' + full.BannerId + '" class="view_banner" title="View Banner"> <span class="icon-wrapper icon-wrapper-alt rounded-circle"> <i class="pe-7s-look text-focus opacity-7 btn-icon-wrapper"> </i> </span> </a> \n'
                    }
                    if (full.Edit) {
                        editurl = editurl + '<a href = "javascript:" onclick="Banner(' + editUrl + ')" class="view_store" title = "Edit Banner" > <span class="icon-wrapper icon-wrapper-alt rounded-circle"> <i class="text-focus opacity-7 btn-icon-wrapper pe-7s-pen"> </i> </span> </a > \n'
                    }

                    '</div ></td > '


                    return editurl;
                }
            },
        ], "initComplete": function (data, json) {
            $(".loader").hide();
        },
        "responsive": "true",
        "serverSide": "true",
        "order": [0, "desc"],

        /// "processing": "true",
        "destroy": "true",
        "searching": "false",
        "language": {
            //"processing": 'Processing- please wait'

        }
    });

});




//function BannerTable(url) {
//    getbanner(url)
//    $('#StoreTable_length').hide();
//    $('#StoreTable_filter').hide();

//}


//function getbanner(url) {

//    $(".loader").show();
//    var Status = $("#Status").val();
//    var Views = $("#Views").val();
//    var LastUser = $("#LastUser").val();

//    var table = $('#bannerTable').DataTable({
//        "ajax": {
//            "url": url,
//            "type": "get",
//            "data": { Title: Title, Image: Image, Views: Views, Status: Status },
//            "datatype": "json",
//        },
//        "columns": [

//            { "data": "Title", "name": "Title" },
//            { "data": "Image", "name": "Image" },
//            { "data": "Views", "name": "Views" },
//            { "data": "Status", "name": "Status" },
//            { "data": "HyperLink", "name": "HyperLink" },
//            { "data": "Action", "name": "non" }
//        ],
//        "columnDefs": [
//            {
//                "targets": 5,
//                "data": null,
//                "orderable": true,
//                "render": function (data, type, full, meta) {
//                    var viewUrl = "'/Home/Detail/" + full.BannerId + "'";
//                    var editUrl = "'/Home/Edit/" + full.BannerId + "'";


//                    //var CloseStoreUrl = "'/Store/CloseStore/" + full.StoreNumber + "'";

//                    var editurl = '<div class="dropdown" style="text-align: center;"> \n'
//                    if (full.View) {
//                        editurl = editurl + '<a href="/Store/' + full.BannerId + '" class="view_banner" title="View Banner"> <span class="icon-wrapper icon-wrapper-alt rounded-circle"> <i class="pe-7s-look text-focus opacity-7 btn-icon-wrapper"> </i> </span> </a> \n'
//                    }
//                    if (full.Edit) {
//                        editurl = editurl + '<a href = "javascript:" onclick="Banner(' + editUrl + ')" class="view_store" title = "Edit Banner" > <span class="icon-wrapper icon-wrapper-alt rounded-circle"> <i class="text-focus opacity-7 btn-icon-wrapper pe-7s-pen"> </i> </span> </a > \n'
//                    }

//                    '</div ></td > '


//                    return editurl;
//                }
//            },
//        ], "initComplete": function (data, json) {
//            $(".loader").hide();
//        },
//        "responsive": "true",
//        "serverSide": "true",
//        "order": [0, "desc"],

//        /// "processing": "true",
//        "destroy": "true",
//        "searching": "false",
//        "language": {
//            //"processing": 'Processing- please wait'

//        }
//    });
//}