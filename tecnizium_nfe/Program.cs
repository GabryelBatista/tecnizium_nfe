using MongoDB.Driver;
using tecnizium_nfe.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

//MongoDB Client
builder.Services.AddSingleton<IMongoClient>((s) => {
    var config = s.GetRequiredService<IConfiguration>();
    var connectionString = config["MongoDb:ConnectionString"];
    var client = new MongoClient(connectionString);
    return client;
});

//MongoDB Service
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
//app.UseHttpsRedirection();




app.Run();
