﻿using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "//nlog.config";

var db = new NWContext();

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try 
{

    string choice;
    do
    {
        Console.WriteLine("1.  Display Categories");
        Console.WriteLine("2.  Add Category");
        Console.WriteLine("3.  Display Category and related products");
        Console.WriteLine("4.  Display all Categories and their related products");
        Console.WriteLine("5.  Add Product");
        Console.WriteLine("6.  Edit Product");
        Console.WriteLine("7.  Display Products");
        Console.WriteLine("8.  Display Specific Product");
        Console.WriteLine("9.  Edit Category");
        Console.WriteLine("10. Delete Category");
        Console.WriteLine("11. Delete Product");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(c => c.CategoryId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            logger.Info("End of data");
        }
        else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddCategory(category);
                    logger.Info("Category added successfully");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products.Where(p => p.Discontinued != true))
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
            logger.Info("End of data");
        }
        else if (choice == "4")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products.Where(p => p.Discontinued != true))
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
            logger.Info("End of data");
        }
        else if (choice == "5")
        {
            Product product = new Product();
            Console.WriteLine("Enter new product name:");
            product.ProductName = Console.ReadLine();
            displayCategories(db);
            Console.WriteLine("Enter category ID:");
            int categoryID = Convert.ToInt32(Console.ReadLine());
            var categoryQuery = db.Categories.OrderBy(p => p.CategoryName);
            try
            {
                if (categoryID > categoryQuery.Count())
                {
                    throw new InvalidCastException();
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            product.CategoryId = categoryID;
            Console.WriteLine("Enter unit price:");
            product.UnitPrice = Convert.ToDecimal(Console.ReadLine());

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Products.Any(p => p.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddProduct(product);
                    logger.Info("Successfully added");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }

        }
        else if (choice == "6")
        {
            Console.WriteLine("Enter the Product ID you want to edit: ");
            Product product = db.Products.FirstOrDefault(p => p.ProductId == Convert.ToInt32(Console.ReadLine()));
            Console.WriteLine("What field do you want to edit?");
            Console.WriteLine("1. Product Name");
            Console.WriteLine("2. Supplier ID");
            Console.WriteLine("3. Category ID");
            Console.WriteLine("4. Quantity Per Unit");
            Console.WriteLine("5. Unit Price");
            Console.WriteLine("6. Units In Stock");
            Console.WriteLine("7. Units On Order");
            Console.WriteLine("8. Reorder Level");
            Console.WriteLine("9. Discontinued");
            int fieldChoice = Convert.ToInt32(Console.ReadLine());

            switch (fieldChoice)
            {
                case 1:
                    Console.WriteLine("Enter new product name:");
                    string name = Console.ReadLine();
                    if (db.Products.Any(p => p.ProductName == name))
                    {
                        Console.WriteLine("No repeat product names allowed");
                        break;
                    }
                    else
                    {
                        product.ProductName = name;
                        break;
                    }

                case 2:
                    displayCategories(db);
                    Console.WriteLine("Enter category ID:");
                    int categoryID = Convert.ToInt32(Console.ReadLine());
                    var categoryQuery = db.Categories.OrderBy(p => p.CategoryName);
                    try
                    {
                        if (categoryID > categoryQuery.Count())
                        {
                            throw new InvalidCastException();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                    product.CategoryId = categoryID;
                    break;
                case 3:
                    Console.WriteLine("Enter unit price:");
                    product.UnitPrice = Convert.ToDecimal(Console.ReadLine());
                    break;
                case 4:
                    Console.WriteLine("Enter supplier ID:");
                    product.SupplierId = Convert.ToInt32(Console.ReadLine());
                    break;
                case 5:
                    Console.WriteLine("Enter Quantity Per Unit:");
                    product.QuantityPerUnit = (Console.ReadLine());
                    break;
                case 6:
                    Console.WriteLine("Enter Units in Stock:");
                    product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
                    break;
                case 7:
                    Console.WriteLine("Enter Units on Order:");
                    product.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
                    break;
                case 8:
                    Console.WriteLine("Enter reorder level:");
                    product.ReorderLevel = Convert.ToInt16(Console.ReadLine());
                    break;
                case 9:
                    product.Discontinued = !product.Discontinued;
                    break;

                default:
                    break;

            }
            db.EditProduct(product);
            logger.Info("Successfully Edited");

        }
        else if (choice == "7")
        {
            Console.WriteLine("1. Display All Products");
            Console.WriteLine("2. Display Discontinued Products");
            Console.WriteLine("3. Display Active Products");
            int productChoice = Convert.ToInt32(Console.ReadLine());
            switch (productChoice)
            {
                case 1:
                    var query = db.Products.OrderBy(p => p.ProductId);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{query.Count()} records returned");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    foreach (var item in query)
                    {
                        Console.WriteLine($"{item.ProductId}. {item.ProductName} - Discontinued? {item.Discontinued}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 2:
                    var query2 = db.Products.Where(p => p.Discontinued == true).OrderBy(p => p.ProductId);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{query2.Count()} records returned");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    foreach (var item in query2)
                    {
                        Console.WriteLine($"{item.ProductId}. {item.ProductName} - Discontinued? {item.Discontinued}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 3:
                    var query3 = db.Products.Where(p => p.Discontinued != true).OrderBy(p => p.ProductId);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{query3.Count()} records returned");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    foreach (var item in query3)
                    {
                        Console.WriteLine($"{item.ProductId}. {item.ProductName} - Discontinued? {item.Discontinued}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    break;

            }
            logger.Info("Records finished");

        }
        else if (choice == "8")
        {
            Console.WriteLine("Enter the Product ID you want more info on: ");
            try
            {
                int selection = Convert.ToInt32(Console.ReadLine());
                Product product = db.Products.FirstOrDefault(p => p.ProductId == selection);
                Console.WriteLine($"ID:{product.ProductId}. {product.ProductName} - Supplier ID:{product.SupplierId} - Category ID{product.CategoryId} - Quantity:{product.QuantityPerUnit} - ${product.UnitPrice} - {product.UnitsInStock} Units - {product.UnitsOnOrder} On Order - Reorder at {product.ReorderLevel} - Discontinued? {product.Discontinued}");
                logger.Info("End of product info");

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                break;
            }

        }
        else if (choice == "9")
        {
            try
            {
                Console.WriteLine("Enter the Category ID you want to edit: ");
                Category category = db.Categories.FirstOrDefault(c => c.CategoryId == Convert.ToInt32(Console.ReadLine()));
                Console.WriteLine("1. Edit Category Name");
                Console.WriteLine("2. Edit Category Description");
                int categoryChoice = Convert.ToInt32(Console.ReadLine());
                switch (categoryChoice)
                {
                    case 1:
                        Console.WriteLine("Enter new category name");
                        string name = Console.ReadLine();
                        if (db.Categories.Any(c => c.CategoryName == name))
                        {
                            Console.WriteLine("No repeat category names allowed");
                            break;
                        }
                        else
                        {
                            category.CategoryName = name;
                            break;
                        }
                    case 2:
                        Console.WriteLine("Enter new category description");
                        category.Description = Console.ReadLine();
                        break;
                    default:
                        break;
                }
                db.EditCategory(category);
                logger.Info("Category successfully edited");
            }
            catch (Exception e)
            {
                logger.Error(e);
            }

        }
        else if (choice == "10")
        {
            var query = db.Categories.OrderBy(c => c.CategoryId);
            displayCategories(db);
            Console.WriteLine("Select the category you would like to delete: ");
            int deleteChoice = Convert.ToInt32(Console.ReadLine());
            Category category = db.Categories.FirstOrDefault(c => c.CategoryId == deleteChoice);

            var productQuery = db.Products.Where(p => p.CategoryId == deleteChoice);
            if (productQuery.Count() == 0)
            {
                db.DeleteCategory(category);
            }
            else
            {
                Console.WriteLine("The category you are about to delete will create orphaned products. \n Are you sure you want to delete this category and its dependants? (enter y to delete)");
                string areYouSure = Console.ReadLine().ToUpper();
                if (areYouSure == "Y")
                {
                    foreach (Product product in productQuery)
                    {
                        DeleteProductDependents(product, db);

                    }
                    db.DeleteCategory(category);
                    logger.Info("Category and dependents successfully delted.");
                }


            }
        }
        else if (choice == "11")
        {
            Console.WriteLine("Enter the ID of the product you want to delete: ");
            int deleteChoice = Convert.ToInt32(Console.ReadLine());
            Product product = db.Products.FirstOrDefault(c => c.ProductId == deleteChoice);
            DeleteProductDependents(product, db);
            logger.Info("Product and dependents successfully delted.");
        }

        Console.WriteLine();
    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");

static void displayCategories(NWContext db)
{
   


    var query = db.Categories.OrderBy(c => c.CategoryId);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.Magenta;
    foreach (var item in query)
    {
        Console.WriteLine($"{item.CategoryId}. {item.CategoryName} - {item.Description}");
    }
    Console.ForegroundColor = ConsoleColor.White;
}
static void DeleteProductDependents(Product product, NWContext db)
{
    

    var orderDetailsQuery = db.OrderDetails.Where(p => p.ProductId == product.ProductId);
    if (orderDetailsQuery.Count() == 0)
    {
        db.DeleteProduct(product);
    }
    else
    {
        Console.WriteLine("The product you are about to delete will create orphaned order details. \n Are you sure you want to delete this category and its dependants? (enter y to delete)");
        string areYouSure = Console.ReadLine().ToUpper();
        if (areYouSure == "Y")
        {
            foreach (OrderDetail orderDetail in orderDetailsQuery)
            {
                db.DeleteOrderDetail(orderDetail);

            }
            db.DeleteProduct(product);

        }

    }
}


