using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFTask.Data;
using Microsoft.EntityFrameworkCore;
using Sharprompt;

namespace EFTask
{
    public class FootballManager
    {
        protected readonly Dictionary<string, Action<FootballContext>> ACTIONS;

        protected bool isRunning = false;

        public FootballManager()
        {
            ACTIONS = new()
            {
                { "Show players", fc => ShowPlayers(fc) },
                { "Insert player", fc => InsertPlayer(fc) },
                { "Update player", fc => UpdatePlayer(fc) },
                { "Delete players", fc => DeletePlayers(fc) },
                { "Show teams", fc => ShowTeams(fc) },
                { "Insert team", fc => InsertTeam(fc) },
                { "Update team", fc => UpdateTeam(fc) },
                { "Delete teams", fc => DeleteTeams(fc) },
                { "Show contracts", fc => ShowContracts(fc) },
                { "Insert contract", fc => InsertContract(fc) },
                { "Update contract", fc => UpdateContract(fc) },
                { "Delete contracts", fc => DeleteContracts(fc) },
                { "Amount of players total", fc => PlayersTotalAmount(fc) },
                { "Amount of players in a team", fc => PlayersTeamAmount(fc) },
                { "Overall expenses", fc => OverallExpenses(fc) },
                { "Team expenses", fc => TeamExpenses(fc) },
                { "Quit", _ => Quit(_) }
            };
        }

        public void Start(FootballContext fc)
        {
            Console.WriteLine("Welcome to the interactive football database console manager!");
            isRunning = true;
            while (isRunning)
            {
                Console.WriteLine();
                var actionKey = Prompt.Select("Select the action to perform", ACTIONS.Keys, 4);
                Console.WriteLine();
                var action = ACTIONS[actionKey];
                action.Invoke(fc);
            }
        }

        public void AddAction(string name, Action<FootballContext> fction) => ACTIONS.Add(name, fction);

        protected void ShowPlayers(FootballContext fc)
        {
            var players = fc.Players.Include(p => p.Contracts).ToHashSet<FootballPlayer?>();
            players.Add(null);
            while (true)
            {
                var playerToDescribe = Prompt.Select("Select a player for additional information", players, 10,
                    textSelector: p => p == null ? "Back" : $"{p.Id}. {p.Name}, {p.Age}");
                if (playerToDescribe == null) return;
                Console.WriteLine("\nFootball player\n\n"
                                    + $"Id: {playerToDescribe.Id}\n"
                                    + $"Name: {playerToDescribe.Name}\n"
                                    + $"Age: {playerToDescribe.Age}\n"
                                    + $"Contracts:\n{string.Join("\n", playerToDescribe.Contracts.Select(c => $"{c.Team.Id}. {c.Team.Name}, {c.Salary} $/year"))}\n\n"
                                    + "Press any key to go back...");
                Console.ReadKey();
                Console.WriteLine();
            }
        }

        protected void InsertPlayer(FootballContext fc)
        {
            Console.WriteLine("Inserting a new football player...");
            var name = Prompt.Input<string>("Enter the player's name");
            var age = Prompt.Input<int>("Enter the player's age");
            var player = new FootballPlayer() { Name = name, Age = age };
            fc.Players.Add(player);
            fc.SaveChanges();
            Console.WriteLine($"Successfully inserted a new player with an id {player.Id}");
        }

        protected void UpdatePlayer(FootballContext fc)
        {
            Console.WriteLine("Updating a football player...");
            var players = fc.Players.ToHashSet<FootballPlayer?>();
            players.Add(null);
            var player = Prompt.Select("Select a player to update", players, 10,
                textSelector: p => p == null ? "Back" : $"{p.Id}. {p.Name}, {p.Age}");
            if (player == null) return;
            player.Name = Prompt.Input<string>("Enter the player's new name", player.Name);
            player.Age = Prompt.Input<int>("Enter the player's new age", player.Age);
            fc.SaveChanges();
            Console.WriteLine($"Successfully updated a player with an id {player.Id}");
        }

