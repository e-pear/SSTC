using SSTC_BaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Modules.DataManager
{
    using SSTC.Modules.DataManager.TabViewModel;

    sealed class ConductorDataManager : DataManager<Conductor>
    {
        private static ConductorDataManager _instance;
        private static readonly object _padlock = new object();
        public static ConductorDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConductorDataManager();
                        }
                    }
                }
                return _instance;
            }
        }
        private ConductorDataManager() : base()
        {
            _dataFile = "conductors.bin";
            _dataPath = _appDirectory + _dataDirectory + _dataFile;
            _data = DeserializeDB(_dataPath);
            _dataKeys = new List<string>(_data.Keys);
        }

        protected override BaseDataManagerTab<Conductor> CreateViewModel()
        {
            return new ConductorManagerTab();
        }
    }
}
