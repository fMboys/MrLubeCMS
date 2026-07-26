function btnPublishToggle(flag) {
    if (flag == true)
        $('#btnPublish').prop('disabled', false);
    else if (flag == false)
        $('#btnPublish').prop('disabled', true);
    //document.getElementById("btnPublish").disabled = true;
}

function closeModal() {
    $('#modalCMS').modal('hide');
}