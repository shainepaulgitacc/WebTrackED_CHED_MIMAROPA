$(function () {
    $("#select-all").on("change", function () {
        $(".checkbox-item").prop("checked", $(this).prop("checked"));
    })
    $(".checkbox-item").on("change", function () {
        if (!$(this).prop("checked")) {
            $("#select-all").prop("checked", false);
        }
    })
    $("#delete-all").on("click", function () {
        let selected_ids = [];
        $(".checkbox-item").each(function () {
            if ($(this).is(":checked")) {
                selected_ids.push($(this).val())
            }
        })
        let final_selected_ids = selected_ids.join(",");
        $("#selected-id").val(final_selected_ids);
        $("#form-delete-all").submit();
    })
})