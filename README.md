# ProductCatalog API

Backend rješenje za Abysalto Akademiju 2026.

ProductCatalog API je ASP.NET Core Web API middleware koji dohvaća proizvode i kategorije iz vanjskog izvora podataka. Trenutni izvor je DummyJSON REST API, a aplikacija je strukturirana tako da se kasnije mogu dodati i drugi izvori podataka bez većih promjena u API sloju.

---

## Pokretanje lokalno

Preduvjet:

```text
.NET 10 SDK
```

Pokretanje aplikacije:

```powershell
dotnet restore
dotnet build ProductCatalog.sln
dotnet run --project ProductCatalog.API
```

Swagger UI je dostupan na:

```text
https://localhost:{port}/swagger
```

Port se vidi u terminalu nakon pokretanja aplikacije.

---

## Konfiguracija servisa

Konfiguracija za DummyJSON nalazi se u:

```text
ProductCatalog.API/appsettings.json
```

```json
"DummyJson": {
  "BaseUrl": "https://dummyjson.com/",
  "TimeoutSeconds": 10,
  "AuthValidationCacheMinutes": 5
}
```

Aplikacija ne koristi lokalnu bazu podataka i nije potreban Docker za pokretanje.

---

## Testni korisnik i autorizacija

Za login se koristi DummyJSON testni korisnik:

```json
{
  "username": "emilys",
  "password": "emilyspass"
}
```

Login endpoint:

```http
POST /api/auth/login
```

Nakon login-a kopirati `accessToken` i koristiti ga za zaštićene endpointove:

```http
Authorization: Bearer {accessToken}
```

U Swagger UI-u kliknuti **Authorize** i unijeti token.

Refresh token endpoint:

```http
POST /api/auth/refresh
```

Primjer bodyja:

```json
{
  "refreshToken": "{refreshToken}"
}
```

---

## Testiranje aplikacije

Pokretanje svih testova:

```powershell
dotnet test ProductCatalog.sln
```

Projekt sadrži unit testove za application service logiku i integration testove za API endpointove.

Ručni test kroz Swagger:

1. Pokrenuti aplikaciju.
2. Otvoriti `/swagger`.
3. Pozvati `POST /api/auth/login`.
4. Kopirati `accessToken`.
5. Kliknuti `Authorize`.
6. Testirati product, category i auth endpointove.

---

## Tehnološki stack

- **.NET 10** – ASP.NET Core Web API
- **Swagger / OpenAPI** – dokumentacija i ručno testiranje endpointova
- **DummyJSON REST API** – izvor proizvoda, kategorija i autentikacijskih podataka
- **IMemoryCache** – cache za ponavljajuće upite i validaciju tokena
- **Serilog** – strukturirano logiranje u konzolu i file
- **ASP.NET Core Authentication** – custom DummyJSON Bearer authentication handler
- **ASP.NET Core Rate Limiting** – zaštita API-ja i auth endpointova
- **xUnit** – unit i integration testovi
- **Microsoft.AspNetCore.Mvc.Testing** – integration testovi API-ja

---

## AI alati

Tijekom izrade korišteni su AI alati kao pomoć pri planiranju arhitekture, provjeri pristupa, pisanju testova i dokumentacije. Implementacija je ručno provjerena kroz lokalni razvoj i refaktoriranje.

---

## Endpointi

### Auth

```http
POST /api/auth/login
POST /api/auth/refresh
GET /api/auth/me
```

### Products

```http
GET /api/products
GET /api/products/{id}
GET /api/products?category=beauty&minPrice=5&maxPrice=100
GET /api/products/search?q=phone
```

### Categories

```http
GET /api/categories
```

### Health

```http
GET /health
```

---

## Implementirane funkcionalnosti

- dohvat liste proizvoda sa slikom, nazivom, cijenom i skraćenim opisom
- dohvat detalja pojedinog proizvoda
- filtriranje po kategoriji i cijeni
- pretraga proizvoda po nazivu
- dohvat kategorija
- autentikacija i autorizacija
- refresh token endpoint
- cache za ponavljajuće pozive s istim parametrima
- Serilog logiranje
- global exception handling middleware
- rate limiting
- health check endpoint
- unit i integration testovi

---

## Struktura projekta

```text
ProductCatalog.API             - controllers, authentication, middleware, startup
ProductCatalog.Application     - services, DTOs, queries, interfaces
ProductCatalog.Domain          - domain modeli
ProductCatalog.Infrastructure  - DummyJSON integracija
ProductCatalog.Tests           - unit i integration testovi
```

Ovisnosti su odvojene po slojevima. API sloj ne dohvaća DummyJSON direktno, nego koristi application service i apstrakcije. Trenutni izvor podataka je DummyJSON, ali se novi izvor može dodati kroz novu implementaciju `IProductSource`.