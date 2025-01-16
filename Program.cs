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
    public List<Client> Clienți { get; set; }  
    public Admin Administrator { get; set; }

    public Pizzeria(string nume, string adresa, Admin admin)
    {
        Nume = nume;
        Adresa = adresa;
        Meniu = new List<Pizza>();
        Comenzi = new List<Comanda>();
        Clienți = new List<Client>(); 
        Administrator = admin; 
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
            Console.WriteLine("Numărul de telefon este deja utilizat de un alt client.");
            return;
        }

        var clientNou = new Client(nume, telefon);
        if (!clientNou.ValidareTelefon())
        {
            Console.WriteLine("Numărul de telefon nu este valid. Formatul corect este +40XXXXXXXXX.");
            return;
        }

        Clienți.Add(clientNou);
        Console.WriteLine($"Clientul {clientNou.Nume} a fost înregistrat cu succes!");
    }

    
    public void VizualizareMeniu(Admin admin)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate gestiona meniul.");
            return;
        }

        Console.WriteLine("Meniu Pizzerie:");
        foreach (var pizza in Meniu)
        {
            Console.WriteLine($"{pizza.Nume} - {pizza.Dimensiune} - Pret: {pizza.CalcularePret()} RON");
        }
    }
   

   
    public void AdaugaPizzaInMeniu(Admin admin, Pizza pizza)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate adăuga pizza.");
            return;
        }

        Meniu.Add(pizza);
        Console.WriteLine($"Pizza {pizza.Nume} a fost adăugată în meniu.");
    }
    
    public void StergePizzaDinMeniu(Admin admin, string numePizza)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate șterge pizza.");
            return;
        }

        var pizzaDeSters = Meniu.FirstOrDefault(p => p.Nume == numePizza);
        if (pizzaDeSters != null)
        {
            Meniu.Remove(pizzaDeSters);
            Console.WriteLine($"Pizza {numePizza} a fost ștearsă din meniu.");
        }
        else
        {
            Console.WriteLine("Pizza nu a fost găsită.");
        }
    }
    
    public void ModificaPizzaInMeniu(Admin admin, string numePizza, Dimensiune dimensiuneNoua, List<Ingredient> ingredienteNoi)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate modifica pizza.");
            return;
        }

        var pizzaDeModificat = Meniu.FirstOrDefault(p => p.Nume == numePizza);
        if (pizzaDeModificat != null)
        {
            pizzaDeModificat.Dimensiune = dimensiuneNoua;
            pizzaDeModificat.Ingrediente = ingredienteNoi;
            Console.WriteLine($"Pizza {numePizza} a fost modificată.");
        }
        else
        {
            Console.WriteLine("Pizza nu a fost găsită.");
        }
    }
  
    
    public void VizualizareIngrediente(Admin admin)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate vizualiza ingredientele.");
            return;
        }

        Console.WriteLine("Ingrediente disponibile:");
        foreach (var pizza in Meniu)
        {
            foreach (var ingredient in pizza.Ingrediente)
            {
                Console.WriteLine($"{ingredient.Nume} - Pret: {ingredient.Pret} RON");
            }
        }
    }
    
    public void ModificaPretIngredient(Admin admin, string numeIngredient, decimal pretNou)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate modifica prețul ingredientului.");
            return;
        }

        var ingredientDeModificat = Meniu.SelectMany(p => p.Ingrediente).FirstOrDefault(i => i.Nume == numeIngredient);
        if (ingredientDeModificat != null)
        {
            ingredientDeModificat.Pret = pretNou;
            Console.WriteLine($"Prețul ingredientului {numeIngredient} a fost modificat la {pretNou} RON.");
        }
        else
        {
            Console.WriteLine("Ingredientul nu a fost găsit.");
        }
    }
    
    public void AdaugaIngredientInMeniu(Admin admin, string numePizza, Ingredient ingredientNou)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate adăuga ingredient.");
            return;
        }

        var pizzaDeModificat = Meniu.FirstOrDefault(p => p.Nume == numePizza);
        if (pizzaDeModificat != null)
        {
            pizzaDeModificat.Ingrediente.Add(ingredientNou);
            Console.WriteLine($"Ingredientul {ingredientNou.Nume} a fost adăugat la pizza {numePizza}.");
        }
        else
        {
            Console.WriteLine("Pizza nu a fost găsită.");
        }
    }
    
    public void StergeIngredientDinMeniu(Admin admin, string numePizza, string numeIngredient)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate șterge ingredient.");
            return;
        }

        var pizzaDeModificat = Meniu.FirstOrDefault(p => p.Nume == numePizza);
        if (pizzaDeModificat != null)
        {
            var ingredientDeSters = pizzaDeModificat.Ingrediente.FirstOrDefault(i => i.Nume == numeIngredient);
            if (ingredientDeSters != null)
            {
                pizzaDeModificat.Ingrediente.Remove(ingredientDeSters);
                Console.WriteLine($"Ingredientul {numeIngredient} a fost șters din pizza {numePizza}.");
            }
            else
            {
                Console.WriteLine("Ingredientul nu a fost găsit.");
            }
        }
        else
        {
            Console.WriteLine("Pizza nu a fost găsită.");
        }
    }
    
    
    public void PlaseazaComanda(Client client, List<Pizza> pizzas, string tipLivrare)
    {
        var comandaNoua = new Comanda(client, pizzas, tipLivrare);
        Comenzi.Add(comandaNoua);
        client.IstoricComenzi.Add(comandaNoua);
        Console.WriteLine($"Comanda pentru {client.Nume} a fost plasată cu succes!");
        comandaNoua.AfisareComanda();
    }

    public void VizualizareIstoricComenziPizzerie()
    {
        Console.WriteLine("Istoric Comenzi Pizzerie:");
        foreach (var comanda in Comenzi)
        {
            comanda.AfisareComanda();
        }
    }

    public void VizualizareIstoricComenziClient(Client client)
    {
        Console.WriteLine($"Istoric Comenzi pentru {client.Nume}:");
        foreach (var comanda in client.IstoricComenzi)
        {
            comanda.AfisareComanda();
        }
    }
   
    public void VizualizareComenzi(Admin admin)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate vizualiza comenzile.");
            return;
        }

        Console.WriteLine("Toate comenzile:");
        foreach (var comanda in Comenzi)
        {
            comanda.AfisareComanda();
        }
    }

    public void RaportComenziFinalizateInZi(Admin admin, DateTime data)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate vizualiza comenzile finalizate.");
            return;
        }

        var comenziFinalizate = Comenzi.Where(c => c.DataComanda.Date == data.Date && c.EstComandaFinalizata).ToList();
        Console.WriteLine($"Comenzi finalizate pe {data.ToShortDateString()}:");
        foreach (var comanda in comenziFinalizate)
        {
            comanda.AfisareComanda();
        }
    }

    public void RaportCeleMaiPopularePizze(Admin admin)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate vizualiza rapoartele.");
            return;
        }

        var pizzaComandata = Comenzi
            .SelectMany(c => c.Pizzas)
            .GroupBy(p => p.Nume)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .ToList();

        Console.WriteLine("Cele mai populare pizza comandate:");
        foreach (var grup in pizzaComandata)
        {
            Console.WriteLine($"{grup.Key} - Comenzi: {grup.Count()}");
        }
    }

    public void RaportVenituriPerioada(Admin admin, DateTime startDate, DateTime endDate)
    {
        if (admin == null || !admin.ArePermisiuni())
        {
            Console.WriteLine("Acces refuzat. Doar administratorul poate vizualiza veniturile.");
            return;
        }

        var venituri = Comenzi
            .Where(c => c.DataComanda >= startDate && c.DataComanda <= endDate)
            .Sum(c => c.CalculareTotalComanda());
        Console.WriteLine($"Venituri din perioada {startDate.ToShortDateString()} - {endDate.ToShortDateString()}: {venituri} RON");
    }
    
    
    public void ScrieComenziInFisier(string caleFisier)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(caleFisier, false))
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
                        total += 10; 
                    }
                    if (comanda.Client.EsteClientFidel())
                    {
                        total *= 0.9m; 
                    }
                    writer.WriteLine($"Total: {total} RON");
                    writer.WriteLine("----------------------------");
                }
            }
            Console.WriteLine($"Comenzile au fost scrise în fișierul: {caleFisier}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare la scrierea în fișier: {ex.Message}");
        }
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
    public Client Client { get; set; }
    public List<Pizza> Pizzas { get; set; }
    public string TipLivrare { get; set; }
    public DateTime DataComanda { get; set; }
    public bool EstComandaFinalizata { get; set; }

    public Comanda(Client client, List<Pizza> pizzas, string tipLivrare)
    {
        Client = client;
        Pizzas=pizzas;
       TipLivrare = tipLivrare;
       DataComanda = DateTime.Now;
       EstComandaFinalizata = true; 

    }

    public void AfisareComanda()
    {
        Console.WriteLine($"Comanda pentru {Client.Nume}:");
        decimal total = 0;
        foreach (var pizza in Pizzas)
        {
            decimal pretPizza = pizza.CalcularePret();
            Console.WriteLine($"{pizza.Nume} ({pizza.Dimensiune}) - Pret: {pretPizza} RON");
            total += pretPizza;
        }
        if (TipLivrare == "livrare")
        {
            total += 10;
        }
        if (Client.EsteClientFidel())
        {
            total *= 0.9m; 
            Console.WriteLine("Reducere de 10% aplicată pentru client fidel.");
        }
        Console.WriteLine($"Tip livrare: {TipLivrare}");
        Console.WriteLine($"Total comanda: {total} RON");
    }
    public decimal CalculareTotalComanda()
    {
        decimal total = Pizzas.Sum(pizza => pizza.CalcularePret());
        if (TipLivrare == "livrare")
        {
            total += 10; 
        }
        return total;
    }
}

