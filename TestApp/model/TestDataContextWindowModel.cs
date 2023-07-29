using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp.model;

public class TestDataContextWindowModel : nac.Forms.model.ViewModelBase
    {

        public ObservableCollection<model.Alphabet> Letters
        {
            get { return GetValue(() => Letters); }
        }

        private List<model.Alphabet> allLetters;

        public string Filter
        {
            get { return GetValue(() => Filter); }
            set { SetValue(() => Filter, value);}
        }

        public string OutputText
        {
            get { return GetValue(() => OutputText); }
            set { SetValue(() => OutputText, value);}
        }

        public model.Alphabet NewLetter
        {
            get { return GetValue(() => NewLetter); }
            set { SetValue(() => NewLetter, value);}
        }

        public TestDataContextWindowModel()
        {
            this.NewLetter = new model.Alphabet();
            this.Filter = "";
        }


        public void refreshLettersWithRandomData()
        {
            this.Letters.Clear();
            
            // generate names that we can filter
            var firstNames = new[]
            {
                "George", "John", "Thomas", "James", "Andrew", "Martin", "William", "Zachary", "Millard", "Franklin",
                "Abraham", "Ulysses", "Rutherford", "Chester", "Arthur"
            };
            var lastNames = new[]
            {
                "Washington", "Adams", "Jefferson", "Madison", "Monroe", "Adams", "Jackson", "Van Buren", "Harrison",
                "Tyler", "Polk", "Taylor", "Fillmore", "Pierce", "Buchanan", "Lincoln", "Johnson", "Grant", "Hayes",
                "Garfield", "Arthur", "Cleveland", "Cleveland", "McKenley", "Roosevelt", "Taft", "Wilson"
            };
                
            // form 100 random names
            var r = new System.Random();
            for (int i = 0; i < 100; ++i)
            {
                var person = new model.Alphabet
                {
                    A = firstNames[r.Next(0, firstNames.Length - 1)],
                    B = lastNames[r.Next(0, lastNames.Length - 1)]
                };
                this.Letters.Add(person);
            }
        }


        public void applyFilter()
        {
            if (this.allLetters == null)
            {
                this.allLetters = new List<Alphabet>();
            }
            if (this.allLetters.Count == 0)
            {
                // copy everything into allLetters
                this.allLetters.AddRange(this.Letters);
            }
            
            this.Letters.Clear();

            var filtered = from l in allLetters
                where l.A.Contains(Filter, StringComparison.OrdinalIgnoreCase) ||
                      l.B.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                select l;
            
            // now apply that back to Letters
            foreach (var l in filtered)
            {
                this.Letters.Add(l);
            }
            
        }
    }