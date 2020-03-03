using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LoopCheckTool.ViewModels
{
    public class WorksheetViewModel : INotifyPropertyChanged
    {
        private bool _hasValidData;

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<ComboBoxItem> Worksheets { get; set; }
        public bool HasValidData
        {
            get
            {
                return _hasValidData;
            }
            set
            {
                _hasValidData = value;
                OnPropertyChanged("HasValidData");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public WorksheetViewModel()
        {
            _hasValidData = false;
            Worksheets = new ObservableCollection<ComboBoxItem>();
        }
    }
}
