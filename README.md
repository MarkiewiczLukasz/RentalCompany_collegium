# Wypożyczalnia sprzętu elektronicznego

Aplikacja webowa do obsługi wypożyczalni sprzętu elektronicznego — od rezerwacji na wybrany termin, przez wydanie i zwrot, po rozliczenie szkód i naprawy.

## Opis funkcjonalny

Zakres projektu obejmuje:

- **Klient (User):** przeglądanie katalogu i dostępności, wybór dat oraz egzemplarzy, tworzenie rezerwacji, podgląd własnych zamówień i obciążeń za szkody.
- **Administrator (Admin):** zarządzanie modelami i egzemplarzami sprzętu, wydawanie i przyjmowanie zwrotów, ocena stanu sprzętu oraz naliczanie uzasadnionych opłat za uszkodzenia.
- **Serwis i raporty:** planowanie przeglądów, obsługa napraw, wyłączanie sprzętu z dostępności oraz zestawienia wypożyczeń i rozliczeń.
- **Płatności testowe:** symulacja opłacenia zamówienia, odmowy płatności, zwrotu wpłaty i dopłaty za szkody, bez połączenia z bankiem.
- **Konta i uprawnienia:** logowanie oraz role User i Admin; klient ma dostęp do własnych zamówień, a administrator do obsługi wypożyczalni.

## Uruchomienie

Wymagany jest działający **Docker Desktop z kontenerami Linux**. W obu wariantach baza danych działa w Dockerze. Polecenia należy wykonywać z głównego katalogu repozytorium.

Konfiguracja jest gotowa do użycia. Przy pierwszym uruchomieniu aplikacja automatycznie przygotowuje bazę i konta demonstracyjne.

### Visual Studio

Wymagane Visual Studio z obsługą ASP.NET Core i .NET 8.

1. Otwarcie rozwiązania `RentalCompany/RentalCompany.sln`.
2. Uruchomienie bazy:

   ```powershell
   docker compose up -d --wait db
   ```

3. Uruchomienie aplikacji przez **F5** lub **Ctrl+F5**.

Aplikacja: [http://localhost:5008](http://localhost:5008).

Projekt startowy to **RentalCompany**, profil **http**. Wybór jest jednorazowy, jeśli Visual Studio nie wskazuje ich automatycznie. Przy kolejnych uruchomieniach wystarczy F5, o ile baza jest włączona.

### Docker — aplikacja i baza

```powershell
docker compose up --build -d
```

Aplikacja: [http://localhost:8080](http://localhost:8080).

Zatrzymanie kontenerów:

```powershell
docker compose stop
```

### Konta demonstracyjne

| Rola | E-mail | Hasło |
| --- | --- | --- |
| Admin | `admin@rental.local` | `Admin123456!` |
| User | `user@rental.local` | `User1234567!` |

Informacje o architekturze, migracjach i pracy z kodem: [dokumentacja dla zespołu](docs/DEVELOPMENT.md).
