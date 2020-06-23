using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Prism.Mvvm;
namespace Cadwise_FileHandlerUnitTest
{
    public partial class FileHandler : BindableBase
    { 
        public FileHandler()
        {
            m_processesLock = new object();
            BindingOperations.EnableCollectionSynchronization(m_processes, m_processesLock);
        }
        public string From { get; set; }
        public string To { get; set; }
        public int Length { get; set; }
        public bool Removing { get; set; }
        private readonly object m_processesLock;
        public ObservableCollection<ProcessData> Processes
        {
            get { lock (m_processesLock) return new ObservableCollection<ProcessData>(m_processes.Values); }
        }
        public void AddProcess()
        {
            int id = m_processes.Count;
            string title = "Converting " + From + " to " + To;
            var processData = new ProcessData() { Title = title, Percentage = 0, IsDone = false, ID = id };
            lock (m_processesLock)
                m_processes.Add(id, processData);
            RaisePropertyChanged("Processes");
            var args = new FiltrationArgs()
            {
                From = this.From,
                To = this.To,
                Length = this.Length,
                Removing = this.Removing,
                ProcessID = id
            };
            ThreadPool.QueueUserWorkItem(Filter, args);
        }
        public bool IsHandling 
        { 
           get 
           {
                foreach (var process in m_processes.Values)
                    if (!process.IsDone && !process.IsAborted)
                        return true;
                return false;
           }
        }
        public void Filter(object o)
        {
            var args = (FiltrationArgs)o;
            var temp = m_processes[args.ProcessID];
            StreamReader reader = null;
            StreamWriter writer = null;
            try
            {
                var targetDriveName = args.To.Split(new char[] { '\\', '/', ':' })[0];
                var freeSpace = new DriveInfo(targetDriveName).AvailableFreeSpace;
                var fileSize = new FileInfo(args.From).Length;
                if (freeSpace < fileSize)
                {
                    throw new Exception("Not enough space for " + args.To);
                }
                int bufferSize = (int)Math.Min(m_processRAM, fileSize);
                reader = new StreamReader(new FileStream(args.From, FileMode.Open, FileAccess.Read), Encoding.UTF8);
                writer = new StreamWriter(new FileStream(args.To, FileMode.CreateNew, FileAccess.ReadWrite), Encoding.UTF8);
                var filter = new TextFilter(args.Length, args.Removing, bufferSize);
                int bytesRead = 0;
                while (!reader.EndOfStream)
                {
                    var readBuf = new char[bufferSize];
                    reader.Read(readBuf, 0, bufferSize);
                    bytesRead += Encoding.UTF8.GetByteCount(readBuf);
                    filter.FilterBuffer(readBuf);
                    writer.Write(filter.Buffer,0, filter.Size);
                    int percentage = (int)((double)bytesRead * 100 / fileSize);
                    temp.Percentage = percentage;
                    if (percentage >= 100)
                    {
                        temp.IsDone = true;
                    }
                    lock (m_processesLock)
                        m_processes[args.ProcessID] = temp;
                    RaisePropertyChanged("Processes");
                }
            }
            catch (Exception ex)
            {
                temp.IsAborted = true;
                lock (m_processesLock)
                    m_processes[args.ProcessID] = temp;
                RaisePropertyChanged("Processes");
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if(writer!=null)
                    writer.Dispose();
                if(reader!=null)
                   reader.Dispose();
            }
        }
        public void Clear()
        {
            foreach (var key in new List<int>(m_processes.Keys))
            {
                if (m_processes[key].IsDone || m_processes[key].IsAborted)
                {
                    lock (m_processesLock)
                        m_processes.Remove(key);
                    RaisePropertyChanged("Processes");
                }
            }
        }
        private const int m_processRAM = 8192;
        private Dictionary<int, ProcessData> m_processes = new Dictionary<int, ProcessData>();
    }
    public struct FiltrationArgs
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Length { get; set; }
        public bool Removing { get; set; }
        public int ProcessID { get; set; }
    }
    public struct ProcessData
    {
        public string Title
        {
            get
            {
                if (IsDone) 
                    return title + " (Done)";
                if (IsAborted) 
                    return title + " (Aborted)";
                return title;
            }
            set { title = value; }
        }
        public bool IsDone
        {
            get
            {
                if (Percentage == 100) 
                    isDone = true;
                return isDone;
            }
            set { isDone = value; }
        }
        public int Percentage { get; set; }
        public int ID { get; set; }
        public bool IsAborted { get; set; }
        private string title;
        private bool isDone;
    }
}
