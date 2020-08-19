using SSTC_Solver;
using SSTC_BaseModel;
using SSTC_ViewResources;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Modules.Calculator
{
    using SSTC.Common;
    using SSTC.Modules.DataManager;
    using SSTC.Modules.Calculator.ModuleInternals;
    using System.Dynamic;
    using System.Globalization;
    using System.ComponentModel;

    class CalculatorTab : Tab
    {
        // FIELDS
        // -> grey eminence:
        private DataManager<Conductor> ConductorBase { get; } // property for internal use
        public ResultantSection SectionModel { get; private set; } // prop for external use
        // -> overall tab state:
        private bool _lockUI;
        public bool LockUI
        {
            get { return _lockUI; }
            private set
            {
                _lockUI = value;
                RaisePropertyChanged("LockUI");
            }
        }
        // -> conductor selection & search part:
        // (state)
        private bool _conductorAdvencedDisplayMode;
        private bool _conductorSearchMode;
        public bool ConductorAdvencedDisplayMode
        {
            get { return _conductorAdvencedDisplayMode; }
            set
            {
                if (_conductorAdvencedDisplayMode != value)
                {
                    _conductorAdvencedDisplayMode = value;
                    RaisePropertyChanged("ConductorAdvencedDisplayMode");
                }
            }
        }
        public bool ConductorSearchMode
        {
            get { return _conductorSearchMode; }
            set
            {
                _conductorSearchMode = value;
                RaisePropertyChanged("ConductorSearchMode");

                if (_conductorSearchMode)
                {
                    SearchedPhrase = "";
                }
                else
                {
                    RaisePropertyChanged("SelectedCategory");
                    RaisePropertyChanged("ConductorOverview");
                    RaisePropertyChanged("SelectedConductor");
                }
            }
        }
        // (searching engine)
        private bool _isSearchResultsListExpanded;
        private string _searchedPhrase;
        private ObservableCollection<Conductor> _conductorSearchResults;
        public bool IsSearchResultsListExpanded
        {
            get { return _isSearchResultsListExpanded; }
            set
            {
                _isSearchResultsListExpanded = value;
                RaisePropertyChanged("IsSearchResultsListExpanded");
            }
        }
        public string SearchedPhrase
        {
            get { return _searchedPhrase; }
            set
            {
                _searchedPhrase = value;
                IEnumerable<Conductor> results = ConductorBase.FindByCodeNamePart(_searchedPhrase);
                if (results.Count() > 0) IsSearchResultsListExpanded = true;
                else IsSearchResultsListExpanded = false;
                ConductorOverview = new ObservableCollection<Conductor>(results);
                RaisePropertyChanged("SearchedPhrase");
            }
        }
        // (base)
        private Conductor _selectedConductor; // display buffer too
        private string _selectedCategory;
        private ObservableCollection<string> _categoryOverview;
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    RaisePropertyChanged("SelctedCategory");
                    RaisePropertyChanged("ConductorOverview");
                }
            }
        }
        public ObservableCollection<string> CategoryOverview
        {
            get { return _categoryOverview; }
            set { _categoryOverview = value; RaisePropertyChanged("CategoryOverview"); }
        }
        public ObservableCollection<Conductor> ConductorOverview
        {
            get
            {
                if (ConductorSearchMode)
                {
                    return _conductorSearchResults;
                }
                else
                {
                    if (_selectedCategory != null)
                    {
                        return new ObservableCollection<Conductor>(ConductorBase.GetSelectedGroup(_selectedCategory));
                    }
                    return null;
                }
            }
            set
            {
                if (ConductorSearchMode)
                {
                    _conductorSearchResults = value;
                    RaisePropertyChanged("ConductorOverview");
                }
            }
        }
        public Conductor SelectedConductor
        {
            get { return _selectedConductor; }
            set
            {

                if (ConductorSearchMode && value != null)
                {
                    int catIndex, codeIndex;
                    string searchRelatedCat;

                    catIndex = CategoryOverview.IndexOf(value.Category);
                    searchRelatedCat = CategoryOverview[catIndex];
                    if (_selectedCategory != searchRelatedCat) _selectedCategory = searchRelatedCat;

                    codeIndex = ConductorOverview.IndexOf(value);
                    _selectedConductor = ConductorOverview[codeIndex];

                    IsSearchResultsListExpanded = false;
                    ConductorSearchMode = false;

                }
                else
                {
                    if (_selectedConductor != value)
                    {
                        _selectedConductor = value;
                        SynchronizeStressWithGivenTension();
                    }
                }
                RaisePropertyChanged("SelectedConductor");
                if (value != null)
                {
                    IsTensionAndStressInputAllowed = true;
                    Solver.SetConductor(value);
                }
                else IsTensionAndStressInputAllowed = false;
            }
        }
        // -> tension and temperature conditions part:
        // (state)
        private bool _isTensionAndStressInputAllowed;
        public bool IsTensionAndStressInputAllowed
        {
            get { return _isTensionAndStressInputAllowed; }
            set
            {
                if (value != _isTensionAndStressInputAllowed)
                {
                    _isTensionAndStressInputAllowed = value;
                    RaisePropertyChanged("IsTensionAndStressInputAllowed");
                    if (!value)
                    {
                        _stress1 = null;
                        _tension0 = null;
                        RaisePropertyChanged("Stress1");
                        RaisePropertyChanged("Tension0");
                        // up: unique case when need to bypass synchro mothods
                    }
                }
            }
        }
        // (display buffers)
        private double? _temperature1;
        private double? _temperature2;
        private double? _tension0;
        private double? _stress1;
        public double? Temperature1
        {
            get { return _temperature1; }
            set
            {
                if (value >= -273.15 && value != _temperature1)
                {
                    _temperature1 = value;
                    RaisePropertyChanged("Temperature1");
                    Solver.SetConditionParams(_tension0, _temperature1, _temperature2);
                }
            }
        }
        public double? Temperature2
        {
            get { return _temperature2; }
            set
            {
                if (value >= -273.15 && value != _temperature2)
                {
                    _temperature2 = value;
                    RaisePropertyChanged("Temperature2");
                    Solver.SetConditionParams(_tension0, _temperature1, _temperature2);
                }
            }
        }
        public double? Tension0
        {
            get { return _tension0; }
            set
            {
                if (value != _tension0)
                {
                    _tension0 = value;
                    RaisePropertyChanged("Tension0");
                    SynchronizeStressWithGivenTension();
                    Solver.SetConditionParams(_tension0, _temperature1, _temperature2);
                }
            }
        }
        public double? Stress1
        {
            get { return _stress1; }
            set
            {
                if (value != _stress1)
                {
                    _stress1 = value;
                    RaisePropertyChanged("Stress1");
                    SynchronizeTensionWithGivenStress();
                    Solver.SetConditionParams(_tension0, _temperature1, _temperature2);
                }
            }
        }
        // -> section builder part:
        public UserSection UserSection { get; }
        // -> section results presentation part:
        public PresentedResultantSection PresentedResultantSection { get; }
        // -> solver part:
        public SolverViewModel Solver { get; }
        public bool HasResults
        {
            get
            {
                if (PresentedResultantSection.HasValidResults) return true;
                return false;
            }
        }
        // HANDLERS
        private void OnSpatialModelUpdate(IEnumerable<ISpanModel> spatialModel)
        {
            Solver.SetSectionParams(spatialModel);
        }
        private void OnSolverCalculationsReportSent(object sender, EventArgs args)
        {
            string flattenReport = "";
            SolverReportEventArgs solverReport = args as SolverReportEventArgs;
            

            if(solverReport.AreCalculationsSucceed.HasValue && solverReport.AreCalculationsSucceed == true)
            {
                PresentedResultantSection.SetPresentedCollection(solverReport.SolutionsVector, solverReport.Results.ToArray());
                SectionModel = new ResultantSection(this.TabTitle, this.UserSection.RawExport(), this.SelectedConductor, this.Tension0.Value, this.Temperature1.Value, this.Temperature2.Value, solverReport.SolutionsVector, solverReport.Results);
                Annalist.Instance.DisplayStatus(this,"I300",solverReport.AdditionalInformation[0]);
            }
            else if (solverReport.AreCalculationsSucceed.HasValue && solverReport.AreCalculationsSucceed == false)
            {
                foreach (string message in solverReport.AdditionalInformation)
                {
                    flattenReport += message + " ";
                }
                PresentedResultantSection.SetPresentedCollection(solverReport.SolutionsVector);
                
                Annalist.Instance.DisplayStatus(this,"E300",new string[1] { flattenReport }); //I know... this is kinda shitty...
            }
            else
            {
                PresentedResultantSection.SetPresentedCollection();
                Annalist.Instance.DisplayStatus(this,"E301",solverReport.AdditionalInformation[0]);
            }

        }
        private void OnSolverPropertyChanged(object sender, EventArgs e)
        {
            PropertyChangedEventArgs pEA = e as PropertyChangedEventArgs;
            if(pEA.PropertyName == "CalculationsInProgress")
            {
                if (Solver.CalculationsInProgress)
                {
                    LockUI = true;
                }
                else LockUI = false;
            }
        }
        // COMMANDS
        // -> UserSection:
        public ICommand JustAddSpan { get; }
        public ICommand JustRemoveSpan { get; }
        public ICommand AddSpanBefore { get; }
        public ICommand AddSpanAfter { get; }
        public ICommand ClearSpan { get; }
        public ICommand RemoveSpan { get; }
        public ICommand ClearAll { get; }
        public ICommand SaveSectionModelAsCsvCommand { get; }
        public ICommand LoadSectionModelFromCsvCommand { get; }
        public ICommand ManuallyCheckUserSectionIntegrity { get; }
        public ICommand ToggleXAxisDataInputModeCommand { get; }
        // -> Conductor:
        public ICommand ToggleConductorDetailsCommand { get; }
        // -> Calculator/Solver:
        public ICommand CalculateCommand { get; }
        public ICommand RestoreSolverSettingsCommand { get; }
        // -> Integrity Report:
        public ICommand CloseReportCommand { get; }
        // CONSTRUCTORS
        public CalculatorTab(string tabTitle)
        {
            ConductorBase = ConductorDataManager.Instance;
            _conductorAdvencedDisplayMode = false;
            _categoryOverview = new ObservableCollection<string>(ConductorBase.GetAvailableCategories());
            _selectedCategory = _categoryOverview[0];

            _conductorSearchMode = false;
            _searchedPhrase = "";
            _conductorSearchResults = new ObservableCollection<Conductor>();

            UserSection = new UserSection();
            UserSection.SpatialModelUpdate += OnSpatialModelUpdate;
            PresentedResultantSection = new PresentedResultantSection();
            Solver = new SolverViewModel(MainSettings.Instance.MainEPS,MainSettings.Instance.GravitionalAcceleration);
            Solver.SubscribeOn_SolverCalculationsReportSent_Event(OnSolverCalculationsReportSent);
            Solver.PropertyChanged += OnSolverPropertyChanged;
            TabTitle = tabTitle;

            LoadSectionModelFromCsvCommand = new CommandRelay(UserSection.LoadSectionModelFromCsv, () => true);
            SaveSectionModelAsCsvCommand = new CommandRelay(UserSection.SaveSectionModelAsCsv, () => true);

            JustAddSpan = new CommandRelay(UserSection.JustAddSpan, () => true);
            JustRemoveSpan = new CommandRelay(UserSection.JustRemoveSpan, CanExecute_JustRemoveSpanCommand);
            AddSpanBefore = new CommandRelay(UserSection.AddSpanBeforeSelectedSpan, CanExecute_BasicSectionCommand);
            AddSpanAfter = new CommandRelay(UserSection.AddSpanAfterSelectedSpan, CanExecute_BasicSectionCommand);
            ClearSpan = new CommandRelay(UserSection.ClearSelectedSpan, CanExecute_BasicSectionCommand);
            RemoveSpan = new CommandRelay(UserSection.RemoveSelectedSpan, CanExecute_RemoveSpanCommand);
            ClearAll = new CommandRelay(UserSection.ClearBackToBasicSection, CanExecute_ClearAllSectionCommand);
            ManuallyCheckUserSectionIntegrity = new CommandRelay(UserSection.ManualIntegrityCheck, () => true);
            ToggleXAxisDataInputModeCommand = new CommandRelay(ToggleXAxisDataInputMode, () => true);

            CloseReportCommand = new CommandRelay(UserSection.HideReport, UserSection.CanExecute_HideReport);

            ToggleConductorDetailsCommand = new CommandRelay(ToggleConductorDetails, () => true);

        }
        public CalculatorTab(ResultantSection resultantSection) : this(resultantSection.ProjectName)
        {
            SectionModel = resultantSection;
            UserSection.UpdateFromSpatialModel(resultantSection.SpatialModel);
            if (SetConductorFromSectionModel(resultantSection.SectionConductor))
            {
                Tension0 = resultantSection.SectionInitialTension;
                Temperature1 = resultantSection.SectionInitialTemperature;
                Temperature2 = resultantSection.SectionTargetTemperature;
                PresentedResultantSection.SetPresentedCollection(resultantSection.Solutions, resultantSection.Results.ToArray());
                _Annalist.DisplayStatus(this.ToString(), "I200", new string[] { resultantSection.ProjectName });
            }
            _Annalist.DisplayStatus(this.ToString(), "E200", new string[] { resultantSection.ProjectName });
        }

        // METHODS
        // -> Own:
        private void SynchronizeStressWithGivenTension()
        {
            if (SelectedConductor != null)
            {
                _stress1 = _tension0 / SelectedConductor.CrossSection;
            }
            else _stress1 = null;
            RaisePropertyChanged("Stress1");
        }
        private void SynchronizeTensionWithGivenStress()
        {
            if (SelectedConductor != null)
            {
                _tension0 = _stress1 * SelectedConductor.CrossSection;
            }
            else _tension0 = null;
            RaisePropertyChanged("Tension0");
        }
        // -> Overrides (Overridens? :) ): 
        public override void TabRefresh()
        {
            // previously I had here compare function which determined need to "refresh" category overview, but compare function was "heavier" than this ;)
            ObservableCollection<string> categoryOverviewBuffer = new ObservableCollection<string>(ConductorBase.GetAvailableCategories());

            string selectedCategoryBuffer = _selectedCategory;
            int index = categoryOverviewBuffer.IndexOf(selectedCategoryBuffer);

            CategoryOverview = categoryOverviewBuffer;
            if (index >= 0)
            {
                SelectedCategory = _categoryOverview[index];
            }
            else SelectedCategory = _categoryOverview[0];

            // refreshing all those pesky combofields in user section
            foreach (UserSpan span in UserSection.Spans)
            {
                span.SynchronizeWithAttachmentSetDatabase();
            }

        }
        // -> Command related:
        private void ToggleConductorDetails()
        {
            if (ConductorAdvencedDisplayMode) ConductorAdvencedDisplayMode = false;
            else ConductorAdvencedDisplayMode = true;
        }
        private void ToggleXAxisDataInputMode()
        {
            if (UserSection.AlternativeInputMode) UserSection.AlternativeInputMode = false;
            else UserSection.AlternativeInputMode = true;
        }
        // -> Command execution methods:
        private bool CanExecute_BasicSectionCommand()
        {
            if (UserSection.SelectedUserSpan != null) return true;
            else return false;
        }
        private bool CanExecute_JustRemoveSpanCommand()
        {
            return (UserSection.Spans.Count > 2);
        }
        private bool CanExecute_RemoveSpanCommand()
        {
            return (CanExecute_BasicSectionCommand() && CanExecute_JustRemoveSpanCommand());
        }
        private bool CanExecute_ClearAllSectionCommand()
        {
            return true;
        }
        // -> Load Project from existing file related:
        private bool SetConductorFromSectionModel(Conductor conductor)
        {
            IEnumerable<Conductor> conductors = ConductorBase.FindByCodeNamePart(conductor.CodeName);

            if (conductors.Count() == 0) return false;
            else
            {
                foreach (Conductor item in conductors)
                {
                    if(item.Equals(conductor))
                    {
                        SelectedCategory = CategoryOverview[CategoryOverview.IndexOf(item.Category)];
                        SelectedConductor = item;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
