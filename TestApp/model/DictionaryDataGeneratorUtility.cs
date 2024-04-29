using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp.model;

public static class DictionaryDataGeneratorUtility
{


    public static IEnumerable<Dictionary<string, object>> GenerateRandomPeopleDictionaryList(int peopleCount = 100)
    {
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
        for (int i = 0; i < peopleCount; ++i)
        {
            string firstName = firstNames[r.Next(0, firstNames.Length - 1)];
            string lastName = lastNames[r.Next(0, lastNames.Length - 1)];

            var personDict = new Dictionary<string, object>
            {
                {"seq", i},
                { nameof(firstName), firstName },
                { nameof(lastName), lastName }
            };
            yield return personDict;
        }
    }



    public static IEnumerable<Dictionary<string,object>> GenerateStaticPropertiesList(int itemCount = 100)
    {
        var r = new System.Random();
        string[] fruits = new[] { "Apple", "Orange", "Watermelon", "Grape", "Muscadine", "Tomato", "Pear" };
        
        for (int i = 0; i < itemCount; ++i)
        {
            yield return (nac.utilities.List.CreateDictionaryFromEnumerable(new[]
            {
                new
                {
                    P1 = fruits[r.Next(0, fruits.Length - 1)],
                    P2 = i,
                    P3 = DateTime.Now.AddMinutes(-1 * (i * 30))
                }
            })).Single();
        }


    }
    
    
    
}