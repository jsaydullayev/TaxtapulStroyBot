using Newtonsoft.Json;
using System.Text.Json;
using System.Xml.Linq;
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

    public async void RemoveProduct(string productCode)
    {
        var product = Products.FirstOrDefault(u => u.code == productCode);
        if(product != null)
        {
            Products.Remove(product);
            WriteToFile();
        }
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
