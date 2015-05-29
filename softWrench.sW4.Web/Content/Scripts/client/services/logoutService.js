var app = angular.module('sw_layout');

app.factory('logoutService', function (contextService,i18NService) {

    return {

        logout: function (mode) {
            var title = mode == "manual" ? "Logout!" : "Automatic Logout!";
            var message = mode == "manual" ? "You have successfully logged out of ServiceIT.</br>To completely logout, please close your browser. To log back into ServiceIT, click the button provided below." : ' Auf Grund zu langer Inaktivität wurden Sie aus Sicherheitsgründen automatisch ausgeloggt \n You have been automatically logged out due to inactivity. \n Usted ha sido cerrada automáticamente debido a la inactividad.';

            bootbox.dialog({
                message: message,
                title: title,
                className: 'logoutmodal',
                closeButton: false,
                buttons: {
                    success: {
                        label: "Close Browser",
                        className: "commandButton btn loginbutton",
                        callback: function () {
                            sessionStorage.removeItem("swGlobalRedirectURL");
                            contextService.clearContext();

                            $window.location.href = isIe() ? url('/SignOut/SignOut') : url('/SignOut/SignOutClosePage');
                            window.open('', '_self', '');
                            window.close();
                        }
                    },
                    danger: {
                        label: "Login to ServiceIT",
                        className: "commandButton btn loginbutton",
                        callback: function () {
                            sessionStorage.removeItem("swGlobalRedirectURL");
                            contextService.clearContext();
                            $window.location.href = url('/SignOut/SignOut');
                        }
                    },
                }
            });
        },
     
    };
});


