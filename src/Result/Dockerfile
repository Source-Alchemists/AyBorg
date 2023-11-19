FROM mcr.microsoft.com/dotnet/sdk:7.0 AS BUILD
COPY ["./src", "/src"]
WORKDIR /src/Result
RUN dotnet publish -c release -o ../../artifacts

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS RUNTIME
EXPOSE 6003:6003
LABEL authors="Levent Tasdemir"
ARG USERNAME=ayborg
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Create the user
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    #
    # [Optional] Add sudo support. Omit if you don't need to install software after connecting.
    && apt-get update \
    && apt-get install -y sudo \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

COPY --from=BUILD /artifacts ./bin/AyBorg
RUN chown -cR $USERNAME /bin/AyBorg
WORKDIR /bin/AyBorg
USER $USERNAME
ENV "ASPNETCORE_URLS"=""
ENV "Kestrel:Endpoints:gRPC:Url"="http://*:6003"
ENV "AyBorg:Service:Url"="http://analytics:6003"
ENV "AyBorg:Gateway:Url"="http://gateway:6000"
ENTRYPOINT ["./AyBorg.Result"]
