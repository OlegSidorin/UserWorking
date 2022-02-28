
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UserWorking;
using ZetaLongPaths;

namespace UserWorking
{
    class MainWindowModel :INotifyPropertyChanged
    {
        public ObservableCollection<UserAction> UserActions { get; set; }
        public MainWindowModel()
        {
            _userName = "Пользователь";
            UserActions = new ObservableCollection<UserAction>();
        }
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged();
                }
            }
        }


        #region on property changed
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        private ICommand openJournalFile;

        public ICommand OpenJournalFile
        {
            get
            {
                if (openJournalFile == null)
                {
                    openJournalFile = new RelayCommand(PerformOpenJournalFile);
                }

                return openJournalFile;
            }
        }

        private void PerformOpenJournalFile(object commandParameter)
        {
            CommonOpenFileDialog openDialog = new CommonOpenFileDialog();
            if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = openDialog.FileName;
                string[] text = FileContent(FilePath);
                UserName = GetUserName(text);
                GetData(text);



            }
            //MessageBox.Show(FilePath);

           

            //SaveDestination = result;

        }
        #endregion

        public string GetUserName(string[] text)
        {
            string name = string.Empty;
            int indexOfDerective = 0;
            int indexOfFirstQuota = 0;
            int indexOfSecondQuota = 0;
            int i = 0;
            foreach (string s in text)
            {
                if (s.Contains("Username"))
                {
                    string str = text[i + 1];
                    indexOfFirstQuota = str.IndexOf(@"""", 0);
                    indexOfSecondQuota = str.IndexOf(@"""", indexOfFirstQuota + 1);

                    return str.Substring(indexOfFirstQuota + 1, indexOfSecondQuota - indexOfFirstQuota - 1);
                }
                i += 1;
            }
            
            return "";
        }

        public void GetData(string[] text)
        {
            int i = 0;
            foreach (string s in text)
            {
                if (s.Contains("Jrn.Command"))
                {
                    //string str = text[i + 1];
                    UserAction uA = new UserAction()
                    {
                        ActionId = "Command"
                    };
                    int indexOfFirstQuota = s.IndexOf(@"""", 11);
                    int indexOfSecondQuota = s.IndexOf(@"""", indexOfFirstQuota + 1);
                    uA.ActionName = s.Substring(indexOfFirstQuota + 1, indexOfSecondQuota - indexOfFirstQuota - 1);
                    UserActions.Add(uA);
                }
                i += 1;
            }
        }

        public string[] FileContent(string path)
        {
            //path = @"c:\temp\MyTest.txt";


            // This text is added only once to the file.
            if (!ZlpIOHelper.FileExists(path))
            {
                MessageBox.Show("Файла не существует");
            }

            // Open the file to read from.
            string[] readText = ZlpIOHelper.ReadAllLines(path, UTF8Encoding.UTF8);
            return readText;
        }
    }
}
