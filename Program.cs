using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
public interface IConsoleWrapper
{
    void WriteLine(string message);
}

public class ConsoleWrapper : IConsoleWrapper
{
    private readonly ILogger<ConsoleWrapper> _logger;

    public ConsoleWrapper(ILogger<ConsoleWrapper> logger)
    {
        _logger = logger;
    }

    public void WriteLine(string message)
    {
        _logger.LogInformation(message); // Folosim ILogger pentru a loga și mesajele
        Console.WriteLine(message); // Afișăm în continuare mesajul pe consolă
    }
}

public interface IFileWrapper
{
    void WriteToFile(string path, string content);
}

public class FileWrapper : IFileWrapper
{
    private readonly ILogger<FileWrapper> _logger;

    public FileWrapper(ILogger<FileWrapper> logger)
    {
        _logger = logger;
    }

    public void WriteToFile(string path, string content)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine(content);
            }
            _logger.LogInformation($"Continutul a fost scris cu succes în fișierul: {path}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Eroare la scrierea în fișier: {ex.Message}");
        }
    }
}

public class Pizzeria
{
    private readonly ILogger<Pizzeria> _logger;
    private readonly IConsoleWrapper _consoleWrapper;
    private readonly IFileWrapper _fileWrapper;

    public string Nume { get; set; }
    public string Adresa { get; set; }
    public List<Pizza> Meniu { get; set; }
    public List<Comanda> Comenzi { get; set; }
    public List<Client> Clienți { get; set; }
    public Admin Administrator { get; set; }

    public Pizzeria(string nume, string adresa, Admin admin, ILogger<Pizzeria> logger, IConsoleWrapper consoleWrapper, IFileWrapper fileWrapper)
    {
        Nume = nume;
        Adresa = adresa;
        Meniu = new List<Pizza>();
        Comenzi = new List<Comanda>();
        Clienți = new List<Client>();
        Administrator = admin;
        _logger = logger;
        _consoleWrapper = consoleWrapper;
        _fileWrapper = fileWrapper;
    }

    public dynamic Autentificare(string telefon, bool esteAdmin)
    {
        if (esteAdmin)
        {
            return Administrator.Telefon == telefon ? Administrator : null;
        }
        else
        {
            return Clienți.FirstOrDefault(c => c.Telefon == telefon);
        }
    }

    public void InregistrareClient(string nume, string telefon)
    {
        if (Clienți.Any(c => c.Telefon == telefon))
        {
            _logger.LogWarning("Numărul de telefon este deja utilizat de un alt client.");
            return;
        }

        var clientNou = new Client(nume, telefon);
        if (!clientNou.ValidareTelefon())
        {
            _logger.LogWarning("Numărul de telefon nu este valid. Formatul corect este +40XXXXXXXXX.");
            return;
        }

        Clienți.Add(clientNou);
        _logger.LogInformation($"Clientul {clientNou.Nume} a fost înregistrat cu succes!");
        SalveazaStareAplicatie("C:\\Users\\Narcisa\\source\\repos\\ConsoleApp5\\state.json");
    }

