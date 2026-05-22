
  # Quiz & Q&A Platform UI/UX

  This is a code bundle for Quiz & Q&A Platform UI/UX. The original project is available at https://www.figma.com/design/Al5DYMAGO5w6c2da5A2tBM/Quiz---Q-A-Platform-UI-UX.

  ## Running the code

  ### React UI reference (`src/`)

  Run `npm i` to install the dependencies.

  Run `npm run dev` to start the development server.

  ### ASP.NET Core MVC app (`Aimbys.*` projects)

  This repo also contains an ASP.NET Core MVC skeleton that targets `net10.0`.
  The `.NET 10` SDK version is pinned in `global.json`.

  ```sh
  dotnet restore Aimbys.slnx
  dotnet build   Aimbys.slnx
  dotnet run --project Aimbys.Web/Aimbys.Web.csproj
  ```

  By default the app listens on `http://localhost:5094` (see
  `Aimbys.Web/Properties/launchSettings.json`). Override with
  `ASPNETCORE_URLS=http://127.0.0.1:5117 dotnet run --project Aimbys.Web --no-launch-profile`.

  Solution layout:

  - `Aimbys.Web` — ASP.NET Core MVC entry point (Bootstrap 5 default template).
  - `Aimbys.Application` — application services / use-cases (references `Aimbys.Domain`).
  - `Aimbys.Domain` — domain entities and value objects (no dependencies).
  - `Aimbys.Infrastructure` — persistence / external integrations (references `Aimbys.Domain` and `Aimbys.Application`).
  