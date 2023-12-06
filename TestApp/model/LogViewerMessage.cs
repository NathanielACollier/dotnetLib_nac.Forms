using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.model;

internal class LogViewerMessage : nac.Forms.model.ViewModelBase
{
    public DateTime Date
    {
        get { return GetValue(() => Date); }
        set { SetValue(() => Date, value); }
    }
    public string Message
    {
        get { return GetValue(() => Message); }
        set { SetValue(() => Message, value); }
    }
    public string Level
    {
        get { return GetValue(() => Level); }
        set { SetValue(() => Level, value); }
    }
}
