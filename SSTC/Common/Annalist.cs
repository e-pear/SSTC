using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SSTC.Common
{
    // The better name would be the "StatusInformer", but... 
    // Object handles information flow from app to user ie.:
    // -> propagates status info through main status bar,
    // -> throws messages through messagebox mechanism,
    // -> handles the Annals (simple log mechanism),
    // -> knows errors list
    // The goal was to have all those pesky jobs in one place so there is only one true Annalist
    public sealed class Annalist : INotifyPropertyChanged
    {
        // INSTANCE
        private static Annalist _instance;
        private static readonly object _padlock = new object();
        // OTHER FIELDS
        private string _actualStatus;
        private string _logPath;
        private Timer _timer;
        private Dictionary<string, string[]> _messageList;
        private List<string> _logBuffer;
        // CONSTRUCTOR
        private Annalist()
        {
            _logPath = Directory.GetCurrentDirectory() + "\\Log\\session_log.txt";

            _timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            _messageList = GetMessageList();
            _logBuffer = new List<string>();
            
            if(MainSettings.Instance.LoggingActions) File.Create(_logPath); 
        }
        ~Annalist()
        {
            if (MainSettings.Instance.LoggingActions)
            {
                try
                {
                    Task task = WriteToAnnalsAsync();
                }
                catch (Exception e)
                {
                    MSGBox(null, "E011", e.Message, true); // E011;Log File Error;Unable to write logs on hard drive
                }
            }
        }
        // INSTANCE GETTER
        public static Annalist Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Annalist();
                        }
                    }
                }
                return _instance;
            }
        }
        // OTHER PROPS
        public string ActualStatus { get { return _actualStatus; } }
        // EVENTS
        public event PropertyChangedEventHandler PropertyChanged;
        // EVENT RAISERS
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // METHODS
        private void ToAnnals(string senderInfo, string baseLogInfo, string additionalLogInfo)
        {
            if (MainSettings.Instance.LoggingActions)
            {
                DateTime now = DateTime.Now;

                string nowH = now.Hour.ToString();
                string nowM = now.Minute.ToString();
                string nowS = now.Second.ToString();

                string timeStamp = nowH + ":" + nowM + ":" + nowS;

                string logLine = timeStamp + "|" + senderInfo + "|" + baseLogInfo;
                if (additionalLogInfo != "" || additionalLogInfo != null) logLine = logLine + "|" + additionalLogInfo;
                _logBuffer.Add(logLine);

                if (_logBuffer.Count >= MainSettings.Instance.LogBufferSize)
                {
                    try
                    {
                        Task task = WriteToAnnalsAsync();
                    }
                    catch(Exception e)
                    {
                        DisplayStatus(MainSettings.Instance.SystemOverseerName,"E001",e.Message,true); // E001;Log File Error;Unable to write logs on hard drive
                    }
                }
            }
        }
        private async Task WriteToAnnalsAsync()
        {

            using (StreamWriter sW = new StreamWriter(_logPath, true))
            {
                foreach (string log in _logBuffer)
                {
                    await sW.WriteLineAsync(log);
                }
            }
            _logBuffer.Clear();
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _actualStatus = "";
            RaisePropertyChanged("ActualStatus");
            _timer.Stop();
        }
        private void TimerOn()
        {
            _timer.Interval = MainSettings.Instance.StatusDisplayDuration;
            _timer.Enabled = true;
        }
        private Dictionary<string,string[]> GetMessageList() // single csv line pattern: error_code;message_title;message_for_user;message_for_log;
        {

            List<string> importedRawData = new List<string>();
            Dictionary<string, string[]> requestedDictionary = new Dictionary<string, string[]>();

            try
            {
                string[] rawCollection = Properties.Resources.MessageCodes.Split('\n');

                foreach (string rawLine in rawCollection)
                {
                    if (!rawLine.StartsWith("#") || !rawLine.StartsWith("")) importedRawData.Add(rawLine);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Application critical error:", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            foreach (string record in importedRawData)
            {
                string[] splittedRecord = record.Split('|');
                string[] value = new string[] { splittedRecord[1], splittedRecord[2], splittedRecord[3] };
                string key = splittedRecord[0];
                requestedDictionary.Add(key, value);
            }

            return requestedDictionary;
        }
        private string GetMessageFromMessageList(string messageCode)
        {
            string[] message;
            if (_messageList.TryGetValue(messageCode, out message)) return message[1];
            else
            {
                _messageList.TryGetValue("E999", out message);
                return message[1];
            }
        }
        private string GetTitleFromMessageList(string messageCode)
        {
            string[] message;
            if (_messageList.TryGetValue(messageCode, out message)) return message[0];
            else
            {
                _messageList.TryGetValue("E999", out message);
                return message[0];
            }
        }
        private string GetUserInfoFromMessageList(string messageCode)
        {
            string[] message;
            if (_messageList.TryGetValue(messageCode, out message)) return message[0] + ". " + message[1];
            else
            {
                _messageList.TryGetValue("E999", out message);
                return message[0] + ". " + message[1];
            }
        }
        private string GetLogInfoFromMessageList(string messageCode)
        {
            string[] message;
            if (_messageList.TryGetValue(messageCode, out message)) return message[0] + ". " + message[2];
            else
            {
                _messageList.TryGetValue("E999", out message);
                return message[0] + ". " + message[2];
            }
        }
        private string GetSenderSignature(object sender, bool simplifiedSignatureRequested = false)
        {
            if (sender is Tab)
            {
                Tab senderTab = sender as Tab;
                string senderName = senderTab.TabTitle;
                string senderType = senderTab.GetType().Name;

                if (simplifiedSignatureRequested) return senderName;
                else return senderType + "@" + senderName;
            }
            else if (sender is string)
            {
                return sender as string;
            }
            else
            {
                return "";
            }
        }
        private string AlterMessageWithVariables(string specifiedMessage, string[] inmessageVariables)
        {
            string insertionPoint;
            for (int insertionIndex = 1; insertionIndex <= inmessageVariables.Length; insertionIndex++)
            {
                insertionPoint = "@" + insertionIndex;
                specifiedMessage = specifiedMessage.Replace(insertionPoint, inmessageVariables[insertionIndex - 1]);
                insertionIndex++;
            }
            return specifiedMessage;
        }
        private bool CheckIfMessageAllowedToLogging(string messageCode)
        {
            switch (messageCode.ElementAt(0))
            {
                case 'Q':
                    return false;
                case 'T':
                    return false;
                default:
                    return true;
            }
        }
        public void DisplayStatus(object sender, string messageCode, string extendedInformation = "", bool bypassLoggingActions = false)
        {
            string senderName = GetSenderSignature(sender, true);
            string statusMessage = GetUserInfoFromMessageList(messageCode);

            if (MainSettings.Instance.LoggingActions && bypassLoggingActions == false)
            {
                if (CheckIfMessageAllowedToLogging(messageCode)) ToAnnals(GetSenderSignature(sender), GetLogInfoFromMessageList(messageCode), extendedInformation);
            }

            _actualStatus = senderName + ": " + statusMessage;
            RaisePropertyChanged("ActualStatus");
            TimerOn();
        }
        public void DisplayStatus(object sender, string messageCode, string[] inmessageVariables, string extendedInformation = "", bool bypassLoggingActions = false)
        {
            string senderName = GetSenderSignature(sender, true);
            string statusMessage = GetUserInfoFromMessageList(messageCode);

            if (MainSettings.Instance.LoggingActions && bypassLoggingActions == false)
            {
                if (CheckIfMessageAllowedToLogging(messageCode))
                {
                    string logMessage = AlterMessageWithVariables(GetLogInfoFromMessageList(messageCode), inmessageVariables);
                    ToAnnals(GetSenderSignature(sender), logMessage, extendedInformation);
                }
            }

            statusMessage = AlterMessageWithVariables(statusMessage, inmessageVariables);

            _actualStatus = senderName + ": " + statusMessage;
            RaisePropertyChanged("ActualStatus");
            TimerOn();
        }
        public MessageBoxResult MSGBox(object sender, string messageCode, string extendedInformation = "", bool bypassLoggingActions = false)
        {
            string message = GetMessageFromMessageList(messageCode);
            string messageTitle = GetTitleFromMessageList(messageCode);
            string additionalMessage = "";
            if (extendedInformation != "") additionalMessage = ".\n" + extendedInformation;
            
            if (message == null) return MessageBoxResult.None;

            if (MainSettings.Instance.LoggingActions && bypassLoggingActions == false)
            {
                if(CheckIfMessageAllowedToLogging(messageCode)) ToAnnals(GetSenderSignature(sender), GetLogInfoFromMessageList(messageCode), extendedInformation);
            }

            switch (messageCode.ElementAt(0))
            {
                case 'E':
                    return MessageBox.Show(message + additionalMessage, messageTitle + ": " + messageCode, MessageBoxButton.OK, MessageBoxImage.Error);
                case 'I':
                    return MessageBox.Show(message + additionalMessage, messageTitle + ": ", MessageBoxButton.OK, MessageBoxImage.Information);
                case 'W':
                    return MessageBox.Show(message + additionalMessage, messageTitle + ": ", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                case 'Q':
                    return MessageBox.Show(message + additionalMessage, messageTitle + ": ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                case 'T':
                    return MessageBox.Show(message + additionalMessage, messageTitle + ": ", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                default:
                    return MessageBoxResult.None;
            }
        }
    }
}
