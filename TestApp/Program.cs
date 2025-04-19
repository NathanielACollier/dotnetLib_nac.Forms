using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Logging;
using nac.Forms;
using nac.Forms.lib;
using nac.Forms.model;
using TestApp.model;

// to bring in the extensions

namespace TestApp;

class Program
{
    private static nac.Logging.Logger log = new();

    static void Main(string[] args)
    {
        nac.Logging.Appenders.ColoredConsole.Setup();
        
        try
        {
            nac.Forms.UITesterApp.TestApp.Run(typeof(lib.TestFunctionGroups.AGroup));
            

        }catch(Exception ex)
        {
            log.Fatal($"App Exception occured: {ex}");
        }

    }
    




}

