using EFTask;

var manager = new FootballManager();
using (var applicationContext = new FootballContext())
    manager.Start(applicationContext);
