namespace <%= solutionName %>.Plugins
{
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using System;
    using System.Linq;
    /// <summary>
    /// Plug-in Context object. 
    /// </summary>
    public class LocalPluginContext
    {
        internal IServiceProvider ServiceProvider { get; private set; }
        /// <summary>
        /// Microsoft Dynamics CRM organization service
        /// </summary>
        internal IOrganizationService OrganizationService { get; private set; }
        private IOrganizationService systemService = null;
        internal IOrganizationService SystemService
        {
            get
            {
                if (systemService == null)
                {
                    systemService = Factory.CreateOrganizationService(null);
                }
                return systemService;
            }
        }
        /// <summary>
        /// IPluginExecutionContext contains information that describes the run-time environment that the plug-in executes, information related to the execution pipeline, and entity business information.
        /// </summary>
        internal IPluginExecutionContext PluginExecutionContext { get; private set; }
        /// <summary>
        /// Synchronous registered plug-ins can post the execution context to the Microsoft Azure Service Bus. <br/> 
        /// It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus.
        /// </summary>
        internal IServiceEndpointNotificationService NotificationService { get; private set; }
        /// <summary>
        /// Provides logging run-time trace information for plug-ins. 
        /// </summary>
        internal ITracingService TracingService { get; private set; }
        private IOrganizationServiceFactory Factory { get; set; }
        private LocalPluginContext()
        {
        }
        /// <summary>
        /// Helper object that stored the services available in this plug-in.
        /// </summary>
        /// <param name="serviceProvider"></param>
        internal LocalPluginContext(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            // Obtain the execution context service from the service provider.
            PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Obtain the tracing service from the service provider.
            TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Get Notification service from the service provider.
            NotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));
            // Obtain the Organization Service factory service from the service provider
            Factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            // Use the factory to generate the Organization Service.
            OrganizationService = Factory.CreateOrganizationService(PluginExecutionContext.UserId);
            this.ServiceProvider = serviceProvider;
        }
        /// <summary>
        /// Writes a Trace Messaged to the CRM Trace Log.
        /// </summary>
        /// <param name="message">Message name to trace.</param>
        internal void Trace(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || TracingService == null)
            {
                return;
            }
            if (PluginExecutionContext == null)
            {
                TracingService.Trace(message);
            }
            else
            {
                TracingService.Trace(
                    "{0}, Correlation Id: {1}, Initiating User: {2}",
                    message,
                    PluginExecutionContext.CorrelationId,
                    PluginExecutionContext.InitiatingUserId);
            }
        }
        /// <summary>
        /// Creates the organization service as the initiaiting user
        /// </summary>
        /// <remarks>
        /// On the Delete action of a connection record, the plugin gets called in the context of the SYSTEM user. 
        /// The only workaround that you have, is that from your plugin, when you create the CRM service you impersonate back the initiating user.
        /// </remarks>
        internal void CreateOrganizationServiceAsInitiatingUser()
        {
            OrganizationService = Factory.CreateOrganizationService(PluginExecutionContext.InitiatingUserId);
        }
        /// <summary>
        /// Gets the proxy implementation for the target entity from the InputParameters.
        /// </summary>
        /// <typeparam name="T">The proxy of the input parameter.</typeparam>
        /// <returns>The proxy for the target entity from the input parameters.</returns>
        internal T GetTargetEntityFromPostEntityImages<T>(string imageName) where T : Entity
        {
            T entity = null;
            if (PluginExecutionContext.PostEntityImages.Contains(imageName))
            {
                entity = PluginExecutionContext.PostEntityImages[imageName].ToEntity<T>();
            }
            return entity;
        }
        /// <summary>
        /// Gets the proxy implementation for the entity from the pre entity images.
        /// </summary>
        /// <typeparam name="T">The proxy type of the entity to be returned.</typeparam>
        /// <returns>The proxy for the entity from the pre entity images.</returns>
        internal T GetTargetEntityFromPreEntityImages<T>(string imageName) where T : Entity
        {
            T entity = null;
            if (PluginExecutionContext.PreEntityImages.Contains(imageName))
            {
                entity = PluginExecutionContext.PreEntityImages[imageName].ToEntity<T>();
            }
            return entity;
        }
        /// <summary>
        /// Gets the target entity from the InputParameters.
        /// </summary>
        /// <param name="entityName">Name of the entity to be retrieved.</param>
        /// <returns>Target entity from the Input parameters.</returns>
        /// <remarks>
        /// If the logical name of the entity does not match the entityName s parameter, then null is returned.
        /// </remarks>
        internal Entity GetTargetEntityFromInputParameters(string entityName)
        {
            Entity entity = this.GetTargetFromInputParameters<Entity>();
            if (entity != null && entity.LogicalName == entityName)
            {
                return entity;
            }
            return null;
        }
        /// <summary>
        /// Gets the proxy implementation for the target entity from the InputParameters.
        /// </summary>
        /// <typeparam name="T">The proxy of the input parameter.</typeparam>
        /// <returns>The proxy for the target entity from the input parameters.</returns>
        internal T GetTargetEntityFromInputParameters<T>() where T : Entity
        {
            var entityName = (typeof(T).GetCustomAttributes(typeof(EntityLogicalNameAttribute),
                true).FirstOrDefault() as EntityLogicalNameAttribute).LogicalName;
            var entity = this.GetTargetEntityFromInputParameters(entityName);
            T targetEntity = null;
            if (entity != null)
            {
                targetEntity = entity.ToEntity<T>();
            }
            return targetEntity;
        }
        /// <summary>
        /// Gets the proxy implementation for the target entity from the EntityReference from InputParameters.
        /// </summary>
        /// <typeparam name="T">Expected proxy type.</typeparam>
        /// <returns>Object of the expected proxy type.</returns>
        internal T GetTargetEntityFromEntityReference<T>() where T : Entity, new()
        {
            EntityReference entityReference = this.GetTargetFromInputParameters<EntityReference>();
            T entity = null;
            var entityLogicalName = (typeof(T).GetCustomAttributes(typeof(EntityLogicalNameAttribute),
                true).FirstOrDefault() as EntityLogicalNameAttribute).LogicalName;
            if (entityReference != null && entityReference.LogicalName == entityLogicalName)
            {
                entity = new T();
                entity.Id = entityReference.Id;
            }
            return entity;
        }
        /// <summary>
        /// Get specific entity from input parameters.
        /// </summary>
        /// <typeparam name="T">Expected type of entity.</typeparam>
        /// <param name="keyName">Name of entity if different from expected type logical name.</param>
        /// <returns>Expected parameter object.</returns>
        internal T GetEntityFromInputParameters<T>(string keyName = null) where T : Entity, new()
        {
            if (keyName == null || keyName == string.Empty)
            {
                keyName = (typeof(T).GetCustomAttributes(typeof(EntityLogicalNameAttribute),
                true).FirstOrDefault() as EntityLogicalNameAttribute).LogicalName;
            }
            T expectedEntity = null;
            Entity entity = null;
            if (PluginExecutionContext.InputParameters.Contains(keyName))
            {
                entity = PluginExecutionContext.InputParameters[keyName] as Entity;
                expectedEntity = entity.ToEntity<T>();
            }
            return expectedEntity;
        }
        /// <summary>
        /// Gets the target entity reference from the InputParameters.
        /// </summary>
        /// <param name="entityName">Name of the entity that is being referenced by the entity reference to be retrieved.</param>
        /// <returns>Target entity reference from the Input parameters.</returns>
        /// <remarks>
        /// If the logical name of the entity does not match the entityName s parameter, then null is returned.
        /// </remarks>
        internal EntityReference GetTargetEntityReferenceFromInputParameters(string entityName)
        {
            EntityReference entityReference = this.GetTargetFromInputParameters<EntityReference>();
            if (entityReference != null && entityReference.LogicalName == entityName)
            {
                return entityReference;
            }
            return null;
        }
        /// <summary>
        /// Indicates if the current Plugin has been invoked from another Plugin or Workflow.
        /// </summary>
        /// <returns>True if the plugin has been invoked from another plugin or workflow and false if it has been invoked from the platform.</returns>
        /// <remarks>
        /// The main goal of this validation is to avoid a plugin from directly or indirectly triggering itself later in the pipeline.
        /// For example, a post-update plugin of a given entity could invoke the update for that entity again, which would invoke the plugin again.
        /// <remarks>
        internal bool HasBeenInvokedFromAnotherPluginOrWorkflow()
        {
            return this.PluginExecutionContext.Depth != 1;
        }
        /// <summary>
        /// Indicates if the current Plugin has been invoked either directly or indirectly from a specific message.
        /// </summary>
        /// <param name="message">The message that the current Plugin might have been invoked from.</param>
        /// <returns>True, if the current Plugin has been invoked either directly or indirectly from the given message.</returns>
        /// <remarks>
        /// This method can be used for optimizations, when it is known that logic is not needed within a specific context.
        /// <remarks>
        internal bool HasBeenInvokedFromMessage(string message)
        {
            IPluginExecutionContext context;
            return HasBeenInvokedFromMessage(message, out context);
        }
        /// <summary>
        /// Indicates if the current Plugin has been invoked either directly or indirectly from a specific message.
        /// </summary>
        /// <param name="message">The message that the current Plugin might have been invoked from.</param>
        /// <param name="invokingContext">The out parameter is set to the context of the invoking plugin.</param>
        /// <returns>True, if the current Plugin has been invoked either directly or indirectly from the given message. 
        /// If true, the invokingContext is set to the context of the invoking plugin. Else, it's set to null.</returns>
        /// <remarks>
        /// This method can be used for optimizations, when it is known that logic is not needed within a specific context.
        /// <remarks>
        internal bool HasBeenInvokedFromMessage(string message, out IPluginExecutionContext invokingContext)
        {
            IPluginExecutionContext context = this.PluginExecutionContext;
            invokingContext = null;
            while (context != null)
            {
                if (context.MessageName.Equals(message, StringComparison.OrdinalIgnoreCase))
                {
                    invokingContext = context;
                    return true;
                }
                context = context.ParentContext;
            }
            return false;
        }
        /// <summary>
        /// Indicates if the current Plugin has been invoked either directly or indirectly from a specific message with the given primary entity.
        /// </summary>
        /// <param name="message">The message that the current Plugin might have been invoked from.</param>
        /// <param name="primaryEntity">The primary entity that the current Plugin might have been for.</param>
        /// <returns>True, if the current Plugin has been invoked either directly or indirectly from the given message.</returns>
        /// <remarks>
        /// This method can be used for optimizations, when it is known that logic is not needed within a specific context.
        /// <remarks>
        internal bool HasBeenInvokedFromMessageWithPrimaryEntity(string message, EntityReference primaryEntity)
        {
            IPluginExecutionContext context = this.PluginExecutionContext;
            while (context != null)
            {
                if (context.MessageName.Equals(message, StringComparison.OrdinalIgnoreCase)
                    && context.PrimaryEntityId == primaryEntity.Id
                    && context.PrimaryEntityName == primaryEntity.LogicalName)
                {
                    return true;
                }
                context = context.ParentContext;
            }
            return false;
        }
        internal bool HasBeenInvokedFromMessageWithEntity(string entityLogicalName)
        {
            IPluginExecutionContext context = this.PluginExecutionContext;
            while (context != null)
            {
                if (context.PrimaryEntityName == entityLogicalName)
                {
                    return true;
                }
                context = context.ParentContext;
            }
            return false;
        }
        /// <summary>
        /// Adds the key value pair to the SharedVariables collection
        /// </summary>
        /// <param name="key">The key of the shared variable</param>
        /// <param name="value">The value of the shared variable</param>
        internal void AddSharedVariable(string key, Object value)
        {
            this.PluginExecutionContext.SharedVariables.Add(key, value);
        }
        /// <summary>
        /// Gets the shared variable from the current context.
        /// </summary>
        /// <typeparam name="T">The type of the shared variable</typeparam>
        /// <param name="key">The key of the shared variable</param>
        /// <returns>The value of the shared variable of type T if found. Null otherwise.</returns>
        internal T GetSharedVariable<T>(string key)
        {
            return this.GetSharedVariableFromContext<T>(key, this.PluginExecutionContext);
        }
        /// <summary>
        /// Gets the shared variable from the parent context.
        /// </summary>
        /// <typeparam name="T">The type of the shared variable</typeparam>
        /// <param name="key">The key of the shared variable</param>
        /// <returns>The value of the shared variable of type T if found. Null otherwise.</returns>
        internal T GetSharedVariableFromParentContext<T>(string key)
        {
            return this.GetSharedVariableFromContext<T>(key, this.PluginExecutionContext.ParentContext);
        }
        /// <summary>
        /// Gets the shared variable from the given context context.
        /// </summary>
        /// <typeparam name="T">The type of the shared variable</typeparam>
        /// <param name="key">The key of the shared variable</param>
        /// <param name="context">The context</param>
        /// <returns>The value of the shared variable of type T if found. Null otherwise.</returns>
        public T GetSharedVariableFromContext<T>(string key, IPluginExecutionContext context)
        {
            T result = default(T);
            if (context != null && context.SharedVariables.Contains(key))
            {
                result = (T)context.SharedVariables[key];
            }
            return result;
        }
        private T GetTargetFromInputParameters<T>()
        {
            if (PluginExecutionContext.InputParameters.Contains(Constants.InputParameterTarget))
            {
                return (T)PluginExecutionContext.InputParameters[Constants.InputParameterTarget];
            }
            return default(T);
        }
    }
}