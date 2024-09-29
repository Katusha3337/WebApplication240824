using System.Text.RegularExpressions;

List<Car> cars = new List<Car>
{
    new() { Id = Guid.NewGuid().ToString(), Make = "Hyundai", Model = "Elantra", Year = 2021 },
    new() { Id = Guid.NewGuid().ToString(), Make = "Audi", Model = "A4", Year = 2020 },
    new() { Id = Guid.NewGuid().ToString(), Make = "Mercedes", Model = "C-Class", Year = 2019 },
    new() { Id = Guid.NewGuid().ToString(), Make = "Toyota", Model = "Camry", Year = 2022 },
    new() { Id = Guid.NewGuid().ToString(), Make = "Ford", Model = "Focus", Year = 2018 },
    new() { Id = Guid.NewGuid().ToString(), Make = "BMW", Model = "X5", Year = 2021 },
    new() { Id = Guid.NewGuid().ToString(), Make = "Honda", Model = "Civic", Year = 2019 }
};

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles(); // Автоматически ищет index.html
app.UseStaticFiles();  // Обслуживание статических файлов

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;

    string expressionForGuid = @"^/api/cars/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

    if (path == "/api/cars" && request.Method == "GET")
    {
        await GetAllCars(response);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        string? id = path.Value?.Split("/")[3];
        await GetCar(id, response, request);
    }
    else if (path == "/api/cars" && request.Method == "POST")
    {
        await CreateCar(response, request);
    }
    else if (path == "/api/cars" && request.Method == "PUT")
    {
        await UpdateCar(response, request);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeleteCar(id, response, request);
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }
});

app.Run();
async Task GetAllCars(HttpResponse response)
{
    await response.WriteAsJsonAsync(cars);
}

async Task GetCar(string? id, HttpResponse response, HttpRequest request)
{
    Car? car = cars.FirstOrDefault(c => c.Id == id);
    if (car != null)
        await response.WriteAsJsonAsync(car);
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Автомобиль не найден" });
    }
}
async Task CreateCar(HttpResponse response, HttpRequest request)
{
    try
    {
        var car = await request.ReadFromJsonAsync<Car>();
        if (car != null)
        {
            car.Id = Guid.NewGuid().ToString();
            cars.Add(car);
            await response.WriteAsJsonAsync(car);
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}
async Task UpdateCar(HttpResponse response, HttpRequest request)
{
    try
    {
        Car? carData = await request.ReadFromJsonAsync<Car>();
        if (carData != null)
        {
            var car = cars.FirstOrDefault(c => c.Id == carData.Id);
            if (car != null)
            {
                car.Make = carData.Make;
                car.Model = carData.Model;
                car.Year = carData.Year;
                await response.WriteAsJsonAsync(car);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Автомобиль не найден" });
            }
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}
async Task DeleteCar(string? id, HttpResponse response, HttpRequest request)
{
    Car? car = cars.FirstOrDefault(c => c.Id == id);
    if (car != null)
    {
        cars.Remove(car);
        await response.WriteAsJsonAsync(car);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Автомобиль не найден" });
    }
}
public class Car
{
    public string Id { get; set; } = "";
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
}
