# eCommerceUoWBasic

A full-stack e-commerce application with an Angular storefront and an ASP.NET Core Web API. The storefront supports browsing and searching products, viewing product details, managing a cart, registering and signing in, and submitting purchases. The API provides product and category operations, account registration and login, and stock-aware purchasing.

## Technology

- **Frontend:** Angular 21, TypeScript, RxJS, Tailwind CSS, Font Awesome, and Angular SSR with Express
- **Backend:** ASP.NET Core on .NET 10, C#, Entity Framework Core, and PostgreSQL
- **Authentication:** ASP.NET Core Identity and JWT bearer tokens
- **API documentation:** OpenAPI in the Development environment

## Requirements

- .NET 10 SDK
- Node.js and npm (the frontend specifies npm 10.9.4)
- PostgreSQL
- The `dotnet-ef` CLI tool, version 10.0.11

## Configuration

Create a PostgreSQL database and set the backend connection string in `server/appsettings.json` or with the `ConnectionStrings__DefaultConnection` environment variable. Configure the JWT settings in the same file for local development.

The frontend development configuration calls the API at `https://localhost:7037/api`. The backend HTTPS launch profile listens on that address.

## Run locally

From the repository root, install the Entity Framework CLI tool if it is not already installed:

```sh
dotnet tool install --global dotnet-ef --version 10.0.11
```

Apply the database migrations:

```sh
dotnet ef database update --project server/eCommerceUoWBasic.csproj --startup-project server/eCommerceUoWBasic.csproj
```

Start the API in a terminal:

```sh
dotnet run --project server/eCommerceUoWBasic.csproj --launch-profile https
```

Start the frontend in another terminal:

```sh
cd clientapp/ecommerce-app
npm install
npm start
```

Open [http://localhost:4200](http://localhost:4200) to use the storefront.