    public void VizualizareMeniu(Admin admin)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate gestiona meniul.");
            return;
        }

        _consoleWrapper.WriteLine("Meniu Pizzerie:");
        foreach (var pizza in Meniu)
        {
            _consoleWrapper.WriteLine($"{pizza.Nume} - {pizza.Dimensiune} - Pret: {pizza.CalcularePret()} RON");
        }
    }

    public void AdaugaPizzaInMeniu(Admin admin, Pizza pizza)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate adăuga pizza.");
            return;
        }

        Meniu.Add(pizza);
        _logger.LogInformation($"Pizza {pizza.Nume} a fost adăugată în meniu.");
        _consoleWrapper.WriteLine($"Pizza {pizza.Nume} a fost adăugată în meniu.");
        SalveazaStareAplicatie("C:\\Users\\Narcisa\\source\\repos\\ConsoleApp5\\state.json");
    }

    public void StergePizzaDinMeniu(Admin admin, string numePizza)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate șterge pizza.");
            return;
        }

        var pizzaDeSters = Meniu.FirstOrDefault(p => p.Nume == numePizza);
        if (pizzaDeSters != null)
        {
            Meniu.Remove(pizzaDeSters);
            _consoleWrapper.WriteLine($"Pizza {numePizza} a fost ștearsă din meniu.");
            SalveazaStareAplicatie("C:\\Users\\Narcisa\\source\\repos\\ConsoleApp5\\state.json");
        }
        else
        {
            _consoleWrapper.WriteLine("Pizza nu a fost găsită.");
        }
    }

    public void ModificaIngredientePizza(Admin admin, string numePizza, List<Ingredient> ingredienteNoi)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate modifica ingredientele.");
            return;
        }

        var pizzaDeModificat = Meniu.FirstOrDefault(p => p.Nume == numePizza);
        if (pizzaDeModificat != null)
        {
            pizzaDeModificat.Ingrediente = ingredienteNoi;
            _consoleWrapper.WriteLine($"Ingredientele pentru pizza {numePizza} au fost actualizate.");
            
        }
        else
        {
            _consoleWrapper.WriteLine("Pizza nu a fost găsită.");
        }
    }

    public void ModificaPretIngredient(Admin admin, string numeIngredient, decimal pretNou)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate modifica prețul ingredientului.");
            return;
        }

        foreach (var pizza in Meniu)
        {
            var ingredient = pizza.Ingrediente.FirstOrDefault(i => i.Nume == numeIngredient);
            if (ingredient != null)
            {
                ingredient.Pret = pretNou;
                _consoleWrapper.WriteLine($"Prețul ingredientului {numeIngredient} a fost modificat.");
                return;
            }
        }

        _consoleWrapper.WriteLine("Ingredientul nu a fost găsit.");
    }

    public void VizualizareIngrediente(Admin admin)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate vizualiza ingredientele.");
            return;
        }

        _consoleWrapper.WriteLine("Ingrediente disponibile:");
        foreach (var pizza in Meniu)
        {
            foreach (var ingredient in pizza.Ingrediente)
            {
                _consoleWrapper.WriteLine($"{ingredient.Nume} - Pret: {ingredient.Pret} RON");
            }
        }
    }

    public void AdaugaIngredientInPizza(Admin admin, string numePizza, Ingredient ingredientNou)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate adăuga ingredient.");
            return;
        }

        var pizzaDeModificat = Meniu.FirstOrDefault(p => p.Nume == numePizza);
        if (pizzaDeModificat != null)
        {
            pizzaDeModificat.Ingrediente.Add(ingredientNou);
            _consoleWrapper.WriteLine($"Ingredientul {ingredientNou.Nume} a fost adăugat la pizza {numePizza}.");
        }
        else
        {
            _consoleWrapper.WriteLine("Pizza nu a fost găsită.");
        }
    }

    public void StergeIngredientDinPizza(Admin admin, string numePizza, string numeIngredient)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate șterge ingredient.");
            return;
        }

        var pizzaDeModificat = Meniu.FirstOrDefault(p => p.Nume == numePizza);
        if (pizzaDeModificat != null)
        {
            var ingredientDeSters = pizzaDeModificat.Ingrediente.FirstOrDefault(i => i.Nume == numeIngredient);
            if (ingredientDeSters != null)
            {
                pizzaDeModificat.Ingrediente.Remove(ingredientDeSters);
                _consoleWrapper.WriteLine($"Ingredientul {numeIngredient} a fost șters din pizza {numePizza}.");
            }
            else
            {
                _consoleWrapper.WriteLine("Ingredientul nu a fost găsit.");
            }
        }
        else
        {
            _consoleWrapper.WriteLine("Pizza nu a fost găsită.");
        }
    }

    public void PlaseazaComanda(Client client, List<Pizza> pizzas, string tipLivrare)
    {
        var comandaNoua = new Comanda(client, pizzas, tipLivrare);
        Comenzi.Add(comandaNoua);
        client.IstoricComenzi.Add(comandaNoua);
        _logger.LogInformation($"Comanda pentru {client.Nume} a fost plasată cu succes!");
        comandaNoua.AfisareComanda(_consoleWrapper);
        SalveazaStareAplicatie("C:\\Users\\Narcisa\\source\\repos\\ConsoleApp5\\state.json");
    }

    public void ScrieComenziInFisier(string caleFisier)
    {
        if (Comenzi == null || Comenzi.Count == 0)
        {
            _logger.LogWarning("Lista de comenzi este goală. Nu s-a scris nimic în fișier.");
            return;
        }

        try
        {
            // Creează sau suprascrie fișierul
            using (StreamWriter writer = new StreamWriter(caleFisier))
            {
                foreach (var comanda in Comenzi)
                {
                    writer.WriteLine($"Comanda pentru {comanda.Client.Nume}:");
                    decimal total = 0;

                    foreach (var pizza in comanda.Pizzas)
                    {
                        decimal pretPizza = pizza.CalcularePret();
                        writer.WriteLine($"{pizza.Nume} ({pizza.Dimensiune}) - Pret: {pretPizza} RON");
                        total += pretPizza;
                    }

                    if (comanda.TipLivrare == "livrare")
                    {
                        total += 10; // Taxă livrare
                    }

                    if (comanda.Client.EsteClientFidel())
                    {
                        total *= 0.9m; // Reducere 10%
                        writer.WriteLine("Reducere de 10% aplicată pentru client fidel.");
                    }

                    writer.WriteLine($"Total comanda: {total} RON");
                    writer.WriteLine("----------------------------");
                }
            }

            _logger.LogInformation($"Comenzile au fost scrise cu succes în fișierul: {caleFisier}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Eroare la scrierea în fișier: {ex.Message}");
        }
    }

    public void SalveazaStareAplicatie(string caleFisier)
    {
        try
        {
            // Salvăm starea aplicației cu meniul și comenzile
            var pizzerieStare = new
            {
                Nume = this.Nume,
                Adresa = this.Adresa,
                Meniu = this.Meniu.Select(pizza => new
                {
                    pizza.Nume,
                    pizza.Dimensiune,
                    Pret = pizza.CalcularePret(),  // Calculăm prețul fiecărei pizza
                    Ingrediente = pizza.Ingrediente.Select(ingredient => new
                    {
                        ingredient.Nume,
                        ingredient.Pret  // Salvăm fiecare ingredient cu prețul său
                    }).ToList()
                }).ToList(),
                Comenzi = this.Comenzi.Select(comanda => new
                {
                    Client = new
                    {
                        comanda.Client.Nume,
                        comanda.Client.Telefon // Telefonul va fi salvat corect
                    },
                    Pizzas = comanda.Pizzas.Select(pizza => new
                    {
                        pizza.Nume,
                        pizza.Dimensiune
                    }).ToList(),
                    TipLivrare = comanda.TipLivrare,
                    TotalComanda = comanda.CalculareTotalComanda()  // Calculăm totalul comenzii
                }).ToList()
            };

            // Serializăm starea aplicației într-un fișier JSON
            var jsonStare = JsonSerializer.Serialize(pizzerieStare, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }  // Convertim enum-urile la string
            });

            // Salvăm fișierul JSON pe disc
            _fileWrapper.WriteToFile(caleFisier, jsonStare);
            _logger.LogInformation($"Starea aplicației a fost salvată în fișier");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Eroare la salvarea stării aplicației: {ex.Message}");
        }
    }


    public void IncarcaStareAplicatie(string caleFisier)
    {
        try
        {
            if (File.Exists(caleFisier))
            {
                var jsonStare = File.ReadAllText(caleFisier);
                var pizzerieStare = JsonSerializer.Deserialize<dynamic>(jsonStare);

                this.Nume = pizzerieStare.Nume;
                this.Adresa = pizzerieStare.Adresa;

                // Deserializare meniu cu ingrediente și prețuri
                this.Meniu = new List<Pizza>();

                foreach (var pizza in pizzerieStare.Meniu)
                {
                    var newPizza = new PizzaStandard(pizza.Nume.ToString(), (Dimensiune)Enum.Parse(typeof(Dimensiune), pizza.Dimensiune.ToString()));
                    newPizza.Ingrediente = new List<Ingredient>();

                    foreach (var ingredient in pizza.Ingrediente)
                    {
                        newPizza.Ingrediente.Add(new Ingredient(ingredient.Nume.ToString(), ingredient.Pret));
                    }

                    this.Meniu.Add(newPizza);
                }

                // Deserializare comenzi
                this.Comenzi = new List<Comanda>();
                foreach (var comanda in pizzerieStare.Comenzi)
                {
                    var client = new Client(comanda.Client.Nume.ToString(), comanda.Client.Telefon.ToString());
                    var pizzas = new List<Pizza>();

                    foreach (var pizza in comanda.Pizzas)
                    {
                        var pizzaNoua = new PizzaStandard(pizza.Nume.ToString(), (Dimensiune)Enum.Parse(typeof(Dimensiune), pizza.Dimensiune.ToString()));
                        pizzas.Add(pizzaNoua);
                    }

                    var comandaNoua = new Comanda(client, pizzas, comanda.TipLivrare.ToString())
                    {
                        EstComandaFinalizata = true
                    };

                    this.Comenzi.Add(comandaNoua);
                }

                _logger.LogInformation($"Starea aplicației a fost încărcată din fișier");
            }
            else
            {
                _logger.LogWarning($"Fișierul {caleFisier} nu există.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Eroare la încărcarea stării aplicației: {ex.Message}");
        }
    }




    public void VizualizareIstoricComenzi(Client client)
    {
        _consoleWrapper.WriteLine($"Istoricul comenzilor pentru {client.Nume}:");
        foreach (var comanda in client.IstoricComenzi)
        {
            comanda.AfisareComanda(_consoleWrapper);
        }
    }

    public void RaportComenziFinalizateInZi(Admin admin, DateTime data)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate vizualiza comenzile finalizate.");
            return;
        }

        var comenziFinalizate = Comenzi.Where(c => c.DataComanda.Date == data.Date && c.EstComandaFinalizata).ToList();
        _consoleWrapper.WriteLine($"Comenzi finalizate pe {data.ToShortDateString()}:");
        foreach (var comanda in comenziFinalizate)
        {
            comanda.AfisareComanda(_consoleWrapper);
        }
    }

    public void RaportCeleMaiPopularePizze(Admin admin)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate vizualiza rapoartele.");
            return;
        }

        var pizzaComandata = Comenzi
            .SelectMany(c => c.Pizzas)
            .GroupBy(p => p.Nume)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .ToList();

        _consoleWrapper.WriteLine("Cele mai populare pizza comandate:");
        foreach (var grup in pizzaComandata)
        {
            _consoleWrapper.WriteLine($"{grup.Key} - Comenzi: {grup.Count()}");
        }
    }

    public void RaportVenituriPerioada(Admin admin, DateTime startDate, DateTime endDate)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            _consoleWrapper.WriteLine("Acces refuzat. Doar administratorul poate vizualiza veniturile.");
            return;
        }

        var venituri = Comenzi
            .Where(c => c.DataComanda >= startDate && c.DataComanda <= endDate)
            .Sum(c => c.CalculareTotalComanda());

        _consoleWrapper.WriteLine($"Venituri din perioada {startDate.ToShortDateString()} - {endDate.ToShortDateString()}: {venituri} RON");
    }
}

