using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Shell;
using ScrumPowerTools.Framework.Composition;
using ScrumPowerTools.Framework.Presentation;
using ScrumPowerTools.Model;
using ScrumPowerTools.Packaging;
using ScrumPowerTools.TfsIntegration;
using ScrumPowerTools.Views;

namespace ScrumPowerTools.ViewModels
{
    [Export(typeof(IMenuCommandHandler))]
    [HandlesMenuCommand(MenuCommands.ShowAffectedChangesetFiles)]
    [HandlesMenuCommand(MenuCommands.ShowChangesetsWithAffectedFiles)]
    [HandlesMenuCommand(MenuCommands.ShowReviewWindow)]
    public class WorkItemContextMenuViewModel : IMenuCommandHandler
    {
        private readonly GeneralOptions options;
        private readonly WorkItemSelectionService workItemSelectionService;
        private readonly ITeamProjectCollectionProvider teamProjectCollectionProvider;

        private readonly Dictionary<int, Action> commandHandlerMappings;

        [ImportingConstructor]
        public WorkItemContextMenuViewModel(GeneralOptions options, WorkItemSelectionService workItemSelectionService,
                                            ITeamProjectCollectionProvider teamProjectCollectionProvider)
        {
            this.options = options;
            this.workItemSelectionService = workItemSelectionService;
            this.teamProjectCollectionProvider = teamProjectCollectionProvider;

            commandHandlerMappings = new Dictionary<int, Action>
            {
                {MenuCommands.ShowAffectedChangesetFiles, ShowAffectedChangesetFiles},
                {MenuCommands.ShowChangesetsWithAffectedFiles, ShowChangesetsWithAffectedFiles},
                {MenuCommands.ShowReviewWindow, ShowReviewWindow}
            };
        }

        private void ShowChangesetsWithAffectedFiles()
        {
            TfsTeamProjectCollection tpc = teamProjectCollectionProvider.GetCurrent();
            var workItemStore = tpc.GetService<WorkItemStore>();
            var versionControlServer = tpc.GetService<VersionControlServer>();
            var workItemCollector = new WorkItemCollector(workItemStore, versionControlServer);
            var model = new ShowChangesetsModel(workItemSelectionService, workItemCollector);

            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            var view = new ShowChangesetsView(dte);

            view.ConnectTo(model);

            model.Execute();
        }

        private void ShowAffectedChangesetFiles()
        {
            TfsTeamProjectCollection tpc = teamProjectCollectionProvider.GetCurrent();
            var workItemStore = tpc.GetService<WorkItemStore>();
            var versionControlServer = tpc.GetService<VersionControlServer>();
            var workItemCollector = new WorkItemCollector(workItemStore, versionControlServer);
            var model = new ShowChangesetItemsModel(workItemSelectionService, workItemCollector);

            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            var view = new ShowChangesetItemsView(dte);

            view.ConnectTo(model);

            model.Execute();
        }

        private void ShowReviewWindow()
        {
            if (workItemSelectionService.HasSelection())
            {
                IoC.GetInstance<EventAggregator>().Publish(new ShowReviewWindowMessage()
                {
                    WorkItemId = workItemSelectionService.GetFirstSelected()
                });
            }
        }

        public void Execute(int commandId)
        {
            commandHandlerMappings[commandId]();
        }

        public bool CanExecute(int commandId)
        {
            return options.IsEnabled(commandId) && workItemSelectionService.HasSelection();
        }
    }
}