var app = angular.module('sw_layout');

app.factory('logoutService', function (contextService, i18NService, $window) {

    return {

        logout: function (mode) {
            if (contextService.getUserData() == null) {
                //this can only be achived if the user hits a back browser button after a logout suceeded
                $window.location.href = url('/SignOut/SignOut');
                return;
            }

            var title = "manual"== mode ? "Logout!" : "Automatic Logout!";
            var message =  "manual" == mode ? "You have successfully logged out of ServiceIT.</br>To completely logout, please close your browser. To log back into ServiceIT, click the button provided below." : ' Auf Grund zu langer Inaktivität wurden Sie aus Sicherheitsgründen automatisch ausgeloggt <br\> You have been automatically logged out due to inactivity. <br\> Usted ha sido cerrada automáticamente debido a la inactividad.';

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


