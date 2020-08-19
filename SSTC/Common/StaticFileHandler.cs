using Microsoft.Win32;
using SSTC_BaseModel;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace SSTC.Common
{
    public static class StaticFileHandler
    {
        //encapsulates obtaining open file path through saveFileDialog
        public static string GetOpenDialogFilePath(string fileGeneralName, string fileExtension, string customTitle = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string path = "";

            openFileDialog.FileName = fileGeneralName + "." + fileExtension;
            openFileDialog.Filter = "." + fileExtension + " File | *." + fileExtension;
            if (customTitle == "") openFileDialog.Title = "Open " + fileGeneralName;
            else openFileDialog.Title = customTitle;

            if (openFileDialog.ShowDialog() == true) path = openFileDialog.FileName;

            return path;
        }
        //encapsulates obtaining save file path through saveFileDialog
        public static string GetSaveDialogFilePath(string fileGeneralName, string fileExtension, string customTitle = "")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string path = "";

            saveFileDialog.FileName = fileGeneralName + "." + fileExtension;
            saveFileDialog.Filter = "." + fileExtension + " File | *." + fileExtension;
            if (customTitle == "") saveFileDialog.Title = "Open " + fileGeneralName;
            else saveFileDialog.Title = customTitle;

            if (saveFileDialog.ShowDialog() == true) path = saveFileDialog.FileName;

            return path;
        }
        // watch out on what file you aiming for this thing :D and make sure to catch it's exceptions.
        public static T DeserializeObjectFromBinaryFile<T>(string path) where T: class
        {
            T reference;

            using (FileStream fS = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                reference = (T)formatter.Deserialize(fS);
            }
            return reference;
        }
        // I'm not sure serializer is enough smart to properly serialize some object sent as object type. Make sure to catch it's exceptions.
        public static void SerializeObjectToBinaryFile<T>(string path, T objectToSerialize) where T: class
        {
            using (FileStream fS = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fS, objectToSerialize);
            }
        }
        // It is something like wrapping a wrapper... but I want to have all file methods here...
        public static IEnumerable<string> ReadAllFromTextFile(string path)
        {
            return File.ReadAllLines(path);
        }
        public static void WriteCollectionToTextFile(string path, IEnumerable<string> collection)
        {
            using (StreamWriter sW = new StreamWriter(path))
            {
                foreach (string element in collection)
                {
                    sW.WriteLine(element);
                }
            }
        }
        public static void WriteCollectionToTextFile(string path, IEnumerable<IDataManageable> collection)
        {
            using (StreamWriter sW = new StreamWriter(path))
            {
                foreach (IDataManageable element in collection)
                {
                    sW.WriteLine(element.ToCSVLine());
                }
            }
        }

    }
}
