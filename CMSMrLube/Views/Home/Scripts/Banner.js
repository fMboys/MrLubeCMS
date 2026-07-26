function NewBanner() {
    var url = "/Home/Create";
    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            $.fn.modal.Constructor.prototype.enforceFocus = function () { };
            $('#FranchiseMode').html(data);
            $('#FranchiseMode').modal('show');
            //mask();
            //$('select').each(function () {
            //    if ($(this).hasClass('js-source-states')) {
            //        $(this).select2();
            //    }
            //});
            //ValidateRules();
        }
    });
}