public class Admin : Client
{
    public Admin(string nume, string telefon) : base(nume, telefon) { }

    public bool ArePermisiuni()
    {
        return true;
    }
}

public class Client
{
    private readonly ILogger<Client> _logger;

    public string Nume { get; set; }
    public string Telefon { get; set; }
    public List<Comanda> IstoricComenzi { get; set; }

    public Client(string nume, string telefon, ILogger<Client> logger = null)
    {
        Nume = nume;
        Telefon = telefon;
        IstoricComenzi = new List<Comanda>();
        _logger = logger;
    }

    public bool EsteClientFidel()
    {
        return IstoricComenzi.Count >= 5;
    }

    public bool ValidareTelefon()
    {
        var regex = new Regex(@"^\+40\d{9}$");
        return regex.IsMatch(Telefon);
    }
}

public class Pizza
{
    public string Nume { get; set; }
    public Dimensiune Dimensiune { get; set; }
    public List<Ingredient> Ingrediente { get; set; }

    public Pizza(string nume, Dimensiune dimensiune)
    {
        Nume = nume;
        Dimensiune = dimensiune;
        Ingrediente = new List<Ingredient>();
    }

    public virtual decimal CalcularePret()
    {
        decimal pret = Dimensiune switch
        {
            Dimensiune.Mica => 20,
            Dimensiune.Medie => 30,
            Dimensiune.Mare => 40,
            _ => 25
        };

        return pret + Ingrediente.Sum(i => i.Pret);
    }
}

