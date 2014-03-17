using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{
    public static class StoreExtensions
    {
        //public static Model LoadModel(this IStore store, out ulong lastEntryId)
        //{
        //    ulong currentEntryId;
        //    Model model = store.LoadMostRecentSnapshot(out currentEntryId);
        //    model.SnapshotRestored();
            
        //    foreach (var command in store.CommandEntriesFrom(currentEntryId + 1))
        //    {
        //        command.Item.Redo(ref model);
        //        currentEntryId = command.Id;
        //    }
        //    model.JournalRestored();
        //    lastEntryId = currentEntryId;
        //    return model;
        //}

        /// <summary>
        /// Get commands beginning from a specific entry id (inclusive)
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns></returns>
        public static IEnumerable<JournalEntry<Command>> CommandEntriesFrom(this IStore store, ulong entryId)
        {
            return CommittedCommandEntries(() => store.GetJournalEntriesFrom(entryId));
        }

        /// <summary>
        /// Get non rolled back commands from a point in time
        /// </summary>
        /// <param name="pointInTime"></param>
        /// <returns></returns>
        public static IEnumerable<JournalEntry<Command>> CommandEntriesFrom(this IStore store, DateTime pointInTime)
        {
            return CommittedCommandEntries(() => store.GetJournalEntriesBeforeOrAt(pointInTime));
        }

        /// <summary>
        /// Select the items of type Command that are not followed by a rollback marker
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static IEnumerable<JournalEntry<Command>> CommittedCommandEntries(Func<IEnumerable<JournalEntry>> enumerator)
        {

            //rollback markers repeat the id of the rolled back command
            JournalEntry<Command> previous = null;

            foreach (JournalEntry current in enumerator.Invoke())
            {
                if (current is JournalEntry<Command>)
                {
                    if (previous != null) yield return previous;
                    previous = (JournalEntry<Command>) current;
                }
                else previous = null;
            }
            if (previous != null) yield return previous;
        }

        /// <summary>
        /// Get the complete sequence of commands starting, excluding any that were rolled back
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<JournalEntry<Command>> CommandEntries( this IStore store)
        {
            return store.CommandEntriesFrom(1);
        }
    }
}