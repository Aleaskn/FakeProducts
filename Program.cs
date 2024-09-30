using Microsoft.OpenApi.Models;
using ProductStore.DB;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Aggiungi i servizi al container qui
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "FakeProducts API", 
        Description = "The list of product that you want!", 
        Version = "v1" 
    });
});

// Aggiungi HttpClient al contenitore dei servizi
builder.Services.AddHttpClient();

// Abilita CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Aggiungi servizi di autenticazione e autorizzazione
builder.Services.AddAuthentication("Bearer") // Configura JWT o altro schema di autenticazione
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            // Configura la convalida del token JWT (chiave, validità, ecc.)
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "your-issuer",  // Cambia con il tuo issuer
            ValidAudience = "your-audience", // Cambia con il tuo audience
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("your-secret-key")) // Chiave segreta per firmare il token
        };
    });

builder.Services.AddAuthorization(); // Aggiungi il servizio di autorizzazione

var app = builder.Build(); // Costruisci l'applicazione dopo aver registrato i servizi

// Configura l'applicazione per usare Swagger
// Usare Swagger per assicurarsi di avere un'API autodocumentata, in cui i documenti cambiano quando viene modificato il codice.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FakeProducts API V1");
    });
}

// Usa CORS
app.UseCors("AllowAll");

// Middleware per autenticazione e autorizzazione (configura quando necessario)
// Questo middleware si occuperà di verificare se un token è presente nelle richieste e se è valido.
app.UseAuthentication();
app.UseAuthorization();

// Tutte le route che usiamo
app.MapGet("/", () => "Hello World!");

/*

Un JWT (JSON Web Token) è una stringa che rappresenta in modo sicuro le informazioni tra due parti. È composto da tre parti principali:

1) Header: contiene il tipo di token e l'algoritmo di firma.
2) Payload: contiene i claims, ovvero le informazioni utente che vogliamo trasferire (ad esempio: username, ruolo, ID utente).
3) Signature: una firma che garantisce l'integrità del token.

*/

// Endpoint che permette al frontend di inviare i dati di un nuovo utente
app.MapPost("/users", async (UserRegistration newUser, HttpClient client) =>
{
    try
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Adatta la politica di serializzazione
            WriteIndented = true
        };

        // Serializza i dati dell'utente in formato JSON
        var userJson = JsonSerializer.Serialize(newUser);
        Console.WriteLine("JSON da inviare: " + userJson);

        var content = new StringContent(userJson, System.Text.Encoding.UTF8, "application/json");
        
        // Aggiungi un log delle intestazioni
        foreach (var header in content.Headers)
        {
            Console.WriteLine($"Header: {header.Key} - Value: {string.Join(", ", header.Value)}");
        }
       
        // Invia la richiesta POST all'API esterna
        var response = await client.PostAsync("https://api.escuelajs.co/api/v1/users", content);
        response.EnsureSuccessStatusCode();

        // Ottieni la risposta e deserializza il risultato
        var responseBody = await response.Content.ReadAsStringAsync();
        var createdUser = JsonSerializer.Deserialize<UserResponse>(responseBody);

        // Restituisci l'utente appena creato
        return Results.Ok(createdUser);
    }
    catch (HttpRequestException ex)
    {
        // Gestisci eventuali errori durante la richiesta HTTP
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error creating user");
    }
});

// Endpoint che permette al frontend di ottenere i dati di un utente esistente, tramite il suo id
app.MapGet("/users/{id}", async (int id, HttpClient client) =>
{
    try
    {
        // Richiesta GET per ottenere i dettagli dell'utente
        var response = await client.GetAsync($"https://api.escuelajs.co/api/v1/users/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound(new { message = $"User with ID {id} not found" });
        }

        response.EnsureSuccessStatusCode();

        // Deserializza la risposta e restituisci i dettagli dell'utente
        var userJson = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserResponse>(userJson);
        return Results.Ok(user);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error fetching user");
    }
});

// Endpoint che restituisce una lista di tutti gli utenti registrati
app.MapGet("/users", async (HttpClient client) =>
{
    try
    {
        // Invia la richiesta GET per ottenere tutti gli utenti
        var response = await client.GetAsync("https://api.escuelajs.co/api/v1/users");
        response.EnsureSuccessStatusCode();

        // Deserializza e restituisci la lista degli utenti
        var usersJson = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserResponse>>(usersJson);
        return Results.Ok(users);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error fetching users");
    }
});


// Endpoint di login che invia una richiesta all'API esterna e restituisce i token
app.MapPost("/login", async (UserLogin loginData, HttpClient client) =>
{
    // Prepara il contenuto della richiesta
    var requestContent = new StringContent(JsonSerializer.Serialize(new
    {
        email = loginData.Username,
        password = loginData.Password
    }), System.Text.Encoding.UTF8, "application/json");

    // Invia la richiesta di autenticazione all'API esterna
    var response = await client.PostAsync("https://api.escuelajs.co/api/v1/auth/login", requestContent);

    // Verifica se la risposta è un successo
    if (response.IsSuccessStatusCode)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        var tokens = JsonSerializer.Deserialize<JwtTokenResponse>(responseBody);

        // Restituisci i token
        return Results.Ok(tokens);
    }
    else
    {
        // Restituisci un errore in caso di fallimento
        return Results.Unauthorized();
    }
});

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
app.MapPost("/products", async (Product newProduct, HttpClient client) =>
{
    try
    {
        var productJson = JsonSerializer.Serialize(newProduct);
        var content = new StringContent(productJson, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"https://api.escuelajs.co/api/v1/products", content);
        response.EnsureSuccessStatusCode();

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