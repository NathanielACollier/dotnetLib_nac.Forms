﻿using System;
using System.Linq;
using System.Threading.Tasks;

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
            Func<string, Task> onFilePathChanged = null,
            FilePathFor_Functions functions = null
        )
        {
            setModelIfNull(fieldName, "");

            var filePicker = new controls.FilePicker();

            filePicker.FileMustExist = fileMustExist;
            
            AddBinding<string>(fieldName, filePicker, controls.FilePicker.FilePathProperty, true);

            filePicker.FilePathChanged += async (_s, _args) =>
            {
                if (onFilePathChanged != null)
                {
                    await onFilePathChanged(_args);
                }
            };
            
            this.AddRowToHost(filePicker);
            return this;
        }



        public Form DirectoryPathFor(string fieldName,
            Func<string, Task> onDirectoryPathChanged = null)
        {
            setModelIfNull(fieldName, ""); // make sure the model has a value for this to start out

            var directoryPicker = new controls.DirectoryPicker();
            
            AddBinding<string>(fieldName, directoryPicker, controls.DirectoryPicker.DirectoryPathProperty, true);

            directoryPicker.DirectoryPathChanged += async (_s, _args) =>
            {
                if (onDirectoryPathChanged != null)
                {
                    await onDirectoryPathChanged(_args);
                }
            };
            
            this.AddRowToHost(directoryPicker);
            return this;
        }




        public async Task<string> ShowOpenFolderDialog()
        {
            /* examples of using newer storage API is here: https://docs.avaloniaui.net/docs/basics/user-interface/file-dialogs
             */
            var storageProvider = this.win.StorageProvider;

            var folders = await storageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                AllowMultiple = false
            });

            if (!folders.Any())
            {
                return "";
            }

            return folders.First().Path.LocalPath;
        }
        
        
        
        
        
    }
}