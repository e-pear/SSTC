using SSTC_BaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Modules.DataManager
{
    using SSTC.Modules.DataManager.TabViewModel;

    sealed class InsulatorSetDataManager : DataManager<InsulatorSet>
    {
        private static InsulatorSetDataManager _instance;
        private static readonly object _padlock = new object();
        public static InsulatorSetDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new InsulatorSetDataManager();
                        }
                    }
                }
                return _instance;
            }
        }
        private InsulatorSetDataManager() : base()
        {
            _dataFile = "sets.bin";
            _dataPath = _appDirectory + _dataDirectory + _dataFile;
            _data = DeserializeDB(_dataPath);
            _dataKeys = new List<string>(_data.Keys);
        }

        protected override BaseDataManagerTab<InsulatorSet> CreateViewModel()
        {
            return new InsulatorSetManagerTab();
        }
    }
}
