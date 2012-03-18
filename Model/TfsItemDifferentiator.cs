using Microsoft.TeamFoundation.VersionControl.Client;
using ScrumPowerTools.Interfaces;

namespace ScrumPowerTools.Model
{
    internal class TfsItemDifferentiator
    {
        private readonly VersionControlServer versionControlServer;

        public TfsItemDifferentiator()
        {
            var tpc = IoC.GetInstance<ITeamProjectCollectionProvider>().GetCurrent();

            versionControlServer = tpc.GetService<VersionControlServer>();
        }

        public void CompareWithPreviousVersion(string serverItem, int changesetId)
        {
            var previousVersionId = GetPreviousChangesetId(serverItem, changesetId);

            var itemFrom = new DiffItemVersionedFile(versionControlServer, serverItem, new ChangesetVersionSpec(previousVersionId));
            var itemTo = new DiffItemVersionedFile(versionControlServer, serverItem, new ChangesetVersionSpec(changesetId));

            Difference.VisualDiffItems(versionControlServer, itemFrom, itemTo);
        }

        private int GetPreviousChangesetId(string serverItem, int changesetId)
        {
            VersionSpec version = new ChangesetVersionSpec(changesetId);

            var itemChangesetHistory = versionControlServer.QueryHistory(serverItem, version, 0, RecursionType.Full, null, null,
                version, 2, false, false);

            int previousVersionId = changesetId;
            foreach (Changeset changeset in itemChangesetHistory)
            {
                previousVersionId = changeset.ChangesetId;
            }
            return previousVersionId;
        }
    }
}