using SSTC_BaseModel;
using SSTC_ViewResources;
using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Modules.DataManager.TabViewModel
{
    using SSTC.Modules.DataManager.DialogBox;
    using SSTC.Common;

    abstract class BaseDataManagerTab<T> : Tab where T: class// T is type of managed elements
    {
        // FIELDS
        // -> data model field
        protected Dictionary<string, List<IDataManageable>> _dataModel;
        // -> buffered element related fields
        protected IEnumerable<string> _bufferPropertyNames; 
        // -> collections' backing fields
        protected ObservableCollection<string> _categories;
        protected ObservableCollection<T> _allElements;
        protected ObservableCollection<T> _selectedElements;
        // -> collections' selection buffer backing fields:
        protected string _selectedCategory;
        protected T _selectedRow;
        // -> status propagation backing fields:
        protected string _databaseStatus;
        protected bool _isChangeCommitted;
        // PROPERTIES
        // -> external datamodel property (unique property exposed for DataManager parent object, not for View)
        public Dictionary<string, List<IDataManageable>> DataModel
        {
            get { return _dataModel; }
            set 
            {
                if (value != null)
                {
                    _dataModel = value;
                    SynchronizeDataModel();
                    UpdateStatus();
                }
            }
        }
        // -> collections:
        public ObservableCollection<T> ShownCollection
        {
            get
            {
                if (_selectedCategory == null || _selectedCategory == Categories[0]) return _allElements;
                else
                {
                    QuerryBySelectedCategory();
                    return _selectedElements;
                }
            }
        }
        public ObservableCollection<string> Categories
        {
            get { return _categories; }
        }
        public ObservableCollection<string> BufferedCategories // this one shows categories without forbidden one - used in side buffer 
        { 
            get 
            {
                return new ObservableCollection<string>(_categories.Skip(1));
            } 
        }
        // -> collections' selection buffer:
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set { _selectedCategory = value; RaisePropertyChanged("SelectedCategory"); RaisePropertyChanged("ShownCollection"); NotifyAboutCategoryDependentDynamicLabelsChange(); }
        }
        public T SelectedRow
        {
            get { return _selectedRow; }
            set 
            {
                if (_selectedRow != value)
                {
                    _selectedRow = value;
                    SynchronizeBufferWithSelectedElement();
                    RaiseElementBufferChanged();
                }
            }
        }
        // -> element selection buffer:
        // ########################################
        // Set of properties defined in child class
        // ########################################
        // -> status:
        public bool IsChangeCommitted
        {
            set
            {
                bool previousState = _isChangeCommitted;
                string presentTitle = TabTitle;
                _isChangeCommitted = value;
                if (previousState == false) TabTitle = string.Concat(presentTitle," *");
                else if (previousState == true && value == false) TabTitle = presentTitle.TrimEnd(' ','*');

                RaisePropertyChanged("IsChangeCommitted");
            }
        }
        public string DatabaseStatus
        {
            get { return _databaseStatus; }
        }
        // -> export/import labels:
        public string ExportLabel
        {
            get
            {
                if (_selectedCategory == _categories[0]) return "Export Database";
                else return "Export from " + _selectedCategory + " Category";
            }
        }
        public virtual string ImportLabel
        {
            get
            {
                if (_selectedCategory == _categories[0]) return "Import and replace Database";
                else return "Import to " + _selectedCategory + " Category";
            }
        }

        // EVENTS:
        // -> internal management closed (passing result of user decision made by view closing):
        public delegate void InternalManagementEndEventRaiser(bool changesAccepted);
        public event InternalManagementEndEventRaiser OnInternalManagementEnd;

        // BASE COMMANDS:
        // -> single record related commands (In-Tab Commands):
        public ICommand SaveAsNewRecordCommand { get; }
        public ICommand OverwriteRecordCommand { get; }
        public ICommand RemoveRecordCommand { get; }
        // -> category scoped database commands (Side Menu Commands):
        public ICommand AddNewCategoryCommand { get; }
        public ICommand RemoveCategoryCommand { get; }
        public ICommand RenameCategoryCommand { get; }
        // -> export/import commands (Side Menu Commands):
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }

        // BASE CONSTRUCTOR
        public BaseDataManagerTab()
        {
            // global var
            string forbiddenCategory = _MainSettings.ForbiddenCategoryName;

            // populating buffers and categories 
            _selectedElements = new ObservableCollection<T>();
            _categories = new ObservableCollection<string> { forbiddenCategory };
            _selectedCategory = _categories[0];
            _isChangeCommitted = false;

            // linking commands
            
            SaveAsNewRecordCommand = new CommandRelay(SaveAsNewRecord, ElementBufferEnteredDataValidationCheck);
            OverwriteRecordCommand = new CommandRelay(OverwriteRecord, CanExecute_RecordCommand_Overwrite);
            RemoveRecordCommand = new CommandRelay(RemoveSelectedRecord, CanExecute_RecordCommand_Remove);

            AddNewCategoryCommand = new CommandRelay(AddNewCategory, CanExecute_AddCategoryCommand);
            RemoveCategoryCommand = new CommandRelay(RemoveCategory, CanExecute_RemRenCategoryCommand);
            RenameCategoryCommand = new CommandRelay(RenameCategory, CanExecute_RemRenCategoryCommand);

            ExportCommand = new CommandRelay(Export, CanExecute_ExportCommand);
            ImportCommand = new CommandRelay(Import, CanExecute_ImportCommand);
        }

        // EVENT RAISERS:

        // -> simple wrapper to raise property event changed for all element's selection buffered properties
        protected void RaiseElementBufferChanged()
        {
            foreach(string propertyName in _bufferPropertyNames)
            {
                RaisePropertyChanged(propertyName);
            }
        }
        // -> raise event for VM caller to tell him about user's actions outcome
        protected void RaiseInternalManagementEndEvent(bool changesAccepted = false)
        {
            OnInternalManagementEnd?.Invoke(changesAccepted);
        }

        // INTERNAL OVERRIDEABLE METHODS:
        protected abstract void SynchronizeBufferWithSelectedElement();
        protected abstract bool IsBufferSynchronizedWithSelectedElement();
        protected abstract bool CodeNameDuplicatesWithinSameCategoryCheck();
        protected abstract bool ElementBufferEnteredDataValidationCheck(); 
        protected abstract T GetElementFromElementBuffer();
        protected abstract T CreateItemFromCSV(string csvLine);
        protected virtual void SortCategories()
        {
            List<string> baseCollection = new List<string> { _categories[0] };
            List<string> sortableCollection = new List<string>();
            for (int i = 1; i < _categories.Count; i++)
            {
                sortableCollection.Add(_categories[i]);
            }
            sortableCollection.Sort();
            baseCollection.AddRange(sortableCollection);

            _categories = new ObservableCollection<string>(baseCollection);
            RaisePropertyChanged("Categories");
        }
        protected virtual void SynchronizeDataModel()
        {
            foreach(string category in _dataModel.Keys)
            {
                _categories.Add(category);
            }
            SortCategories();
            
            _allElements = new ObservableCollection<T>(CastTo_T(_dataModel.SelectMany(d => d.Value.ToList())));
        }
        protected virtual void QuerryBySelectedCategory()
        {
            IEnumerable<IDataManageable> collection = CastFrom_T(_allElements);
            IEnumerable<IDataManageable> selectedElements = from element in collection where element.Category == _selectedCategory select element;
            _selectedElements = new ObservableCollection<T>(CastTo_T(selectedElements));
        }
        protected virtual void NotifyAboutCategoryDependentDynamicLabelsChange()
        {
            RaisePropertyChanged("ExportLabel"); 
            RaisePropertyChanged("ImportLabel");
        }
        protected virtual void Import(IEnumerable<string> importedRawData) // Import and replace all database or import and merge new data to selected category.
        {
            // AUXILIARY VARIABLES

            T importedElement;
            int counter_RawData, counter_ImportedData, counter_AlteredRecords;
            
            List<T> importedData = new List<T>();
            List<string> importedCategories = new List<string>();

            string forbiddenCategory = _categories[0];

            bool extend = (_selectedCategory != _categories[0]);
            counter_RawData = importedRawData.Count();

            // DATA PROCESSING
            foreach (string record in importedRawData)
            {
                importedElement = CreateItemFromCSV(record);
                if (!(importedElement == null)) importedData.Add(importedElement);
            }
            counter_ImportedData = importedData.Count();
            // -> category processing
            foreach (IDataManageable element in importedData)
            {
                if (!importedCategories.Contains(element.Category)) importedCategories.Add(element.Category);
            }
            if (!extend)
            {
                if (!importedCategories.Contains(forbiddenCategory)) importedCategories.Insert(0, forbiddenCategory);
                else
                {
                    _Annalist.DisplayStatus(this, "E129", new string[1] { forbiddenCategory }); // E129;Database management error;Database import operation aborted. Forbidden category name exploitation attempt detected. Please do not use: '@1' as category name in imported data file.
                    return;
                }
            }

            // IMPORTING DATA
            if (extend) // User's choice - append new data to existing category:
            {
                // here elements from imported data are assigned to choosen category
                foreach (IDataManageable element in importedData)
                {
                    element.Category = _selectedCategory;
                }
                // appending new data and removing any duplicates
                IEnumerable<T> extendedData = _allElements.Concat(importedData);
                counter_AlteredRecords = AlterDuplicates(extendedData);
                // setting extedned data back to buffer
                _allElements = new ObservableCollection<T>(extendedData);
            }
            else // User's choice - import new data and replace existing one:
            {
                // setting new data to buffer
                counter_AlteredRecords = AlterDuplicates(importedData);
                _allElements = new ObservableCollection<T>(importedData);
                _categories = new ObservableCollection<string>(importedCategories);
                _selectedCategory = _categories[0];
                SortCategories();
            }
            // Inform world about changes
            RaisePropertyChanged("ShownCollection");
            RaisePropertyChanged("Categories");
            RaisePropertyChanged("BufferedCategories");
            RaisePropertyChanged("SelectedCategory");
            UpdateStatus();
            IsChangeCommitted = true;

            _Annalist.DisplayStatus(this, "I125", new string[3] { counter_RawData.ToString(), counter_ImportedData.ToString(), counter_AlteredRecords.ToString() }); // I125;Database management;Database import operation completed. There were @1 records in total, @2 records have been imported successfully. @3 record Code Names was altered.
        }
        protected virtual IEnumerable<T> CastTo_T(IEnumerable<IDataManageable> collection)
        {
            List<T> castedCollection = new List<T>();
            foreach (IDataManageable record in collection) castedCollection.Add((T)record);
            return castedCollection;
        }
        protected virtual IEnumerable<IDataManageable> CastFrom_T(IEnumerable<T> collection)
        {
            List<IDataManageable> castedCollection = new List<IDataManageable>();
            foreach (T record in collection) castedCollection.Add((IDataManageable)record);
            return castedCollection;
        }
        // -> status methods
        protected virtual void UpdateStatus()
        {
            int totalRecords = _allElements.Count;
            int totalCategories = _categories.Count - 1;
            _databaseStatus = $"{ totalRecords} records \n in {totalCategories} categories";
            RaisePropertyChanged("DatabaseStatus");
        }
        // INTERNAL NONOVERRIDEABLE METHODS:
        protected bool? ExportToCsV(bool wholeRange)
        {
            IEnumerable<T> collection;

            // range selection
            if (wholeRange) // export all database elements
            {
                collection = _allElements;
            }
            else collection = _selectedElements; // export selected part of database
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string path;

            saveFileDialog.FileName = "new file.csv";
            saveFileDialog.Filter = ".csv File | *.csv";
            saveFileDialog.Title = "Export data:";

            if (saveFileDialog.ShowDialog() == true) path = saveFileDialog.FileName;
            else return null;

            try
            {
                using (StreamWriter sW = new StreamWriter(path))
                {
                    foreach (IDataManageable element in collection)
                    {
                        sW.WriteLine(element.ToCSVLine());
                    }
                }
            }
            catch (IOException e)
            {
                _Annalist.MSGBox(this, "E101", e.Message); // E101;Database file error;The file could not be saved - operation aborted
                return false;
            }
            return true;
        }
        protected IEnumerable<string> ImportFromCSV()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string path;

            string[] importedRawData;
            List<T> importedData = new List<T>();

            openFileDialog.FileName = "data.csv";
            openFileDialog.Filter = ".csv File | *.csv";
            openFileDialog.Title = "Import data:";

            if (openFileDialog.ShowDialog() == true) path = openFileDialog.FileName;
            else return null;

            try
            {
                importedRawData = File.ReadAllLines(path);
            }
            catch (IOException e)
            {
                _Annalist.MSGBox(this, "E102", e.Message); // E102;Database file error;The file could not be read - operation aborted
                return null;
            }
            return importedRawData;
        }
        // -> advanced category querries
        private int AlterDuplicates(IEnumerable<T> collection) // searches duplicated code names within same category and alters their names with asterisks symbols
        {
            // Classic...
            if (collection == null) return -1;

            // Checking if there are records with same code names within single category.
            List<IDataManageable> iCollection = new List<IDataManageable>(CastFrom_T(collection));
            var grouped = iCollection.GroupBy(element => new { element.Category, element.CodeName }).Where(group => group.Count() > 1);
            if (!grouped.Any()) return 0;

            int counter = 0;

            // Altering duplicated names. Every duplicated name will be altered with number of "*".
            foreach (var group in grouped)
            {
                string alteredCodeName = group.Key.CodeName + "*";

                foreach (IDataManageable element in group)
                {
                    int index = iCollection.IndexOf(element);
                    iCollection[index].CodeName = alteredCodeName;
                    alteredCodeName += "*";
                    counter++;
                }
            }
            collection = CastTo_T(iCollection);
            return counter;
        }
        // COMMAND RELATED METHODS:
        // -> command actions related methods:
        protected virtual void SaveAsNewRecord()
        {
            // deep data integrity checking
            if (CodeNameDuplicatesWithinSameCategoryCheck())
            {

                _Annalist.DisplayStatus(this, "E111"); // E111;Database structural error;Unable to save new record. The code name already exists in the category
                return;
            }
            // method logic
            _allElements.Add(GetElementFromElementBuffer());
            RaisePropertyChanged("ShownCollection");
            _Annalist.DisplayStatus(this, "I111"); // I111;;Record has been saved successfully
            UpdateStatus();
            IsChangeCommitted = true;
        } // Adds new record. No same code names in single category allowed.
        protected virtual void RemoveSelectedRecord()
        {
            _allElements.Remove(_selectedRow);
            RaisePropertyChanged("ShownCollection");
            _Annalist.DisplayStatus(this, "I111"); // I111;Database management;Record has been saved successfully
            UpdateStatus();
            IsChangeCommitted = true;
        }
        protected virtual void OverwriteRecord() // Alters selected record. No same code names in single category allowed.
        {
            // method logic
            int index = _allElements.IndexOf(_selectedRow);
            _allElements[index] = GetElementFromElementBuffer();

            _Annalist.DisplayStatus(this, "I112"); // I112;Database management;Record has been modified successfully.
            RaisePropertyChanged("ShownCollection");
            IsChangeCommitted = true;
        }
        protected virtual void AddNewCategory() // Adds new empty category. Empty categories are forgotten after window closed.
        {

            // method logic - integrity checking is done on dialogbox level
            TextDialogBox dialogBox = new TextDialogBox("Please enter name of new category:", "", "Add new category", _categories);
            if ((bool)dialogBox.ShowDialog())
            {
                if (_categories.Contains(dialogBox.Answer)) return;
                _categories.Add(dialogBox.Answer);
                _selectedCategory = dialogBox.Answer;
                SortCategories();
                RaisePropertyChanged("Categories");
                RaisePropertyChanged("BufferedCategories");
                RaisePropertyChanged("SelectedCategory");
                RaisePropertyChanged("ShownCollection");
                _Annalist.DisplayStatus(this, "I113"); // I113;Database management;New category has been added successfully.
                UpdateStatus();
                IsChangeCommitted = true;
            }
        }
        protected virtual void RemoveCategory() // Removes selected category along with its contents.
        {
            {
                MessageBoxResult result = _Annalist.MSGBox(this, "Q111"); // Q111;Database alteration;Removing selected category will also remove all related records. Continue?
                if (result is MessageBoxResult.Yes)
                {
                    IEnumerable<IDataManageable> tempCollection = CastFrom_T(_allElements);
                    IEnumerable<IDataManageable> tempResult = from element in tempCollection where element.Category != _selectedCategory select element;
                    _allElements = new ObservableCollection<T>(CastTo_T(tempResult));

                    _categories.Remove(_selectedCategory);
                    _selectedCategory = _categories[0];
                    RaisePropertyChanged("Categories");
                    RaisePropertyChanged("BufferedCategories");
                    RaisePropertyChanged("SelectedCategory");
                    RaisePropertyChanged("ShownCollection");
                    NotifyAboutCategoryDependentDynamicLabelsChange();
                    _Annalist.DisplayStatus(this, "I114"); // I114;Database management;Category has been removed successfully.
                    UpdateStatus();
                    IsChangeCommitted = true;
                }
            }
        }
        protected virtual void RenameCategory() // Renames category name and alters all records category field.
        {
            // getting new name for category (constraints included)
            List<string> exclusions = new List<string>(_categories);
            exclusions.Remove(_selectedCategory);
            TextDialogBox dialogBox = new TextDialogBox("Please enter new name of category:", _selectedCategory + "*", "Rename category", exclusions);
            
            // altering elements to meet new category name
            if ((bool)dialogBox.ShowDialog())
            {
                foreach (IDataManageable element in _allElements)
                {
                    if (element.Category == _selectedCategory) element.Category = dialogBox.Answer;
                }

                int index = _categories.IndexOf(_selectedCategory);
                _categories[index] = dialogBox.Answer;
                _selectedCategory = dialogBox.Answer;
                SortCategories();
                RaisePropertyChanged("Categories");
                RaisePropertyChanged("BufferedCategories");
                RaisePropertyChanged("SelectedCategory");
                RaisePropertyChanged("ShownCollection");
                _Annalist.DisplayStatus(this,"I115"); // I115;Database management;Category has been renamed successfully.
                UpdateStatus();
                IsChangeCommitted = true;
            }
        }
        protected virtual void Export() // Exports all database or single category to .csv file.
        {            
            if (_selectedCategory == _categories[0])
            {
                bool? result = ExportToCsV(true);

                if (result == true) _Annalist.DisplayStatus(this, "I120", new string[] { _allElements.Count().ToString() }); // I120;Database management;Database has been exported successfully. Total number of exported records: @1
                else if (result == null) _Annalist.DisplayStatus(this, "I122"); // I122;Database management;Export operation aborted by user.
                else _Annalist.DisplayStatus(this,"E122"); // E122;Database management error;Export failed. An unknown error has occured.
            }
            else
            {
                bool? result = ExportToCsV(false);

                if (result == true) _Annalist.DisplayStatus(this, "I121", new string[] { _selectedElements.Count().ToString() }); // I121;Database management;Category has been exported successfully. Total number of exported records: @1
                else if (result == null) _Annalist.DisplayStatus(this, "I122"); // I122;Database management;Export operation aborted by user.
                else _Annalist.DisplayStatus(this, "E122"); // E122;Database management error;Export failed. An unknown error has occured.
            }
        }
        protected virtual void Import() // Import and replace all database or import and merge new data to selected category.
        {
            IEnumerable<string> importedRawData = ImportFromCSV();
            
            if (importedRawData == null)
            {
                _Annalist.DisplayStatus(this, "E127"); // E127;Database management error;Import operation failed. An unknown error has occured.
                return;
            }
            
            Import(importedRawData);
        }
        protected override bool ClosingAction()
        {
            if (_isChangeCommitted == true)
            {
                // Messing with data is serious thing, so here user has last chance to discard his changes
                
                MessageBoxResult message = _Annalist.MSGBox(this, "T111"); // T111;Database management;Save all changes?

                switch (message)
                {
                    // Saving changes
                    case MessageBoxResult.Yes:
                        // altering DATA MODEL
                        // -> little cast
                        IEnumerable<IDataManageable> iDBM_Elements = CastFrom_T(_allElements);
                        // -> rebuilding main dataModel (dictionary) i.e. save changes to application memory.
                        var listsByCategory = iDBM_Elements.GroupBy(element => element.Category).Select(group => group.ToList()).ToList();
                        var dataModel = listsByCategory.ToDictionary(list => list.First().Category, list => list);
                        DataModel = dataModel;
                        IsChangeCommitted = false;
                        _Annalist.DisplayStatus(this, "I128"); // I128;Database management;Database changes approved and saved by User.
                        // -> inform supervisor (supervisor is responsible for saving changes to HDD)
                        RaiseInternalManagementEndEvent(true);
                        return true; // <- this one confirms user requests to close tab
                    // Discard changes (closing through "close window button" acts same way - for now)
                    case MessageBoxResult.No:
                        RaiseInternalManagementEndEvent();
                        _Annalist.DisplayStatus(this, "I129"); // I129;Database management;Database changes discarded by User.
                        return true; // <- this one confirms user requests to close tab
                    // Return to managing
                    case MessageBoxResult.Cancel:
                        return false;
                    default:
                        return false;
                }
            }
            // Close without making any change 
            else
            {
                RaiseInternalManagementEndEvent();
                return true;
            }
        }
        // -> tab command execution ability related methods:
        protected virtual bool CanExecute_RecordCommand_Overwrite()
        {
            if (_selectedRow == null) return false;
            else return true;
        }
        protected virtual bool CanExecute_RecordCommand_Remove()
        {
            if (_selectedRow == null) return false;
            else if (IsBufferSynchronizedWithSelectedElement()) return true; //it's purpose is to force user to reselect record before remove in case of earlier unsaved modifications of record.
            else return false;
        }
        protected virtual bool CanExecute_AddCategoryCommand()
        {
            return true;
        }
        protected virtual bool CanExecute_RemRenCategoryCommand()
        {
            if (_selectedCategory == null || _selectedCategory == _categories[0]) return false;
            else return true;
        }
        protected virtual bool CanExecute_ExportCommand()
        {
            if (_selectedCategory == null && _selectedElements.Count == 0) return false;
            else if (_allElements == null || _allElements.Count == 0) return false;
            else return true;
        }
        protected virtual bool CanExecute_ImportCommand()
        {
            if (_selectedCategory == null || _allElements == null) return false;
            else return true;
        }
        // -> tab control command execution ability related methods:
        protected override bool CanExecute_ToggleRenamingCommand() // User isn't allowed to change title of any data manager tab
        {
            return false;
        }
    }
}
