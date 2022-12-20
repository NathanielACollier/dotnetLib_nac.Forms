using System;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups;

public class FileSystem
{
    public static void FilePickerBasic(Form f)
    {
        f.FilePathFor("myPath",
                onFilePathChanged: async (newFileName) => { model.LogEntry.debug($"New Filename is: {newFileName}"); })
            .HorizontalGroup(hg =>
            {
                hg.Text("You picked file: ")
                    .TextFor("myPath");
            });
    }


    public static void FilePicker_TwoWithDifferentChangeEvents(Form f)
    {
        f.HorizontalGroup(h =>
            {
                h.FilePathFor("myPath1",
                        onFilePathChanged: async (newFileName1) =>
                        {
                            model.LogEntry.debug($"------New Filename for path1 is: {newFileName1}");
                        })
                    .HorizontalGroup(hg =>
                    {
                        hg.Text("myPath1 You picked file: ")
                            .TextFor("myPath1");
                    });
            })
            .HorizontalGroup(h =>
            {
                h.FilePathFor("myPath2",
                        onFilePathChanged: async (newFileName2) =>
                        {
                            model.LogEntry.debug($"++++++New Filename for path2 is: {newFileName2}");
                        })
                    .HorizontalGroup(hg =>
                    {
                        hg.Text("myPath2 You picked file: ")
                            .TextFor("myPath2");
                    });
            })
            .HorizontalGroup(h =>
            {
                h.DirectoryPathFor("myDirPath1",
                        onDirectoryPathChanged: async (newDirPath1) =>
                        {
                            model.LogEntry.debug($"++++++New DirectoryPath for path1 is: {newDirPath1}");
                        })
                    .HorizontalGroup(hg =>
                    {
                        hg.Text("myDirPath1 You picked file: ")
                            .TextFor("myDirPath1");
                    });
            })
            .HorizontalGroup(h =>
            {
                h.DirectoryPathFor("myDirPath2",
                        onDirectoryPathChanged: async (newDirPath2) =>
                        {
                            model.LogEntry.debug($"------New DirectoryPath for path2 is: {newDirPath2}");
                        })
                    .HorizontalGroup(hg =>
                    {
                        hg.Text("myDirPath2 You picked file: ")
                            .TextFor("myDirPath2");
                    });
            });
    }
    
    
    
    public static void FilePicker_NewFile(Form f)
    {
        f.FilePathFor("myPath", fileMustExist: false,
                onFilePathChanged: async (newFileName) =>
                {
                    model.LogEntry.debug($"New filename is: {newFileName}");
                })
            .HorizontalGroup(hg =>
            {
                hg.Text("You picked file: ")
                    .TextFor("myPath");
            });
    }
    
    
    
    
    public static void DirectoryPath_Simple(Form f)
    {
        f.Model["myPath"] = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        f.DirectoryPathFor("myPath",
                onDirectoryPathChanged: async (newFilePath) =>
                {
                    model.LogEntry.debug($"New Directory path is: {newFilePath}");
                })
            .HorizontalGroup(hg =>
            {
                hg.Text("You picked directory: ")
                    .TextFor("myPath");
            });
    }
    
    
    public static void DirectoryPath_ClassBinding(Form f)
    {
        var myModel = new model.DirectoryPathFormWindowModel();
        f.DataContext = myModel;

        myModel.myPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        f.DirectoryPathFor(nameof(model.DirectoryPathFormWindowModel.myPath),
                onDirectoryPathChanged: async (newFilePath) =>
                {
                    model.LogEntry.info($"New Directory path is: {newFilePath}");
                    model.LogEntry.info($"      Model myPath: {myModel.myPath}");
                })
            .HorizontalGroup(hg =>
            {
                hg.Text("You picked directory: ")
                    .TextFor(nameof(model.DirectoryPathFormWindowModel.myPath));
            })
            .Text("The below directory picker doesn't start with a path")
            .DirectoryPathFor(nameof(model.DirectoryPathFormWindowModel.pathWithoutBeingInit),
                onDirectoryPathChanged: async (newPath) =>
                {
                    model.LogEntry.info($"Old path: {myModel.pathWithoutBeingInit}");
                    model.LogEntry.info($"New path: {newPath}");
                });
    }
    
    
    
    public static void DirectoryAndFilePath_NoEvents(Form f)
    {
        f.HorizontalGroup(hg =>
            {
                hg.Text("FilePath ")
                    .FilePathFor("f1");
            })
            .HorizontalGroup(hg =>
            {
                hg.Text("DirectoryPath ")
                    .DirectoryPathFor("d1");
            });
    }




    public static void OpenFolderDialog_ButtonPress(Form f)
    {
        f.HorizontalGroup(hg =>
        {
            hg.Button("Choose Folder", async () =>
            {
                f.Model["CurrentPath"] = await f.ShowOpenFolderDialog();
            }).TextFor("CurrentPath");
        });
    }
    
    
    
    
    
    
}