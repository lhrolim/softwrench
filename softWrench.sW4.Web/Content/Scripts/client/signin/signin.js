// IMPORTANT:
// DO NOT INCLUDE ME IN _layout.cshtml
// I SHOULD NOT BE LOADED AFTER THE USER HAS LOGGED IN
$(document).ready(function () {
    $("#userTimezoneOffset").val(new Date().getTimezoneOffset());
    sessionStorage['ctx_loggedin'] = false;
    
    $(".no-touch [rel=tooltip]").tooltip({ container: "body", trigger: "hover" });

    $('#btnLogin').click(function () {
        var username = $('#userName');
        var password = $('#password');
        var userNameMessage = $('#userNameMessage');
        var passwordMessage = $('#passwordMessage');

        if (username.val() === '') {
            passwordMessage.hide();
            if (userNameMessage.hide()) {
                userNameMessage.toggle();
            }
            return false;
        }

        if (password.val() === '') {
            userNameMessage.hide();
            if (passwordMessage.hide()) {
                passwordMessage.toggle();
            }
            return false;
        }
        return true;
    });
});