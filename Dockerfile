# Imagem usada pela esteira de DAST (.github/workflows/dast.yml).
# Ambiente Staging: o Program.cs só ativa UseHttpsRedirection em Production, e a esteira
# acessa a aplicação por HTTP em localhost.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ProdutosAPI.csproj --configuration Release --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Staging \
    ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
# O banco SQLite e os logs são gravados em /app, então o usuário sem privilégios precisa de escrita nela.
RUN chown -R app:app /app
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProdutosAPI.dll"]
