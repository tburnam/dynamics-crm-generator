namespace <%= solutionName %>.Plugins
{
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// This class stores the context which is shared by code handling the same message.
    /// </summary>
    /// <see cref="ExecutionContextManager"/>
    internal class ExecutionContext
    {
        /// <summary>
        /// Creates a new instance of execution context.
        /// </summary>
        /// <param name="organizationService">The organization service being used in current request.</param>
        /// <param name="initiatingUserId">The ID of the user who initiated the request.</param>
        /// <param name="organizationId">The ID of the organization for which the request is being processed.</param>
        internal ExecutionContext(
            IOrganizationService organizationService,
            Guid initiatingUserId,
            Guid organizationId,
            Guid correlationId = new Guid(),
            Guid requestId = new Guid())
        {
            this.OrganizationService = organizationService;
            this.InitiatingUserId = initiatingUserId;
            this.OrganizationId = organizationId;
            this.CorrelationId = correlationId;
            this.RequestId = requestId;
            this.EventContainer = new LinkedList<String>();
        }
        /// <summary>
        /// The ID of the user who sent the message request.
        /// </summary>
        internal Guid InitiatingUserId { get; private set; }
        /// <summary>
        /// The <c>IOrganizationService</c> being used to process the current message.
        /// </summary>
        internal IOrganizationService OrganizationService { get; private set; }
        /// <summary>
        /// The ID of the organization where the message is being processed.
        /// </summary>
        internal Guid OrganizationId { get; private set; }
        /// <summary>
        /// Correlation Id
        /// </summary>
        internal Guid CorrelationId { get; private set; }
        /// <summary>
        /// Request Id which is consistent across a web request
        /// </summary>
        internal Guid RequestId { get; private set; }
        /// <summary>
        /// List that acts as a container where tracing information for the plugins can be stored
        /// </summary>
        internal LinkedList<String> EventContainer { get; private set; }
    }
}