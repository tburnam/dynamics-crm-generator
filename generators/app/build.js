const mkdirp = require('mkdirp');

// ------------------------------------ Build method
const writeMainFolder = generator => {
  // Build outer file structure
  mkdirp.sync(generator.props.solutionName);
  mkdirp.sync(generator.props.solutionName + '/build');
  mkdirp.sync(generator.props.solutionName + '/references');
  mkdirp.sync(generator.props.solutionName + '/scripts');

  // Copy dirs;
  generator.fs.copy(
    generator.templatePath('MainFolder/dirs.proj'),
    generator.destinationPath(generator.props.solutionName + '/dirs.proj')
  );

  generator.fs.copy(
    generator.templatePath('MainFolder/init.cmd'),
    generator.destinationPath(generator.props.solutionName + '/init.cmd')
  );

  generator.fs.copy(
    generator.templatePath('MainFolder/init.ps1'),
    generator.destinationPath(generator.props.solutionName + '/init.ps1')
  );

  generator.fs.copy(
    generator.templatePath('MainFolder/.gitignore'),
    generator.destinationPath(generator.props.solutionName + '/.gitignore')
  );
};

// ------------------------------------ Build method
const writeBuildFolder = generator => {
  mkdirp.sync(generator.props.solutionName + '/build/agent');
  mkdirp.sync(generator.props.solutionName + '/build/config');
  mkdirp.sync(generator.props.solutionName + '/build/include');

  // Copy /build/agent folder over directly
  generator.fs.copy(
    generator.templatePath('MainFolder/build/agent'),
    generator.destinationPath(generator.props.solutionName + '/build/agent/')
  );

  // Copy /build/include folder over directly
  generator.fs.copy(
    generator.templatePath('MainFolder/build/include'),
    generator.destinationPath(generator.props.solutionName + '/build/include/')
  );

  // Copy /build/config folder over directly + add .nuspec file (changing name)
  generator.fs.copy(
    generator.templatePath('MainFolder/build/config'),
    generator.destinationPath(generator.props.solutionName + '/build/config/')
  );

  generator.fs.copyTpl(
    generator.templatePath('MainFolder/build/SOLUTION_NAME.nuspec'),
    generator.destinationPath(
      generator.props.solutionName +
        '/build/config/' +
        generator.props.solutionName +
        '.nuspec'
    ),
    { solutionName: generator.props.solutionName, creators: generator.props.creators }
  );
};

// ------------------------------------ Build method
const writeScriptsInitFolder = generator => {
  // Make init folder
  mkdirp.sync(generator.props.solutionName + '/scripts/init');

  // Copy /init/scripts folder over directly
  generator.fs.copy(
    generator.templatePath('MainFolder/init'),
    generator.destinationPath(generator.props.solutionName + '/scripts/init/')
  );
};

// ------------------------------------ Build method
const writeSolutionsFolder = generator => {
  // Make /solutions folder
  mkdirp.sync(generator.props.solutionName + '/solutions');

  // Make base solution folder
  mkdirp.sync(
    generator.props.solutionName + '/solutions/' + generator.props.solutionName
  );

  const outerSolutionsFolderPath = generator.props.solutionName + '/solutions';
  const solutionFolderPath =
    generator.props.solutionName + '/solutions/' + generator.props.solutionName;

  // Copy solutions build files directly
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/baseFiles'),
    generator.destinationPath(outerSolutionsFolderPath),
    { solutionName: generator.props.solutionName }
  );

  // Copy base solution build files directly
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/baseFiles'),
    generator.destinationPath(solutionFolderPath),
    { solutionName: generator.props.solutionName, pluginFlag: generator.props.pluginFlag }
  );

  // Copy sln file
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/SOLUTION_NAME.sln'),
    generator.destinationPath(
      solutionFolderPath + '/' + generator.props.solutionName + '.sln'
    ),
    { solutionName: generator.props.solutionName, pluginFlag: generator.props.pluginFlag }
  );

  // Build /Package folder
  mkdirp.sync(solutionFolderPath + '/Package');

  // Copy /Package files
  generator.fs.copy(
    generator.templatePath(
      'MainFolder/solutions/solution/Package/RegisterFile.crmregister'
    ),
    generator.destinationPath(solutionFolderPath + '/Package/RegisterFile.crmregister')
  );

  generator.fs.copyTpl(
    generator.templatePath(
      'MainFolder/solutions/solution/Package/SOLUTION_NAMEPackage.csproj'
    ),
    generator.destinationPath(
      solutionFolderPath + '/Package/' + generator.props.solutionName + 'Package.csproj'
    ),
    { solutionName: generator.props.solutionName }
  );

  // Build /PVSPackage folder and subfolders
  mkdirp.sync(solutionFolderPath + '/PVSPackage');
  mkdirp.sync(solutionFolderPath + '/PVSPackage/Properties');
  mkdirp.sync(solutionFolderPath + '/PVSPackage/PackageExtra');
  mkdirp.sync(solutionFolderPath + '/PVSPackage/msdyn_' + generator.props.solutionName);

  // Copy /PVSPackage base files
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/PVSPackage/baseFiles'),
    generator.destinationPath(solutionFolderPath + '/PVSPackage'),
    { solutionName: generator.props.solutionName, creators: generator.props.creators }
  );

  // Copy PVS csproj
  generator.fs.copyTpl(
    generator.templatePath(
      'MainFolder/solutions/solution/PVSPackage/SOLUTION_NAMEPVS.csproj'
    ),
    generator.destinationPath(
      solutionFolderPath + '/PVSPackage/' + generator.props.solutionName + 'PVS.csproj'
    ),
    { solutionName: generator.props.solutionName }
  );

  // Copy /PVSPackage/Properties files
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/PVSPackage/Properties'),
    generator.destinationPath(solutionFolderPath + '/PVSPackage/Properties'),
    { solutionName: generator.props.solutionName }
  );

  // Copy /PVSPackage/PackageExtra files
  generator.fs.copy(
    generator.templatePath('MainFolder/solutions/solution/PVSPackage/PackageExtra'),
    generator.destinationPath(solutionFolderPath + '/PVSPackage/PackageExtra')
  );

  // Copy /PVSPackage/msdyn_ files
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/PVSPackage/msdyn_'),
    generator.destinationPath(
      solutionFolderPath + '/PVSPackage/msdyn_' + generator.props.solutionName
    ),
    { solutionName: generator.props.solutionName }
  );

  // Build /Solution folder
  mkdirp.sync(solutionFolderPath + '/Solution');
  mkdirp.sync(solutionFolderPath + '/Solution/Other');

  // Copy /Solution/Other files
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/Solution/Other'),
    generator.destinationPath(solutionFolderPath + '/Solution/Other'),
    { solutionName: generator.props.solutionName, pluginFlag: generator.props.pluginFlag }
  );

  // Copy /Solution csproj
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/Solution/SOLUTION_NAME.csproj'),
    generator.destinationPath(
      solutionFolderPath + '/Solution/' + generator.props.solutionName + '.csproj'
    ),
    { solutionName: generator.props.solutionName }
  );

  // Copy /Solution WebResources file
  generator.fs.copy(
    generator.templatePath('MainFolder/solutions/solution/Solution/WebResources.xml'),
    generator.destinationPath(solutionFolderPath + '/Solution/WebResources.xml')
  );
};

