IICL Blog Management App

Backend: ASP.NET Core 9 Web API + EF Core (SQLite dev, PostgreSQL-ready)
Frontend: Angular 19 + Angular Material (clean, responsive admin UI)

## Features
- BlogPost entity: id, title, content, author, createdAt, updatedAt
- REST API: POST/GET (all + by id)/PUT/DELETE at `/api/blogs`
- Validation and consistent error responses
- Pagination/filtering: `GET /api/blogs?page=1&pageSize=10&author=John&from=2024-01-01&to=2025-12-31&search=text`
- Security/Hardening: CORS, rate limiting (fixed window), global error handler, logging via built-in ASP.NET logging
- OpenAPI/Swagger in Development at `/swagger`

## Local Development

### Prerequisites
- .NET SDK 9
- Node.js 18 or 20 LTS (Angular 19 recommends Node 20+)

### Backend (API)
cd backend
# Ensure DB created and migrations applied (already done once)
dotnet ef database update --project BlogApi/BlogApi.csproj
# Run
dotnet run --project BlogApi/BlogApi.csproj

Default dev URL: http://localhost:5162
Swagger: http://localhost:5162/swagger

Configuration: backend/BlogApi/appsettings.json
- UsePostgres: false (dev uses SQLite). For Render, set to true and provide ConnectionStrings:Postgres.
- CORS FrontendUrl defaults to http://localhost:4200.

### Frontend (Angular)
cd frontend
npm install
npm start

Dev URL: http://localhost:4200

The frontend calls the API at http://localhost:5162/api. Adjust in src/environments/environment.ts if needed.

## API Examples
- Create
POST /api/blogs
{
  "title": "My Post",
  "content": "Long content...",
  "author": "Alice"
}

- List with paging/search
GET /api/blogs?page=1&pageSize=10&search=post

## Deploying to Render

### Backend (Web Service)
1. Repo path: backend/BlogApi
2. Build command: dotnet build -c Release
3. Start command: dotnet BlogApi.dll (Render auto runs after dotnet publish)
   - Recommended: Set Build Command to dotnet publish -c Release -o out and Start Command to dotnet out/BlogApi.dll
4. Environment Variables:
   - ASPNETCORE_URLS = http://0.0.0.0:10000
   - UsePostgres = true
   - ConnectionStrings__Postgres = postgres://... or standard Host=...;Database=...;Username=...;Password=...
   - Cors__FrontendUrl = https://<your-frontend>.onrender.com

### Frontend (Static Site)
1. Repo path: frontend
2. Build command: npm ci && npm run build
3. Publish directory: dist/blog-admin/browser
4. API base URL: set in src/environments/environment.ts before building production, e.g.
export const environment = {
  production: true,
  apiBase: 'https://<your-backend>.onrender.com/api'
};
Commit and redeploy.

## Notes
- To change DB locally, edit appsettings.json connection strings. SQLite file is backend/BlogApi/blog.db.
- Rate limiting is basic (100 requests/10s). Adjust in Program.cs if needed.
- Logging: standard ASP.NET logging is enabled; add Serilog if you want advanced structured logging.


Live URLS : 
FrontEnd : https://iicl-blog-1-1.onrender.com 
Backend : https://iicl-blog-1.onrender.com
API endpoints : https://iicl-blog-1.onrender.com/api/blogs
