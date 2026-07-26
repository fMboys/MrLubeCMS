function ValidateRules() {
    //var formModeCheck = '@ViewBag.Title';
    var imgfile = "";
    if (formModeCheck === "Edit") {
        imgfile = null;
    }
    else {
        imgfile= "required";
    }
    //jQuery.validator.addMethod("accept", function(value, element, param) {
    //    return value.match(new RegExp("/^[a-zA-Z0-9_\.%\+\-]+@@[a-zA-Z0-9\.\-]+\.[a-zA-Z]{2,}$/)");
    //}, 'please enter a valid email');
    $("#bannerupload").validate({
        
        rules: {
            
            imgfile: imgfile,
            BannerTitle: "required",
            Hyperlink: "required"
        },
        messages: {
            imgfile: { required: "Please Select the Image" },

            BannerTitle: { required: "Please Enter Banner Title" } ,
           
            Hyperlink: { required: "Please Enter the Hyper Link" }
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