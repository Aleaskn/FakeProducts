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


