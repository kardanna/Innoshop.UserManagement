FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Innoshop.UserManagement/ Innoshop.UserManagement/
COPY Innoshop.Contracts/ Innoshop.Contracts/
RUN dotnet restore Innoshop.UserManagement/src/UserManagement.API/UserManagement.API.csproj
RUN dotnet publish Innoshop.UserManagement/src/UserManagement.API/UserManagement.API.csproj -c Release -o /app/publish
RUN ls -l /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "UserManagement.API.dll" ]