public class PizzaStandard : Pizza
{
    public PizzaStandard(string nume, Dimensiune dimensiune) : base(nume, dimensiune) { }
}

public enum Dimensiune
{
    Mica,
    Medie,
    Mare
}

public class Ingredient
{
    public string Nume { get; set; }
    public decimal Pret { get; set; }

    public Ingredient(string nume, decimal pret)
    {
        Nume = nume;
        Pret = pret;
    }
}

public class Comanda
{
    private readonly ILogger<Comanda> _logger;

    public Client Client { get; set; }
    public List<Pizza> Pizzas { get; set; }
    public string TipLivrare { get; set; }
    public DateTime DataComanda { get; set; }
    public bool EstComandaFinalizata { get; set; }

    public Comanda(Client client, List<Pizza> pizzas, string tipLivrare, ILogger<Comanda> logger = null)
    {
        Client = client;
        Pizzas = pizzas;
        TipLivrare = tipLivrare;
        DataComanda = DateTime.Now;
        EstComandaFinalizata = true; // Presupunem că comenzile sunt finalizate
        _logger = logger;
    }

    public decimal CalculareTotalComanda()
    {
        decimal total = Pizzas.Sum(p => p.CalcularePret());

        if (TipLivrare == "livrare")
        {
            total += 10; // Taxă de livrare
        }

        if (Client.EsteClientFidel())
        {
            total *= 0.9m; // Reducere de 10% pentru clienții fideli
        }

        return total;
    }

