using System.ComponentModel.DataAnnotations;
using Core;
using Messages.Search;
using Pinotchio;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services
    .AddSingleton<IMessagesQuerier, MessagesQuerier>()
    .AddSingleton<IReadRepository<Message>, PinotRepository<Message>>()
    .AddSingleton<QueryParser>()
    .AddPinotClient(options =>
    {
        options.ControllerUri = builder.Configuration.GetValue<string>("Pinot:ControllerUri")!;
    });

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/api/messages/search", async (IMessagesQuerier querier, [Required] string q, int? page, int? pageSize) =>
{
    page ??= 1;
    pageSize ??= 25;

    var parsedQuery = new QueryParser().Parse(q);
    parsedQuery.Page = page;
    parsedQuery.PageSize = pageSize;

    // Execute the query
    var results = await querier.SearchAsync(parsedQuery);
    var totalCount = await querier.CountAsync(parsedQuery);

    return Results.Ok(new SearchResponse<Message>
    {
        Results = results,
        TotalCount = totalCount,
        Page = page.Value,
        PageSize = pageSize.Value
    });
});

app.MapPost("/api/messages/search", async (IMessagesQuerier querier, SearchRequest request) =>
{
    var parsedQuery = new QueryParser().Parse(request.Query);

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

app.MapPost("/api/messages/query", async (IMessagesQuerier querier, IMessageQuery query) =>
    Results.Ok(await querier.SearchAsync(query)));

app.MapGet("/api/messages/fulltext", async (IMessagesQuerier querier, [Required] string searchText) =>
Results.Ok(await querier.FullTextSearchAsync(searchText)));

app.MapPost("/api/parse", (QueryParser parser, string query) =>
{
    var parsed = parser.Parse(query);
    return Results.Ok(new
    {
        parsed.OriginalQuery,
        parsed.FreeTextTerms,
        parsed.Phrases,
        parsed.FromParticipants,
        parsed.ToParticipants,
        parsed.InvolvingParticipants,
        parsed.Channels,
        parsed.Statuses,
        parsed.Tags,
        parsed.AnyTags,
        parsed.AllTags,
        parsed.HasFeatures,
        parsed.ExcludedHasFeatures,
        parsed.IsPinned,
        parsed.CreatedRange,
        parsed.ModifiedRange,
        parsed.SizeFilter,
        parsed.SortField,
        parsed.SortDirection,
        parsed.Limit,
        parsed.Offset,
        parsed.Page,
        parsed.PageSize,
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
    IMessageQuery? query = null;
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