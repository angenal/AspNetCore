using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace WebFramework
{
    /// <summary>
    /// ApiController Extensions IWorkflowController
    /// </summary>
    public partial class ApiController// : IWorkflowController
    {
        #region Implementation interface : IWorkflowController
        //public IWorkflowHost WorkflowHost => _workflowHost ??= HttpContext.RequestServices.GetService<IWorkflowHost>();
        //private IWorkflowHost _workflowHost;
        /// <summary></summary>
        protected async Task PublishEvent(string eventName, string eventKey, object eventData, System.DateTime? effectiveDate = null)
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
            }
            else
            {
                await host.PublishEvent(eventName, eventKey, eventData, effectiveDate);
            }
        }
        /// <summary></summary>
        protected void RegisterWorkflow<TWorkflow>() where TWorkflow : IWorkflow
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
            }
            else
            {
                host.RegisterWorkflow<TWorkflow>();
            }
        }
        /// <summary></summary>
        protected void RegisterWorkflow<TWorkflow, TData>() where TWorkflow : IWorkflow<TData> where TData : new()
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
            }
            else
            {
                host.RegisterWorkflow<TWorkflow, TData>();
            }
        }
        /// <summary></summary>
        protected async Task<bool> ResumeWorkflow(string workflowId)
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
                return await Task.FromResult(false);
            }
            return await host.ResumeWorkflow(workflowId);
        }
        /// <summary></summary>
        protected async Task<string> StartWorkflow(string workflowId, object data = null, string reference = null)
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
                return await Task.FromResult(string.Empty);
            }
            return await host.StartWorkflow(workflowId, data, reference);
        }
        /// <summary></summary>
        protected async Task<string> StartWorkflow(string workflowId, int? version, object data = null, string reference = null)
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
                return await Task.FromResult(string.Empty);
            }
            return await host.StartWorkflow(workflowId, version, data, reference);
        }
        /// <summary></summary>
        protected async Task<string> StartWorkflow<TData>(string workflowId, TData data = null, string reference = null) where TData : class, new()
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
                return await Task.FromResult(string.Empty);
            }
            return await host.StartWorkflow<TData>(workflowId, data, reference);
        }
        /// <summary></summary>
        protected async Task<string> StartWorkflow<TData>(string workflowId, int? version, TData data = null, string reference = null) where TData : class, new()
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
                return await Task.FromResult(string.Empty);
            }
            return await host.StartWorkflow<TData>(workflowId, version, data, reference);
        }
        /// <summary></summary>
        protected async Task<bool> SuspendWorkflow(string workflowId)
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
                return await Task.FromResult(false);
            }
            return await host.SuspendWorkflow(workflowId);
        }
        /// <summary></summary>
        protected async Task<bool> TerminateWorkflow(string workflowId)
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
                return await Task.FromResult(false);
            }
            return await host.TerminateWorkflow(workflowId);
        }
        #endregion

        #region WorkflowHostExtensions
        protected IEnumerable<Models.DTO.OpenUserAction> GetOpenUserActions(string workflowId)
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
                return System.Array.Empty<Models.DTO.OpenUserAction>();
            }
            return host.GetOpenUserActions(workflowId).Select(t => new Models.DTO.OpenUserAction { Key = t.Key, Prompt = t.Prompt, AssignedPrincipal = t.AssignedPrincipal, Options = t.Options });
        }
        protected async Task PublishUserAction(string actionKey, string user, object value)
        {
            var host = HttpContext.RequestServices.GetService<IWorkflowHost>();
            if (host == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <IWorkflowHost> from the ServiceProvider.");
            }
            else
            {
                await host.PublishUserAction(actionKey, user, value);
            }
        }
        #endregion
    }
}