    public void AfisareComanda(IConsoleWrapper consoleWrapper)
    {
        consoleWrapper.WriteLine($"Comanda pentru {Client.Nume}:");
        decimal total = 0;
        foreach (var pizza in Pizzas)
        {
            decimal pretPizza = pizza.CalcularePret();
            consoleWrapper.WriteLine($"{pizza.Nume} ({pizza.Dimensiune}) - Pret: {pretPizza} RON");
            total += pretPizza;
        }

        if (TipLivrare == "livrare")
        {
            consoleWrapper.WriteLine("Taxă de livrare: 10 RON");
            total += 10;
        }

        if (Client.EsteClientFidel())
        {
            total *= 0.9m;
            consoleWrapper.WriteLine("Reducere client fidel: 10%");
        }

        consoleWrapper.WriteLine($"Total de plată: {total} RON");
        consoleWrapper.WriteLine("----------------------------");

        if (_logger != null)
        {
            _logger.LogInformation($"Comanda pentru {Client.Nume} a fost procesată cu succes. Total de plată: {total} RON");
        }
    }
}

public class Program
{
    static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Înregistrare servicii standard
                services.AddLogging(config => config.AddConsole());
                services.AddSingleton<IConsoleWrapper, ConsoleWrapper>();
                services.AddSingleton<IFileWrapper, FileWrapper>();

                // Înregistrare Admin
                var adminTest = new Admin("Admin", "+40712345678");
                services.AddSingleton<Admin>(sp => adminTest);

                // Înregistrare Client de test
                var clientTest = new Client("Maria Popescu", "+40700000000");
                services.AddSingleton<Client>(sp => clientTest);

