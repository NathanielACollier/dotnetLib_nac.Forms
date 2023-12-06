using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using nac.Forms;
using nac.Forms.model;

namespace TestApp.lib.TestFunctionGroups;

public class Table
{
    public static void DisplayObservableCollection(Form f)
    {
        var people = new ObservableCollection<model.Person>
        {
            new model.Person
            {
                First = "George",
                Last = "Washington"
            },
            new model.Person
            {
                First = "John",
                Last = "Adams"
            }
        };
        f.Model["persons"] = people;
                
        f.Table<model.Person>("persons");

    }

    public static void Display2Tables(Form f)
    {
        var p1 = new ObservableCollection<model.Person>
        {
            new model.Person
            {
                First = "Ringo",
                Last = "Star"
            },
            new model.Person
            {
                First = "Paul",
                Last = "McCarthy"
            }
        };

        var p2 = new ObservableCollection<model.Person>
        {
            new model.Person
            {
                First = "Grape",
                Last = "Fruit"
            },
            new model.Person
            {
                First = "Orange",
                Last = "Apple"
            }
        };
        
        f.Model["p1"] = p1;
        f.Model["p2"] = p2;
        f.VerticalGroup( vg =>
        {
            vg.Table<model.Person>("p1")
                .Table<model.Person>("p2");
        }, isSplit:true);
    }


    public static void AddEntryToBlankList(Form f)
    {
        var people = new ObservableCollection<model.Person>();

        f.Model["people"] = people;
        f.Text("Person Editor")
            .VerticalGroup(vg =>
            {
                vg.VerticalGroup(newEntryEditor =>
                {
                    newEntryEditor.HorizontalGroup(hg =>
                        {
                            hg.Text("First Name")
                                .TextBoxFor("firstName");
                        })
                        .HorizontalGroup(hg =>
                        {
                            hg.Text("Last Name")
                                .TextBoxFor("lastName");
                        })
                        .Button("Add", async () =>
                        {
                            people.Add(new model.Person
                            {
                                First = f.Model["firstName"] as string,
                                Last = f.Model["lastName"] as string
                            });
                        });
                }).Table<model.Person>("people");
            }, isSplit:true);

    }




    public static void ObservableCollectionOfDictionary(Form f)
    {
        var list = new ObservableCollection<Dictionary<string, object>>();

        f.Model["list"] = list;

        f.HorizontalGroup(hg =>
            {
                hg.Text("First Name")
                    .TextBoxFor("firstName");
            })
            .Button("Add", async () =>
            {
                list.Add(new Dictionary<string, object>
                {
                    {"First Name", f.Model["firstName"] as string}
                });
            })
            .Table<Dictionary<string, object>>("list");

    }




    public static void ObservableCollectionBindableDictionary(Form f)
    {
        var list = new ObservableCollection<nac.utilities.BindableDynamicDictionary>();

        f.Model["list"] = list;

        var firstItem = new nac.utilities.BindableDynamicDictionary();
        firstItem["firstName"] = "Apple";
        list.Add(firstItem);

        f.HorizontalGroup(hg =>
            {
                hg.Text("First Name")
                    .TextBoxFor("firstName");
            })
            .Button("Add", async () =>
            {
                dynamic newItem = new nac.utilities.BindableDynamicDictionary();
                newItem.firstName = f.Model["firstName"] as string;
                list.Add(newItem);
            })
            .Table<nac.utilities.BindableDynamicDictionary>("list",
                columns: new[]
                {
                    new nac.Forms.model.Column
                    {
                        Header = "First Name",
                        modelBindingPropertyName = "firstName"
                    }
                });
    }



    public static void SpecifiedColumnBinding(Form f)
    {
        var list = new ObservableCollection<model.Alphabet>();
        
        f.Model["list"] = list;

        f.HorizontalGroup(hg =>
            {
                hg.Text("X")
                    .TextBoxFor("X");
            })
            .Button("Add", async () =>
            {
                var newItem = new model.Alphabet();
                newItem.X = f.Model["X"] as string;
                list.Add(newItem);
            })
            .Table<model.Alphabet>("list",
                columns: new[]
                {
                    new nac.Forms.model.Column
                    {
                        Header = "Duplicate of X",
                        modelBindingPropertyName = "X"
                    }
                });
    }


