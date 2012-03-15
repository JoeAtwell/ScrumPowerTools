﻿using System;
using EnvDTE;
using Microsoft.TeamFoundation.Common;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using ScrumPowerTools.Models;

namespace ScrumPowerTools.Model
{
    public class ShowChangesetItemsModel
    {
        private readonly WorkItemCollector workItemCollector;
        private readonly WorkItemSelectionService workItemSelectionService;
        public event EventHandler<ShowChangesetItemsEventArgs> ShowChangesetItems = delegate {};

        public ShowChangesetItemsModel(DTE dte, DocumentService docService, IVsTeamExplorer teamExplorer)
        {
            workItemSelectionService = new WorkItemSelectionService(dte, docService);
            workItemCollector = new WorkItemCollector(teamExplorer);
        }

        public void Execute()
        {
            var collector = new ChangesetServerItemCollectorStrategy();

            workItemCollector.CollectItems(workItemSelectionService.GetFirstSelected(), collector);

            ShowChangesetItems(this, new ShowChangesetItemsEventArgs(collector.ServerItems));
        }
    }
}