FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /app

# copy all files from solution
COPY ./ ./

# Build used referenced projects
RUN dotnet build -c Release CrdInfo

# Publish to 'out' directory single main project
RUN dotnet publish KubernetesController1/KubernetesController1.csproj -p:PublishSingleFile=true -r alpine-x64 -c Release --self-contained true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true  -o out

# Create final repository
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine3.14-amd64 AS runtime
WORKDIR /app
LABEL maintainer="az"
RUN addgroup az && adduser -D -G az az
RUN chown -R az:az /app
USER az
# copy published files in destination folder
COPY --from=build /app/out ./
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
	DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["./KubernetesController1"]

# docker build -t sbraer/kubernetescontroller1 .
# docker push sbraer/kubernetescontroller1
