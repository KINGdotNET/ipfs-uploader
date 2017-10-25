using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IpfsUploader.Models;

namespace IpfsUploader.Managers
{
    public static class ProgressManager
    {
        private static ConcurrentDictionary<Guid, FileContainer> progresses = new ConcurrentDictionary<Guid, FileContainer>();

        public static void RegisterProgress(FileContainer fileContainer)
        {
            progresses.TryAdd(fileContainer.ProgressToken, fileContainer);

            // Supprimer le suivi progress après 1j
            Task taskClean = Task.Run(() =>
            {
                Thread.Sleep(24 * 60 * 60 * 1000); // 1j
                FileContainer thisFileContainer;
                progresses.TryRemove(fileContainer.ProgressToken, out thisFileContainer);
            });
        }

        public static FileContainer GetFileContainerByToken(Guid progressToken)
        {
            FileContainer fileContainer;
            progresses.TryGetValue(progressToken, out fileContainer);
            return fileContainer;
        }

        public static FileContainer GetFileContainerBySourceHash(string sourceHash)
        {
            return  progresses.Values
                .OrderByDescending(s => s.SourceFileItem.IpfsLastTimeProgressChanged)
                .FirstOrDefault(s => s.SourceFileItem.IpfsHash == sourceHash);
        }
    }
}