public class Program
{
    public static void Main()
    {
      
        var admin = new Admin("Admin", "+40123456789");
        
        var pizzeria = new Pizzeria("Pizza La Noi", "Strada Exemplu, 12", admin);

        
        Console.WriteLine("Înregistrare client nou:");
        pizzeria.InregistrareClient("Ion Popescu", "+40701234567");  
        pizzeria.InregistrareClient("Maria Ionescu", "+40701234568");  

       
        Console.WriteLine("\nAutentificare admin");
        var adminAutentificat = pizzeria.Autentificare("+40123456789", true);
        if (adminAutentificat != null)
        {
            Console.WriteLine($"Bun venit, {adminAutentificat.Nume}!");  
        }

       
        var pizzaMargherita = new PizzaStandard("Margherita", Dimensiune.Mare);
        pizzaMargherita.Ingrediente.Add(new Ingredient("Mozzarella", 5));
        pizzaMargherita.Ingrediente.Add(new Ingredient("Tomate", 3));
        pizzeria.AdaugaPizzaInMeniu(admin, pizzaMargherita);

        
        pizzeria.VizualizareMeniu(admin);

       
        var ingredienteNoua = new List<Ingredient>
        {
            new Ingredient("Mozzarella", 5),
            new Ingredient("Pepperoni", 7),
            new Ingredient("Olive", 4)
        };
        pizzeria.ModificaPizzaInMeniu(admin, "Margherita", Dimensiune.Mare, ingredienteNoua);
        
        pizzeria.VizualizareIngrediente(admin);
        
        pizzeria.ModificaPretIngredient(admin, "Mozzarella", 6);
        
        
        var client = pizzeria.Clienți[0]; 
        var comanda = new List<Pizza> { pizzaMargherita };
        pizzeria.PlaseazaComanda(client, comanda, "livrare");

        
        pizzeria.VizualizareIstoricComenziPizzerie();
        pizzeria.VizualizareIstoricComenziClient(client);
        
        Console.WriteLine("\n=== Scriere comenzi în fișier ===");
        string caleFisier = @"C:\\Users\\Admin\\Desktop\\Pizza-poo\\pizza-poo_incercari\\comenzi.txt"; 
        pizzeria.ScrieComenziInFisier(caleFisier);
    }
    
}
