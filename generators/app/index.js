'use strict';
const Generator = require('yeoman-generator');
const chalk = require('chalk');
const yosay = require('yosay');

// TODO: Does this work on windows?
const mkdirp = require('mkdirp');

module.exports = class extends Generator {
  prompting() {
    // Have Yeoman greet the user.
    this.log(yosay(`Welcome to the ${chalk.red('generator-dynamicscrm')} generator!\n`));

    const prompts = [
      {
        type: 'input',
        name: 'solutionName',
        message: 'What is the name of your solution?',
        default: this.appname
      },

      {
        type: 'input',
        name: 'creators',
        message: 'Who are the creators? (seperate by space)',
        default: this.appname
      }
    ];

    return this.prompt(prompts).then(props => {
      // To access props later use this.props.someAnswer;
      this.props = props;
      console.log(props);
    });
  }

  writing() {
    // Write methods
    const writeMainFolder = () => {
      // Build outer file structure
      mkdirp.sync(this.props.solutionName);
      mkdirp.sync(this.props.solutionName + '/build');
      mkdirp.sync(this.props.solutionName + '/references');
      mkdirp.sync(this.props.solutionName + '/scripts');

      // Copy dirs;
      this.fs.copy(
        this.templatePath('MainFolder/dirs.proj'),
        this.destinationPath(this.props.solutionName + '/dirs.proj')
      );

      this.fs.copy(
        this.templatePath('MainFolder/init.cmd'),
        this.destinationPath(this.props.solutionName + '/init.cmd')
      );

      this.fs.copy(
        this.templatePath('MainFolder/init.ps1'),
        this.destinationPath(this.props.solutionName + '/init.ps1')
      );

      this.fs.copy(
        this.templatePath('MainFolder/.gitignore'),
        this.destinationPath(this.props.solutionName + '/.gitignore')
      );
    };

    // Writes /build folder
    const writeBuildFolder = () => {
      mkdirp.sync(this.props.solutionName + '/build/agent');
      mkdirp.sync(this.props.solutionName + '/build/config');
      mkdirp.sync(this.props.solutionName + '/build/include');

      // Copy /build/agent folder over directly
      this.fs.copy(
        this.templatePath('MainFolder/build/agent'),
        this.destinationPath(this.props.solutionName + '/build/agent/')
      );

      // Copy /build/include folder over directly
      this.fs.copy(
        this.templatePath('MainFolder/build/include'),
        this.destinationPath(this.props.solutionName + '/build/include/')
      );

      // Copy /build/config folder over directly + add .nuspec file (changing name)
      this.fs.copy(
        this.templatePath('MainFolder/build/config'),
        this.destinationPath(this.props.solutionName + '/build/config/')
      );

      this.fs.copyTpl(
        this.templatePath('MainFolder/build/SOLUTION_NAME.nuspec'),
        this.destinationPath(
          this.props.solutionName + '/build/config/' + this.props.solutionName + '.nuspec'
        ),
        { solutionName: this.props.solutionName, creators: this.props.creators }
      );
    };

    // Writes /scripts/init folder
    const writeScriptsInitFolder = () => {
      // Make init folder
      mkdirp.sync(this.props.solutionName + '/scripts/init');

      // Copy /init/scripts folder over directly
      this.fs.copy(
        this.templatePath('MainFolder/init'),
        this.destinationPath(this.props.solutionName + '/scripts/init/')
      );
    };

    const writeSolutionsFolder = () => {
      // Make /solutions folder
      mkdirp.sync(this.props.solutionName + '/solutions');

      // Make base solution folder
      mkdirp.sync(this.props.solutionName + '/solutions/' + this.props.solutionName);

      const outerSolutionsFolderPath = this.props.solutionName + '/solutions';
      const solutionFolderPath =
        this.props.solutionName + '/solutions/' + this.props.solutionName;

      // Copy solutions build files directly
      this.fs.copyTpl(
        this.templatePath('MainFolder/solutions/baseFiles'),
        this.destinationPath(outerSolutionsFolderPath),
        { solutionName: this.props.solutionName }
      );

      // Copy base solution build files directly
      this.fs.copyTpl(
        this.templatePath('MainFolder/solutions/solution/baseFiles'),
        this.destinationPath(solutionFolderPath),
        { solutionName: this.props.solutionName }
      );

      // Copy sln file
      this.fs.copyTpl(
        this.templatePath('MainFolder/solutions/solution/SOLUTION_NAME.sln'),
        this.destinationPath(solutionFolderPath + '/' + this.props.solutionName + '.sln'),
        { solutionName: this.props.solutionName }
      );

      // Build /Package folder
      mkdirp.sync(solutionFolderPath + '/Package');

      // Copy /Package files
      this.fs.copy(
        this.templatePath(
          'MainFolder/solutions/solution/Package/RegisterFile.crmregister'
        ),
        this.destinationPath(solutionFolderPath + '/Package/RegisterFile.crmregister')
      );

      this.fs.copyTpl(
        this.templatePath(
          'MainFolder/solutions/solution/Package/SOLUTION_NAMEPackage.csproj'
        ),
        this.destinationPath(
          solutionFolderPath + '/Package/' + this.props.solutionName + 'Package.csproj'
        ),
        { solutionName: this.props.solutionName }
      );

      // Build /PVSPackage folder and subfolders
      mkdirp.sync(solutionFolderPath + '/PVSPackage');
      mkdirp.sync(solutionFolderPath + '/PVSPackage/Properties');
      mkdirp.sync(solutionFolderPath + '/PVSPackage/PackageExtra');
      mkdirp.sync(solutionFolderPath + '/PVSPackage/msdyn_' + this.props.solutionName);

      // Copy /PVSPackage base files
      this.fs.copyTpl(
        this.templatePath('MainFolder/solutions/solution/PVSPackage/baseFiles'),
        this.destinationPath(solutionFolderPath + '/PVSPackage'),
        { solutionName: this.props.solutionName, creators: this.props.creators }
      );

      // Copy PVS csproj
      this.fs.copyTpl(
        this.templatePath(
          'MainFolder/solutions/solution/PVSPackage/SOLUTION_NAMEPVS.csproj'
        ),
        this.destinationPath(
          solutionFolderPath + '/PVSPackage/' + this.props.solutionName + 'PVS.csproj'
        ),
        { solutionName: this.props.solutionName }
      );

      // Copy /PVSPackage/Properties files
      this.fs.copyTpl(
        this.templatePath('MainFolder/solutions/solution/PVSPackage/Properties'),
        this.destinationPath(solutionFolderPath + '/PVSPackage/Properties'),
        { solutionName: this.props.solutionName }
      );

      // Copy /PVSPackage/PackageExtra files
      this.fs.copy(
        this.templatePath('MainFolder/solutions/solution/PVSPackage/PackageExtra'),
        this.destinationPath(solutionFolderPath + '/PVSPackage/PackageExtra')
      );

      // Copy /PVSPackage/msdyn_ files
      this.fs.copyTpl(
        this.templatePath('MainFolder/solutions/solution/PVSPackage/msdyn_'),
        this.destinationPath(
          solutionFolderPath + '/PVSPackage/msdyn_' + this.props.solutionName
        ),
        { solutionName: this.props.solutionName }
      );

      // Build /Solution folder
      mkdirp.sync(solutionFolderPath + '/Solution');
      mkdirp.sync(solutionFolderPath + '/Solution/Other');

      // Copy /Solution/Other files
      this.fs.copyTpl(
        this.templatePath('MainFolder/solutions/solution/Solution/Other'),
        this.destinationPath(solutionFolderPath + '/Solution/Other'),
        { solutionName: this.props.solutionName }
      );

      // Copy /Solution csproj
      this.fs.copyTpl(
        this.templatePath('MainFolder/solutions/solution/Solution/SOLUTION_NAME.csproj'),
        this.destinationPath(
          solutionFolderPath + '/Solution/' + this.props.solutionName + '.csproj'
        ),
        { solutionName: this.props.solutionName }
      );

      // Copy /Solution WebResources file
      this.fs.copy(
        this.templatePath('MainFolder/solutions/solution/Solution/WebResources.xml'),
        this.destinationPath(solutionFolderPath + '/Solution/WebResources.xml')
      );
    };

    // Run
    writeMainFolder();
    writeBuildFolder();
    writeScriptsInitFolder();
    writeSolutionsFolder();
  }

  install() {
    // This.installDependencies();
  }
};
