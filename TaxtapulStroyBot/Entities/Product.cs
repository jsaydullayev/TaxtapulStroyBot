﻿namespace TaxtapulStroyBot.Entities;
public class Product
{
    public int Id { get; set; }
    public string code { get; set; }
    public string Name { get; set; }
    public string Price { get; set; }
    public string? Thickness { get; set; }
    public string? Length { get; set; }
    public string? PackLength { get; set; }
    public string Description { get; set; }
}
