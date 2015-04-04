module.exports = function(grunt) {

  // Project configuration.
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    
     clean: {
	    folder: "Content/Vendor/scripts/",
	    folder2: "Content/Vendor/styles/"
    },
    

    
    
  	bowercopy:{
  		
  		options:{
  			 destPrefix: 'Content/Vendor/scripts'
  		},
  		
  		
  		 jquery: {
             files: {
                 'jquery.min.js': 'jquery/dist/jquery.min.js',
//                 'jquery.js': 'jquery/dist/jquery.js',
//                 'jquery.min.map': 'jquery/dist/jquery.min.map',
             }
  		 },

  		 angular: {
  		     files: {
  		         'angular.min.js': 'angular/angular.min.js',
  		         'angular.min.js.map': 'angular/angular.min.js.map',
  		         'angular-sanitize.min.js': 'angular-sanitize/angular-sanitize.min.js',
  		         'angular-sanitize.min.js.map': 'angular-sanitize/angular-sanitize.min.js.map',
  		         'angular-ui-router.min.js': 'angular-ui-router/release/angular-ui-router.min.js',
  		         'angular-animate.min.js': 'angular-animate/angular-animate.min.js',
  		         'angular-animate.min.js.map': 'angular-animate/angular-animate.min.js.map',
  		          'angular.js': 'angular/angular.js',
  		     }
  		 },

  		 ngCordova: {
  		     files: {
  		         //  		         'ng-cordova.min.js': 'ngCordova/dist/ng-cordova.min.js',
  		         'ng-cordova.min.js': 'ngCordova/dist/ng-cordova.js',
  		     }
  		 },

  		 ionic: {
  		     files: {
  		         'ionic.min.js': 'ionic/release/js/ionic.min.js',
  		         'ionic-angular.min.js': 'ionic/release/js/ionic-angular.min.js',
//  		         'ionic.bundle.min.js': 'ionic/release/js/ionic.bundle.min.js',
  		     }
  		 },

  		 ionic_css: {

  		     options: {
  		         destPrefix: 'Content/Vendor/styles'
  		     },

  		     files: {
  		         'ionic.min.css': 'ionic/release/css/ionic.min.css',
  		     }
  		 },

  		 ionic_fonts: {

  		     options: {
  		         destPrefix: 'Content/Vendor/fonts'
  		     },

  		     files: {
  		         'ionicons.ttf': 'ionic/release/fonts/ionicons.ttf',
  		         'ionicons.woff': 'ionic/release/fonts/ionicons.woff',
  		     }
  		 },
         
//         angular: {
//             files: {
//                 'angular.js': 'angular/angular.min.js',
//             }
//         },
//         
//         
//         fontawesome: {
//        	 options:{
//        		 destPrefix: 'Content/Vendor/styles/'
//        	 },
//        	 
//             files: {
//                 'fontawesome.min.css': 'components-font-awesome/css/font-awesome.min.css'
//             }
//         },

//         folders: {
//
//             options: {
//                 destPrefix: 'src/main/webapp/resources/'
//             },
//
//             files: {
//                 // Note: when copying folders, the destination (key) will be used as the location for the folder 
//                 'fonts': 'components-font-awesome/fonts/',
//             }
//         },
  	},


  	tags: {
        options: {
            openTag: '<!-- start auto template tags, grunt will generate it for dev environment, do not remove this -->'
        },

  	    build: {
  	        src: ["Content/Mobile/scripts/controllers/**/*.js","Content/Mobile/scripts/services/**/*.js", "Content/Mobile/scripts/utils/**/*.js"],
  	        dest: 'layout.html'
  	    }
  	},

  	concat: {
  	    mobileScripts: {
  	        src: ["Content/Mobile/scripts/services/**/*.js","Content/Mobile/scripts/utils/**/*.js"],            
  	        dest: "scripts/dist/mobile_angular.js"
  	    },
  	}
  
  
  });


  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-concat');
  grunt.loadNpmTasks('grunt-contrib-clean');
  grunt.loadNpmTasks('grunt-bowercopy');
  grunt.loadNpmTasks('grunt-script-link-tags');

  // Default task(s).
  grunt.registerTask('defaultdev', ['clean','bowercopy','tags']);
  grunt.registerTask('default', ['clean','bowercopy','uglify']);

};