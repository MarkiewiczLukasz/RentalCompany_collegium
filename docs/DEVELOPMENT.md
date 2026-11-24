# Dokumentacja dla zespołu

Opis funkcjonalny i podstawowe uruchomienie znajdują się w [README](../README.md).

## Technologie i konfiguracja

Technologie: C#/.NET 8, Blazor Server / Interactive Server, ASP.NET Core Identity, Entity Framework Core i PostgreSQL. Aplikacja może działać lokalnie z Visual Studio lub w Dockerze. Baza PostgreSQL działa w kontenerze Docker w obu wariantach.

Przy starcie aplikacji wykonywane są migracje i inicjalizacja ról. Brakujące konta demonstracyjne są tworzone automatycznie, a ich hasła synchronizowane z `DemoAccounts` w `appsettings.json`. Istniejące dane i przypisane role są zachowywane. Konto User nie ma dostępu do `/admin`. Aplikacja nie udostępnia samodzielnej rejestracji ani odzyskiwania hasła.

Połączenie z PostgreSQL: baza `rental_company`, użytkownik `rental`, hasło `RentalDb123!`. Aplikacja uruchamiana lokalnie korzysta z `localhost:54329`, a kontener App z `db:5432`.

Kontener App można zatrzymać na czas pracy w Visual Studio:

```powershell
docker compose stop app
```

Podgląd stanu, logów oraz zatrzymanie i ponowne uruchomienie kontenerów:

```powershell
docker compose ps
docker compose logs --tail 100 app
docker compose stop
docker compose start
```

Kontenery używają trwałych wolumenów: danych PostgreSQL i kluczy szyfrowania ciasteczek. `docker compose down` zachowuje wolumeny. **`docker compose down -v` usuwa bazę i klucze** — polecenie służy wyłącznie do świadomego resetu lokalnych danych demonstracyjnych.

Compose obsługuje lokalne HTTP i publikuje porty na `127.0.0.1`. Obraz aplikacji działa jako użytkownik `app`.

## Uruchomienie z terminala i kompilacja

Rozwiązanie: `RentalCompany/RentalCompany.sln`. Do kompilacji wymagany jest SDK .NET 8 lub nowszy.

```powershell
dotnet build RentalCompany/RentalCompany.sln
.\scripts\Start-Local.ps1
```

Skrypt uruchamia PostgreSQL w Dockerze i App lokalnie pod **http://localhost:5008**. Aplikacja odczytuje gotową konfigurację z `appsettings.json` i używa tego samego profilu `http` co Visual Studio.

Warianty lokalny i kontenerowy korzystają z tej samej bazy. Compose nadpisuje adres połączenia na `db:5432` dla kontenera App; aplikacja uruchamiana z Visual Studio lub terminala używa `localhost:54329`.

## Architektura

```text
RentalCompany (App) → Core → Data → PostgreSQL
```

- **App**: komponenty Blazor, Razor Pages logowania/wylogowania, transport cookie, nawigacja i adapter aktualnego użytkownika. Jedyna referencja projektowa prowadzi do Core.
- **Core**: logika biznesowa, walidacja i kontrola dostępu, w tym usługi `IAccountService` oraz `IUserAccess`. Rejestracja `AddCore` podłącza Data wewnętrznie. Publiczne kontrakty nie ujawniają typów Data.
- **Data**: Identity, encje i kontekst EF, migracje, inicjalizacja i `IIdentityGateway`. Brak referencji do Core/App. Każda operacja Identity tworzy osobny scope z krótkotrwałym DbContext.
- **Tests**: testy granic architektury App/Core/Data i publicznych kontraktów Core.

Logika przypadków użycia znajduje się w Core. Operacja wymagająca zalogowania rozpoczyna się od `IUserAccess.RequireUserAsync()`, a administracyjna od `RequireAdminAsync()`. Identyfikator klienta pochodzi ze zwróconego `CurrentUser`, nie z formularza. Serwisy biznesowe odpowiadają za weryfikację uprawnień i własności obsługiwanych zasobów.

## Logowanie i sesje

