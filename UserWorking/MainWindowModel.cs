
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UserWorking;
using ZetaLongPaths;
using Newtonsoft;
using Newtonsoft.Json;

namespace UserWorking
{
    class MainWindowModel :INotifyPropertyChanged
    {

        #region Fields

        public ObservableCollection<UserAction> UserActions { get; set; }

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

        private ICommand _openJournalFile;

        public ICommand OpenJournalFile
        {
            get
            {
                if (_openJournalFile == null)
                {
                    _openJournalFile = new RelayCommand(PerformOpenJournalFile);
                }

                return _openJournalFile;
            }
        }

        #endregion

        #region Constructor
        public MainWindowModel()
        {
            _userName = "Пользователь";
            UserActions = new ObservableCollection<UserAction>();
        }
        #endregion

        #region on property changed
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }


        #endregion

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
                // >' 0:< this journal = C:\Users\o.sidorin\AppData\Local\Autodesk\Revit\Autodesk Revit 2021\Journals\journal.2245.txt 
                if (s.Contains("0:< this journal = "))
                {
                    //string str = text[i + 1];

                    // индексы для названия команды  >'C 28-Feb-2022 14:40:42.172;  Journal Init 
                    //int indexOfFirstQuotaActionName = s.IndexOf(@"Journal Init");
                    //int indexOfSecondQuotaActionName = s.IndexOf(@"Journal Init") + 12;


                    // индексы для описания 
                    //int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    //int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды >'C 28-Feb-2022 14:40:42.172;  Journal Init 
                    //string timeString = s;
                    //int indexOfStartTime = 3;
                    //int indexOfEndTime = timeString.IndexOf("; ");
                    //timeString = timeString.Substring(indexOfStartTime, indexOfEndTime - indexOfStartTime);

                    // расознавание времени из строчки
                    //DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss.fff", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = "Journal File",
                        ActionName = "Файл журнала",
                        FilePath = s.Substring(21),
                        ActionTime = "",
                        ActionRUTime = ""
                    };


