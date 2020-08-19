using SSTC_BaseModel;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace SSTC.Modules.DataManager.TabViewModel
{
    // A highly type dependant object just like its view it has no idea about (almost)
    // General idea: every change in database committed by user takes place in ViewModel which acts as database buffer. Change are saved (or not) to proper database during View closing time.
    class ConductorManagerTab : BaseDataManagerTab<Conductor>, INotifyPropertyChanged
    {
        // PROPERTIES:
        // ##### Element Buffer #####
        public double? EB_CrossSection { get; set; }
        public double? EB_Diameter { get; set; }
        public double? EB_RTS { get; set; }
        public double? EB_WeightPerLength { get; set; }
        public double? EB_ThermalExpansionCoefficient { get; set; }
        public double? EB_ModulusOfElasticity { get; set; }
        public double? EB_Resistance20oCPerLength { get; set; }
        public string EB_Category { get; set; }
        public string EB_CodeName { get; set; }
        public string EB_Description { get; set; }
        // ##### ############### #####

        // BASE CONSTRUCTOR:
        public ConductorManagerTab()
        {
            // populating type dependent status field
            TabTitle = "Conductor Data Manager";
            // initializing buffer's element property names list
            _bufferPropertyNames = new List<string> { "EB_CrossSection", "EB_Diameter", "EB_RTS", "EB_WeightPerLength", "EB_ThermalExpansionCoefficient", "EB_ModulusOfElasticity", "EB_Resistance20oCPerLength", "EB_Category","EB_CodeName", "EB_Description" };

            SynchronizeBufferWithSelectedElement();
            // linking expansion commands

        }
        // EXPANSION COMMANDS:
        // -> ...
        //INTERNAL METHODS:
        // -> strictly internal methods:
        protected override void SynchronizeBufferWithSelectedElement()
        {
            if (_selectedRow == null)
            {
                EB_CrossSection = null;
                EB_Diameter = null;
                EB_RTS = null;
                EB_WeightPerLength = null;
                EB_ThermalExpansionCoefficient = null;
                EB_ModulusOfElasticity = null;
                EB_Resistance20oCPerLength = null;
                EB_Category = null;
                EB_CodeName = null;
                EB_Description = null;
            }
            else
            {
                EB_CrossSection = _selectedRow.CrossSection;
                EB_Diameter = _selectedRow.Diameter;
                EB_RTS = _selectedRow.RTS;
                EB_WeightPerLength = _selectedRow.WeightPerLength;
                EB_ThermalExpansionCoefficient = _selectedRow.ThermalExpansionCoefficient;
                EB_ModulusOfElasticity = _selectedRow.ModulusOfElasticity;
                EB_Resistance20oCPerLength = _selectedRow.Resistance20oCPerLength;
                EB_Category = _selectedRow.Category;
                EB_CodeName = _selectedRow.CodeName;
                EB_Description = _selectedRow.Description;
            }
        }
        protected override bool IsBufferSynchronizedWithSelectedElement()
        {
            if (!(EB_CrossSection.GetValueOrDefault(0) == _selectedRow.CrossSection)) return false;
            if (!(EB_Diameter.GetValueOrDefault(0) == _selectedRow.Diameter)) return false;
            if (!(EB_RTS.GetValueOrDefault(0) == _selectedRow.RTS)) return false;
            if (!(EB_WeightPerLength.GetValueOrDefault(0) == _selectedRow.WeightPerLength)) return false;
            if (!(EB_ThermalExpansionCoefficient.GetValueOrDefault(0) == _selectedRow.ThermalExpansionCoefficient)) return false;
            if (!(EB_ModulusOfElasticity.GetValueOrDefault(0) == _selectedRow.ModulusOfElasticity)) return false;
            if (!(EB_Resistance20oCPerLength.GetValueOrDefault(0) == _selectedRow.Resistance20oCPerLength)) return false;
            if (!(EB_Category == _selectedRow.Category)) return false;
            if (!(EB_CodeName == _selectedRow.CodeName)) return false;
            if (!(EB_Description == _selectedRow.Description)) return false;
            return true;
        }
        protected override bool CodeNameDuplicatesWithinSameCategoryCheck()
        {
            var tempResults = from conductor in _allElements where (conductor.Category == EB_Category && conductor.CodeName == EB_CodeName) select conductor;
            if (tempResults.Any()) return true;
            else return false;
        }
        protected override bool ElementBufferEnteredDataValidationCheck()
        {
            if (EB_CrossSection == null) return false;
            if (EB_Diameter == null) return false;
            if (EB_RTS == null) return false;
            if (EB_WeightPerLength == null) return false;
            if (EB_ThermalExpansionCoefficient == null) return false;
            if (EB_ModulusOfElasticity == null) return false;
            if (EB_Resistance20oCPerLength == null) return false;
            if (EB_Category == null) return false;
            if (EB_CodeName == null) return false;
            //if (EB_Description == null) return false;
            return true;
        }
        protected override Conductor GetElementFromElementBuffer()
        {
            return new Conductor(EB_Category, EB_CodeName, EB_CrossSection.GetValueOrDefault(0), EB_Diameter.GetValueOrDefault(0), EB_RTS.GetValueOrDefault(0), EB_WeightPerLength.GetValueOrDefault(0), EB_ThermalExpansionCoefficient.GetValueOrDefault(0), EB_ModulusOfElasticity.GetValueOrDefault(0), EB_Resistance20oCPerLength.GetValueOrDefault(0), EB_Description);
        }
        protected override Conductor CreateItemFromCSV(string csvLine)
        {
            string[] strings = csvLine.Split(';');      // [cat, code, A, d, mC, RTS, aT, E, R20, desc]
            double[] doubles = new double[7];           // [A, d, mC, RTS, aT, E, R20]

            for (int i = 0; i < 7; i++)
            {
                if (!double.TryParse(strings[2 + i], System.Globalization.NumberStyles.Any,CultureInfo.InvariantCulture,out doubles[i])) return null;
            }

            double A = doubles[0];
            double d = doubles[1];
            double RTS = doubles[3];
            double mC = doubles[2];
            double aT = doubles[4];
            double E = doubles[5];
            double R20 = doubles[6];

            string Cat = strings[0];
            string Code = strings[1];
            string Desc = strings[9];

            return new Conductor(Cat, Code, A, d, RTS, mC, aT, E, R20, Desc);
        }
    }
}