- `/Account/Login`: formularz HTTP z CSRF, sprawdzenie hasła przez Identity i sesja cookie `HttpOnly`. Brak JWT i osobnego API.
- `/Account/Logout`: wyłącznie POST z CSRF. Usunięcie bieżącej sesji z bazy uniemożliwia ponowne użycie jej ciasteczka; inne sesje tego konta pozostają aktywne.
- `UserSessions`: techniczna tabela sesji powiązana z Identity. Czas ważności domyślnie 480 minut, konfigurowany przez `Authentication:Session:LifetimeMinutes`. „Zapamiętaj mnie” utrwala cookie do tego terminu, bez automatycznego przedłużania sesji.
- Każde chronione wywołanie Core oraz żądanie HTTP sprawdza sesję, security stamp, blokadę konta i aktualne role. Stan otwartej karty Blazora jest dodatkowo sprawdzany co minutę. Zmiana ról lub hasła wymaga ponownego logowania.
- Pięć błędnych prób powoduje blokadę konta na pięć minut. Błędne dane i blokada mają ten sam komunikat. ReturnUrl dopuszcza tylko adresy lokalne.

## Migracje

Polecenia wykonywane z katalogu repozytorium przy dostępnym narzędziu `dotnet ef`.

### Dodawanie migracji

Po zmianie modeli lub konfiguracji EF, przy uruchomionej bazie PostgreSQL:

```powershell
dotnet ef migrations add NazwaZmiany --project RentalCompany/Data --output-dir Persistence/Migrations
dotnet ef database update --project RentalCompany/Data
```

Migracja `InitialIdentity` tworzy tabele Identity oraz sesji. Schemat bazy jest zarządzany przez migracje EF Core, stosowane automatycznie przed rozpoczęciem obsługi żądań, zarówno przy starcie z Visual Studio, jak i w Dockerze. Aktualizacja schematu zakłada uruchomienie jednej instancji aplikacji naraz.

Pliki `schema.sql` i `seed.sql` wymagane do oddania projektu BD są przygotowywane osobno w ramach [KAN-24](https://studenci-team-spfznxmc.atlassian.net/browse/KAN-24).

### Usuwanie ostatniej migracji

Jeśli ostatnia migracja nie została jeszcze zastosowana w bazie:

```powershell
dotnet ef migrations remove --project RentalCompany/Data
```

Polecenie usuwa pliki ostatniej migracji i aktualizuje snapshot modelu. Nie usuwa się tych plików ręcznie.

### Wycofanie migracji z bazy

Przed wycofaniem zmian należy zatrzymać aplikację w Visual Studio, a jeśli działa kontener App — również `docker compose stop app`. PostgreSQL pozostaje uruchomiony. Kolejny start aplikacji automatycznie zastosuje wszystkie migracje pozostawione w projekcie.

Lista migracji:

```powershell
dotnet ef migrations list --project RentalCompany/Data
```

Jeśli ostatnia migracja została już zastosowana, najpierw następuje cofnięcie bazy do poprzedniej migracji, a następnie usunięcie ostatniej migracji z projektu:

```powershell
dotnet ef database update NazwaPoprzedniejMigracji --project RentalCompany/Data
dotnet ef migrations remove --project RentalCompany/Data
```

`NazwaPoprzedniejMigracji` oznacza migrację, która ma pozostać w bazie. Samo `database update` cofa nowsze migracje przez ich metody `Down()`, pozostawiając pliki w projekcie. Cofnięcie może usunąć dane z wycofywanych tabel lub kolumn.

Wycofanie pierwszej migracji wymaga celu `0`, czyli stanu sprzed wszystkich migracji:

```powershell
dotnet ef database update 0 --project RentalCompany/Data
dotnet ef migrations remove --project RentalCompany/Data
```

W przypadku `InitialIdentity` oznacza to usunięcie tabel Identity i sesji wraz z ich danymi. `migrations remove` zawsze usuwa tylko ostatnią migrację. Po jej usunięciu można poprawić modele i wygenerować nową migrację. Szczegóły: [zarządzanie migracjami EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing#remove-a-migration).

## Testy

```powershell
dotnet test RentalCompany/RentalCompany.sln
```

Testy sprawdzają zależności App/Core/Data oraz brak typów Data i EF Core w publicznych kontraktach Core. Nie wymagają uruchomionej bazy ani Dockera.
