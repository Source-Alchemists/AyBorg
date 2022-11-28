FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy

########### START -- environment variables ###########
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV XDG_CONFIG_HOME="/workspace/ApplicationData"
ENV TZ=Europe/Berlin
########### END -- environment variables ###########



########### START -- build arguments ###########
ARG USERNAME=dev
ARG USER_UID=1000
ARG USER_GID=$USER_UID
ARG POSTGRES_PW="'devcontainer'"
ARG DEBIAN_FRONTEND=noninteractive
########### END -- build arguments ###########


########### START -- install general utility ###########
RUN apt-get update && \
    apt-get install -y software-properties-common && \
    rm -rf /var/lib/apt/lists/*
RUN mkdir -p $XDG_CONFIG_HOME
########### END -- install general utility ###########


########### START -- configure dev user ###########
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    #
    # [Optional] Add sudo support. Omit if you don't need to install software after connecting.
    && apt-get update \
    && apt-get install -y sudo \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

RUN  apt-get update \
    && apt-get install -y wget git curl gnupg \
    && rm -rf /var/lib/apt/lists/*
########### END -- configure dev user ###########



########### START -- install mosquitto as provider ###########
# default port 1883
RUN apt-add-repository ppa:mosquitto-dev/mosquitto-ppa
RUN apt-get update
RUN apt-get install mosquitto -y
########### END -- install mosquitto as provider ###########



########### START -- install posgres as db ###########
# default port for postgres 5432 for pgadmin as example
RUN apt-get install -y tzdata
# Set the locale
RUN apt-get -y install locales
RUN sed -i '/en_US.UTF-8/s/^# //g' /etc/locale.gen && \
    locale-gen
ENV LANG en_US.UTF-8  
ENV LANGUAGE en_US:en  
ENV LC_ALL en_US.UTF-8  
RUN apt-get update
RUN apt-get install postgresql postgresql-contrib -y
# user postgres installed by postgre package
USER postgres
# Create a PostgreSQL role named ``docker`` with ``docker`` as the password
RUN    /etc/init.d/postgresql start &&\
    psql --command "CREATE USER docker WITH SUPERUSER PASSWORD $POSTGRES_PW;"
USER root
########### END -- install posgres as db ###########

USER $USERNAME
RUN dotnet dev-certs https --trust
