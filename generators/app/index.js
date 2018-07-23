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
      }
    ];

    return this.prompt(prompts).then(props => {
      // To access props later use this.props.someAnswer;
      this.props = props;
      console.log(props);
    });
  }

  writing() {
    // Build outer file structure
    mkdirp.sync(this.props.solutionName);
    mkdirp.sync(this.props.solutionName + '/build');
    mkdirp.sync(this.props.solutionName + '/references');
    mkdirp.sync(this.props.solutionName + '/scripts');

    this.fs.copy(
      this.templatePath('dummyfile.txt'),
      this.destinationPath(this.props.solutionName + '/dummyfile.txt')
    );
  }

  install() {
    // This.installDependencies();
  }
};
