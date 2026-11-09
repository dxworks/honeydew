FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Honeydew/Honeydew.csproj", "Honeydew/"]
COPY ["Honeydew.DesignSmellsDetection/Honeydew.DesignSmellsDetection.csproj", "Honeydew.DesignSmellsDetection/"]
COPY ["Honeydew.Extractors/Honeydew.Extractors.csproj", "Honeydew.Extractors/"]
COPY ["Honeydew.Models/Honeydew.Models.csproj", "Honeydew.Models/"]
COPY ["DxWorks.ScriptBee.Plugins.Honeydew/DxWorks.ScriptBee.Plugins.Honeydew.csproj", "DxWorks.ScriptBee.Plugins.Honeydew/"]
COPY ["Honeydew.Models.CSharp/Honeydew.Models.CSharp.csproj", "Honeydew.Models.CSharp/"]
COPY ["Honeydew.Models.VisualBasic/Honeydew.Models.VisualBasic.csproj", "Honeydew.Models.VisualBasic/"]
COPY ["Honeydew.Extractors.CSharp/Honeydew.Extractors.CSharp.csproj", "Honeydew.Extractors.CSharp/"]
COPY ["Honeydew.Extractors.Dotnet/Honeydew.Extractors.Dotnet.csproj", "Honeydew.Extractors.Dotnet/"]
COPY ["Honeydew.Extractors.VisualBasic/Honeydew.Extractors.VisualBasic.csproj", "Honeydew.Extractors.VisualBasic/"]
RUN dotnet restore "Honeydew/Honeydew.csproj"
COPY . .
WORKDIR "/src/Honeydew"
RUN dotnet build "Honeydew.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Honeydew.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS final
# The base image can lag behind Ubuntu's security fixes (it shipped apt 2.8.3, CVE-2026-109518),
# so apply the pending package updates in the image that is actually shipped.
RUN apt-get update && \
    apt-get upgrade -y --no-install-recommends && \
    rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Honeydew.dll"]
