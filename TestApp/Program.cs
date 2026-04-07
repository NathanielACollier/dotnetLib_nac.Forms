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
using lib = TestApp.lib;

var log = new nac.Logging.Logger();

nac.Logging.Appenders.ColoredConsole.Setup();
        
try
{
    await nac.Forms.UITesterApp.TestApp.Run(typeof(lib.TestFunctionGroups.AGroup));
    
}catch(Exception ex)
{
    log.Fatal($"App Exception occured: {ex}");
}

