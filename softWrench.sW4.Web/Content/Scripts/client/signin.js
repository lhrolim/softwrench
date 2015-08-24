$(document).ready(function () {
    $("#userTimezoneOffset").val(new Date().getTimezoneOffset());
    sessionStorage['ctx_loggedin'] = false;

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