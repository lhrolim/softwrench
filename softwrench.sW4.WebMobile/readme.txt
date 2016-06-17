Installs:

1) Cordova Tools for Visual Studio 3.1.1 http://www.microsoft.com/en-us/download/details.aspx?id=42675
2) Task Runner Explorer https://www.google.com.br/webhp?sourceid=chrome-instant&ion=1&espv=2&ie=UTF-8#q=visual%20studio%20grunt
3) Install cordova-cli npm install cordova -g
4) Install grunt npm install grunt -g
5) Install grunt-cli npm install grunt-cli -g
6) Install bower npm install bower -g



gotchas:

1) Modify cordova settings, at %APPDATA%\npm\node_modules\vs-mda\lib\util.js:
util.getDefaultFileNameEndingExclusions = function () {
    return [settings.projectSourceDir + '/bin',
            settings.projectSourceDir + '/bld',
            settings.projectSourceDir + '/merges',
            settings.projectSourceDir + '/plugins',
            settings.projectSourceDir + '/res',
            settings.projectSourceDir + '/test',
            settings.projectSourceDir + '/tests',
			settings.projectSourceDir + '/node_modules',
			settings.projectSourceDir + '/bower_components',
            '.jsproj', '.jsproj.user'];
}

 http://stackoverflow.com/questions/25274968/multi-device-hybrid-apps-how-to-do-combine-minify-and-obfuscate-in-release-and/28281131#28281131

 2) Install all device extras on SDK manager

 3) Debug USB on development mode on the phone

 4) Be careful, cause on the device the case is sensitive!

 5) On Ripple, open a second tab before hitting Chrome´s F12 or it will kill the application.
 
 6) On Ripple, Windows 8 Turn off Cross Domain Proxy Settings 

