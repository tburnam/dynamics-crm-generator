namespace <%= solutionName %>.Plugins
{
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Diagnostics;
    /// <summary>
    /// Base class for all Plug-in classes.
    /// </summary>    
    public abstract class PluginBase : IPlugin
    {
        /// <summary>
        /// Gets or sets the name of the child class.
        /// </summary>
        /// <value>The name of the child class.</value>
        protected string ChildClassName { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class.
        /// </summary>
        /// <param name="childClassName">The <see cref=" cred="Type"/> of the derived class.</param>
        internal PluginBase(Type childClassName)
        {
            ChildClassName = childClassName.ToString();
        }
        /// <summary>
        /// Executes the plug-in.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics CRM caches plug-in instances. 
        /// The plug-in's Execute method should be written to be stateless as the constructor 
        /// is not called for every invocation of the plug-in. Also, multiple system threads 
        /// could execute the plug-in at the same time. All per invocation state information 
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            // Construct the Local plug-in context.
            LocalPluginContext localcontext = new LocalPluginContext(serviceProvider);
            InitializeExecutionContext(localcontext);
            try
            {
                // Invoke the customer implementation 
                ExecuteCrmPlugin(localcontext);
                // now exit - if the derived plug-in has incorrectly registered overlapping event registrations,
                // guard against multiple executions.
                return;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Initializes the execution context for the plugin.
        /// </summary>
        /// <param name="localContext">Plugin context for the plug-in being executed.</param>
        protected virtual void InitializeExecutionContext(LocalPluginContext localContext)
        {
            Guid requestId = (localContext.PluginExecutionContext.RequestId != null) ? (Guid)localContext.PluginExecutionContext.RequestId : Guid.Empty;
            var executionContext = new ExecutionContext(
                localContext.OrganizationService,
                localContext.PluginExecutionContext.UserId,
                localContext.PluginExecutionContext.OrganizationId,
                localContext.PluginExecutionContext.CorrelationId,
                requestId);
            ExecutionContextManager.InitiatingExecution(executionContext);
        }
        protected virtual void FinalizeExecutionContext()
        {
            ExecutionContextManager.FinalizingExecution();
        }
        /// <summary>
        /// Placeholder for Custom Plug-in Implementation. 
        /// </summary>
        /// <param name="localcontext">Context for the current plug-in.</param>
        protected abstract void ExecuteCrmPlugin(LocalPluginContext localContext);
    }
}