                    UserActions.Add(uA);
                }
                else if (s.Contains("Journal Init"))
                {
                    //string str = text[i + 1];

                    // индексы для названия команды  >'C 28-Feb-2022 14:40:42.172;  Journal Init 
                    int indexOfFirstQuotaActionName = s.IndexOf(@"Journal Init");
                    int indexOfSecondQuotaActionName = s.IndexOf(@"Journal Init") + 12;


                    // индексы для описания 
                    //int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    //int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды >'C 28-Feb-2022 14:40:42.172;  Journal Init 
                    string timeString = s;
                    int indexOfStartTime = 3;
                    int indexOfEndTime = timeString.IndexOf("; ");
                    timeString = timeString.Substring(indexOfStartTime, indexOfEndTime - indexOfStartTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss.fff", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = "Journal Init",
                        ActionName = s.Substring(indexOfFirstQuotaActionName, indexOfSecondQuotaActionName - indexOfFirstQuotaActionName),
                        ActionDescription = "Старт записи журнала",
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };


                    UserActions.Add(uA);
                }
                else if (s.Contains(@" Jrn.Data ""Transaction Successful"""))
                {
                    string str = text[i + 1];
                    // >         , "Редактировать"
                    // >         , "Воздуховод"
                    int indexTransactonName01 = str.IndexOf(@" """) + 2;
                    int indexTransactonName02 = str.Length - 1;
                    int lenghtTransactonName = indexTransactonName02 - indexTransactonName01;


                    // индексы для названия команды  > Jrn.Command "Internal" , "Выход из программы с предложением сохранить проекты , ID_APP_EXIT"
                    //int indexOfFirstQuotaActionName = s.IndexOf(@"""", 11);
                    //int indexOfSecondQuotaActionName = s.IndexOf(@"""", indexOfFirstQuotaActionName + 1);


                    // индексы для описания 
                    //int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    //int indexOfSecondQuotaActionDescription = s.Length - 1;

                    // >  'H 25-Feb-2022 10:20:22.545;   1:< 
                    // >  'H 25-Feb-2022 10:34:51.010;   1:< 
                    // > 'H 05-Mar-2022 11:16:05.811;   1:< 
                    string timeString = text[i - 1];
                    int indexOfStartTime = timeString.IndexOf("'H") + 3;
                    int indexOfEndTime = timeString.IndexOf(";  ");
                    timeString = timeString.Substring(indexOfStartTime, 20);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    UserAction uA = new UserAction()
                    {
                        ActionId = "Transaction",
                        ActionName = str.Substring(indexTransactonName01, lenghtTransactonName),
                        ActionDescription = str.Substring(indexTransactonName01, lenghtTransactonName),
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };


                    UserActions.Add(uA);
                }
                else if (s.Contains("DirSnap(-1V) Snap("))
                {
                    int index11 = s.IndexOf("DirSnap(-1V) Snap(") + 12;
                    int index12 = s.IndexOf("-", index11);

                    UserAction uA = new UserAction()
                    {
                        ActionId = "Edit Element",
                        ActionName = "Редактирование элемента",
                        ActionDescription = s.Substring(index11, index12 - index11),
                        ActionTime = "",
                        ActionRUTime = ""
                    };

                    UserActions.Add(uA);

                }
                else if (s.Contains("Jrn.Command") && s.IndexOf("Jrn.Command") == 1)
                {
                    //string str = text[i + 1];

                    // индексы для названия команды  > Jrn.Command "Internal" , "Выход из программы с предложением сохранить проекты , ID_APP_EXIT"
                    int indexOfFirstQuotaActionName = s.IndexOf(@"""", 11);
                    int indexOfSecondQuotaActionName = s.IndexOf(@"""", indexOfFirstQuotaActionName + 1);


                    // индексы для описания 
                    int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды, она выше > 'E 28-Feb-2022 14:42:11.508;   0:< 
                    string timeString = text[i - 1];
                    int indexOfStartTime = timeString.IndexOf(" 'E") + 4;
                    int indexOfEndTime = timeString.IndexOf(";   ");
                    timeString = timeString.Substring(indexOfStartTime, indexOfEndTime - indexOfStartTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss.fff", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = "Command",
                        ActionName = s.Substring(indexOfFirstQuotaActionName + 1, indexOfSecondQuotaActionName - indexOfFirstQuotaActionName - 1),
                        ActionDescription = s.Substring(indexOfFirstQuotaActionDescription + 3).Substring(0, s.Substring(indexOfFirstQuotaActionDescription + 3).Length - 1),
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };
                    

                    UserActions.Add(uA);
                }
                else if (s.Contains("Jrn.Close") && s.IndexOf("Jrn.Close") == 1)
                {
                    //string str = text[i + 1];

                    // индексы для названия команды  > Jrn.Close "[Проект2.rvt]" , "План этажа: Уровень 1"
                    int indexOfFirstQuotaActionName = s.IndexOf(@"""", 10);
                    int indexOfSecondQuotaActionName = s.IndexOf(@"""", indexOfFirstQuotaActionName + 1);


                    // индексы для описания 
                    int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды, она выше > 'E 28-Feb-2022 14:42:11.508;   0:< 
                    string timeString = text[i - 1];
                    int indexOfStartTime = timeString.IndexOf(" 'E") + 4;
                    int indexOfEndTime = timeString.IndexOf(";   ");
                    timeString = timeString.Substring(indexOfStartTime, indexOfEndTime - indexOfStartTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss.fff", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = "Close",
                        ActionName = s.Substring(indexOfFirstQuotaActionName + 1, indexOfSecondQuotaActionName - indexOfFirstQuotaActionName - 1),
                        ActionDescription = s.Substring(indexOfFirstQuotaActionDescription + 3).Substring(0, s.Substring(indexOfFirstQuotaActionDescription + 3).Length - 1),
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };


                    UserActions.Add(uA);
                }
                else if (s.Contains("Jrn.RibbonEvent") && s.IndexOf("Jrn.RibbonEvent") == 1 && s.Contains("ModelBrowserOpenDocumentEvent:open:"))
                {
                    //string str = text[i + 1];

                    // > Jrn.RibbonEvent "ModelBrowserOpenDocumentEvent:open:{""projectGuid"":null,""modelGuid"":null,""id"":""C:\\Users\\o.sidorin\\Downloads\\Проект2.rvt"",""displayName"":""Проект2"",""region"":null}"

                    //int indexOfFirstQuotaActionName = s.IndexOf(@"""", 10);
                    //int indexOfSecondQuotaActionName = s.Length - 1;
                    string json = s.Substring(53, s.Length - 54);
                    json = json.Replace(@"""""", @""""); //.Replace(@"\\", @"\");
                    FileOpen openFile = JsonConvert.DeserializeObject<FileOpen>(json);

                    // индексы для описания 
                    //int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    //int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды, она выше > 'E 28-Feb-2022 14:42:11.508;   0:< 
                    string timeString = text[i - 1];
                    int indexOfStartTime = timeString.IndexOf(" 'E") + 4;
                    int indexOfEndTime = timeString.IndexOf(";   ");
                    timeString = timeString.Substring(indexOfStartTime, indexOfEndTime - indexOfStartTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss.fff", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = "RibbonEvent",
                        ActionName = "Открытие документа",
                        FilePath = openFile.id,
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };

                    UserActions.Add(uA);
                }
                else if (s.Contains(@"[ISL] On open,"))
                {

                    // > 'E 25-Feb-2022 10:16:25.400;   9:< 
                    // > ' [Jrn.BasicFileInfo] Rvt.Attr.Worksharing: Not enabled Rvt.Attr.Username:  Rvt.Attr.CentralModelPath:  Rvt.Attr.RevitBuildVersion: Autodesk Revit 2021 (Build: 20211018_1515(x64)) Rvt.Attr.LastSavePath: C:\Users\o.sidorin\Downloads\Проект1.rvt Rvt.Attr.LTProject: notLTProject Rvt.Attr.LocaleWhenSaved: RUS Rvt.Attr.FileExt: rvt 
                    // > 'C 25-Feb-2022 10:16:25.310;  [ISL] On open, Adler Checksum: 0x862965e9 [C:\Users\o.sidorin\Downloads\Проект1.rvt] 
                    // > ' 9:< [ISL] Read last modification time: 26-Jan-2022 12:53:27 (UTC: 26-Jan-2022 09:53:27) 

                    string actionId = "On Open";

                    int indexFilePath001 = s.IndexOf(@"[ISL] On save,");
                    int indexFilePath01 = s.IndexOf(@"[", indexFilePath001 + 1) + 1;
                    int indexFilePath02 = s.Length - 2;
                    int lenghtFilePath = indexFilePath02 - indexFilePath01;

                    string filePath = s.Substring(indexFilePath01, lenghtFilePath);

                    int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    int indexOfSecondQuotaActionDescription = s.Length - 1;

                    string timeString = s;
                    int indexTime01 = timeString.IndexOf("'C") + 3;
                    int indexTime02 = indexTime01 + 20;
                    int lenghtTime = 20;
                    timeString = timeString.Substring(indexTime01, lenghtTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = actionId,
                        ActionName = "Сохранить файл",
                        FilePath = filePath,
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };


                    UserActions.Add(uA);
                }
                else if (s.Contains("Jrn.RibbonEvent") && s.IndexOf("Jrn.RibbonEvent") == 1)
                {
                    //string str = text[i + 1];

                    // индексы для названия команды  > Jrn.RibbonEvent "TabActivated:Архитектура"
                    // > Jrn.RibbonEvent "ModelBrowserOpenDocumentEvent:open:{""projectGuid"":null,""modelGuid"":null,""id"":""C:\\Users\\o.sidorin\\Downloads\\Проект2.rvt"",""displayName"":""Проект2"",""region"":null}"

                    int indexOfFirstQuotaActionName = s.IndexOf(@"""", 10);
                    int indexOfSecondQuotaActionName = s.Length - 1;


                    // индексы для описания 
                    //int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    //int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды, она выше > 'E 28-Feb-2022 14:42:11.508;   0:< 
                    string timeString = text[i - 1];
                    int indexOfStartTime = timeString.IndexOf(" 'E") + 4;
                    int indexOfEndTime = timeString.IndexOf(";   ");
                    timeString = timeString.Substring(indexOfStartTime, indexOfEndTime - indexOfStartTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss.fff", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = "RibbonEvent",
                        ActionName = s.Substring(indexOfFirstQuotaActionName + 1, indexOfSecondQuotaActionName - indexOfFirstQuotaActionName - 1),
                        ActionDescription = "",
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };


                    UserActions.Add(uA);
                }
                else if (s.Contains(@"Jrn.Data ""TaskDialogResult""") && s.IndexOf(@"Jrn.Data ""TaskDialogResult""") == 0)
                {
                    // >Jrn.Data "TaskDialogResult"


                    // >        , "Сохранить изменения в файле Пример проека ОВ.rvt?",  _
                    // >         "Yes", "IDYES"
                    string actionDescription = text[i + 1];
                    int indexActionName01 = actionDescription.IndexOf(@"""") + 1;
                    int indexActionName02 = actionDescription.IndexOf(@"_") - 4;
                    int lenghtActionName = indexActionName02 - indexActionName01;
                    actionDescription = actionDescription.Substring(indexActionName01, lenghtActionName);

                    string fileName = actionDescription.Replace("Сохранить изменения в файле ", "").Replace("?", "");
                    
                    int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды, она выше >'H 05-Mar-2022 12:38:35.314;   1:< 
                    string timeString = text[i - 1];
                    int indexTime01 = 3;
                    int indexTime02 = 27;
                    int lenghtTime = 24;
                    timeString = timeString.Substring(indexTime01, lenghtTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss.fff", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = "TaskDialogResult",
                        ActionDescription = actionDescription,
                        FileName = fileName,
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };


                    UserActions.Add(uA);
                }
                else if (s.Contains(@"[ISL] On save,"))
                {
                    // >  ' 2:< [ISL] On save, Adler Checksum: 0x7e2902e6 [C:\Users\o.sidorin\Downloads\Пример проека ОВ.rvt] 
                    // >  ' 2:< [ISL] Saved last modification time: 05-Mar-2022 12:38:35 (UTC: 05-Mar-2022 09:38:35) 

                    string actionId = "On Save";

                    int indexFilePath001 = s.IndexOf(@"[ISL] On save,");
                    int indexFilePath01 = s.IndexOf(@"[", indexFilePath001 + 1) + 1;
                    int indexFilePath02 = s.Length - 2;
                    int lenghtFilePath = indexFilePath02 - indexFilePath01;

                    string filePath = s.Substring(indexFilePath01, lenghtFilePath);

                    int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды, она выше >'H 05-Mar-2022 12:38:35.314;   1:< 
                    string timeString = text[i + 1];
                    int indexTime01 = timeString.IndexOf("Saved last modification time: ") + 30;
                    int indexTime02 = indexTime01 + 20;
                    int lenghtTime = 20;
                    timeString = timeString.Substring(indexTime01, lenghtTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = actionId,
                        ActionName = "Сохранить файл",
                        FilePath = filePath,
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };


                    UserActions.Add(uA);
                }
                else if (s.Contains("Journal Exit"))
                {
                    //string str = text[i + 1];

                    // индексы для названия команды  >'C 28-Feb-2022 14:40:42.172;  Journal Init 
                    int indexOfFirstQuotaActionName = s.IndexOf(@"Journal Exit");
                    int indexOfSecondQuotaActionName = s.IndexOf(@"Journal Exit") + 12;


                    // индексы для описания 
                    //int indexOfFirstQuotaActionDescription = s.IndexOf(@", ");
                    //int indexOfSecondQuotaActionDescription = s.Length - 1;


                    // строчка времени команды >'C 28-Feb-2022 14:40:42.172;  Journal Init 
                    string timeString = s;
                    int indexOfStartTime = 3;
                    int indexOfEndTime = timeString.IndexOf("; ");
                    timeString = timeString.Substring(indexOfStartTime, indexOfEndTime - indexOfStartTime);

                    // расознавание времени из строчки
                    DateTime dt = DateTime.ParseExact(timeString, "dd-MMM-yyyy HH:mm:ss.fff", new CultureInfo("en-EN"));

                    UserAction uA = new UserAction()
                    {
                        ActionId = "Journal Exit",
                        ActionName = s.Substring(indexOfFirstQuotaActionName, indexOfSecondQuotaActionName - indexOfFirstQuotaActionName),
                        ActionDescription = "Конец записи журнала",
                        ActionTime = timeString,
                        ActionDate = dt,
                        ActionRUTime = dt.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ru-RU"))
                    };

                    UserActions.Add(uA);

                    UserAction uA1 = new UserAction()
                    {
                        ActionId = "-",
                        ActionName = "-",
                        ActionDescription = "-",
                        ActionTime = "-"
                    };
                    UserActions.Add(uA1);
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
            string[] readText = ZlpIOHelper.ReadAllLines(path, UTF8Encoding.Default);
            return readText;
        }

    }
    public class FileOpen
    {
        // {""projectGuid"":null,""modelGuid"":null,""id"":""C:\\Users\\o.sidorin\\Downloads\\Проект2.rvt"",""displayName"":""Проект2"",""region"":null}"
        public string projectGuid { get; set; }
        public string modelGuid { get; set; }
        public string id { get; set; }
        public string displayName { get; set; }
        public string region { get; set; }
    }
}
