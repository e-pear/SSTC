using SSTC_BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Modules.DataManager.TabViewModel
{
    using System.Globalization;
    // Similar functionality as ConductorManagerTab, although this one has a fixed category set
    using SSTC.Common;
    class InsulatorSetManagerTab : BaseDataManagerTab<InsulatorSet>, INotifyPropertyChanged
    {
        // PROPERTIES:
        private List<string> _availableTypes;
        // private bool _openingAngleVisibility;
        private double? eb_OpeningAngle;
        private string eb_SelectedType;
        public List<string> AvailableTypes { get { return _availableTypes; } }
        public string DisplayedImage
        {
            get
            {
                if (EB_SelectedType == _availableTypes[0]) return "/AssemblyName;component/Images/strain.png";
                else if (EB_SelectedType == _availableTypes[1]) return "/AssemblyName;component/Images/standard_suspension.png";
                //else if (EB_SelectedType == _availableTypes[2]) return "/AssemblyName;component/Images/invertedv_suspension.png";
                else return "";
            }
        }
        // -> dynamic labels
        public string ArmLengthLabel 
        { 
            get 
            {
                /*
                if (EB_SelectedType != _availableTypes[2]) return "Length [m]:";
                else return "Arm length [m]:";
                */
                return "Arm length [m]:";
            } 
        }
        public string ArmWeightLabel
        {
            get
            {
                /*
                if (EB_SelectedType != _availableTypes[2]) return "Weight [kg]:";
                else return "Arm weight [kg]:";
                */
                return "Arm weight [kg]:";
            }
        }
        public bool OpeningAngleVisibility
        {
            get
            {
                /*
                bool _openingAngleVisibilityLastValue = _openingAngleVisibility;
                if (SelectedRow == null)
                {

                    if (EB_SelectedType == _availableTypes[2]) _openingAngleVisibility = true;
                    else _openingAngleVisibility = false;
                }
                else
                {
                    bool bufferIs0OrNull = (EB_OpeningAngle == 0 || EB_OpeningAngle == null);
                    if (IsBufferSynchronizedWithSelectedElement() && bufferIs0OrNull) _openingAngleVisibility = false;
                    else _openingAngleVisibility = true;
                }
                if (_openingAngleVisibilityLastValue != _openingAngleVisibility)
                {
                    EB_OpeningAngle = null;
                    RaisePropertyChanged("EB_OpeningAngle");
                }
                return _openingAngleVisibility;
                */
                return false; // <- The math model doesn't support calculations with inverted V insulators yet. So invertd-Vs are disabled
            }
        }
        // ##### Element Buffer #####
        public double? EB_ArmLength { get; set; }
        public double? EB_ArmWeight { get; set; }
        public double? EB_OpeningAngle { get { return eb_OpeningAngle; } set { eb_OpeningAngle = value; RaisePropertyChanged("OpeningAngleVisibility"); } }
        public string EB_SelectedType { get { return eb_SelectedType; } set { eb_SelectedType = value; RaisePropertyChanged("EB_SelectedType"); RaisePropertyChanged("ArmLengthLabel"); RaisePropertyChanged("ArmWeightLabel"); RaisePropertyChanged("OpeningAngleVisibility"); } }
        public string EB_CodeName { get; set; }
        public string EB_Description { get; set; }
        // ##### ############### #####

        // BASE CONSTRUCTOR:
        public InsulatorSetManagerTab()
        {
            // populating type dependent status field
            TabTitle = "Attachment Sets Data Manager";
            //_openingAngleVisibility = false;
            // initializing buffer's element property names list
            _bufferPropertyNames = new List<string> { "EB_ArmLength", "EB_ArmWeight", "EB_OpeningAngle", "EB_SelectedType", "EB_CodeName", "EB_Description", "DisplayImage" };
            // extending category set
            //_categories.Add(_MainSettings.FixedCategoryName0);
            //_categories.Add(_MainSettings.FixedCategoryName1);
            // populating available types
            _availableTypes = new List<string>(_MainSettings.AttachmentSetsTypes);
            EB_SelectedType = _availableTypes[0];
            // SynchronizeBufferWithSelectedElement();
            // linking expansion commands

        }
        // EXPANSION COMMANDS:
        protected override bool CanExecute_AddCategoryCommand()
        {
            return false;
        }
        protected override bool CanExecute_RemRenCategoryCommand()
        {
            return false;
        }
        //INTERNAL METHODS:
        // -> overrides:
        protected override void SynchronizeBufferWithSelectedElement()
        {
            if (_selectedRow == null)
            {
                EB_ArmLength = null;
                EB_ArmWeight = null;
                EB_OpeningAngle = null;
                EB_SelectedType = null;
                EB_CodeName = null;
                EB_Description = null;
            }
            else
            {
                EB_ArmLength = _selectedRow.ArmLength;
                EB_ArmWeight = _selectedRow.ArmWeight;
                EB_OpeningAngle = _selectedRow.OpeningAngle;
                EB_SelectedType = GetTypeFromSelectedElement();
                EB_CodeName = _selectedRow.CodeName;
                EB_Description = _selectedRow.Description;
            }
        }
        protected override bool IsBufferSynchronizedWithSelectedElement()
        {
            if (!(EB_ArmLength.GetValueOrDefault(0) == _selectedRow.ArmLength)) return false;
            if (!(EB_ArmWeight.GetValueOrDefault(0) == _selectedRow.ArmWeight)) return false;
            if (!(EB_OpeningAngle.GetValueOrDefault(0) == _selectedRow.OpeningAngle)) return false;
            if (!(EB_SelectedType == GetTypeFromSelectedElement())) return false;
            if (!(EB_CodeName == _selectedRow.CodeName)) return false;
            if (!(EB_Description == _selectedRow.Description)) return false;
            return true;
        }
        protected override bool CodeNameDuplicatesWithinSameCategoryCheck()
        {
            var tempResults = from element in _allElements where (element.Category == EB_SelectedType && element.CodeName == EB_CodeName) select element;
            if (tempResults.Any()) return true;
            else return false;
        }
        protected override bool ElementBufferEnteredDataValidationCheck()
        {
            if (EB_ArmLength == null) return false;
            if (EB_ArmWeight == null) return false;
            //if (EB_SelectedType == _availableTypes[2] && (EB_OpeningAngle == null || EB_OpeningAngle == 0)) return false;
            //if (EB_SelectedType != _availableTypes[2] && (EB_OpeningAngle.HasValue && EB_OpeningAngle != 0)) return false;
            if (EB_SelectedType == null || EB_SelectedType == "") return false;
            if (EB_CodeName == null) return false;
            //if (EB_Description == null) return false;
            return true;
        }
        protected override void SortCategories() { } // no need to sort anything here
        protected override InsulatorSet GetElementFromElementBuffer()
        {
            return new InsulatorSet(GetCategoryFromTypedElement(), EB_CodeName, EB_Description, EB_ArmLength.GetValueOrDefault(0), EB_ArmWeight.GetValueOrDefault(0), EB_OpeningAngle.GetValueOrDefault(0));
        }
        protected override InsulatorSet CreateItemFromCSV(string csvLine)
        {
            string[] strings = csvLine.Split(';');      // [cat, code, Lins, Mins, ains, desc]
            double[] doubles = new double[3];           // [Lins, Mins, ains]

            for (int i = 0; i < 3; i++)
            {
                if (!double.TryParse(strings[2 + i],NumberStyles.Any,CultureInfo.InvariantCulture, out doubles[i])) return null;
            }

            double Lins = doubles[0];
            double Mins = doubles[1];
            double ains = doubles[2];

            string Cat = strings[0];
            string Code = strings[1];
            string Desc = strings[5];

            return new InsulatorSet(Cat, Code, Desc, Lins, Mins, ains);
        }
        protected override void Import()
        {
            if (_selectedCategory == _categories[0])
            {
                IEnumerable<string> importedRawData = ImportFromCSV();
                if (IsImportedDataCompatibleAccordingToCategory(importedRawData)) Import(importedRawData);
                else _Annalist.DisplayStatus(this, "E128", new string[1] { _MainSettings.FixedCategoryName0 + ", " + _MainSettings.FixedCategoryName1 }); // E128;Database management error;Database import operation aborted. Please use only allowed category names in imported data file. Allowed category names: @1.
            }
            else base.Import();
        }
        // -> own methods:
        protected bool IsImportedDataCompatibleAccordingToCategory(IEnumerable<string> importedTextData)
        {
            if (importedTextData == null)
            {
                _Annalist.DisplayStatus(this, "E127"); // E127;Database management error;Import operation failed. An unknown error has occured.
                return false;
            }

            int firstSplitterIndex;
            int incompatibleCategoriesCount = 0;
            string actualCategory;
            bool condition;

            foreach(string line in importedTextData)
            {
                firstSplitterIndex = line.IndexOf(';');
                actualCategory = line.Substring(0, firstSplitterIndex);

                condition = (actualCategory == _MainSettings.FixedCategoryName0 || actualCategory == _MainSettings.FixedCategoryName1);
                if (!condition) incompatibleCategoriesCount++;
            }

            if (incompatibleCategoriesCount == 0) return true;
            else return false;
        }
        protected string GetTypeFromSelectedElement()
        {
            if (_selectedRow.Category == _MainSettings.FixedCategoryName0) return _availableTypes[0]; // if element belongs to "Dead-End Sets" then return "Dead-End Set"
            else if (_selectedRow.Category == _MainSettings.FixedCategoryName1)
            {
                if (_selectedRow.OpeningAngle == 0) return _availableTypes[1]; // if element belongs to "Suspension Sets" and has no opening angle then return "Suspension Set"
                else return _availableTypes[2]; // if element belongs to "Suspension Sets" and has opening angle then return "Inverted-V Set"
            }
            else return ""; // if something goes wrong...
        }
        protected string GetCategoryFromTypedElement()
        {
            if (EB_SelectedType == _availableTypes[0]) return _MainSettings.FixedCategoryName0;
            else if (EB_SelectedType == _availableTypes[1] || EB_SelectedType == _availableTypes[2]) return _MainSettings.FixedCategoryName1;
            else return ""; // <- that should never happened because of ElementBufferEnteredDataValidationCheck() method... but who knows.
        }
    }
}
