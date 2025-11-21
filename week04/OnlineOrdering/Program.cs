using System;
using System.Collections.Generic;
using System.Globalization;

// Product class
class Product
{
    private string Name;
    private string ProductId;
    private double Price;
    private int Quantity;

    public Product(string name, string productId, double price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException("Product ID cannot be empty.");
        }

        if (price < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        Name = name;
        ProductId = productId;
        Price = price;
        Quantity = quantity;
    }

    public string GetName()
    {
        return Name;
    }

    public string GetProductId()
    {
        return ProductId;
    }

    public double GetPrice()
    {
        return Price;
    }

    public int GetQuantity()
    {
        return Quantity;
    }

    public double GetTotalCost()
    {
        return Price * Quantity;
    }

    public string GetPackingInfo()
    {
        return Name + " (ID: " + ProductId + ")";
    }

    public override string ToString()
    {
        return GetName()
            + " | ID: " + GetProductId()
            + " | Qty: " + GetQuantity()
            + " | Unit: " + GetPrice().ToString("C2", CultureInfo.CurrentCulture)
            + " | Subtotal: " + GetTotalCost().ToString("C2", CultureInfo.CurrentCulture);
    }
}

// Address class
class Address
{
    private string Street;
    private string City;
    private string StateOrProvince;
    private string Country;

    public Address(string street, string city, string stateOrProvince, string country)
    {
        Street = street;
        City = city;
        StateOrProvince = stateOrProvince;
        Country = country;
    }

    public bool IsInUSA()
    {
        return Country != null && Country.Trim().ToUpper() == "USA";
    }

    public string GetFullAddress()
    {
        return Street + "\n" + City + ", " + StateOrProvince + "\n" + Country;
    }

    public override string ToString()
    {
        return GetFullAddress();
    }
}

// Customer class
class Customer
{
    private string Name;
    private Address CustomerAddress;

    public Customer(string name, Address address)
    {
        Name = name;
        CustomerAddress = address;
    }

    public string GetName()
    {
        return Name;
    }

    public bool IsInUSA()
    {
        return CustomerAddress.IsInUSA();
    }

    public string GetShippingInfo()
    {
        return Name + "\n" + CustomerAddress.GetFullAddress();
    }

    public override string ToString()
    {
        return GetShippingInfo();
    }
}

// Order class
class Order
{
    private List<Product> Products;
    private Customer Customer;
    private readonly double DomesticShippingCost = 5;
    private readonly double InternationalShippingCost = 35;

    public Order(Customer customer)
    {
        Customer = customer;
        Products = new List<Product>();
    }

    public void AddProduct(Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException("product");
        }
        Products.Add(product);
    }

    public double GetShippingCost()
    {
        return Customer.IsInUSA() ? DomesticShippingCost : InternationalShippingCost;
    }

    public double CalculateProductsTotal()
    {
        double total = 0;
        foreach (Product product in Products)
        {
            total += product.GetTotalCost();
        }
        return total;
    }

    public double CalculateTotal()
    {
        double productsTotal = CalculateProductsTotal();
        double shipping = GetShippingCost();
        return productsTotal + shipping;
    }

    public string GetPackingLabel()
    {
        string label = "Packing Label:\n";
        foreach (Product product in Products)
        {
            label += "- " + product.GetPackingInfo() + "\n";
        }
        return label;
    }

    public string GetShippingLabel()
    {
        return "Shipping Label:\n" + Customer.GetShippingInfo();
    }

    public string GetDetailedSummary()
    {
        string summary = "";
        summary += GetPackingLabel() + "\n";
        summary += GetShippingLabel() + "\n\n";
        summary += "Order Details:\n";

        foreach (Product product in Products)
        {
            summary += product.ToString() + "\n";
        }

        double productsTotal = CalculateProductsTotal();
        double shipping = GetShippingCost();
        double grandTotal = productsTotal + shipping;

        summary += "\nProducts Total: " + productsTotal.ToString("C2", CultureInfo.CurrentCulture) + "\n";
        summary += "Shipping:       " + shipping.ToString("C2", CultureInfo.CurrentCulture) + "\n";
        summary += "Grand Total:    " + grandTotal.ToString("C2", CultureInfo.CurrentCulture) + "\n";

        return summary;
    }
}

// Program class
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Create sample data for demonstration
        List<Order> orders = CreateSampleOrders();

        // Display prebuilt order details
        Console.WriteLine("=== Sample Orders ===");
        foreach (Order order in orders)
        {
            PrintOrderToConsole(order);
        }

        // Simple interactive loop to add one additional order
        Console.WriteLine("=== Create a New Order ===");
        Order customOrder = BuildOrderFromUserInput();
        if (customOrder != null)
        {
            Console.WriteLine("\n=== Custom Order Summary ===");
            PrintOrderToConsole(customOrder);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static List<Order> CreateSampleOrders()
    {
        // Create addresses
        Address address1 = new Address("123 Main St", "New York", "NY", "USA");
        Address address2 = new Address("456 Maple Ave", "Toronto", "ON", "Canada");

        // Create customers
        Customer customer1 = new Customer("Alice Johnson", address1);
        Customer customer2 = new Customer("Bob Smith", address2);

        // Create products
        Product product1 = new Product("Laptop", "P1001", 1200.00, 1);
        Product product2 = new Product("Mouse", "P1002", 25.50, 2);
        Product product3 = new Product("Keyboard", "P1003", 45.00, 1);
        Product product4 = new Product("Monitor", "P1004", 250.00, 1);

        // Create orders
        Order order1 = new Order(customer1);
        order1.AddProduct(product1);
        order1.AddProduct(product2);

        Order order2 = new Order(customer2);
        order2.AddProduct(product3);
        order2.AddProduct(product4);

        return new List<Order> { order1, order2 };
    }

    static void PrintOrderToConsole(Order order)
    {
        Console.WriteLine(order.GetDetailedSummary());
        Console.WriteLine(new string('-', 60));
    }

    static Order BuildOrderFromUserInput()
    {
        Console.Write("Customer name: ");
        string name = Console.ReadLine();

        Console.Write("Street: ");
        string street = Console.ReadLine();

        Console.Write("City: ");
        string city = Console.ReadLine();

        Console.Write("State/Province: ");
        string state = Console.ReadLine();

        Console.Write("Country: ");
        string country = Console.ReadLine();

        Address address = new Address(street, city, state, country);
        Customer customer = new Customer(name, address);
        Order order = new Order(customer);

        Console.WriteLine("Add products to the order. Leave name empty to finish.");

        while (true)
        {
            Console.Write("\nProduct name (enter to finish): ");
            string productName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(productName))
            {
                break;
            }

            Console.Write("Product ID: ");
            string productId = Console.ReadLine();

            double price = ReadDouble("Unit price: ");
            int quantity = ReadInt("Quantity: ");

            try
            {
                Product product = new Product(productName, productId, price, quantity);
                order.AddProduct(product);
                Console.WriteLine("Product added.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not add product: " + ex.Message);
            }
        }

        return order;
    }

    static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            double value;

            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value) ||
                double.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                return value;
            }

            Console.WriteLine("Invalid number, please try again.");
        }
    }

    static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            int value;

            if (int.TryParse(input, out value) && value > 0)
            {
                return value;
            }

            Console.WriteLine("Invalid integer, please try again.");
        }
    }
}