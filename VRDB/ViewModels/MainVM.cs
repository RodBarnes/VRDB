﻿using Common;
using Common.UserControls;
using Common.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;
using VRDB.Models;
using VRDB.OpenXML;
using VRDB.PDF;

namespace VRDB.ViewModels
{
    partial class MainVM : BaseVM
    {
        private static MainWindow window;
        private static BackgroundWorker worker;
        private static string dataPath;
        private static string importFilename;
        private static DateTime importStartTime;
        private ImportStatus importStatus = new ImportStatus();
        private static Logger Logger;
        private static List<SearchVM> searchList;
        private static string docFilePath;
        private static string[] headerTokens;

        public AboutProperties AboutProperties { get; set; }

        public MainVM(MainWindow wdw)
        {
            window = wdw;

            if (AlreadyRunning())
            {
                ExitApplication();
            }

            try
            {
                AssyInfo = new AssemblyInfo(Assembly.GetExecutingAssembly());
#if DEBUG
                ProgramAppPath = Directory.GetCurrentDirectory();
                UserAppDataPath = ProgramAppPath;
                dataPath = $@"{UserAppDataPath}\Database";
                docFilePath = $@"D:\Source\BitBucket\{AssyInfo.Product}";
#else
                var productPath = $@"{AssyInfo.Company}\{AssyInfo.Product}";
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var assyPath = Uri.UnescapeDataString(uri.Path);
                ProgramAppPath = Path.GetDirectoryName(assyPath);
                UserAppDataPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{productPath}";
                dataPath = UserAppDataPath;
                docFilePath = ProgramAppPath;
#endif
                SettingsFilePath = $@"{UserAppDataPath}\Settings.xml";
                DatabaseManager.DatabasePath = dataPath;

                InitAboutPanel();
                InitBusyPanel(10000);
                InitMainPanel();

                SettingsLoad();
                DatabaseManager.AddressDirections = AddressDirectionTokens.Split(',');
                DatabaseManager.AddressTypes = AddressTypeTokens.Split(',');
                headerTokens = HeaderTokens.Split(',');

            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        #region Commands

        public Command ExitApplicationCommand { get; set; }
        private void ExitApplicationAction(object obj) => ExitApplication();

        public Command SearchCommand { get; set; }
        private void SearchAction(object obj) => Search();

        public Command ClearResultsCommand { get; set; }
        private void ClearResultsAction(object obj) => ClearResults();

        public Command LoadDataCommand { get; set; }
        private void LoadDataAction(object obj) => LoadData();

        public Command ClearDataCommand { get; set; }
        private void ClearDataAction(object obj) => PromptClearData();

        public Command CompareDataCommand { get; set; }
        private void CompareDataAction(object obj) => CompareData();

        public Command ExportResultsCommand { get; set; }
        private void ExportResultsAction(object obj) => ExportResults();

        public Command ShowAboutCommand { get; set; }
        private void ShowAboutAction(object obj) => ShowAbout();

        public Command ShowInstructionsCommand { get; set; }
        private void ShowInstructionsAction(object obj) => RunProcess($@"{docFilePath}\Instructions.pdf");

        public Command ShowFaqCommand { get; set; }
        private void ShowFaqAction(object obj) => RunProcess($@"{docFilePath}\FAQ.pdf");

        public Command SubmitRequestCommand { get; set; }
        private void SubmitRequestAction(object obj) => RunProcess(RequestUri);

        public Command CaptureSystemInfoCommand { get; set; }
        private void CaptureSystemInfoAction(object obj) => CaptureSystemInfo();

        #endregion

        #region Closing Handler

        private Command closingCommand;
        public Command ClosingCommand
        {
            get
            {
                if (closingCommand == null)
                    closingCommand = new Command(ExecuteClosing, CanExecuteClosing);

                return closingCommand;
            }
        }

        private void ExecuteClosing(object obj)
        {
            SettingsSave();
        }

        private bool CanExecuteClosing(object obj)
        {
            return true;
        }

        #endregion

        #region MessagePanel

        public enum MessageAction
        {
            Acknowledge,
            ClearDatabase
        }

        private MessageAction currentMessageAction;

        private string mainMessageResponse;
        public string MainMessageResponse
        {
            get => mainMessageResponse;
            set
            {
                mainMessageResponse = value;
                NotifyPropertyChanged();
                if (mainMessageResponse == "Proceed")
                {
                    switch (currentMessageAction)
                    {
                        case MessageAction.Acknowledge:
                            break;
                        case MessageAction.ClearDatabase:
                            ClearData();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #endregion

        #region Settings

        public string AddressDirectionTokens { get; private set; }
        public string AddressTypeTokens { get; private set; }
        public string HeaderTokens { get; private set; }

        private string requestUri;
        public string RequestUri
        {
            get => requestUri ?? "https://www.sos.wa.gov/elections/vrdb/extract-requests.aspx";
            private set => requestUri = value;
        }

        private bool useConditionalFormatting = true;
        public string UseConditionalFormatting
        {
            get => useConditionalFormatting ? "True" : "False";
            set
            {
                useConditionalFormatting = (value == "True");
                NotifyPropertyChanged();
            }
        }

        private bool excludeMissing = false;
        public string ExcludeMissing
        {
            get => excludeMissing ? "True" : "False";
            set
            {
                excludeMissing = (value == "True");
                NotifyPropertyChanged();
            }
        }

        private bool excludeSame = true;
        public string ExcludeSame
        {
            get => excludeSame ? "True" : "False";
            set
            {
                excludeSame = (value == "True");
                NotifyPropertyChanged();
            }
        }

        private bool includeFullFirstName = true;
        public string IncludeFullFirstName
        {
            get => includeFullFirstName ? "True" : "False";
            set
            {
                includeFullFirstName = (value == "True");
                NotifyPropertyChanged();
            }
        }

        private bool includeGender = false;
        public string IncludeGender
        {
            get => includeGender ? "True" : "False";
            set
            {
                includeGender = (value == "True");
                NotifyPropertyChanged();
            }
        }

        private bool includeMiddleInitial = false;
        public string IncludeMiddleInitial
        {
            get => includeMiddleInitial ? "True" : "False";
            set
            {
                includeMiddleInitial = (value == "True");
                NotifyPropertyChanged();
            }
        }

        public string InitialDirectoryLoad { get; set; }
        public string InitialDirectoryCompare { get; set; }
        public string InitialDirectoryExport { get; set; }

        #endregion

        #region Properties

        // These are captured from the DataGrid during startup of the window
        // This allows for changing them in the XAML and not requiring hard-coded values
        public string HighlightMissing { get; set; }
        public string HighlightSame { get; set; }
        public string HighlightDifferent { get; set; }
        public string HighlightHeader { get; set; }
        public bool SearchEnabled => DatabaseManager.HasData && !string.IsNullOrEmpty(LastName);
        public bool ClearResultsEnabled => SearchResults.Count > 0;
        public bool LoadEnabled => !DatabaseManager.HasData;
        public bool ClearEnabled => DatabaseManager.HasData;
        public bool CompareEnabled => DatabaseManager.HasData;

        private ObservableCollection<SearchVM> searchResults = new ObservableCollection<SearchVM>();
        public ObservableCollection<SearchVM> SearchResults
        {
            get => searchResults;
            set
            {
                searchResults = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ClearResultsEnabled));
            }
        }

        private Logger.LogLevel logLevelSetting;
        public string LogLevel
        {
            get => logLevelSetting.ToString();
            set
            {
                if (!Enum.TryParse(value, out logLevelSetting))
                {
                    logLevelSetting = Logger.LogLevel.None;
                }
                if (logLevelSetting == Logger.LogLevel.None)
                {
                    DisableLogging();
                }
                else
                {
                    EnableLogging(logLevelSetting);
                }
            }
        }

        public string LogLevelNone
        {
            get => (LogLevel == "None") ? "True" : "False";
            set
            {
                if (value == "True")
                {
                    LogLevel = "None";
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(LogLevelTrace));
                    NotifyPropertyChanged(nameof(LogLevelDebug));
                }
            }
        }

        public string LogLevelTrace
        {
            get => (LogLevel == "Trace") ? "True" : "False";
            set
            {
                if (value == "True")
                {
                    LogLevel = "Trace";
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(LogLevelNone));
                    NotifyPropertyChanged(nameof(LogLevelDebug));
                }
            }
        }

        public string LogLevelDebug
        {
            get => (LogLevel == "Debug") ? "True" : "False";
            set
            {
                if (value == "True")
                {
                    LogLevel = "Debug";
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(LogLevelTrace));
                    NotifyPropertyChanged(nameof(LogLevelNone));
                }
            }
        }

        private string lastName;
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SearchEnabled));
            }
        }

        private string firstName;
        public string FirstName
        {
            get => firstName;
            set
            {
                firstName = value;
                NotifyPropertyChanged();
            }
        }

        private string gender;
        public string Gender
        {
            get => gender;
            set
            {
                gender = value;
                NotifyPropertyChanged();
            }
        }

        private string birthYear;
        public string BirthYear
        {
            get => birthYear;
            set
            {
                birthYear = value;
                NotifyPropertyChanged();
            }
        }

        private string operationStatus;
        public string OperationStatus
        {
            get => operationStatus;
            set
            {
                operationStatus = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void InitAboutPanel()
        {
            AboutProperties = new AboutProperties
            {
                ApplicationName = AssyInfo.Product,
                ApplicationVersion = AssyInfo.AssemblyVersion.ToString(),
                Background = window.Background.ToString(),
                Owner = window,
                ImagePath = $"/{AssyInfo.Product};component/Images/searchicon-400x400.png",
                Description = "This application facilitates searching the Washington State " +
                $"Voter Registration Database.\n" +
                $"{AssyInfo.Copyright} {AssyInfo.Company}. Apace 2.0 Licensed."
            };
        }

        private void InitMainPanel()
        {
            ExitApplicationCommand = new Command(ExitApplicationAction);
            SearchCommand = new Command(SearchAction);
            ClearResultsCommand = new Command(ClearResultsAction);
            LoadDataCommand = new Command(LoadDataAction);
            ClearDataCommand = new Command(ClearDataAction);
            ExportResultsCommand = new Command(ExportResultsAction);
            CompareDataCommand = new Command(CompareDataAction);

            ShowAboutCommand = new Command(ShowAboutAction);
            ShowInstructionsCommand = new Command(ShowInstructionsAction);
            ShowFaqCommand = new Command(ShowFaqAction);
            SubmitRequestCommand = new Command(SubmitRequestAction);
            CaptureSystemInfoCommand = new Command(CaptureSystemInfoAction);

            UpdateStatusMessage();
        }

        private void ExitApplication()
        {
            window.Close();
        }

        private void ShowAbout()
        {
            var dlg = new AboutWindow(AboutProperties);
            dlg.ShowDialog();
        }

        private void CaptureSystemInfo()
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    DefaultExt = "txt",
                    FileName = $"{AssyInfo.Product}_system",
                    Filter = "Text (*.txt)|*.txt"
                };

                bool? result = dlg.ShowDialog(window);
                if (result == true)
                {
                    using (new WaitCursor())
                    {
                        var process = Process.Start(new ProcessStartInfo("dxdiag.exe", $@"{dlg.FileName}"));
                        process.WaitForExit();
                        currentMessageAction = MessageAction.Acknowledge;
                        window.MessagePanel.Show("DONE", $@"Created file {dlg.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void RunProcess(string statement, string args = "")
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo(statement, args));
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void ClearResults()
        {
            LastName = "";
            FirstName = "";
            BirthYear = "";
            Gender = "";
            SearchResults = new ObservableCollection<SearchVM>();
        }

        private void PromptClearData()
        {
            // Confirm whether to proceed
            currentMessageAction = MessageAction.ClearDatabase;
            window.MessagePanel.Show("Database contains data!", $"There are {Utility.FormatWithComma(DatabaseManager.RowCount)} rows in the database. " +
                $" If you proceed the data will have to be loaded before performing any search.  The last import took {importStatus.TimeString()}.",
                "Are you SURE you want to reload the data?", MessagePanel.MessageType.YesNo);
        }

        private void Search()
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");
            try
            {
                using (new WaitCursor())
                {
                    SearchResults = new ObservableCollection<SearchVM>(DatabaseManager.Search(LastName, FirstName, BirthYear, Gender));
                }
                OperationStatus = $"Search returned {Utility.FormatWithComma(SearchResults.Count)} rows.";
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void ClearData()
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");
            try
            {
                DatabaseManager.Clear();
                ClearResults();
                importStatus = new ImportStatus();

                UpdateButtonStatus();
                UpdateStatusMessage();
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void LoadData()
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");
            try
            {
                var dlg = new OpenFileDialog
                {
                    InitialDirectory = InitialDirectoryLoad,
                    DefaultExt = "txt",
                    Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*"
                };

                bool? result = dlg.ShowDialog(window);
                if (result == true)
                {
                    InitialDirectoryLoad = Path.GetDirectoryName(dlg.FileName);
                    LoadFromFile(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void ExportResults()
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");
            try
            {
                var dlg = new SaveFileDialog
                {
                    InitialDirectory = InitialDirectoryExport,
                    DefaultExt = "csv",
                    Filter = "XLSX (Excel) (*.xlsx)|*.xlsx|CSV (Comma delimited) (*.csv)|*.csv|Text (Tab delimited) (*.txt)|*.txt"
                };

                bool? result = dlg.ShowDialog(window);
                if (result == true)
                {
                    InitialDirectoryExport = Path.GetDirectoryName(dlg.FileName);
                    if (dlg.Filter.Contains("XLSX") &&  dlg.FilterIndex == 1)
                    {
                        XmlLibrary.ExportToXslx(
                            "Comparison", 
                            dlg.FileName, 
                            SearchResults.ToList().ToDataTable(), 
                            useConditionalFormatting, 
                            excludeMissing, 
                            excludeSame, 
                            HighlightHeader, 
                            HighlightMissing, 
                            HighlightSame, 
                            HighlightDifferent
                        );
                    }
                    else
                    {
                        ExportToDelimited(dlg.FileName, dlg.FilterIndex);
                    }
                    currentMessageAction = MessageAction.Acknowledge;
                    window.MessagePanel.Show("DONE", $@"Created file {dlg.FileName}");
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void CompareData()
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");
            try
            {
                var dlg = new OpenFileDialog
                {
                    InitialDirectory = InitialDirectoryCompare,
                    DefaultExt = "pdf",
                    Filter = "PDF (pdf)|*.pdf"
                };

                bool? result = dlg.ShowDialog(window);
                if (result == true)
                {
                    InitialDirectoryCompare = Path.GetDirectoryName(dlg.FileName);
                    CompareToDatabase(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void LoadFromFile(string inFile)
        {
            var args = new WorkerArgs
            {
                FileName = inFile,
                MaxProgressValue = BusyProgressMaximum,
                ImportResult = importStatus
            };

            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.DoWork += Background_LoadDataDoWork;
            worker.ProgressChanged += Background_LoadDataProgressChanged;
            worker.RunWorkerCompleted += Background_LoadDataRunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync(args);
            }
        }

        private void CompareToDatabase(string inFile)
        {
            var args = new WorkerArgs
            {
                FileName = inFile,
                MaxProgressValue = BusyProgressMaximum,
                Level = GetCompareLevel()
            };

            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.DoWork += Background_CompareDataDoWork;
            worker.ProgressChanged += Background_CompareDataProgressChanged;
            worker.RunWorkerCompleted += Background_CompareDataRunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync(args);
            }
        }

        private int GetCompareLevel()
        {
            int rtn = 0;

            rtn += includeGender ? 1 : 0;
            rtn += includeFullFirstName ? 2 : 0;
            rtn += includeMiddleInitial ? 4 : 0;

            return rtn;
        }

        private void ExportToDelimited(string exportPath, int filterIndex)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");

            StringBuilder sb;

            char del;
            char sep;
            if (filterIndex == 1)
            {
                del = ',';
                sep = '"';
            }
            else
            {
                del = '\t';
                sep = '\0';
            }

            // Copy the contents of SearchResults to a CSV
            using (var writer = new StreamWriter(exportPath))
            {
                sb = new StringBuilder();
                var dg = window.ResultsDataGrid;
                var firstCol = true;
                foreach (var col in dg.Columns)
                {
                    if (firstCol)
                    {
                        sb.Append($"{sep}{col.Header}{sep}");
                        firstCol = false;
                    }
                    else
                    {
                        sb.Append($"{del}{sep}{col.Header}{sep}");
                    }
                }
                writer.WriteLine($"{sb}");

                foreach (var item in SearchResults)
                {
                    writer.WriteLine(
                        $"{item.LastName}" +
                        $"{del}{sep}{item.FirstName}{sep}" +
                        $"{del}{sep}{item.MiddleName}{sep}" +
                        $"{del}{item.BirthDate}" +
                        $"{del}{item.Gender}" +
                        $"{del}{sep}{item.Address}{sep}" +
                        $"{del}{sep}{item.City}{sep}" +
                        $"{del}{sep}{item.State}{sep}" +
                        $"{del}{item.Zip}" +
                        $"{del}{item.RegistrationDate}" +
                        $"{del}{item.LastVoted}" +
                        $"{del}{sep}{item.Status}{sep}" +
                        $"{del}{sep}{item.Compare}{sep}");
                }
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private List<SearchVM> ExtractContent(List<PdfTextLine> lines)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");

            var list = new List<SearchVM>();
            foreach (var line in lines)
            {
                try
                {

                    // Data line
                    var item = new SearchVM(line.Content.ToUpper());
                    list.Add(item);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Exception in line {lines.IndexOf(line)}", ex);
                }
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
            return list;
        }

        #endregion

        #region Helpers

        [STAThread]
        private void ShowException(Exception ex)
        {
            var fullEx = Utility.ParseException(ex);
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Exception > {fullEx}");
            currentMessageAction = MessageAction.Acknowledge;
            window.MessagePanel.Show("EXCEPTION!", fullEx);
        }

        private void DisableLogging()
        {
            Logger = null;
            DatabaseManager.Logger = null;
        }

        private void EnableLogging(Logger.LogLevel level)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    DefaultExt = "log",
                    FileName = $"{AssyInfo.Product}_{DateTime.Now.ToString("yyyyMMddhhmmss")}",
                    Filter = "Text (*.log)|*.log"
                };

                bool? result = dlg.ShowDialog(window);
                if (result == true)
                {
                    Logger = new Logger(dlg.FileName, level);
                    DatabaseManager.Logger = Logger;
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void UpdateButtonStatus()
        {
            NotifyPropertyChanged(nameof(ClearEnabled));
            NotifyPropertyChanged(nameof(LoadEnabled));
            NotifyPropertyChanged(nameof(SearchEnabled));
            NotifyPropertyChanged(nameof(CompareEnabled));
        }

        private void UpdateStatusMessage()
        {
            DatabaseManager.StatusRead(importStatus);
            OperationStatus = importStatus.ToString();
        }

        #endregion

        #region Background Workers

        private void Background_LoadDataDoWork(object sender, DoWorkEventArgs e)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");

            var args = (WorkerArgs)e.Argument;
            importFilename = Path.GetFileName(args.FileName);
            importStartTime = DateTime.Now;

            ShowBusyPanel("Importing data...");

            // Read the file
            using (var reader = new StreamReader(args.FileName))
            {
                Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Opened stream");

                bool headerRead = false;
                int byteCount = 0;
                int lineCount = 0;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!headerRead)
                    {
                        // Skip the header row
                        headerRead = true;
                        continue;
                    }

                    Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Read line[{lineCount}]");

                    var reg = new Registration(line);

                    Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:DB Insert[{reg.StateVoterId}]");

                    if (!DatabaseManager.RegistrationExists(reg.StateVoterId))
                    {
                        DatabaseManager.RegistrationInsert(reg);
                    }
                    else
                    {
                        args.ImportResult.ImportRejects.Add(reg.StateVoterId);
                    }

                    byteCount += line.Length;
                    lineCount++;

                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    worker.ReportProgress((int)(byteCount * args.MaxProgressValue / reader.BaseStream.Length), lineCount);
                }
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void Background_LoadDataProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");

            int lineCount = (int)e.UserState;
            BusyProgressValue = e.ProgressPercentage;
            BusyProgressText = $"{lineCount}";

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void Background_LoadDataRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");

            if ((e.Cancelled == true))
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Cancelled");
                HideBusyPanel();
                currentMessageAction = MessageAction.Acknowledge;
                OperationStatus = "User cancelled load of data.";
                DatabaseManager.Clear();
                UpdateButtonStatus();
                return;
            }
            else if (!(e.Error == null))
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Error");
                HideBusyPanel();
                currentMessageAction = MessageAction.Acknowledge;
                window.MessagePanel.Show("Error!!", Utility.ParseException(e.Error));
                OperationStatus = e.Result.ToString();
                DatabaseManager.Clear();
                UpdateButtonStatus();
                return;
            }

            var span = DateTime.Now - importStartTime;
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:StatusUpdate");
            DatabaseManager.StatusUpdate(importFilename, span.Ticks);

            UpdateButtonStatus();
            UpdateStatusMessage();
            HideBusyPanel();

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void Background_CompareDataDoWork(object sender, DoWorkEventArgs e)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");

            var args = (WorkerArgs)e.Argument;

            try
            {
                var lines = new List<PdfTextLine>();
                using (var document = PdfDocument.Open(args.FileName))
                {
                    // Read all of the text lines
                    lines = document.ReadTextLines(headerTokens);
                    //document.WriteText($@"C:\Temp\pdfdata.txt");
                }
                // Extract the content read from the PDF into SearchVM objects
                searchList = ExtractContent(lines);
            }
            catch (PdfDocumentFormatException ex)
            {
                throw new Exception($"'{args.FileName}' does not appear to have the correct format.  It must be a PDF file generated from Leader & Clerk Resources." +
                    $"  Go to 'Help > Instructions', section 'Creating the Voter Registration Comparison' report.", ex);
            }

            var reportName = Path.GetFileNameWithoutExtension(args.FileName);
            ShowBusyPanel(reportName);

            // Compare each member in the list with the registration database
            DatabaseManager.CompareSearch(searchList, worker, e);

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void Background_CompareDataProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");

            int lineCount = (int)e.UserState;
            BusyProgressValue = e.ProgressPercentage;
            BusyProgressText = $"{lineCount}";

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        private void Background_CompareDataRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Enter");

            if ((e.Cancelled == true))
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Cancelled");
                HideBusyPanel();
                currentMessageAction = MessageAction.Acknowledge;
                OperationStatus = "User cancelled comparison of data.";
                return;
            }
            else if (!(e.Error == null))
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Error");
                HideBusyPanel();
                currentMessageAction = MessageAction.Acknowledge;
                window.MessagePanel.Show("Error!!", Utility.ParseException(e.Error));
                OperationStatus = e.Error.Message;
                return;
            }

            var span = DateTime.Now - importStartTime;
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:SearchResults");

            SearchResults = new ObservableCollection<SearchVM>(searchList);
            var missingCnt = SearchResults.Count(s => s.Compare == Constants.LabelMissing);
            var sameCnt = SearchResults.Count(s => s.Compare == Constants.LabelSame);
            var diffCnt = SearchResults.Count(s => s.Compare == Constants.LabelDifferent);
            OperationStatus = $"Compared {Utility.FormatWithComma(SearchResults.Count)} member entries from report.\n" +
                $"{Constants.LabelMissing}: {missingCnt}\n" +
                $"{Constants.LabelSame}: {sameCnt}\n" +
                $"{Constants.LabelDifferent}: {diffCnt}";

            HideBusyPanel();

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(MainVM).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        #endregion
    }
}