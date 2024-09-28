using Microsoft.OpenApi.Models;
using ProductStore.DB;
using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Usare Swagger per assicurarsi di avere un'API autodocumentata, in cui i documenti cambiano quando viene modificato il codice.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "FakeProducts API", 
        Description = "The list of product that you want!", 
        Version = "v1" });
});

// Aggiungi HttpClient al contenitore dei servizi
builder.Services.AddHttpClient();

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
//(a parte la home, le altre sono API di prova che usavamo per vedere i vari collegamenti iniziali)
/*
app.MapGet("/pizzas/{id}", (int id) => PizzaDB.GetPizza(id));
app.MapGet("/pizzas", () => PizzaDB.GetPizzas());
app.MapPost("/pizzas", (Pizza pizza) => PizzaDB.CreatePizza(pizza));
app.MapPut("/pizzas", (Pizza pizza) => PizzaDB.UpdatePizza(pizza));
app.MapDelete("/pizzas/{id}", (int id) => PizzaDB.RemovePizza(id));
*/

// Ottieni il prodotto da un'API esterna tramite id
app.MapGet("/products/{id}", async (int id, HttpClient client) =>
{
    try
    {
        var response = await client.GetAsync($"https://api.escuelajs.co/api/v1/products/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound(new { message = $"Product with ID {id} not found" });
        }

        response.EnsureSuccessStatusCode();
        var productJson = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(productJson);
        return Results.Ok(product);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error fetching product");
    }
});

// Ottieni tutti i prodotti da un'API esterna
app.MapGet("/products", async (HttpClient client) =>
{
    try
    {
        var response = await client.GetAsync($"https://api.escuelajs.co/api/v1/products");
        response.EnsureSuccessStatusCode();
        var productJson = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(productJson);
        return Results.Ok(products);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error fetching products");
    }
});

// Crea un nuovo prodotto tramite un'API esterna
app.MapPost("/products", async (Product product, HttpClient client) =>
{
    try
    {
        var productJson = JsonSerializer.Serialize(product);
        var content = new StringContent(productJson, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"https://api.escuelajs.co/api/v1/products", content);
        response.EnsureSuccessStatusCode();

        // Opzionale: puoi deserializzare la risposta
        var responseBody = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<Product>(responseBody);

        return Results.Ok(new { message = "Product created successfully" });
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error creating product");
    }
});

// Aggiorna un prodotto tramite un'API esterna
app.MapPut("/products/{id}", async (int id, Product product, HttpClient client) =>
{
    try
    {

        // Imposta l'ID del prodotto
        product.Id = id;

        // Serializza solo le proprietà richieste perché sennò prenderebbe tutto e riporterebbe un errore
        var productJson = JsonSerializer.Serialize(new
        {
            title = product.Title,
            price = product.Price
        });

        var content = new StringContent(productJson, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"https://api.escuelajs.co/api/v1/products/{product.Id}", content);
        response.EnsureSuccessStatusCode();

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return Results.Problem(detail: errorContent, statusCode: (int)response.StatusCode, title: "Error updating product");
        }
        return Results.Ok(new { message = "Product updated successfully" });
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error updating product");
    }
});

// Elimina un prodotto tramite un'API esterna
app.MapDelete("/products/{id}", async (int id, HttpClient client) =>
{
    try
    {
        var response = await client.DeleteAsync($"https://api.escuelajs.co/api/v1/products/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound(new { message = $"Product with ID {id} not found" });
        }

        response.EnsureSuccessStatusCode();
        return Results.Ok(new { message = "Product deleted successfully" });
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error deleting product");
    }
});

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