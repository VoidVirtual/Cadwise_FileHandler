using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using Prism.Commands;
namespace Cadwise_FileHandler
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            vm = new FileHandlerVM();
            this.DataContext = vm;
        }
        private void ClosingWindow(object sender, CancelEventArgs e)
        {
            if (!vm.IsHandling)
                return;
                var res = MessageBox.Show(
                "The program is currently handling files. Do you want to exit anyway?",
                "Simple File Handler",
                MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No)
                e.Cancel = true;
        }
        private FileHandlerVM vm;
    }
    public partial class FileHandlerVM : BindableBase
    {
        public FileHandlerVM()
        {
            m_model.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            FilterCommand = new DelegateCommand<object>(obj =>
            {
                m_model.AddProcess();
            });
            ClearCommand = new DelegateCommand<object>(obj =>
            {
                m_model.Clear();
            });
        }
        public bool IsHandling {get {return m_model.IsHandling; } }
        public string From
        {
            get {return m_model.From;}
            set {m_model.From = value; }
        }
        public string To
        {
            get { return m_model.To; }
            set { m_model.To = value; }
        }
        public int Length
        {
            get { return m_model.Length; }
            set { m_model.Length = value; }
        }
        public bool Removing
        {
            get { return m_model.Removing; }
            set { m_model.Removing = value; }
        }
        public ObservableCollection<ProcessData> Processes { get { return m_model.Processes; } }
        public DelegateCommand<object> FilterCommand { get; }
        public DelegateCommand<object> ClearCommand { get; }
        readonly FileHandler m_model = new FileHandler();
    }
}
