using Microsoft.OpenApi.Models;
using PizzaStore.DB;

var builder = WebApplication.CreateBuilder(args);

// Usare Swagger per assicurarsi di avere un'API autodocumentata, in cui i documenti cambiano quando viene modificato il codice.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo { Title = "FakeProducts API", Description = "The list of product that you want!", Version = "v1" });
});

var app = builder.Build();

// Codice che usa Swagger 
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI(c =>
   {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "FakeProducts API V1");
   });
}

// Tutte le route che usiamo
app.MapGet("/", () => "Hello World!");
app.MapGet("/pizzas/{id}", (int id) => PizzaDB.GetPizza(id));
app.MapGet("/pizzas", () => PizzaDB.GetPizzas());
app.MapPost("/pizzas", (Pizza pizza) => PizzaDB.CreatePizza(pizza));
app.MapPut("/pizzas", (Pizza pizza) => PizzaDB.UpdatePizza(pizza));
app.MapDelete("/pizzas/{id}", (int id) => PizzaDB.RemovePizza(id));

app.Run();


/*
L'API minima consente di creare un'API con poche righe di codice.
Include tutte le principali funzionalità usate per l'inserimento delle dipendenze, la comunicazione con i database e la gestione delle route.
Un'API minima è diversa da un'API basata su controller perché si specificano in modo esplicito le route necessarie invece di basarsi su un approccio basato su convenzioni come con un'API basata su controller.

Questo approccio presenta numerosi vantaggi:
1) Attività iniziali semplificate: sono sufficienti quattro righe di codice per creare un'API operativa in tempi rapidi.
2) Ottimizzazione progressiva: consente di aggiungere funzionalità quando sono necessarie. Fino ad allora, il codice del programma resta ridotto.
3) Funzionalità più recenti di .NET 8: usare tutte le funzionalità più recenti di .NET 8, ad esempio istruzioni e record di primo livello.

*/