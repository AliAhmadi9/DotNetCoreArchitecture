$body = $("body");

$(document).on({
    ajaxStart: function () { $body.addClass("loading"); },
    ajaxStop: function () { $body.removeClass("loading"); }
});

function getPosition(string, subString, index) {
    return string.split(subString, index).join(subString).length;
}

function getMessageOfException(message) {
    return message.substring(parseInt(getPosition(message, `:`, 1)) + 2, parseInt(getPosition(message, `,`, 1)) - 1);
}

function onlyNumberKey(evt) {

    const ASCIICode = (evt.which) ? evt.which : evt.keyCode;
    if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57))
        return false;
    return true;
}

function onlyNumberKeyWithDot(element, evt) {

    const ASCIICode = (evt.which) ? evt.which : evt.keyCode;

    if (element.value.includes(".") && ASCIICode === 46) return false;

    if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57) && ASCIICode !== 46) return false;

    return true;
}

function filterTable(inputId, tableId) {
    $(`#${inputId}`).on("keyup", function () {
        var value = $(this).val().toLowerCase();
        $(`#${tableId} tr`).filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });
}

function changeWorkspace(workspaceId) {

    $.ajax({
        type: "GET",
        url: `/api/v1/Workspace/ChangeWorkspace?workspaceId=${workspaceId}`,
        success: function (result) {

            if (result.isSuccess && result.data) {
                location.reload();
            }
        }
    });
}

function removeItemAllInArray(arr, value) {
    var i = 0;
    while (i < arr.length) {
        if (arr[i] === value) {
            arr.splice(i, 1);
        } else {
            ++i;
        }
    }
    return arr;
}

function setWorkspaceSetting() {

    $.ajax({
        type: "GET",
        url: `/api/v1/Workspace/CurrentWorkspace`,
        success: function (result) {

            if (result.isSuccess) {

                const setting = result.data.workspaceSetting;

                if (setting == null)
                    return;

                $(`#${setting.themeColor}`).click();
                $(`#${setting.topStripType}`).click();
                $(`#${setting.footerType}`).click();
                $("#icon-animation-switch").prop("checked", setting.animation);
                $("#collapse-sidebar-switch").prop("checked", setting.menuClose);
                $("#card-shadow-switch").prop("checked", setting.cardShadow);
                $("#hide-scroll-top-switch").prop("checked", setting.hideScrollToTop);
                $(`#${setting.menuColor}`).addClass("selected");
                $(`#${setting.topStripColor}`).addClass("selected");
            }
        }
    });
}

$(document).ready(function () {

    //setWorkspaceSetting();

});

function changeWorkspaceUiSetting(propertyName, propertyValue) {

    const model = {
        ThemeColor: $('input[name="layoutOptions"]:checked')[0].id,
        MenuColor: $("#customizer-theme-colors ul .selected")[0].id,
        TopStripColor: $("#customizer-navbar-colors ul .selected")[0].id,
        TopStripType: $('input[name="navbarType"]:checked')[0].id,
        FooterType: $('input[name="footerType"]:checked')[0].id,
        Animation: $("#icon-animation-switch")[0].checked,
        MenuClose: $("#collapse-sidebar-switch")[0].checked,
        CardShadow: $("#card-shadow-switch")[0].checked,
        HideScrollToTop: $("#hide-scroll-top-switch")[0].checked
    };

    model[propertyName] = propertyValue;

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/api/v1/Workspace/SetWorkspaceSetting",
        data: JSON.stringify(model),
        beforeSend: function () {
            $body.removeClass("loading");
        }
    });
}

function checkBeforeChange(element, propertyName) {

    if ($(element).hasClass("selected"))
        return;

    changeWorkspaceUiSetting(propertyName, element.id);
}