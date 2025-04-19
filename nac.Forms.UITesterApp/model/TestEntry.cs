using System;
using nac.Forms;

namespace nac.Forms.UITesterApp.model;

public class TestEntry
{
   public string Name { get; set; }
   public Action<Form> CodeToRun { get; set; }
   public bool SetupChildForm { get; set; }

   public override string ToString()
   {
       return this.Name;
   }

   public TestEntry()
   {
       this.SetupChildForm = true;
   }
   
}
