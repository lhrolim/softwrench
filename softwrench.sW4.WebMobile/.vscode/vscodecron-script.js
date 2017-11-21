exports.tick = function (args) {
    // access Node.js API provided by VS Code
    // s.  (s. https://nodejs.org/api/)
    var fs = require('fs');
  
    // access an own module
    var touch = args.require("touch");
  
    touch(args.globals + '/www/Content/Mobile/scripts/utils/mobilelareleasebuiltimes.js', {}, (err) => {
      if (err) {
        args.log("From 'myEvent': " + err);
      }
    });
  
  
  
    // ...
  }