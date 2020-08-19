using SSTC_BaseModel;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SSTC.Modules.DataManager
{
    // "DBManager" is base class for whole dbManager Module. It is different from other tab based objects. Because its main purpose is to provide (in read only manner) data objects for calculator, it's tabs are encapsulated.
    // 
    using SSTC.Common;
    using SSTC.Modules.DataManager.TabViewModel;
    abstract class DataManager<Item> where Item : class, IDataManageable
    {
        // Global Variables
        private Annalist _Annalist;
        protected string _dataFile;
        protected string _dataDirectory = "\\Database\\";
        protected string _appDirectory = Directory.GetCurrentDirectory();
        protected string _dataPath;
        // Data Managing ViewModel
        protected BaseDataManagerTab<Item> _dataManagerTabViewModel;
        // Database Content
        protected Dictionary<string,List<IDataManageable>> _data;
        protected List<string> _dataKeys;
        // CONSTRUCTOR
        public DataManager()
        {
            _Annalist = Annalist.Instance;
            _dataManagerTabViewModel = null;
        }

        public BaseDataManagerTab<Item> GetNewManagerTab()
        {
            _dataManagerTabViewModel = CreateViewModel();
            _dataManagerTabViewModel.DataModel = _data;
            _dataManagerTabViewModel.OnInternalManagementEnd += InternalManagementEnded; // No code smell right? Publisher in this case has all references and short life :)
            
            return _dataManagerTabViewModel;
        }
        public IEnumerable<Item> GetSelectedGroup(string category)
        {
            List<IDataManageable> group;
            List<Item> requestedGroup;
            if (_data.TryGetValue(category, out group))
            {
                requestedGroup = new List<Item>();
                foreach(IDataManageable element in group)
                {
                    requestedGroup.Add(element as Item);
                }
                return requestedGroup;
            }
            else return new List<Item>();
        }
        public IEnumerable<string> GetAvailableCategories()
        {
            return _dataKeys;
        }
        // Finder
        public IEnumerable<Item> FindByCodeNamePart(string searchKey)
        {
            List<Item> results = new List<Item>();
            if (searchKey == null || searchKey.Count() < 1) return results;

            IEnumerable<IDataManageable> searchingArea = (_data.Values.ToList()).SelectMany(x => x);
            IEnumerable<IDataManageable> rawResults = searchingArea.Where(x => x.CodeName.CaseInsensitiveContains(searchKey));

            if (rawResults.Count() > 0)
            {
                foreach(IDataManageable rawResult in rawResults)
                {
                    results.Add(rawResult as Item);
                }
            }

            return results;
        }
        // TabFactory Method
        protected abstract BaseDataManagerTab<Item> CreateViewModel();
        // Loads database from binary file on HDD to memory.
        protected virtual Dictionary<string, List<IDataManageable>> DeserializeDB(string path)
        {
            if (!File.Exists(path))
            {
                _Annalist.MSGBox(this.ToString(), "E030", path);
                return new Dictionary<string, List<IDataManageable>>();
            }
            Dictionary<string, List<IDataManageable>> loadedData = null;
            FileStream fS = new FileStream(path, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                loadedData = (Dictionary<string, List<IDataManageable>>)formatter.Deserialize(fS);
                _Annalist.DisplayStatus(MainSettings.Instance.SystemOverseerName, "I030", new string[] { path });
            }
            catch (SerializationException e)
            {
                _Annalist.MSGBox(this.ToString(), "E031", e.Message);
                return new Dictionary<string, List<IDataManageable>>();
            }
            finally
            {
                fS.Close();
            }
            return loadedData;
        }
        // Saves database as binary file on HDD.
        protected virtual void SerializeDB(Dictionary<string, List<IDataManageable>> data, string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(Directory.GetCurrentDirectory() + _dataDirectory);
            FileStream fS = new FileStream(path, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fS, data);
            }
            catch (SerializationException e)
            {
                _Annalist.MSGBox(this.ToString(), "E032", e.Message);
            }
            finally
            {
                fS.Close();
            }
        }
        protected virtual void InternalManagementEnded(bool managementResult)
        {
            if (managementResult)
            {
                _data = _dataManagerTabViewModel.DataModel;
                _dataKeys = new List<string>(_data.Keys);
                _dataKeys.Sort();
                SerializeDB(_data, _dataPath);
            }
        }
        // Self identification
        public override string ToString()
        {

            return "<" + this.GetType().Name + ">";
        }
    }
}
