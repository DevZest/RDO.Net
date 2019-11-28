using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;

namespace DevZest.Data.Tools
{
    class DynamicItemMenuCommand : OleMenuCommand
    {
        public DynamicItemMenuCommand(CommandID rootId, Predicate<int> matches, EventHandler invokeHandler, EventHandler beforeQueryStatusHandler)
            : base(invokeHandler, null /*changeHandler*/, beforeQueryStatusHandler, rootId)
        {
            _matches = matches ?? throw new ArgumentNullException(nameof(matches));
        }

        private readonly Predicate<int> _matches;

        public override bool DynamicItemMatch(int cmdId)
        {
            // Call the supplied predicate to test whether the given cmdId is a match.  
            // If it is, store the command id in MatchedCommandid   
            // for use by any BeforeQueryStatus handlers, and then return that it is a match.  
            // Otherwise clear any previously stored matched cmdId and return that it is not a match.  
            if (_matches(cmdId))
            {
                MatchedCommandId = cmdId;
                return true;
            }

            MatchedCommandId = 0;
            return false;
        }
    }
}
