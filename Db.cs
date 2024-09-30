namespace ProductStore.DB; 
using System.Text.Json.Serialization;
 /* Da adesso questa classe verrà utilizzata per rappresentare il modello di dati dei prodotti, 
 senza nessuna logica di gestione interna, poiché questa sarà demandata alle API esterne.
*/
public class Product {
    [JsonPropertyName("id")] 
    public int Id { get; set; }

    [JsonPropertyName("title")] 
    public string? Title { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("category")]
    public Category Category { get; set; }
    
    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; } 

    [JsonPropertyName("images")]
    public List<string>? Images { get; set; }
}

public class Category {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

        [JsonPropertyName("creationAt")]
    public DateTime CreationAt { get; set; }

    [JsonPropertyName("updatedAt")]
     public DateTime UpdatedAt { get; set; }
}


// Creiamo una classe per rappresentare il login dell'utente
public class UserLogin
{
    public string Username { get; set; }
    public string Password { get; set; }
}

// Modello per la risposta con i token JWT
public class JwtTokenResponse
{
    public string Access_Token { get; set; }
    public string Refresh_Token { get; set; }
}

public class UserRegistration
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Avatar { get; set; }
}

public class UserResponse
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Avatar { get; set; }
    public string Role { get; set; }
    public int Id { get; set; }
}


/*
            I claims sono delle affermazioni contenute nel payload di un token JWT. Possono includere:

            1) ClaimTypes.Name: indica il nome dell'utente.
            2) ClaimTypes.Role: specifica il ruolo (admin, user, etc.).
            3) ClaimTypes.Email: contiene l'email dell'utente.

            Nella pratica, i claims sono semplicemente coppie chiave-valore che contengono informazioni sull'utente. 
            Possono essere standard (come sub, iat, exp) o definiti dall'utente.
            Il payload di un JWT può contenere claims come:

            sub: l'ID dell'utente (ad esempio, 1).
            iat: il timestamp di creazione del token.
            exp: il timestamp di scadenza del token.
*/