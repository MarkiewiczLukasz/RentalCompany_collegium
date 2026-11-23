FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY RentalCompany/Data/Data.csproj RentalCompany/Data/
COPY RentalCompany/Core/Core.csproj RentalCompany/Core/
COPY RentalCompany/RentalCompany/RentalCompany.csproj RentalCompany/RentalCompany/
RUN dotnet restore RentalCompany/RentalCompany/RentalCompany.csproj
COPY RentalCompany/ RentalCompany/
RUN dotnet publish RentalCompany/RentalCompany/RentalCompany.csproj -c Release --no-restore -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R app:app /home/app/.aspnet
COPY --from=build /out .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "RentalCompany.dll"]