        protected void DeletePlayers(FootballContext fc)
        {
            Console.WriteLine("Deleting football players...");
            var playersToDelete = Prompt.MultiSelect("Select players to delete", fc.Players, 10, 0,
                    textSelector: p => $"{p.Id}. {p.Name}, {p.Age}");
            foreach (var player in playersToDelete)
                fc.Players.Remove(player);
            fc.SaveChanges();
            Console.WriteLine($"Successfully deleted {playersToDelete.Count()} players");
        }

        protected void ShowTeams(FootballContext fc)
        {
            var teams = fc.Teams.Include(t => t.Contracts).ToHashSet<FootballTeam?>();
            teams.Add(null);
            while (true)
            {
                var teamToDescribe = Prompt.Select("Select a team for additional information", teams, 10,
                    textSelector: t => t == null ? "Back" : $"{t.Id}. {t.Name}");
                if (teamToDescribe == null) return;
                Console.WriteLine("\nFootball team\n\n"
                                    + $"Id: {teamToDescribe.Id}\n"
                                    + $"Name: {teamToDescribe.Name}\n"
                                    + $"Contracts:\n{string.Join("\n", teamToDescribe.Contracts.Select(c => $"{c.Player.Id}. {c.Player.Name}, {c.Player.Age}"))}\n\n"
                                    + "Press any key to go back...");
                Console.ReadKey();
                Console.WriteLine();
            }
        }

        protected void InsertTeam(FootballContext fc)
        {
            Console.WriteLine("Inserting a new football team...");
            var name = Prompt.Input<string>("Enter the team's name");
            var team = new FootballTeam() { Name = name };
            fc.Teams.Add(team);
            fc.SaveChanges();
            Console.WriteLine($"Successfully inserted a new team with an id {team.Id}");
        }

        protected void UpdateTeam(FootballContext fc)
        {
            Console.WriteLine("Updating a football team...");
            var teams = fc.Teams.ToHashSet<FootballTeam?>();
            teams.Add(null);
            var team = Prompt.Select("Select a team to update", teams, 10,
                textSelector: t => t == null ? "Back" : $"{t.Id}. {t.Name}");
            if (team == null) return;
            team.Name = Prompt.Input<string>("Enter the team's new name", team.Name);
            fc.SaveChanges();
            Console.WriteLine($"Successfully updated a team with an id {team.Id}");
        }

        protected void DeleteTeams(FootballContext fc)
        {
            Console.WriteLine("Deleting football teams...");
            var teamsToDelete = Prompt.MultiSelect("Select teams to delete", fc.Teams, 10, 0,
                    textSelector: t => $"{t.Id}. {t.Name}");
            foreach (var team in teamsToDelete)
                fc.Teams.Remove(team);
            fc.SaveChanges();
            Console.WriteLine($"Successfully deleted {teamsToDelete.Count()} teams");
        }

        protected void ShowContracts(FootballContext fc)
        {
            var contracts = fc.Contracts.Include(c => c.Player).Include(c => c.Team).ToHashSet<FootbalContract?>();
            contracts.Add(null);
            while (true)
            {
                var contractToDescribe = Prompt.Select("Select a team for additional information", contracts, 10,
                    textSelector: c => c == null ? "Back" : $"{c.Id}. {c.Player.Name} - {c.Team.Name}");
                if (contractToDescribe == null) return;
                Console.WriteLine("\nFootball contract\n\n"
                                    + $"Id: {contractToDescribe.Id}\n"
                                    + $"Player:\n"
                                    + $"\tId: {contractToDescribe.PlayerId}\n"
                                    + $"\tName: {contractToDescribe.Player.Name}\n"
                                    + $"\tAge: {contractToDescribe.Player.Age}\n"
                                    + $"Team:\n"
                                    + $"\tId: {contractToDescribe.TeamId}\n"
                                    + $"\tName: {contractToDescribe.Team.Name}\n"
                                    + $"Salary: {contractToDescribe.Salary} $/year\n\n"
                                    + "Press any key to go back...");
                Console.ReadKey();
                Console.WriteLine();
            }
        }

