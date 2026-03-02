var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

#region Test Resources (InMemory)

var products = new List<Product>
{
    new Product { Id = 1, Name = "Ноут", Price = 40000 },
    new Product { Id = 2, Name = "Телефон", Price = 70000 }
};

var students = new List<Student>
{
    new Student { Id = 1, Name = "Альберт", Year = 2024 },
    new Student { Id = 2, Name = "Элизабет", Year = 2025 }
};

#endregion

// 1-2. Базовый маршрут api/[controller]

// продукты
var productGroup = app.MapGroup("/api/products");

// GET api/products?page=1&pageSize=10&sort=name
productGroup.MapGet("/", (int page = 1, int pageSize = 10, string? sort = null) =>
{
    var query = products.AsQueryable();

    if (!string.IsNullOrEmpty(sort))
        query = query.OrderBy(p => p.Name);

    return Results.Ok(query.Skip((page - 1) * pageSize).Take(pageSize));
});

// GET api/products/1
productGroup.MapGet("/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

// POST api/products
productGroup.MapPost("/", (Product product) =>
{
    product.Id = products.Max(p => p.Id) + 1;
    products.Add(product);
    return Results.Created($"/api/products/{product.Id}", product);
});

// PUT api/products/1
productGroup.MapPut("/{id:int}", (int id, Product updated) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null) return Results.NotFound();

    product.Name = updated.Name;
    product.Price = updated.Price;

    return Results.Ok(product);
});

// DELETE api/products/1
productGroup.MapDelete("/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null) return Results.NotFound();

    products.Remove(product);
    return Results.NoContent();
});


// 4. Маршруты с параметрами

// GET api/students/byname/Элизабет
app.MapGet("/api/students/byname/{name}", (string name) =>
{
    return students.Where(s => s.Name.ToLower() == name.ToLower());
});
// GET api/students/byyear/2024
app.MapGet("/api/students/byyear/{year:int}", (int year) =>
{
    return students.Where(s => s.Year == year);
});


// 5. Ограничения маршрутов

// int constraint
app.MapGet("/api/test/{id:int}", (int id) => $"ID: {id}");

// datetime constraint
app.MapGet("/api/test/date/{date:datetime}", (DateTime date) =>
    $"Date: {date.ToShortDateString()}");

// guid constraint
app.MapGet("/api/test/{guid:guid}", (Guid guid) =>
    $"GUID: {guid}");

// minlength constraint
app.MapGet("/api/test/slug/{slug:minlength(3)}", (string slug) =>
    $"Slug: {slug}");

// 6. Необязательный параметр

// GET api/optional или api/optional/5
app.MapGet("/api/optional/{id?}", (int? id) =>
{
    if (id is null)
        return "ID не предоставлено.";

    return $"ID: {id}";
});

// 7. Вложенный маршрут

// GET api/students/1/courses
app.MapGet("/api/students/{id:int}/courses", (int id) =>
{
    return Results.Ok(new
    {
        StudentId = id,
        Courses = new[] { "Математика", "Философия", "Астрономия" }
    });
});


// маршруты из ЛР3

app.MapGet("/hello", () => Results.Content("<h1>Hello world!</h1>", "text/html"));
app.MapGet("/text", () => Results.Text("One two three", "text/plain"));
app.MapGet("/json", () => new { Message = "Who are you?", Date = DateTime.Now });
app.MapGet("/xml", () =>
{
    var xml = "<note><message>Hello XML</message></note>";
    return Results.Content(xml, "application/xml");
});
app.MapGet("/csv", () => Results.Text("Name,Age\nJohn,18\nLora,21", "text/csv"));
app.MapGet("/binary", () =>
{
    byte[] data = { 1, 2, 3, 4, 5 };
    return Results.File(data, "application/octet-stream", "file.bin");
});
app.MapGet("/image", () => Results.File("wwwroot/image.png", "image/png"));
app.MapGet("/pdf", () => Results.File("wwwroot/file.pdf", "application/pdf"));
app.MapGet("/redirect", () => Results.Redirect("/hello", permanent: false));
app.MapGet("/redirect-permanent", () => Results.Redirect("/hello", permanent: true));

app.Run();

// модели
record Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

record Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Year { get; set; }
}