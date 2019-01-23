using docreminder.InfoShareService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace docreminder.BO
{

    /// <summary>
    /// Defines methods for the handling of processes.
    /// </summary>
    public class ProcessService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ProcessClient ProcessClient;

        public ProcessService()
        {
            this.ProcessClient = new ProcessClient();
        }

        public ProcessService(ProcessClient processClient)
        {
            this.ProcessClient = processClient;
        }

        public ProcessContract CreateProcess(string connectionId, ProcessContract process)
        {
            return ProcessClient.CreateProcess(connectionId, process, false);
        }

        internal ProcessContract UpdateProcess(string connectionId, ProcessContract process)
        {
            return ProcessClient.UpdateProcess(connectionId, process);
        }

        internal ProcessContract ForwardTask(string connectionId, ProcessContract process, string[] userIds)
        {
            return ProcessClient.ForwardTask(connectionId, process.Id, userIds);
        }

        internal StartProcessResultContract StartProcess(string connectionId, ProcessContract process, bool assignUsers)
        {
            return ProcessClient.StartProcess(connectionId, process, assignUsers);
        }
    }
}
