'use strict';
const Generator = require('yeoman-generator');
const chalk = require('chalk');
const yosay = require('yosay');
const build = require('./build.js');

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
      },

      {
        type: 'checkbox',
        name: 'options',
        message: 'Select which components you would like included in your new solution',
        choices: [{ name: 'Plugins' }, { name: 'Workflows' }, { name: 'Web Resources' }],
        default: []
      }
    ];

    return this.prompt(prompts).then(props => {
      // To access props later use this.props.someAnswer;
      this.props = props;

      // Process flags
      this.props.pluginFlag = this.props.options.indexOf('Plugins') > -1;
      this.props.webResourceFlag = this.props.options.indexOf('Workflows') > -1;
      this.props.workflowFlag = this.props.options.indexOf('Web Resources') > -1;

      // TODO: Debug step
      console.log(props);
    });
  }

  writing() {
    // Run
    build.writeMainFolder(this);
    build.writeBuildFolder(this);
    build.writeScriptsInitFolder(this);
    build.writeSolutionsFolder(this);

    if (this.props.pluginFlag === true) {
      build.writePlugin(this);
    }
  }

  install() {
    // This.installDependencies();
  }
};
