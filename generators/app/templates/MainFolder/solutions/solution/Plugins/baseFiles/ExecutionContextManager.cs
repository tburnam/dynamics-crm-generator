namespace <%= solutionName %>.Plugins
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    /// <summary>
    /// This class manages the <c>ExecutionContext</c> initialization and finalization by service handlers (e.g. plugin base class)
    /// as well as its retrieval by subsequent code processing the service call.
    /// </summary>
    /// <remarks>
    /// In CRM plugins in general we cannot use global/static class variables due to the multi-threaded nature of the execution flow, with
    /// the platform caching the plugin code for better performance.
    /// However, every code the handles a given message run in the same thread and share some basic properties, e.g. organization service, 
    /// user ID, organization, etc.
    /// Those are managed by the platform and handled over to the plugin or workflow base class which then allow them to be passed along to other classes.
    /// Since the code runs within the context of the same thread, it is possible for them to share this context, as if they were global variables, without 
    /// having to pass them as parameters to all the interested classes by using the executing thread's ID as an identifier to retrieve such context.
    /// </remarks>
    internal static class ExecutionContextManager
    {
        private static readonly ConcurrentDictionary<int, Stack<ExecutionContext>> contextPerThreadId = new ConcurrentDictionary<int, Stack<ExecutionContext>>();
        /// <summary>
        /// Indicates that an execution thread is starting, caching an execution context for the thread.
        /// </summary>
        /// <param name="currentContext">The <c>ExecutionContext</c> with the context for the thread that will start its execution.</param>
        /// <remarks>FinalizingExecution should be called at the end of the thread execution by the base class
        /// to deallocate the resources.</remarks>
        internal static void InitiatingExecution(ExecutionContext currentContext)
        {
            var currentThreadId = GetCurrentThreadId();
            if (!contextPerThreadId.ContainsKey(currentThreadId))
            {
                contextPerThreadId[currentThreadId] = new Stack<ExecutionContext>();
            }
            contextPerThreadId[currentThreadId].Push(currentContext);
        }
        /// <summary>
        /// Indicates that a thread has done its job in the current context.
        /// </summary>
        internal static void FinalizingExecution()
        {
            var currentThreadId = GetCurrentThreadId();
            if (contextPerThreadId.ContainsKey(currentThreadId))
            {
                Stack<ExecutionContext> executionContexts = contextPerThreadId[currentThreadId];
                if (executionContexts != null && (executionContexts.Count == 0 || executionContexts.Count == 1))
                {
                    contextPerThreadId.TryRemove(currentThreadId, out executionContexts);
                }
                else
                {
                    contextPerThreadId[currentThreadId].Pop();
                }
            }
        }
        /// <summary>
        /// Retrieves the <c>ExecutionContext</c> for the running thread.
        /// </summary>
        /// <returns>The <c>ExecutionContext</c> for the running thread.</returns>
        internal static ExecutionContext GetCurrentContext()
        {
            var currentThreadId = GetCurrentThreadId();
            ExecutionContext executionContext = null;
            if (contextPerThreadId.ContainsKey(currentThreadId))
            {
                executionContext = contextPerThreadId[currentThreadId].Peek();
            }
            return executionContext;
        }
        /// <summary>
        /// Retrieves the thread ID which is used internally as the key for the execution context.
        /// </summary>
        /// <returns>The current thread's ID.</returns>
        private static int GetCurrentThreadId()
        {
            var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            return currentThreadId;
        }
    }
}
