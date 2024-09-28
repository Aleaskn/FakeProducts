namespace ProductStore.DB; 
using System.Text.Json.Serialization;
 /* Da adesso questa classe verrà utilizzata per rappresentare il modello di dati dei prodotti, 
 senza nessuna logica di gestione interna, poiché questa sarà demandata alle API esterne.
*/
public class Product
{
    [JsonPropertyName("id")] // Assicurati che questo campo esista nel JSON
    public int Id { get; set; }

    [JsonPropertyName("title")] // Verifica che questo campo esista nel JSON
    public string? Title { get; set; }
}


 /* Fake db che ora non mi serve più
 public class ProductsDB
 {
   private static List<Product> _product = new List<Product>()
   {
     new Product{ Id=1, Title="Montemagno, Pizza shaped like a great mountain" },
     new Product{ Id=2, Title="The Galloway, Pizza shaped like a submarine, silent but deadly"},
     new Product{ Id=3, Title="The Noring, Pizza shaped like a Viking helmet, where's the mead"} 
   };


   public static List<Product> GetProducts() 
   {
     return _product;
   } 

   public static Product ? GetProduct(int id) 
   {
     return _product.SingleOrDefault(pizza => pizza.Id == id);
   } 

   public static Product CreateProduct(Product product) 
   {
     _product.Add(product);
     return product;
   }

   public static Product UpdateProduct(Product update) 
   {
     _product = _product.Select(product =>
     {
       if (product.Id == update.Id)
       {
         product.Title = update.Title;
       }
       return product;
     }).ToList();
     return update;
   }

   public static void RemoveProduct(int id)
   {
     _product = _product.FindAll(product => product.Id != id).ToList();
   }
 }
 */