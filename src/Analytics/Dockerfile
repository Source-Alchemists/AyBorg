FROM mcr.microsoft.com/dotnet/sdk:7.0 as BUILD
COPY [".", "/src"]
WORKDIR /src/src/Analytics
RUN dotnet publish -c release -o ../../../artifacts

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS RUNTIME
EXPOSE 6001:6001
LABEL authors="Levent Tasdemir"
COPY --from=BUILD /artifacts ./bin/AyBorg
WORKDIR /bin/AyBorg
ENV "ASPNETCORE_URLS"=""
ENV "Kestrel:Endpoints:gRPC:Url"="http://*:6001"
ENV "AyBorg:Service:Url"="http://analytics-1:6001"
ENV "AyBorg:Gateway:Url"="http://gateway-1:6000"
ENTRYPOINT ["dotnet", "AyBorg.Analytics.dll"]
