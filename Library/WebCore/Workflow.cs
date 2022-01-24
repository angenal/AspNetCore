using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Plugins;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using WebInterface.Settings;

namespace WebCore
{
    /// <summary>
    /// Workflow manager.
    /// </summary>
    public class Workflow
    {
        public WorkflowRuntime Runtime { get; private set; }
        public IContainer Container { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration, string autofac = "autofac.json")
        {
            // Create the container builder.
            var builder = new ContainerBuilder();
            builder.Populate(services);

            var config = new ConfigurationBuilder();
            config.AddJsonFile(autofac);

            var module = new ConfigurationModule(config.Build());
            builder.RegisterInstance(configuration);
            builder.RegisterModule(module);
            Container = builder.Build();

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(Container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            //Runtime = WorkflowInit.Create(new DataServiceProvider(Container));
        }

        private static WorkflowRuntime CreateRuntime(Func<List<string>, List<string>> getUserNamesByIds = null, UsersInRoleAsyncDelegate usersInRoleAsync = null, SmtpSettings smtpSettings = null)
        {
            var loopPlugin = new LoopPlugin();
            var filePlugin = new FilePlugin();
            var approvalPlugin = new ApprovalPlugin();

            #region ApprovalPlugin Settings

            if (getUserNamesByIds != null)
            {
                approvalPlugin.GetUserNamesByIds += getUserNamesByIds;
                // approvalPlugin.AutoApprovalHistory = true;
                // approvalPlugin.NameParameterForComment = "Comment";
            }

            #endregion ApprovalPlugin Settings

            var basicPlugin = new BasicPlugin();

            #region BasicPlugin Settings

            //Settings for SendEmail actions
            if (smtpSettings != null)
            {
                basicPlugin.Setting_Mailserver = smtpSettings.Host;
                basicPlugin.Setting_MailserverPort = smtpSettings.Port;
                basicPlugin.Setting_MailserverFrom = smtpSettings.Username;
                basicPlugin.Setting_MailserverLogin = smtpSettings.Username;
                basicPlugin.Setting_MailserverPassword = smtpSettings.Password;
                basicPlugin.Setting_MailserverSsl = true;
            }

            basicPlugin.UsersInRoleAsync += usersInRoleAsync;

            #endregion BasicPlugin Settings

            var provider = DataServiceProvider.Get<IPersistenceProviderContainer>().Provider;

            var externalParametersProvider = new ExternalParametersProvider();
            externalParametersProvider.GetDocument += GetDocument;

            var builder = new WorkflowBuilder<XElement>(provider, new XmlWorkflowParser(), provider).WithDefaultCache();
            var runtime = new WorkflowRuntime();

            runtime.GetUsersWithIdentitiesAsync += GetUsersWithIdentitiesAsync;
            runtime.GetUserByIdentityAsync += GetUserById;
            runtime.AssignmentApi.OnUpdateAssignment += OnUpdateAssignment;
            runtime.AssignmentApi.OnPostUpdateAssignment += OnPostUpdateAssignment;
            runtime.GetCustomTimerValueAsync += GetCustomTimerValueAsync;
            runtime.GetCustomTimerValueAsync += GetCustomTimerValueAsync;

            return runtime.WithBuilder(builder)
                .WithActionProvider(new ActionProvider(DataServiceProvider))
                .WithRuleProvider(new RuleProvider(DataServiceProvider))
                .WithDesignerAutocompleteProvider(new AutoCompleteProvider())
                .WithPersistenceProvider(provider)
                .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                .RegisterAssemblyForCodeActions(Assembly.GetEntryAssembly())
                .WithPlugins(null, basicPlugin, loopPlugin, filePlugin, approvalPlugin, assignmentPlugin)
                .WithExternalParametersProvider(externalParametersProvider)
                .CodeActionsDebugOn()
                .AsSingleServer() //.AsMultiServer()
                                  //.WithConsoleAllLogger()
                .Start();
        }
    }
    //using WorkflowCore.Interface;
    //using WorkflowCore.Models;
    //public class Workflow1 : IWorkflow
    //{
    //    public string Id { get; set; } = "1";

    //    public int Version { get; set; } = 1;

    //    public void Build(IWorkflowBuilder<object> builder)
    //    {
    //        builder.StartWith(context => ExecutionResult.Next())
    //            .UserTask("Do you approve", data => @"domain\bob")
    //            .WithOption("yes", "I approve").Do(then => then.StartWith(context => Console.WriteLine("You approved")))
    //            .WithOption("no", "I do not approve").Do(then => then.StartWith(context => Console.WriteLine("You did not approve")))
    //            .WithEscalation(x => TimeSpan.FromSeconds(20), x => @"domain\frank", action => action.StartWith(context => Console.WriteLine("Escalated task")).Then(context => Console.WriteLine("Sending notification...")))
    //            .Then(context => Console.WriteLine("end"));
    //    }
    //}
}