        protected void InsertContract(FootballContext fc)
        {
            Console.WriteLine("Inserting a new football contract...");
            if (fc.Players.Count() == 0)
            {
                Console.WriteLine("No players were found, insert a new player to make a contract");
                Console.WriteLine("Cancelling operation...");
                return;
            }
            var player = Prompt.Select("Select the contract's player", fc.Players, 10,
                textSelector: p => $"{p.Id}. {p.Name}, {p.Age}");
            var availableTeams = fc.Teams.ToHashSet().Except(player.Contracts.Select(p => p.Team));
            if (availableTeams.Count() == 0)
            {
                Console.WriteLine("No available teams were found, insert a new team to make a contract with this player");
                Console.WriteLine("Cancelling operation...");
                return;
            }
            var team = Prompt.Select("Select the contract's team", availableTeams, 10,
                textSelector: t => $"{t.Id}. {t.Name}");
            var salary = Prompt.Input<decimal>("Input the player's salary ($/year)");
            var contract = new FootbalContract() { Player = player, Team = team, Salary = salary };
            fc.Contracts.Add(contract);
            fc.SaveChanges();
            Console.WriteLine($"Successfully inserted a new contract with an id {contract.Id}");
        }

        protected void UpdateContract(FootballContext fc)
        {
            Console.WriteLine("Updating a football contract...");
            var contracts = fc.Contracts.ToHashSet<FootbalContract?>();
            contracts.Add(null);
            var contract = Prompt.Select("Select a contract to update", contracts, 10,
                textSelector: c => c == null ? "Back" : $"{c.Id}. {c.Player.Name} - {c.Team.Name}");
            if (contract == null) return;

            var players = fc.Players.Where(p => p.Id != contract.PlayerId).ToHashSet<FootballPlayer?>();
            players.Add(null);
            var player = Prompt.Select("Select a new player", players, 10,
                textSelector: p => p == null ? "Do not change" : $"{p.Id}. {p.Name}, {p.Age}");
            if (player != null)
                contract.Player = player;

            var teams = fc.Teams.Where(t => t.Id != contract.TeamId).ToHashSet<FootballTeam?>();
            teams.Add(null);
            var team = Prompt.Select("Select a new team", teams, 10,
                textSelector: t => t == null ? "Do not change" : $"{t.Id}. {t.Name}");
            if (team != null)
                contract.Team = team;

            var salary = Prompt.Input<decimal>("Input the player's salary", contract.Salary, contract.Salary.ToString());
            contract.Salary = salary;
            fc.SaveChanges();
            Console.WriteLine($"Successfully updated a contract with an id {contract.Id}");
        }

        protected void DeleteContracts(FootballContext fc)
        {
            Console.WriteLine("Deleting football contracts...");
            var contractsToDelete = Prompt.MultiSelect("Select contracts to delete", fc.Contracts, 10, 0,
                    textSelector: c => $"{c.Id}. {c.Player.Name} - {c.Team.Name}");
            foreach (var contract in contractsToDelete)
                fc.Contracts.Remove(contract);
            fc.SaveChanges();
            Console.WriteLine($"Successfully deleted {contractsToDelete.Count()} contracts");
        }

        protected void PlayersTeamAmount(FootballContext fc)
        {
            Console.WriteLine("Calculating amount of players in a team...");
            var team = Prompt.Select("Select a team", fc.Teams, 10,
                textSelector: t => $"{t.Id}. {t.Name}");
            var cachedValue = fc.RedisCache.StringGet(team.Id.ToString());
            if (cachedValue != StackExchange.Redis.RedisValue.Null)
            {
                var result = cachedValue.ToString();
                Console.WriteLine($"There are {result} players in team {team.Name}");
            }
        }

        protected void PlayersTotalAmount(FootballContext fc)
        {
            throw new NotImplementedException();
        }

        protected void TeamExpenses(FootballContext fc)
        {
            throw new NotImplementedException();
        }

        protected void OverallExpenses(FootballContext fc)
        {
            throw new NotImplementedException();
        }

        protected void Quit(FootballContext _)
        {
            isRunning = false;
            Console.WriteLine();
            Console.WriteLine("Quitting...");
        }
    }
}
