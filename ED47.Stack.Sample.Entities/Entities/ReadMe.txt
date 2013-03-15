1- Add your EF POCO entities here
2- Make sure to remove the redundant ".Entities" in the namespace
3- Make it inherit from BaseDbEntity or DbEntity
4- Add them as a DBSet in the context (here SampleContext.cs)
5- Open the Package Manager Console
6- Make sure this (the "XXX.Entities") project is selected in the dropdown list$
7- Type "update-database" to update the database (add "-force" switch if changes resul in data loss and you are Ok with that)