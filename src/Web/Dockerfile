FROM mcr.microsoft.com/dotnet/sdk:7.0 as BUILD
COPY [".", "/src"]
WORKDIR /src/src/Web
RUN dotnet publish -c release -o ../../../artifacts

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS RUNTIME
EXPOSE 6010:6010
EXPOSE 6011:6011
LABEL authors="Levent Tasdemir"
COPY --from=BUILD /artifacts ./bin/AyBorg
WORKDIR /bin/AyBorg
ENV "ASPNETCORE_URLS"=""
ENV "Kestrel:Endpoints:Http:Url"="http://*:6010"
ENV "Kestrel:Endpoints:Https:Url"="http://*:6011"
ENV "AyBorg:Service:Url"="https://web-1:6011"
ENV "AyBorg:Gateway:Url"="http://gateway-1:6000"
ENTRYPOINT ["dotnet", "AyBorg.Web.dll"]
