using SSTC_BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Modules.Calculator.ModuleInternals
{
    // Object represents a pair of "tower" and the information about length to next one. So in section the last span object will be holding infos only about "tower".
    // Quite smart container.
    using SSTC.Common;
    using SSTC.Modules.DataManager;
    using System.Collections.ObjectModel;
    class UserSpan : INotifyPropertyChanged, ISpanModel
    {
        // AUXILLIARY FIELDS
        private string userDefinedName;
        private InsulatorSet dummySet;

        private DataManager<InsulatorSet> insulatorManager;
        private bool isLastSpan;
        private UserSpanState state;

        public string UserDefinedName
        {
            get { return userDefinedName; }
        }
        public bool IsLastSpan
        {
            get { return isLastSpan; }
            set
            {
                if (isLastSpan != value)
                {
                    if (value) SpanLength = null;
                    else SelectedItem = dummySet;
                    
                    isLastSpan = value;
                    RaisePropertyChanged("IsFirstSpan");
                    RaisePropertyChanged("IsLastSpan");
                    RaisePropertyChanged("InsulatorSetsAvailableSelection");
                    
                }
            }
        }
        public bool IsFirstSpan
        {
            get 
            {
                if (index == 1) return true;
                else return false;
            }
        }
        public bool IsHighlighted { get; private set; }
        public UserSpanState State 
        {
            get { return state; }
            private set
            {
                state = value;
                RaisePropertyChanged("State");
            }
        }
        public ObservableCollection<IDataManageable> InsulatorSetsAvailableSelection
        {
            get
            {
                if (IsFirstSpan || IsLastSpan)
                {
                    ObservableCollection<IDataManageable> baseCollection = new ObservableCollection<IDataManageable>() { dummySet };
                    IEnumerable<IDataManageable> mainCollection = insulatorManager.GetSelectedGroup(MainSettings.Instance.FixedCategoryName0);
                    foreach (IDataManageable item in mainCollection)
                    {
                        baseCollection.Add(item);
                    }
                    return baseCollection;
                }
                else
                {
                    ObservableCollection<IDataManageable> baseCollection = new ObservableCollection<IDataManageable>() { dummySet };
                    IEnumerable<IDataManageable> mainCollection = insulatorManager.GetSelectedGroup(MainSettings.Instance.FixedCategoryName1);
                    foreach (IDataManageable item in mainCollection)
                    {
                        baseCollection.Add(item);
                    }
                    return baseCollection;
                }
            }
        }
        // SPAN FIELDS
        private int index;
        private string ordinalDescription;
        // -> Tower Part
        private double? towerAbscissa;
        private double? towerOrdinate;
        private double? towerAPHeight;
        // -> Span Part
        private double? spanLength;
        // -> Attachment Set Part
        private InsulatorSet selectedItem;
        private double? insulatorSetLength;
        private double? insulatorSetWeight;
        private double? insulatorSetOpeningAngle;
        // -> Loads Part
        private double? insulatorSetIceLoad;
        private double? spanIceLoad;
        private double? spanWindLoad;
        // SPAN PROPERTIES
        public int OrdinalIndex 
        { 
            get { return index; } 
            set 
            { 
                if(index != value)
                {
                    index = value;
                    RaisePropertyChanged("OrdinalIndex");
                }
            } 
        }
        public string OrdinalDescription
        {
            get { return ordinalDescription; }
            set
            {
                if (ordinalDescription != value)
                {
                    ordinalDescription = value;
                    RaisePropertyChanged("OrdinalDescription");
                }
            }
        }
        // -> Tower Part
        public double? TowerAbscissa
        {
            get { return towerAbscissa; }
            set
            {
                if (towerAbscissa != value)
                {
                    towerAbscissa = value;
                    RaisePropertyChanged("TowerAbscissa");
                }
            }
        }
        public double? TowerOrdinate
        {
            get { return towerOrdinate; }
            set
            {
                if (towerOrdinate != value)
                {
                    towerOrdinate = value;
                    RaisePropertyChanged("TowerOrdinate");
                }
            }
        }
        public double? TowerAPHeight
        {
            get { return towerAPHeight; }
            set
            {
                if (towerAPHeight != value)
                {
                    if (value <= 0) towerAPHeight = null;
                    else towerAPHeight = value;
                    RaisePropertyChanged("TowerAPHeight");
                }
            }
        }
        // -> Span Part
        public double? SpanLength
        {
            get { return spanLength; }
            set
            {
                if (spanLength != value)
                {
                    if (value <= 0) spanLength = null;
                    else spanLength = value;
                    RaisePropertyChanged("SpanLength");
                }
            }
        }
        // -> Attachment Set Part
        public InsulatorSet SelectedItem
        {
            get { return selectedItem; }
            set
            {
                InsulatorSet insulatorSet = value;

                if (insulatorSet != dummySet && insulatorSet != null)
                {
                    InsulatorSetLength = insulatorSet.ArmLength;
                    InsulatorSetWeight = insulatorSet.ArmWeight;
                    InsulatorSetOpeningAngle = insulatorSet.OpeningAngle;
                    selectedItem = value;
                }
                else if (insulatorSet == dummySet || insulatorSet == null) 
                {
                    InsulatorSetLength = null;
                    InsulatorSetWeight = null;
                    InsulatorSetOpeningAngle = null;
                    selectedItem = dummySet;
                }
                RaisePropertyChanged("SelectedItem");
            }
        }
        public double? InsulatorSetLength
        {
            get { return insulatorSetLength; }
            set
            {
                if (insulatorSetLength != value)
                {
                    if (value < 0) insulatorSetLength = null;
                    else insulatorSetLength = value;
                    MarkAsUserDefined();
                    RaisePropertyChanged("InsulatorSetLength");
                }
            }
        }
        public double? InsulatorSetWeight
        {
            get { return insulatorSetWeight; }
            set
            {
                if (insulatorSetWeight != value)
                {
                    if (value < 0) insulatorSetWeight = null;
                    else insulatorSetWeight = value;
                    MarkAsUserDefined();
                    RaisePropertyChanged("InsulatorSetWeight");
                }
            }
        }
        public double? InsulatorSetOpeningAngle
        {
            get { return insulatorSetOpeningAngle; }
            set
            {
                if (insulatorSetOpeningAngle != value)
                {
                    if (value < 0) insulatorSetOpeningAngle = null;
                    else insulatorSetOpeningAngle = value;
                    MarkAsUserDefined();
                    RaisePropertyChanged("InsulatorSetOpeningAngle");
                }
            }
        }
        // -> Loads Part
        public double? InsulatorSetIceLoad
        {
            get { return insulatorSetIceLoad; }
            set
            {
                if (insulatorSetIceLoad != value)
                {
                    if (value <= 0) insulatorSetIceLoad = null;
                    else insulatorSetIceLoad = value;
                    RaisePropertyChanged("InsulatorSetIceLoad");
                }
            }
        }
        public double? SpanIceLoad
        {
            get { return spanIceLoad; }
            set
            {
                if (spanIceLoad != value)
                {
                    if (value <= 0) spanIceLoad = null;
                    else spanIceLoad = value;
                    RaisePropertyChanged("SpanIceLoad");
                }
            }
        }
        public double? SpanWindLoad
        {
            get { return spanWindLoad; }
            set
            {
                if (spanWindLoad != value)
                {
                    if (value <= 0) spanWindLoad = null;
                    else spanWindLoad = value;
                    RaisePropertyChanged("SpanWindLoad");
                }
            }
        }
        // -> ISpanModel Interface Part
        string ISpanModel.TowerDescription { get { return ordinalDescription; } }
        double ISpanModel.TowerAbscissa { get { return towerAbscissa.GetValueOrDefault(0); } }
        double ISpanModel.TowerOrdinate { get { return towerOrdinate.GetValueOrDefault(0); } }
        double ISpanModel.AttachmentPointHeight { get { return towerAPHeight.GetValueOrDefault(0); } }
        string ISpanModel.InsulatorSetCode { get { return SelectedItem.CodeName; } }
        double ISpanModel.InsulatorArmLength { get { return insulatorSetLength.GetValueOrDefault(0); } }
        double ISpanModel.InsulatorArmWeight { get { return insulatorSetWeight.GetValueOrDefault(0); } }
        double ISpanModel.InsulatorOpeningAngle { get { return insulatorSetOpeningAngle.GetValueOrDefault(0); } }
        double ISpanModel.InsulatorIceLoad { get { return insulatorSetIceLoad.GetValueOrDefault(0); } }
        double ISpanModel.SpanIceLoad { get { return spanIceLoad.GetValueOrDefault(0); } }
        double ISpanModel.SpanWindLoad { get { return spanWindLoad.GetValueOrDefault(0); } }

        // CONSTRUCTORS
        public UserSpan()
        {
            this.userDefinedName = "User Defined";
            this.insulatorManager = InsulatorSetDataManager.Instance;
            this.dummySet = new InsulatorSet("", userDefinedName, "", 0, 0);
            this.selectedItem = dummySet;

            isLastSpan = false;
            this.State = UserSpanState.EditionPending;

            OrdinalIndex = 0;
            Clear();
        }
        // EVENTS
        public event PropertyChangedEventHandler PropertyChanged;
        // TAB EVENT RAISERS
        protected void RaisePropertyChanged(string propertyName)
        {
            EvaluateState();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // METHODS
        public void Clear()
        {
            SelectedItem = dummySet;

            OrdinalDescription = null;
            TowerAbscissa = null;
            TowerOrdinate = null;
            TowerAPHeight = null;
            SpanLength = null;

            InsulatorSetLength = null;
            InsulatorSetWeight = null;
            InsulatorSetOpeningAngle = null;
        }
        public void SynchronizeWithAttachmentSetDatabase()
        {
            int index;
            InsulatorSet bufferedInsulatorSet = SelectedItem;

            RaisePropertyChanged("InsulatorSetsAvailableSelection"); // I'm pretty sure combobox forget about it's selected item when it's itemsource will be updated/changed

            index = InsulatorSetsAvailableSelection.IndexOf(bufferedInsulatorSet);
            if (index >= 0) SelectedItem = (InsulatorSet)InsulatorSetsAvailableSelection[index];
            else SelectedItem = null;
        }
        public void Higlight()
        {
            if (!IsHighlighted)
            {
                IsHighlighted = true;
                RaisePropertyChanged("IsHighlighted");
            }
        }
        public void GoDark()
        {
            if (IsHighlighted)
            {
                IsHighlighted = false;
                RaisePropertyChanged("IsHighlighted");
            }
        }
        private void MarkAsUserDefined()
        {
            selectedItem = dummySet; // intended: changes only selected item (its name to "User Defined"). Occurs when user manualy changes any attachment set related value to inform user that from now attachment set is defined by him (case with intended desynchronization between selected item and its related values) uff... in other places where attachment set reset is required the statement looks like this: SelectedItem = dummySet;
            RaisePropertyChanged("SelectedItem");
        }
        private void EvaluateState()
        {
            bool indicator = true;

            if (!TowerOrdinate.HasValue) indicator = false;
            if (!TowerAPHeight.HasValue) indicator = false;
            if (!TowerAbscissa.HasValue && !SpanLength.HasValue) indicator = false;

            if(indicator)
            {
                if (State == UserSpanState.EditionPending) State = UserSpanState.EditionSettled;
            }
            else
            {
                if (State == UserSpanState.EditionSettled) State = UserSpanState.EditionPending;
            }
        }
    }
}
