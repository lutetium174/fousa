using Core;
using Messages.Search;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<IMessagesQuerier, MessagesQuerier>();
builder.Services.AddSingleton<QueryParser>();

// Add OpenAPI/Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Search Service API",
        Version = "v1",
        Description = "A search service with DSL (Domain Specific Language) query support for messages. " +
                      "Supports rich querying with operators like from:, to:, during:, status:, size:, sort:, etc.",
        Contact = new OpenApiContact
        {
            Name = "Search Service",
            Email = "support@example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    // Include XML comments for better documentation
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFilename);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Enable Scalar UI
    options.CustomSchemaIds(type => type.FullName);
    
    // Add security definitions if needed
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
});

var app = builder.Build();

// Enable OpenAPI/Swagger for all environments
app.UseSwagger();

// Enable Swagger UI at /swagger
// For Scalar documentation (modern alternative to Swagger UI):
// 1. Install Node.js and Scalar CLI: npm install -g @scalar/cli
// 2. Run: npx @scalar/cli --url http://localhost:<port>/swagger/v1/swagger.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Search Service API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Search Service API - Swagger";
});

// Add test data endpoint - removed as GetAllMessages is not in interface

// DSL Query endpoint - search using query string
app.MapGet("/api/messages/search", (IMessagesQuerier querier, string? q, int? page, int? pageSize) =>
{
    page ??= 1;
    pageSize ??= 25;
    
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest("Query parameter 'q' is required");
    
    // Parse the DSL query
    var parser = new QueryParser();
    var parsedQuery = parser.Parse(q);
    
    // Apply pagination from query params
    parsedQuery.Page = page;
    parsedQuery.PageSize = pageSize;
    
    // Execute the query
    var results = querier.SearchAsync(parsedQuery).Result;
    
    return Results.Ok(new SearchResponse<Message>
    {
        Results = results,
        TotalCount = results.Count,
        Page = page.Value,
        PageSize = pageSize.Value
    });
});

// DSL Query endpoint with POST body
app.MapPost("/api/messages/search", async (IMessagesQuerier querier, SearchRequest request) =>
{
    // Parse the DSL query
    var parser = new QueryParser();
    var parsedQuery = parser.Parse(request.Query);
    
    // Apply pagination from request
    parsedQuery.Page = request.Page;
    parsedQuery.PageSize = request.PageSize;
    
    // Execute the query
    var results = await querier.SearchAsync(parsedQuery);
    var totalCount = await querier.CountAsync(parsedQuery);
    
    return Results.Ok(new SearchResponse<Message>
    {
        Results = results,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    });
});

// Query builder endpoint - test the fluent API
app.MapPost("/api/messages/query", async (IMessagesQuerier querier, Core.IMessageQuery query) =>
{
    var results = await querier.SearchAsync(query);
    return Results.Ok(results);
});

// Full-text search endpoint
app.MapGet("/api/messages/fulltext", async (IMessagesQuerier querier, string? searchText) =>
{
    if (string.IsNullOrWhiteSpace(searchText))
        return Results.BadRequest("searchText parameter is required");
    
    var results = await querier.FullTextSearchAsync(searchText);
    return Results.Ok(results);
});

// Parse DSL query (for testing the parser)
app.MapPost("/api/parse", (QueryParser parser, string query) =>
{
    var parsed = parser.Parse(query);
    return Results.Ok(new
    {
        OriginalQuery = parsed.OriginalQuery,
        FreeTextTerms = parsed.FreeTextTerms,
        Phrases = parsed.Phrases,
        FromParticipants = parsed.FromParticipants,
        ToParticipants = parsed.ToParticipants,
        InvolvingParticipants = parsed.InvolvingParticipants,
        Channels = parsed.Channels,
        Statuses = parsed.Statuses,
        Tags = parsed.Tags,
        AnyTags = parsed.AnyTags,
        AllTags = parsed.AllTags,
        HasFeatures = parsed.HasFeatures,
        ExcludedHasFeatures = parsed.ExcludedHasFeatures,
        IsPinned = parsed.IsPinned,
        CreatedRange = parsed.CreatedRange,
        ModifiedRange = parsed.ModifiedRange,
        SizeFilter = parsed.SizeFilter,
        SortField = parsed.SortField,
        SortDirection = parsed.SortDirection,
        Limit = parsed.Limit,
        Offset = parsed.Offset,
        Page = parsed.Page,
        PageSize = parsed.PageSize,
        Tokens = parsed.Tokens.Select(t => new
        {
            t.Operator,
            t.Value,
            t.IsNegated,
            t.IsPhrase,
            t.IsWildcard
        }).ToList()
    });
});

// Get message by ID
app.MapGet("/api/messages/{id}", async (IMessagesQuerier querier, string id) =>
{
    var message = await querier.GetByIdAsync(id);
    return message is null ? Results.NotFound() : Results.Ok(message);
});

// Count endpoint
app.MapGet("/api/messages/count", async (IMessagesQuerier querier, string? q) =>
{
    Core.IMessageQuery? query = null;
    if (!string.IsNullOrWhiteSpace(q))
    {
        var parser = new QueryParser();
        query = parser.Parse(q);
    }
    
    var count = await querier.CountAsync(query);
    return Results.Ok(new { Count = count });
});

// Health check
app.MapGet("/", () => "Search Service - DSL API Ready");

app.Run();