                // Înregistrare Pizzeria cu parametri expliciți
                services.AddSingleton(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<Pizzeria>>();
                    var consoleWrapper = sp.GetRequiredService<IConsoleWrapper>();
                    var fileWrapper = sp.GetRequiredService<IFileWrapper>();

                    var pizzerie = new Pizzeria("Pizzeria Buena", "Strada Cocorilor, nr 88", adminTest, logger, consoleWrapper, fileWrapper);
                    pizzerie.Clienți.Add(clientTest);  // Adăugăm clientul de test în lista de clienți
                    pizzerie.Clienți.Add(adminTest);   // Adăugăm adminul în lista de clienți
                    return pizzerie;
                });
            })
            .Build();

        var pizzerie = host.Services.GetRequiredService<Pizzeria>();

        // Logare utilizator
        Client client = LogareUtilizator(pizzerie);

        if (client != null)
        {
            bool exit = false;

            // Meniu interactiv
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== Meniu Aplicatie ===");
                if (client is Admin)
                {
                    Console.WriteLine("1. Vizualizare comenzi finalizate");
                    Console.WriteLine("2. Vizualizare cele mai populare pizza");
                    Console.WriteLine("3. Vizualizare venituri");
                    Console.WriteLine("4. Adaugare/Ștergere pizza/ingredient");
                }
                else
                {
                    Console.WriteLine("1. Plasare comanda");
                    Console.WriteLine("2. Vizualizare istoric comenzi");
                }
                Console.WriteLine("0. Iesire");
                Console.Write("Alegeți o opțiune: ");
                var optiune = Console.ReadLine();

                switch (optiune)
                {
                    case "1":
                        if (client is Admin)
                        {
                            Console.WriteLine("\nComenzi finalizate:");
                            pizzerie.RaportComenziFinalizateInZi(client as Admin, DateTime.Now);
                        }
                        else
                        {
                            PlasareComanda(pizzerie, client);
                        }
                        break;

                    case "2":
                        if (client is Admin)
                        {
                            Console.WriteLine("\nCele mai populare pizza:");
                            pizzerie.RaportCeleMaiPopularePizze(client as Admin);
                        }
                        else
                        {
                            VizualizareIstoricComenzi(client);
                        }
                        break;

                    case "3":
                        if (client is Admin)
                        {
                            Console.WriteLine("\nVenituri pentru perioada aleasă:");
                            pizzerie.RaportVenituriPerioada(client as Admin, DateTime.Now.AddDays(-7), DateTime.Now);
                        }
                        break;

                    case "4":
                        if (client is Admin)
                        {
                            AdministrareMeniu(pizzerie, client as Admin);
                        }
                        break;

                    case "0":
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Opțiune invalidă.");
                        break;
                }

                Console.WriteLine("\nApăsați orice tastă pentru a continua...");
                Console.ReadKey();
            }
        }
        else
        {
            Console.WriteLine("Datele introduse sunt invalide.");
        }
    }

    private static Client LogareUtilizator(Pizzeria pizzerie)
    {
        Console.WriteLine("=== Logare utilizator ===");
        Console.Write("Nume: ");
        string nume = Console.ReadLine();
        Console.Write("Telefon: ");
        string telefon = Console.ReadLine();

        // Căutăm clientul înregistrat
        var client = pizzerie.Clienți.FirstOrDefault(c => c.Nume == nume && c.Telefon == telefon);

        if (client == null)
        {
            Console.WriteLine("Clientul nu a fost găsit, încercați din nou.");
            return null;
        }
        else
        {
            Console.WriteLine($"Bine ai venit, {client.Nume}!");
            return client;
        }
    }

    private static void PlasareComanda(Pizzeria pizzerie, Client client)
    {
        
        Console.WriteLine("Plasare comandă...");
    }

    private static void VizualizareIstoricComenzi(Client client)
    {
       
        Console.WriteLine("Vizualizare istoric comenzi...");
    }

    private static void AdministrareMeniu(Pizzeria pizzerie, Admin admin)
    {
       
        Console.WriteLine("Administrare meniu...");
    }
}

