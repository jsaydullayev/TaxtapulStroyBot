using Newtonsoft.Json;
using TaxtapulStroyBot.Entities;

namespace TaxtapulStroyBot.Services;
public class ProductService
{
    public List<Product> Products { get; set; }
    private const string path = "products.json";
    
    public ProductService()
    {
        Products = new List<Product>();
        ReadFromFile();
    }

    public void AddProduct(Product product)
    {
        Products.Add(product);
        WriteToFile();
    }



    private void WriteToFile()
    {
        var jsonData = JsonConvert.SerializeObject(Products);
        File.WriteAllText(path, jsonData);
    }

    public void ReadFromFile()
    {
        if (File.Exists(path))
        {
            var jsonData = File.ReadAllText(path);
            Products = JsonConvert.DeserializeObject<List<Product>>(jsonData)!;
        }
    }
}
