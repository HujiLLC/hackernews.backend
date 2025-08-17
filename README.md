# Hacker News Backend API

A .NET 8.0 Web API that provides access to Hacker News stories with caching, search functionality, and pagination.

## Features

- 🚀 **High Performance**: Built with .NET 8.0 and optimized for speed
- 💾 **Memory Caching**: Intelligent caching to reduce API calls to Hacker News
- 🔍 **Search Functionality**: Search stories by title, content, or author
- 📄 **Pagination**: Efficient pagination with configurable page sizes
- 🛡️ **Error Handling**: Comprehensive error handling and logging
- 🧪 **Testing**: Full unit and integration test coverage
- 📚 **API Documentation**: Swagger/OpenAPI documentation
- 🔧 **Dependency Injection**: Clean architecture with DI container
- 🌐 **CORS Support**: Configured for Angular frontend integration

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code
- Internet connection (for Hacker News API)

## Installation & Setup

1. **Clone the repository**
```bash
git clone https://github.com/elite-dev301/hackernews.backend.git
cd HackerNews.Backend
```

2. **Restore dependencies**
```bash
dotnet restore
```

3. **Update configuration** (optional)
   - Modify `appsettings.json` to adjust cache duration or API settings
   - Update CORS policy in `Program.cs` if needed

4. **Run the application**
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Get Newest Stories
```
GET /api/stories/newest
```

**Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 100)
- `search` (optional): Search term to filter stories

**Example:**
```
GET /api/stories/newest?page=1&pageSize=20&search=javascript
```

### Search Stories
```
GET /api/stories/search
```

**Parameters:**
- `query` (optional): Search query
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 100)

**Example:**
```
GET /api/stories/search?query=react&page=2&pageSize=10
```

### Health Check
```
GET /api/stories/health
```

Returns API health status and timestamp.

## Response Format

All endpoints return data in this format:

```json
{
  "data": {
    "stories": [...],
    "totalCount": 500,
    "page": 1,
    "pageSize": 20,
    "totalPages": 25
  },
  "success": true,
  "message": null
}
```

### Story Object Structure

```json
{
  "id": 12345,
  "title": "Story Title",
  "url": "https://example.com",
  "score": 100,
  "by": "username",
  "time": 1640995200,
  "descendants": 50,
  "type": "story",
  "text": "Optional story text content"
}
```

## Configuration

### appsettings.json

```json
{
  "HackerNewsApi": {
    "BaseUrl": "https://hacker-news.firebaseio.com/v0",
    "CacheDurationMinutes": 5,
    "MaxConcurrentRequests": 10
  }
}
```

**Configuration Options:**
- `BaseUrl`: Hacker News API base URL
- `CacheDurationMinutes`: How long to cache story data
- `MaxConcurrentRequests`: Maximum concurrent requests to Hacker News API

## Architecture

### Services

- **IHackerNewsService**: Direct integration with Hacker News API
- **IStoryService**: Business logic for story operations
- **Caching**: Memory caching for improved performance
- **Error Handling**: Global error handling middleware

### Dependency Injection

The application uses .NET's built-in DI container:

```csharp
builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
```

### Caching Strategy

- **Story IDs**: Cached for 5 minutes (configurable)
- **Individual Stories**: Cached for 10 minutes (2x story ID cache)
- **Cache Keys**: `newest_story_ids`, `story_{id}`

## Testing

### Run Unit Tests
```bash
dotnet test --filter "Category=Unit"
```

### Run Integration Tests
```bash
dotnet test --filter "Category=Integration"
```

### Run All Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure
- **Unit Tests**: Services and controllers
- **Integration Tests**: Full API endpoint testing
- **Mocking**: Moq for external dependencies
- **Assertions**: FluentAssertions for readable tests

## Performance Considerations

- **Caching**: Reduces API calls to Hacker News by 80-90%
- **Semaphore**: Limits concurrent requests to prevent rate limiting
- **Async/Await**: Non-blocking I/O operations
- **Pagination**: Efficient data handling for large result sets

## Deployment

### Azure App Service

This project is already connected to the Azure.

### Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HackerNews.Backend.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HackerNews.Backend.dll"]
```

## Monitoring & Logging

- **Structured Logging**: Uses Microsoft.Extensions.Logging
- **Log Levels**: Info, Warning, Error with appropriate context
- **Performance Tracking**: Request timing and cache hit rates

## CORS Configuration

Configured for Angular development:
```csharp
options.AddPolicy("AllowAngularApp", policy =>
{
    policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
});
```

Update for production deployment.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Update documentation if needed
6. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For issues or questions:
1. Check the API documentation at `/swagger`
2. Review the logs for detailed error information
3. Ensure Hacker News API is accessible
4. Verify configuration settings