const writePlugin = generator => {
  const solutionFolderPath =
    generator.props.solutionName + '/solutions/' + generator.props.solutionName;

  const solutionAssemblyPath =
    solutionFolderPath +
    '/Solution/PluginAssemblies/' +
    generator.props.solutionName +
    '-' +
    'SDK_GUID/';

  // Build /Plugins folder
  mkdirp.sync(solutionFolderPath + '/Plugins');

  // Build /Plugins/Plugins folder
  mkdirp.sync(solutionFolderPath + '/Plugins/Plugins');

  // Build SDKMessageProcessingSteps folder
  mkdirp.sync(solutionFolderPath + '/Solution/SDKMessageProcessingSteps');

  // Build PluginAssemblies/SOLUTION_NAME-SDK_GUID/ folder
  mkdirp.sync(solutionAssemblyPath);

  // Copy dll.data.xml file for PluginAssemblies
  generator.fs.copyTpl(
    generator.templatePath(
      'MainFolder/solutions/solution/Solution/PluginAssemblies/SOLUTION_NAMEPlugins.dll.data.xml'
    ),
    generator.destinationPath(
      solutionAssemblyPath + generator.props.solutionName + 'Plugins.dll.data.xml'
    ),
    { solutionName: generator.props.solutionName }
  );

  // Copy base files to /Plugins folder
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/Plugins/baseFiles'),
    generator.destinationPath(solutionFolderPath + '/Plugins/'),
    { solutionName: generator.props.solutionName }
  );

  // Copy Properties directory/files to /Plugins folder
  generator.fs.copyTpl(
    generator.templatePath('MainFolder/solutions/solution/Plugins/Properties'),
    generator.destinationPath(solutionFolderPath + '/Plugins/'),
    { solutionName: generator.props.solutionName }
  );

  // Copy Plugins.sln to /Plugins folder
  generator.fs.copyTpl(
    generator.templatePath(
      'MainFolder/solutions/solution/Plugins/SOLUTION_NAMEPlugins.sln'
    ),
    generator.destinationPath(
      solutionFolderPath + '/Plugins/' + generator.props.solutionName + 'Plugins.sln'
    ),
    { solutionName: generator.props.solutionName }
  );

  // Copy Plugins.csproj to /Plugins folder
  generator.fs.copyTpl(
    generator.templatePath(
      'MainFolder/solutions/solution/Plugins/SOLUTION_NAMEPlugins.csproj'
    ),
    generator.destinationPath(
      solutionFolderPath + '/Plugins/' + generator.props.solutionName + 'Plugins.csproj'
    ),
    { solutionName: generator.props.solutionName }
  );
};

module.exports.writeMainFolder = writeMainFolder;
module.exports.writeBuildFolder = writeBuildFolder;
module.exports.writeScriptsInitFolder = writeScriptsInitFolder;
module.exports.writeSolutionsFolder = writeSolutionsFolder;
module.exports.writePlugin = writePlugin;
