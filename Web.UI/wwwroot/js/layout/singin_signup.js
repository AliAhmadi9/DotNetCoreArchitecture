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


$("#btnResetPassword").click(function (e) {

    e.preventDefault();

    if ($("#frmResetPassword").valid()) {

        $(this).prop("disabled", true);

        const model = {
            Email: $("#Email").val().trim(),
            Password: $("#Password").val().trim(),
            ConfirmPassword: $("#ConfirmPassword").val().trim(),
            Code: $("#Code").val()
        };

        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/api/v1/user/ResetPassword",
            data: JSON.stringify(model),
            success: function (result) {

                if (result.isSuccess) {
                    toastr.success(result.message);

                    setTimeout(
                        function () {
                            window.location = "/Login";
                        }, 1500);
                }
            },
            error: function (error) {
                if (error.responseJSON.Message != undefined && error.responseJSON.Message != '')
                    toastr.error(error.responseJSON.Message);
                else
                    toastr.error('خطایی نامشخص رخ داده است!');

            },
            complete: function () {
                $("#btnResetPassword").prop("disabled", false);
            }
        });
    }
});

$("#btnRegister").click(function (e) {

    e.preventDefault();

    const isValid = $("#frmRegister").valid();
    if (isValid) {

        $("#btnRegister").prop("disabled", true);

        const model = {
            Password: $("#Password").val().trim(),
            ConfirmPassword: $("#ConfirmPassword").val().trim(),
            Email: $("#Email").val().trim(),
            UserName: $("#Email").val().trim(),
            FullName: $("#FullName").val().trim(),
            Gender: 1
        };

        $.ajax({
            type: "post",
            contentType: "application/json; charset=utf-8",
            url: "/api/v1/user",
            data: JSON.stringify(model),
            success: function (result) {

                if (result.isSuccess) {

                    window.location = "/Home/Index";
                }
            },
            error: function (error) {

                if (error.responseJSON.Message != undefined && error.responseJSON.Message != '') {
                    const msg = error.responseJSON.Message;

                    if (msg.toLowerCase().includes("already taken"))
                        toastr.error('این ایمیل از قبل وجود دارد');
                    else
                        toastr.error(msg);

                }
                else
                    toastr.error('خطایی نامشخص رخ داده است!');

            },
            complete: function () {
                $("#btnRegister").prop("disabled", false);
            }
        });
    }
});

$("#btnForgotPassword").click(function (e) {

    e.preventDefault();

    if ($("#frmForgotPassword").valid()) {

        $(this).prop("disabled", true);

        const model = {
            Email: $("#Email").val().trim()
        };

        $.ajax({
            type: "post",
            contentType: "application/json; charset=utf-8",
            url: "/api/v1/user/ForgotPassword",
            data: JSON.stringify(model),
            success: function (result) {
                debugger;
                if (result.isSuccess) {
                    toastr.success(result.message);
                    setTimeout(
                        function () {
                            window.location = "/Home/Index";
                        }, 2000);
                }
            },
            error: function (error) {

                if (error.responseJSON.Message != undefined && error.responseJSON.Message != '')
                    toastr.error(error.responseJSON.Message);
                else
                    toastr.error('خطایی نامشخص رخ داده است!');

            },
            complete: function () {
                $("#btnForgotPassword").prop("disabled", false);
            }
        });
    }
});

$("#btnLogin").click(function (e) {

    e.preventDefault();

    if ($("#frmLogin").valid()) {

        $("#btnLogin").prop("disabled", true);

        const email = $("#Email").val().trim();

        const model = {
            Password: $("#Password").val().trim(),
            Email: email,
            RememberMe: $("#RememberMe").prop("checked")
        };

        $.ajax({
            type: "post",
            //dataType: 'json',
            contentType: "application/json; charset=utf-8",
            url: "/api/v1/user/token",
            data: JSON.stringify(model),
            success: function (result) {
                if (result.isSuccess) {
                    window.location = "/Home/Index";
                }
            },
            error: function (error) {
                if (error.responseJSON.Message != undefined && error.responseJSON.Message != '') {
                    if (error.responseJSON.Message.includes("حساب شما فعال نیست")) {

                        window.location = "/Account/ResendEmailConfirmation";

                    } else {
                        toastr.error(error.responseJSON.Message);
                    }
                }
                else
                    toastr.error('خطایی نامشخص رخ داده است!');
            },
            complete: function () {
                $("#btnLogin").prop("disabled", false);
            }
        });
    }
});

$("#btnResendEmail").click(function (e) {

    e.preventDefault();

    if ($("#frmResendEmail").valid()) {

        $(this).prop("disabled", true);

        const model = {
            Email: $("#Email").val().trim()
        };

        $.ajax({
            type: "POST",
            //dataType: 'json',
            contentType: "application/json; charset=utf-8",
            url: "/api/v1/User/SendEmail",
            data: JSON.stringify(model),
            success: function (result) {

                if (result.isSuccess)
                    window.location = "/Account/Confirmation";
            },
            error: function (error) {

                if (error.responseJSON.Message != undefined && error.responseJSON.Message != '')
                    toastr.error(error.responseJSON.Message);
                else
                    toastr.error('خطایی نامشخص رخ داده است!');
            },
            complete: function () {
                $("#btnResendEmail").prop("disabled", false);
            }
        });
    }
});

$("#btnException").click(function (e) {

    e.preventDefault();
    //$("#frmLogin").validate();
    const isValid = $("#frmException").valid();
    if (isValid) {
        $(this).prop("disabled", true);

        const model = {
            UserEmail: $("#UserEmail").val(),
            Exception: $("#Exception").val()
        };

        $.ajax({
            type: "post",
            contentType: "application/json; charset=utf-8",
            url: "/Error/ReportException",
            data: JSON.stringify(model),
            success: function (result) {

                if (result) {
                    toastr.success("خطا با موفقیت گزارش شد. باتشکر از همکاری شما");
                    setTimeout(
                        function () {
                            window.location = "/Login";
                        }, 2000);
                }
            },
            error: function (error) {

                if (error.responseJSON.Message != undefined && error.responseJSON.Message != '')
                    toastr.error(error.responseJSON.Message);
                else
                    toastr.error('خطایی نامشخص رخ داده است!');

            },
            complete: function () {
                $("#btnException").prop("disabled", false);
            }
        });
    }
});