    public static void TemplateColumn_ButtonCounter(Form f)
    {
        var list = new ObservableCollection<model.Alphabet>();
        
        f.DebugAvalonia(); // enable this child form to be debugged
                
        f.Model["list"] = list;
        f.HorizontalStack(hs =>
        {
            hs.Text("Use this to add an item to the list: ")
                .Button("Add Item", async () =>
                {
                    var newItem = new model.Alphabet();
                    newItem.A = DateTime.Now.ToString();
                    newItem.C = "0";
                    newItem.G = "77"; // model is working if initially set
                    list.Add(newItem);
                });
        }).Table<model.Alphabet>(itemsModelFieldName: "list", columns: new[]
        {
            new nac.Forms.model.Column
            {
                Header = "My Counter",
                template = (myColTemplate) =>
                {
                    myColTemplate.Button("Incriment", async () =>
                    {
                        var model = myColTemplate.DataContext as model.Alphabet;

                        var counter = string.IsNullOrWhiteSpace(model.C) ? 0 : Convert.ToInt32(model.C);
                        ++counter;
                        model.C = counter.ToString();
                    });
                }
            },
            new nac.Forms.model.Column
            {
                Header = "C (Dupe)",
                template = (myColTemplate) =>
                {
                    myColTemplate.TextFor("C");
                }
            }
        });
    }



    public static void DataContext_Test(Form f)
    {
        var model = new model.TestDataContextWindowModel();
        f.DataContext = model;

        f.Text("Letters")
            .HorizontalGroup(h =>
            {
                h.Text("A: ")
                    .TextBoxFor("NewLetter.A");
            })
            .HorizontalGroup(h =>
            {
                h.Text("B: ")
                    .TextBoxFor("NewLetter.B");
            })
            .Button("Add", async () =>
            {
                model.Letters.Add(model.NewLetter);
                model.NewLetter = new model.Alphabet();
            })
            .Table<model.Alphabet>(itemsModelFieldName: "Letters", columns: new[]
            {
                new nac.Forms.model.Column
                {
                    Header = "A",
                    modelBindingPropertyName = "A"
                },
                new nac.Forms.model.Column
                {
                    Header = "B",
                    modelBindingPropertyName = "B"
                }
            }, autoGenerateColumns: false);
    }



    public static void FilterTableResults(Form f)
    {
        var model = new model.TestDataContextWindowModel();
                f.DataContext = model;
                
                model.refreshLettersWithRandomData();

                f.HorizontalGroup(h =>
                {
                    h.Button("randomize", async () =>
                        {
                            model.refreshLettersWithRandomData();
                        })
                        .Button("Filter", async () =>
                        {
                            model.applyFilter();
                        })
                        .TextBoxFor("Filter");
                })
                .Text("Output")
                .TextBoxFor("OutputText", multiline: true, style: new nac.Forms.model.Style{height = 50})
                .Table<model.Alphabet>(itemsModelFieldName: "Letters", columns: new[]
                {
                    new nac.Forms.model.Column
                    {
                        Header  = "",
                        template = myColTemplate =>
                        {
                            myColTemplate.Button("Set", async () =>
                            {
                                var l = myColTemplate.DataContext as model.Alphabet;

                                model.OutputText = $@"
                                    A: {l.A}
                                    B: {l.B}
                                ";
                            });
                        }
                    },
                    new nac.Forms.model.Column
                    {
                        Header = "A",
                        modelBindingPropertyName = "A"
                    },
                    new nac.Forms.model.Column
                    {
                        Header  = "A (Template)",
                        template = r =>
                        {
                            r.TextFor("A");
                        }
                    },
                    new nac.Forms.model.Column
                    {
                        Header = "B",
                        modelBindingPropertyName = "B"
                    },
                    new nac.Forms.model.Column
                    {
                        Header = "B (Template)",
                        template = r =>
                        {
                            r.TextFor("B");
                        }
                    }
                }, autoGenerateColumns: false);
    }
    
    
    
    
}