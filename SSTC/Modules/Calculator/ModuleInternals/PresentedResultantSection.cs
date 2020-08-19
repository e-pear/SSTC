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
    class PresentedResultantSection : INotifyPropertyChanged
    {
        // PROPS 'N FIELDS

        public ObservableCollection<PresentedResultantSpan> PresentedResultantSpans { get; private set; }
        public bool SpanPresentedResultsGroupVisible { get; private set; }
        public bool PresentedSolutionsVisible { get; private set; }
        public bool HasValidResults { get; private set; }
        // EVENT CORNER
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // CTOR
        public PresentedResultantSection()
        {
            PresentedResultantSpans = new ObservableCollection<PresentedResultantSpan>();
            SpanPresentedResultsGroupVisible = false;
            PresentedSolutionsVisible = false;
            HasValidResults = false;
        }
        // SETTER METHODS
        public void SetPresentedCollection(double[] solutionsVector)
        {
            int uniSize = solutionsVector.Length;
            
            PresentedResultantSpan[] newPresentedCollection = new PresentedResultantSpan[uniSize];
            string[] ordinals = SetPresentedOrdinals(uniSize);

            for (int i = 0; i < uniSize; i++)
            {
                newPresentedCollection[i] = new PresentedResultantSpan(ordinals[i], solutionsVector[i], null);
            }
            PresentedResultantSpans = new ObservableCollection<PresentedResultantSpan>(newPresentedCollection);
            SpanPresentedResultsGroupVisible = false;
            PresentedSolutionsVisible = true;
            HasValidResults = false;
            RaisePropertyChanged("PresentedResultantSpans");
            RaisePropertyChanged("SpanPresentedResultsGroupVisible");
            RaisePropertyChanged("PresentedSolutionsVisible");
            RaisePropertyChanged("HasValidResults");
        }
        public void SetPresentedCollection(double[] solutionsVector, ResultantSpan[] resultantSpans)
        {
            int uniSize = solutionsVector.Length;

            PresentedResultantSpan[] newPresentedCollection = new PresentedResultantSpan[uniSize];
            string[] ordinals = SetPresentedOrdinals(uniSize);

            for (int i = 0; i < uniSize; i++)
            {
                newPresentedCollection[i] = new PresentedResultantSpan(ordinals[i], solutionsVector[i], resultantSpans[i]);
            }
            PresentedResultantSpans = new ObservableCollection<PresentedResultantSpan>(newPresentedCollection);
            SpanPresentedResultsGroupVisible = true;
            PresentedSolutionsVisible = true;
            HasValidResults = true;
            RaisePropertyChanged("PresentedResultantSpans");
            RaisePropertyChanged("SpanPresentedResultsGroupVisible");
            RaisePropertyChanged("PresentedSolutionsVisible");
            RaisePropertyChanged("HasValidResults");
        }
        public void SetPresentedCollection()
        {
            PresentedResultantSpans = new ObservableCollection<PresentedResultantSpan>();
            SpanPresentedResultsGroupVisible = false;
            PresentedSolutionsVisible = false;
            HasValidResults = false;
            RaisePropertyChanged("PresentedResultantSpans");
            RaisePropertyChanged("SpanPresentedResultsGroupVisible");
            RaisePropertyChanged("PresentedSolutionsVisible");
            RaisePropertyChanged("HasValidResults");
        }
        // AUX
        private string[] SetPresentedOrdinals(int spanCount)
        {
            string buffer;
            List<string> buffers = new List<string>();
            for (int i = 0; i < spanCount; i++)
            {
                buffer = (i + 1).ToString() + " - " + (i + 2).ToString();
                buffers.Add(buffer);
            }
            return buffers.ToArray();
        }

    }
}
