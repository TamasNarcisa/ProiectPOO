using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Pizzeria
{
    public string Nume { get; set; }
    public string Adresa { get; set; }
    public List<Pizza> Meniu { get; set; }
    public List<Comanda> Comenzi { get; set; }
    public List<Client> Clienți { get; set; }  // Lista de clienți

    public Pizzeria(string nume, string adresa)
    {
        Nume = nume;
        Adresa = adresa;
        Meniu = new List<Pizza>();
        Comenzi = new List<Comanda>();
        Clienți = new List<Client>();  // Inițializăm lista de clienți
    }

    // Funcționalitate pentru înregistrarea unui client nou
    public void InregistrareClient(string nume, string telefon)
    {
        // Verificăm dacă clientul există deja
        var clientExistă = Clienți.Any(c => c.Telefon == telefon);
        if (clientExistă)
        {
            Console.WriteLine("Clientul cu acest număr de telefon este deja înregistrat.");
        }
        else
        {
            var clientNou = new Client(nume, telefon);
            Clienți.Add(clientNou);
            Console.WriteLine($"Clientul {nume} a fost înregistrat cu succes!");
        }
    }

    // Funcționalitate pentru autentificarea unui client existent
    public Client AutentificareClient(string telefon)
    {
        var client = Clienți.FirstOrDefault(c => c.Telefon == telefon);
        if (client != null)
        {
            Console.WriteLine($"Bun venit, {client.Nume}!");
            return client;
        }
        else
        {
            Console.WriteLine("Clientul nu există. Vă rugăm să vă înregistrați.");
            return null;
        }
    }

    // Celelalte metode din Pizzeria
    public void AdaugaPizzaInMeniu(Pizza pizza)
    {
        Meniu.Add(pizza);
    }

    public void VizualizareMeniu()
    {
        Console.WriteLine("Meniu Pizzerie:");
        foreach (var pizza in Meniu)
        {
            Console.WriteLine($"{pizza.Nume} - {pizza.Dimensiune} - Pret: {pizza.CalcularePret()} RON");
        }
    }

    public void PlaseazaComanda(Client client, List<Pizza> pizzaComandate, string metodaLivrare)
    {
        if (client.ValidareTelefon())
        {
            if (metodaLivrare != "ridicare" && metodaLivrare != "livrare")
            {
                Console.WriteLine("Metoda de livrare invalidă! Folosiți 'ridicare' sau 'livrare'.");
                return;
            }

            var comanda = new Comanda(client, pizzaComandate, metodaLivrare);
            Comenzi.Add(comanda);
            client.IstoricComenzi.Add(comanda);
            Console.WriteLine("Comanda a fost plasată cu succes!");
        }
        else
        {
            Console.WriteLine("Numărul de telefon al clientului nu este valid!");
        }
    }

    public void VizualizareIstoricComenzi()
    {
        Console.WriteLine("Istoricul comenzilor:");
        foreach (var comanda in Comenzi)
        {
            Console.WriteLine($"Client: {comanda.Client.Nume}, Livrare: {comanda.MetodaLivrare}, PretTotal: {comanda.PretTotal}");
        }
    }
}

public class Client
{
    public string Nume { get; set; }
    public string Telefon { get; set; }
    public List<Comanda> IstoricComenzi { get; set; }

    public Client(string nume, string telefon)
    {
        Nume = nume;
        Telefon = telefon;
        IstoricComenzi = new List<Comanda>();
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

public class Comanda
{
    public Client Client { get; set; }
    public List<Pizza> PizzaComandate { get; set; }
    public string MetodaLivrare { get; set; }
    public decimal PretTotal { get; set; }

    public Comanda(Client client, List<Pizza> pizzaComandate, string metodaLivrare)
    {
        Client = client;
        PizzaComandate = pizzaComandate;
        MetodaLivrare = metodaLivrare;
        PretTotal = CalcularePretTotal();
    }

    public decimal CalcularePretTotal()
    {
        decimal total = PizzaComandate.Sum(pizza => pizza.CalcularePret());
        if (MetodaLivrare == "livrare")
        {
            total += 10; // Costul de livrare
        }

        if (Client.EsteClientFidel())
        {
            total *= 0.9m; // Reducere pentru client fidel
        }

        return total;
    }

    public void AfisareComanda()
    {
        Console.WriteLine($"Client: {Client.Nume}, PretTotal: {PretTotal}, Pizza comandata: {string.Join(", ", PizzaComandate.Select(p => p.Nume))}");
    }
}

public abstract class Pizza
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

    public abstract decimal CalcularePret();
}

public class PizzaStandard : Pizza
{
    public PizzaStandard(string nume, Dimensiune dimensiune)
        : base(nume, dimensiune) { }

    public override decimal CalcularePret()
    {
        decimal pretDeBaza = Dimensiune switch
        {
            Dimensiune.Mica => 25,
            Dimensiune.Medie => 30,
            Dimensiune.Mare => 40,
            _ => 30
        };

        return pretDeBaza;
    }
}

public class PizzaPersonalizata : Pizza
{
    public PizzaPersonalizata(string nume, Dimensiune dimensiune)
        : base(nume, dimensiune) { }

    public override decimal CalcularePret()
    {
        decimal pretIngrediente = Ingrediente.Sum(ingredient => ingredient.Pret);
        return 30 + pretIngrediente;  // Prețul de bază + prețul ingredientelor
    }
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

public enum Dimensiune
{
    Mica,
    Medie,
    Mare
}

public class Program
{
    public static void Main()
    {
        var pizzeria = new Pizzeria("Pizza La Noi", "Strada Exemplu, 12");

        // Înregistrarea unui client nou
        Console.WriteLine("Înregistrare client nou");
        pizzeria.InregistrareClient("Ion Popescu", "+40987654321");

        // Autentificarea unui client existent
        Console.WriteLine("Autentificare client");
        var clientAutentificat = pizzeria.AutentificareClient("+40987654321");

        if (clientAutentificat != null)
        {
            // Crearea și adăugarea de pizza în meniu
            var pizzaMargherita = new PizzaStandard("Margherita", Dimensiune.Medie);
            var pizzaPepperoni = new PizzaPersonalizata("Pepperoni", Dimensiune.Mare);
            pizzaPepperoni.Ingrediente.Add(new Ingredient("Pepperoni", 5));
            pizzaPepperoni.Ingrediente.Add(new Ingredient("Mozzarella extra", 4));

            pizzeria.AdaugaPizzaInMeniu(pizzaMargherita);
            pizzeria.AdaugaPizzaInMeniu(pizzaPepperoni);

            // Vizualizarea meniului
            pizzeria.VizualizareMeniu();

            // Plasarea unei comenzi
            var pizzaComandata = new List<Pizza> { pizzaPepperoni };
            var comanda = new Comanda(clientAutentificat, pizzaComandata, "livrare");
            pizzeria.PlaseazaComanda(clientAutentificat, pizzaComandata, "livrare");

            // Afișează comanda
            comanda.AfisareComanda();

            // Vizualizarea istoricului comenzilor
            pizzeria.VizualizareIstoricComenzi();
        }
    }
}
