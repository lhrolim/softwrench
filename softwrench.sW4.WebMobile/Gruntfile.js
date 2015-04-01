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
  		         'ng-cordova.min.js': 'ngCordova/dist/ng-cordova.min.js',
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
      
         
  	}
  
  
  });

  
  // Load the plugin that provides the "uglify" task.
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-clean');
  grunt.loadNpmTasks('grunt-bowercopy');grunt

  // Default task(s).
  grunt.registerTask('defaultdev', ['clean','bowercopy']);
  grunt.registerTask('default', ['clean','bowercopy','uglify']);

};