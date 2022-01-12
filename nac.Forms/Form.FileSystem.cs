using System;

namespace nac.Forms
{
    public partial class Form
    {
        public class FilePathFor_Functions
        {
            public Action<string> updateFileName { get; set; }
        }
        
        public Form FilePathFor(string fieldName, 
            string fileFilter = null,
            string initialFileName = null,
            bool fileMustExist = true,
            Action<string> onFilePathChanged = null,
            FilePathFor_Functions functions = null
        )
        {
            // initialize the filename in the model
            setModelValue(fieldName, "");

            var filePicker = new controls.FilePicker();

            filePicker.FileMustExist = fileMustExist;
            
            AddBinding<string>(fieldName, filePicker, controls.FilePicker.FilePathProperty, true);

            filePicker.FilePathChanged += (_s, _args) =>
            {
                onFilePathChanged?.Invoke(_args);
            };
            
            this.AddRowToHost(filePicker);
            return this;
        }



        public Form DirectoryPathFor(string fieldName,
            Action<string> onDirectoryPathChanged = null)
        {
            setModelValue(fieldName, ""); // make sure the model has a value for this to start out

            var directoryPicker = new controls.DirectoryPicker();
            
            AddBinding<string>(fieldName, directoryPicker, controls.DirectoryPicker.DirectoryPathProperty, true);

            directoryPicker.DirectoryPathChanged += (_s, _args) =>
            {
                onDirectoryPathChanged?.Invoke(_args);
            };
            
            this.AddRowToHost(directoryPicker);
            return this;
        }
        
        
    }
}