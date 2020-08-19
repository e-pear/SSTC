using SSTC_BaseModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Modules.Calculator.ModuleInternals
{
    using System.Globalization;
    using System.Windows;
    // It's basically a viewModel object class for quite specific part of calculator view. Some thing like buffer elements from datamanager objects but more complicated ...and it should be named UserSectionBuilder ... whatever
    using SSTC.Common;
    using SSTC.Modules.DataManager;

    class UserSection : INotifyPropertyChanged
    {
        private DataManager<InsulatorSet> _insulatorManager;
        // DATA INPUT MODE: True if incremental input mode is active
        private bool _alternativeInputMode;
        public bool AlternativeInputMode
        {
            get { return _alternativeInputMode; }
            set 
            { 
                if (_alternativeInputMode != value)
                {
                    _alternativeInputMode = value;
                    RaisePropertyChanged("AlternativeInputMode");
                }
            }
        }
        // SECTION BUILDER
        private UserSpan _selectedSpan;
        public ObservableCollection<UserSpan> Spans { get; }
        public UserSpan SelectedUserSpan
        {
            get { return _selectedSpan; }
            set
            {
                if (_selectedSpan != value)
                {
                    _selectedSpan = value;
                    RaisePropertyChanged("SelectedUserSpan");
                    if (EvaluateNeedToPerformInternalIntegrityCheck()) CheckInternalIntegrity();
                }
            }
        }
        // INTEGRITY CHECKING AND REPORTING
        // -> report
        private bool _isIntegrityReportShown;
        public ObservableCollection<string> IntegrityReport { get; private set; }
        public bool IsIntegrityReportShown
        {
            get { return _isIntegrityReportShown; }
            set
            {
                _isIntegrityReportShown = value;
                RaisePropertyChanged("IsIntegrityReportShown");
            }
        }
        // -> indicator lights :)
        private IndicatorLight _light;
        public bool IsRedLightOn
        {
            get 
            {
                if (_light == IndicatorLight.Red) return true;
                else return false;
            }
        }
        public bool IsGreenLightOn
        {
            get
            {
                if (_light == IndicatorLight.Green) return true;
                else return false;
            }
        }
        public bool IsYellowLightOn
        {
            get
            {
                if (_light == IndicatorLight.Yellow) return true;
                else return false;
            }
        }
        // CONSTRUCTOR
        public UserSection()
        {
            _insulatorManager = InsulatorSetDataManager.Instance;
            _alternativeInputMode = false;
            _isIntegrityReportShown = false;
            _light = IndicatorLight.None;
            IntegrityReport = new ObservableCollection<string>();
            Spans = new ObservableCollection<UserSpan>();
            CreateBasicSection();
        }
        // DELEGATES
        public delegate void SpatialModelUpdateEventHandler(IEnumerable<ISpanModel> newSpatialModel);
        // EVENTS
        public event PropertyChangedEventHandler PropertyChanged;
        public event SpatialModelUpdateEventHandler SpatialModelUpdate;
        // EVENT RAISERS
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void RaiseSpatialModelUpdateEvent()
        {
            SpatialModelUpdate?.Invoke(Spans);
        }
        // METHODS
        // -> Basic section handling:
        public void JustAddSpan()
        {
            UserSpan newSpan = new UserSpan(); // add span at the end of current section
            newSpan.PropertyChanged += OnUserSpanPropertyChanged;
            Spans.Add(newSpan);
            OrderingSection();
            SelectedUserSpan = Spans.Last();
        }
        public void JustRemoveSpan() // removes last span
        {
            if (Spans.Count <= 2) return;
            Spans.Last().PropertyChanged -= OnUserSpanPropertyChanged;
            Spans.Remove(Spans.Last());
            OrderingSection();
            if (EvaluateNeedToPerformInternalIntegrityCheck()) CheckInternalIntegrity();
        }
        public void AddSpanBeforeSelectedSpan()
        {
            int index = Spans.IndexOf(_selectedSpan);
            UserSpan newSpan = new UserSpan();
            newSpan.PropertyChanged += OnUserSpanPropertyChanged;
            Spans.Insert(index, newSpan);
            OrderingSection();
            SelectedUserSpan = Spans[index];
        }
        public void AddSpanAfterSelectedSpan()
        {
            int index = Spans.IndexOf(_selectedSpan);
            UserSpan newSpan = new UserSpan();
            newSpan.PropertyChanged += OnUserSpanPropertyChanged;
            Spans.Insert(index + 1, newSpan);
            OrderingSection();
            SelectedUserSpan = Spans[index + 1];
        }
        public void RemoveSelectedSpan() // removes selected (doh) span
        {
            if (Spans.Count <= 2) return;
            int index = Spans.IndexOf(_selectedSpan);
            _selectedSpan.PropertyChanged -= OnUserSpanPropertyChanged;
            Spans.Remove(_selectedSpan);

            OrderingSection();

            if (index >= Spans.Count()) return; // last span was removed so nothing to do
            else OnTowerAbscissaPropertyChanged(Spans[index]); // altering span lengths as a consequence of the tower abscissas change (change of tower abscissa has higher priority than span length)

            if (EvaluateNeedToPerformInternalIntegrityCheck()) CheckInternalIntegrity();
        }
        public void ClearSelectedSpan()
        {
            Spans[Spans.IndexOf(_selectedSpan)].Clear();
            SelectedUserSpan.Clear();
        }
        public void ClearBackToBasicSection()
        {
            int index = Spans.Count - 1;
            UserSpan span;
            if (Spans.Count > 2)
            {
                do
                {
                    span = Spans.Last();
                    span.PropertyChanged -= OnUserSpanPropertyChanged;
                    Spans.Remove(span);
                    index--;

                } while (index > 1);
            }

            foreach (UserSpan remainingSpan in Spans)
            {
                remainingSpan.Clear();
            }

            OrderingSection();
            _light = IndicatorLight.None;
        }
        public void SaveSectionModelAsCsv() // method is intended to be dumb: is saves what user creates so no bother with integrity checks
        {
            List<string> collection = new List<string>();
            string csvString;

            string path = StaticFileHandler.GetSaveDialogFilePath("section model", "csv");
            if (path == "") return;

            foreach(UserSpan span in Spans)
            {
                csvString = span.OrdinalDescription + ";"
                    + span.SelectedItem.CodeName + ";"
                    + span.TowerAbscissa.ToString() + ";"
                    + span.TowerOrdinate.ToString() + ";"
                    + span.TowerAPHeight.ToString() + ";"
                    + span.SpanLength.ToString() + ";"
                    + span.InsulatorSetLength.ToString() + ";"
                    + span.InsulatorSetWeight.ToString() + ";"
                    + span.InsulatorSetOpeningAngle.ToString() + ";"
                    + span.InsulatorSetIceLoad.ToString() + ";"
                    + span.SpanIceLoad.ToString() + ";"
                    + span.SpanWindLoad.ToString();

                collection.Add(csvString);
            }

            try
            {
                StaticFileHandler.WriteCollectionToTextFile(path, collection);
            }
            catch (Exception e)
            {
                Annalist.Instance.DisplayStatus("Section Builder", "E251", e.Message);
                return;
            }
            Annalist.Instance.DisplayStatus("Section Builder", "I251");
        }
        public void LoadSectionModelFromCsv()
        {
            IEnumerable<string> collection;
            string path = StaticFileHandler.GetOpenDialogFilePath("section model", "csv");
            if (path == "") return;
            try
            {
                collection = StaticFileHandler.ReadAllFromTextFile(path);
            }
            catch(Exception e)
            {
                Annalist.Instance.DisplayStatus("Section Builder", "E252", e.Message);
                return;
            }
            UpdateFromStringArray(collection.ToArray());
            if (EvaluateNeedToPerformInternalIntegrityCheck()) CheckInternalIntegrity();
            Annalist.Instance.DisplayStatus("Section Builder", "I252");
        }
        public void ManualIntegrityCheck() // wrapper for external command call
        {
            CheckInternalIntegrity();
        }
        // -> Creates user sectrion from external collection... (i.e. from loaded file)
        public void UpdateFromSpatialModel(IEnumerable<ISpanModel> spatialModel)
        {
            int sectionSize = spatialModel.Count();
            int index = 0;
            Spans.Clear();
            // cutting proper size
            for (int i = 0; i < sectionSize; i++) JustAddSpan();
            foreach (ISpanModel rawSpan in spatialModel)
            {
                Spans[index].OrdinalDescription = rawSpan.TowerDescription;
                Spans[index].TowerAbscissa = rawSpan.TowerAbscissa;
                Spans[index].TowerOrdinate = rawSpan.TowerOrdinate;
                Spans[index].TowerAPHeight = rawSpan.AttachmentPointHeight;
                SetInsulatorSetFromExternalData(index, sectionSize, rawSpan.InsulatorSetCode, rawSpan.InsulatorArmLength, rawSpan.InsulatorArmWeight, rawSpan.InsulatorOpeningAngle);
                Spans[index].InsulatorSetIceLoad = rawSpan.InsulatorIceLoad;
                Spans[index].SpanIceLoad = rawSpan.SpanIceLoad;
                Spans[index].SpanWindLoad = rawSpan.SpanWindLoad;
                index++;
            }
            //CheckInternalIntegrity();
        }
        private void UpdateFromStringArray(string[] stringArray)
        {
            int sectionSize = stringArray.Count();
            string[] stringLine;
            double?[] doubleLine;
            Spans.Clear();
            // cutting proper size
            for (int k = 0; k < sectionSize; k++) JustAddSpan();
            
            for (int i = 0; i < sectionSize; i++)
            {
                stringLine = stringArray[i].Split(';');
                if(stringLine.Count() == 12)
                {
                    Spans[i].OrdinalDescription = stringLine[0];

                    doubleLine = new double?[10];
                    
                    for (int p = 1; p < 11; p++)
                    {
                        doubleLine[p - 1] = stringLine[p + 1].AdvancedParseToNullableDouble();
                    }

                    if (_alternativeInputMode) Spans[i].SpanLength = doubleLine[3];
                    else Spans[i].TowerAbscissa = doubleLine[0];
                    Spans[i].TowerOrdinate = doubleLine[1];
                    Spans[i].TowerAPHeight = doubleLine[2];
                    Spans[i].InsulatorSetIceLoad = doubleLine[7];
                    Spans[i].SpanIceLoad = doubleLine[8];
                    Spans[i].SpanWindLoad = doubleLine[9];

                    SetInsulatorSetFromExternalData(i, sectionSize, stringLine[1], doubleLine[4], doubleLine[5], doubleLine[6]);
                } 
            }
        }
        // -> raw exporter
        public IEnumerable<ISpanModel> RawExport()
        {
            RawSectionSpan[] exports = new RawSectionSpan[Spans.Count];
            for(int i=0;i<Spans.Count;i++)
            {
                exports[i] = new RawSectionSpan(Spans[i]);
            }
            return exports;
        }
        // -> Report closing
        public void HideReport()
        {
            IsIntegrityReportShown = false;
        }
        public bool CanExecute_HideReport()
        {
            if (IsIntegrityReportShown) return true;
            else return false;
        }
        // -> Section handling backstage:
        private void CreateBasicSection()
        {
            UserSpan span0 = new UserSpan();
            UserSpan span1 = new UserSpan();

            span0.PropertyChanged += OnUserSpanPropertyChanged;
            span1.PropertyChanged += OnUserSpanPropertyChanged;

            if (Spans.Count != 0) LowLevelSectionErase();
            
            Spans.Add(span0);
            Spans.Add(span1);

            OrderingSection();
        }
        private void OrderingSection()
        {
            int index = 0;
            foreach (UserSpan span in Spans)
            {
                index++;
                span.OrdinalIndex = index;
                span.IsLastSpan = false;
            }
            Spans.Last().IsLastSpan = true;
        }
        private void LowLevelSectionErase()
        {
            foreach (UserSpan span in Spans)
            {
                span.PropertyChanged -= OnUserSpanPropertyChanged;
            }
            Spans.Clear();
        }
        // -> UserSpan PropertyChanged External Reaction
        // --> main wrapper
        private void OnUserSpanPropertyChanged(object sender, EventArgs e)
        {
            PropertyChangedEventArgs pe = e as PropertyChangedEventArgs;
            UserSpan actualactualSpan = sender as UserSpan;
            if (pe.PropertyName == "TowerAbscissa" || pe.PropertyName == "SpanLength")
            {
                if (AlternativeInputMode && actualactualSpan.SpanLength != null)
                {
                    OnSpanLengthPropertyChanged(sender);
                    if (EvaluateNeedToPerformInternalIntegrityCheck()) CheckInternalIntegrity();
                }
                else if (actualactualSpan.TowerAbscissa != null) 
                {
                    OnTowerAbscissaPropertyChanged(sender);
                    if (EvaluateNeedToPerformInternalIntegrityCheck()) CheckInternalIntegrity();
                }
            }
            else 
            {
                if (EvaluateNeedToPerformInternalIntegrityCheck()) CheckInternalIntegrity();
            }
        }
        // --> reaction on concrete property change
        private void OnTowerAbscissaPropertyChanged(object sender)
        {
            UserSpan actualSpan = sender as UserSpan;
            
            int index = Spans.IndexOf(actualSpan);
            double? value;

            for (int i = index - 1; i < Spans.Count - 1; i++)
            {
                if (i >= 0)
                {
                    value = CalculateSpanLengthFromPosition(Spans[i]);
                    //if (!value.HasValue) break;
                    Spans[i].SpanLength = value;
                }
            }
        }
        private void OnSpanLengthPropertyChanged(object sender)
        {
            UserSpan actualSpan = sender as UserSpan;
            int index = Spans.IndexOf(actualSpan);
            double? value;

            if (!actualSpan.TowerAbscissa.HasValue) actualSpan.TowerAbscissa = 0;
            for (int i = index; i < Spans.Count - 1; i++)
            {
                value = CalculatePositionFromSpanLength(Spans[i+1]);
                //if (!value.HasValue) break;
                Spans[i+1].TowerAbscissa = value;
            }

        }
        // --> auxilliaries for above mentioned:
        private double? CalculateSpanLengthFromPosition(UserSpan referenceSpan)
        {
            if (referenceSpan.IsLastSpan) return null;
            else
            {
                double? towerAbscissa1 = referenceSpan.TowerAbscissa;
                double? towerAbscissa2 = Spans[Spans.IndexOf(referenceSpan) + 1].TowerAbscissa;
                double? spanLength;

                if (towerAbscissa1.HasValue && towerAbscissa2.HasValue)
                {
                    spanLength = towerAbscissa2 - towerAbscissa1;
                    if (spanLength <= 0) return null;
                    else return spanLength;
                }
                else return null;
            }
        }
        private double? CalculatePositionFromSpanLength(UserSpan referenceSpan)
        {
            double? spanLength;

            double? towerAbscissa0;

            int indexOfActualSpan;
            int indexOfPreviousSpan;

            indexOfActualSpan = Spans.IndexOf(referenceSpan);
            
            if (indexOfActualSpan == 0)
            {
                if (referenceSpan.TowerAbscissa.HasValue) return referenceSpan.TowerAbscissa;
                else return 0;
            }
            else
            {
                indexOfPreviousSpan = indexOfActualSpan - 1;

                spanLength = Spans[indexOfPreviousSpan].SpanLength;
                towerAbscissa0 = Spans[indexOfPreviousSpan].TowerAbscissa;

                if (towerAbscissa0.HasValue && spanLength.HasValue) return towerAbscissa0 + spanLength;
                else return null;
            }
        }
        // -> Indicators
        private void SetIndicatorLight(IndicatorLight light)
        {
            _light = light;
            RaisePropertyChanged("IsRedLightOn");
            RaisePropertyChanged("IsGreenLightOn");
            RaisePropertyChanged("IsYellowLightOn");
        }
        // -> Integrity Checker
        private bool CheckInternalIntegrity()
        {
            // Digression: only critical codnitions are highlighted and prevent further calculations when doesn't meet. 
            int lastIndex = Spans.Count - 1;
            
            int majorConditionsViolationCount = 0;
            int minorConditionsViolationCount = 0;
            string generalAttachmentSetWarningRowList = "";

            IntegrityReport.Clear();
            GoDarkAllSpans();

            // Cardinal condition:
            if (lastIndex < 1)
            {
                IntegrityReport.Add($"Section model must have atleast two rows: a single ordinary span and the terminating one.");
                SetIndicatorLight(IndicatorLight.None);
                return false;
            }

            for (int i = 0; i <= lastIndex; i++)
            {
                // First (major) condition check: No nulls in nullforbidden fields check
                if (Spans[i].TowerAbscissa == null)
                {
                    IntegrityReport.Add($"Row {i + 1}: Missing tower abscissa value.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].TowerOrdinate == null)
                {
                    IntegrityReport.Add($"Row {i + 1}: Missing tower ordinate value.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].TowerAPHeight == null)
                {
                    IntegrityReport.Add($"Row {i + 1}: Missing tower attachment point height value.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].SpanLength == null && i < lastIndex) // unique case: last span must have null span length
                {
                    IntegrityReport.Add($"Row {i + 1}: Missing span horizontal length value.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                // Second (minor) condition check: Nulls in nullallowed fields found. Here is unique case too. Insulator Opening Angle along with loads arent't checked 
                if (Spans[i].InsulatorSetLength == null || Spans[i].InsulatorSetWeight == null)
                {
                    generalAttachmentSetWarningRowList.Concat($"{i + 1}, ");
                    minorConditionsViolationCount++;
                }
                // Third (major) condition check: Entered values must be in the proper range
                if (Spans[i].TowerAPHeight <= 0)
                {
                    IntegrityReport.Add($"Row {i + 1}: Tower Attachment Point Height must not be equal or less than zero."); // negative value here hasn't much sense from phisical point of view but it's ok from mathematical one.
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].InsulatorSetLength <= 0)
                {
                    IntegrityReport.Add($"Row {i + 1}: Attachment Set length must not be equal or less than zero.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].InsulatorSetWeight <= 0)
                {
                    IntegrityReport.Add($"Row {i + 1}: Attachment Set weight must not be equal or less than zero.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].InsulatorSetOpeningAngle != null && Spans[i].InsulatorSetOpeningAngle < 0)
                {
                    IntegrityReport.Add($"Row {i + 1}: Insulator Set opening angle must not be less than zero if defined."); 
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].InsulatorSetIceLoad < 0)
                {
                    IntegrityReport.Add($"Row {i + 1}: Insulator Set ice load must not be less than zero if defined.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].SpanIceLoad < 0)
                {
                    IntegrityReport.Add($"Row {i + 1}: Span Ice Load must not be less than zero if defined.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                if (Spans[i].SpanWindLoad < 0)
                {
                    IntegrityReport.Add($"Row {i + 1}: Span Wind Load must not be less than zero if defined.");
                    Spans[i].Higlight();
                    majorConditionsViolationCount++;
                }
                // Last but not least (major) condition check: Relation beteen tower abscissas, and horisontal spans' lengths
                if (i != lastIndex)
                {
                    if (!(Spans[i + 1].TowerAbscissa - Spans[i].TowerAbscissa == Spans[i].SpanLength))
                    {
                        IntegrityReport.Add($"Row {i + 1}: Span horizontal length does not match to corresponding tower abscissas.");
                        Spans[i].Higlight();
                        majorConditionsViolationCount++;
                    }
                }
            }

            if(minorConditionsViolationCount == 0 && majorConditionsViolationCount == 0)
            {
                SetIndicatorLight(IndicatorLight.Green);

                IntegrityReport.Add("Section Model is integral and ready for further calculations.");
                IsIntegrityReportShown = false;
                RaiseSpatialModelUpdateEvent();
                return true;
            }
            else if (minorConditionsViolationCount != 0 && majorConditionsViolationCount == 0)
            {
                IntegrityReport.Add($"Rows {generalAttachmentSetWarningRowList}: Attachment set incorrectly defined or not defined at all. If situation is intended, please be aware whole section model will be threated in calculations as a set of independent strain spans.");
                SetIndicatorLight(IndicatorLight.Yellow);
                IsIntegrityReportShown = false;
                RaiseSpatialModelUpdateEvent();
                return true;
            }
            else
            {
                SetIndicatorLight(IndicatorLight.Red);
                IsIntegrityReportShown = true;
                return false;
            }
        }
        private void GoDarkAllSpans()
        {
            foreach(UserSpan span in Spans)
            {
                span.GoDark();
            }
        }
        private bool EvaluateNeedToPerformInternalIntegrityCheck() // CheckInternalIntegrity() method is quite heavy I suppose. So this is it's younger, lighter brother. If it'll return true that will be the indicator to do full check...
        {
            foreach (UserSpan span in Spans)
            {
                if (span.State == UserSpanState.EditionPending) return false;
            }
            return true;
        }
        // -> DB Finder
        private void SetInsulatorSetFromExternalData(int index, int sectionSize, string code, double? armLength, double? armWeight, double? openingAngle)
        {
            IEnumerable<InsulatorSet> insulators;
            IEnumerable<InsulatorSet> matchedInsulators;
            InsulatorSet bufferedInsulatorSet = new InsulatorSet("dummyCat", "dummyCode", "", armLength.GetValueOrDefault(0), armWeight.GetValueOrDefault(0), openingAngle.GetValueOrDefault(0));

            if (index == 0 || index == sectionSize - 1)
            {
                insulators = _insulatorManager.GetSelectedGroup(MainSettings.Instance.FixedCategoryName0);
                matchedInsulators = insulators.Where(ins => ins.CodeName == code);
            }
            else
            {
                insulators = _insulatorManager.GetSelectedGroup(MainSettings.Instance.FixedCategoryName1);
                matchedInsulators = insulators.Where(ins => ins.CodeName == code);
            }
            
            if (matchedInsulators.Count() == 1)
            {
                if (matchedInsulators.First().Equals(bufferedInsulatorSet))
                {
                    Spans[index].SelectedItem = matchedInsulators.First();
                    return;
                }
            }

            Spans[index].InsulatorSetLength = armLength;
            Spans[index].InsulatorSetWeight = armWeight;
            Spans[index].InsulatorSetOpeningAngle = openingAngle;
            return;

        }
    }
}
