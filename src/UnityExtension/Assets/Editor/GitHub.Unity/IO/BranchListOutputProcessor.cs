using System;
using System.Text.RegularExpressions;
using GitHub.Unity.Logging;

namespace GitHub.Unity
{
    class BranchListOutputProcessor : BaseOutputProcessor
    {
        private static readonly Regex trackingBranchRegex = new Regex(@"\[[\w]+\/.*\]");

        private static readonly ILogger logger = Logger.GetLogger<BranchListOutputProcessor>();

        public event Action<GitBranch> OnBranch;

        public override void LineReceived(string line)
        {
            base.LineReceived(line);

            if (line == null || OnBranch == null)
                return;

            var proc = new LineParser(line);
            if (proc.IsAtEnd)
                return;

            var active = proc.Matches('*');
            proc.SkipWhitespace();
            var detached = proc.Matches("(HEAD ");
            var name = "detached";
            if (detached)
            {
                proc.MoveToAfter(')');
            }
            else
            {
                name = proc.ReadUntilWhitespace();
            }
            proc.SkipWhitespace();
            proc.ReadUntilWhitespace();
            var tracking = proc.Matches(trackingBranchRegex);
            var trackingName = "";
            if (tracking)
            {
                trackingName = proc.ReadChunk('[', ']');
            }

            logger.Debug("Branch - Name: " + name + " TrackedAs: " + trackingName + " Active: " + active);

            var branch = new GitBranch(name, trackingName, active);

            OnBranch(branch);
        }
    }
}