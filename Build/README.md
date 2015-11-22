# Development instructions

In order to build the solution locally there are a few steps required:
- use `git` with version >= 1.9.3
  - run `git config --global core.longpaths true` or, in the project's root folder run `git config core.longpaths true`
- import solution in `Visual Studio` (`File -> Open Project -> <path to softWrench.sW4.sln>`)
- set the module `softWrench.sW4.Web` as `StartUp Project`  
  - in the `solution view/panel` -> right-click the module -> select the option `Set as StartUp Project`
- import all `NuGet` external dependencies
  - in the `solution view/panel` -> right-click the solution -> select `Manage NuGet Dependencies for Solution...` -> 
  on top of the dialog click `Restore` -> wait for it to finish
- install [NAnt](http://nant.sourceforge.net/)
  - follow the installation [guidelines](http://nant.sourceforge.net/release/latest/help/introduction/installation.html)
  - As an external step: http://stackoverflow.com/questions/8605122/how-do-i-resolve-configuration-errors-with-nant-0-91  
  in order to create a `nant.bat` executable file
- create a `NAnt` external task to create `symlinks` in between projects  
  - in `Visual Studio` -> click `TOOLS` -> select `External Tools...` -> `Add`
    - Title: whatever you want
    - Command: '<path to nant.bat>'
    - Arguments: -f:build.xml generatelinks generatelinksMobile
    - Initial directory: $(SolutionDir)\Build
    - check option `Use Output Window`
- Run the `NAnt` task on the solution
  - `TOOLS` -> select the task
- Now you can deploy the project in `IIS` from within `Visual Studio` (`F5` key by default; `shift+F5` to stop)
- Access the web application by opening a browser of your choosing and hitting `http://http://localhost:8080/sw4/`
  
