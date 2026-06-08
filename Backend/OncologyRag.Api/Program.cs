using OncologyRag.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddHttpClient("pdf", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("OncologyRag/1.0");
});

builder.Services.AddHttpClient("voyage", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<PdfDownloader>();
builder.Services.AddSingleton<OcrExtractor>();
builder.Services.AddSingleton<TextChunker>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<VectorStoreService>();
builder.Services.AddHostedService<IndexingService>();

var app = builder.Build();

app.UseCors("Angular");
app.MapControllers();

app.Run();
