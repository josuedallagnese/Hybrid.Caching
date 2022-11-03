Project was inspired on [EasyCaching](https://github.com/dotnetcore/EasyCaching), however much more simple and stupid!
Hybrid Caching composition by Redis and Memory, synchronizing with Dapr Pub/Sub. 

You should install Dapr cli to run this sample: (https://docs.dapr.io/getting-started/)

**This project is in progress...**

## How to:
The purposes:
  - First level: redis distributed cache
  - Second level: memory cache
  - Synchronism between cache instances and fast memory cache access

Synchronism:
  - Using [Dapr] (https://github.com/dapr/dapr) Pub/Sub
  
 ## Run sample project

1. FIRST:
```bash
Visit [this](https://docs.dapr.io/developing-applications/building-blocks/pubsub/) link for more information about 
Dapr and Pub-Sub.
```

1. Navigate to the directory and install dependencies: 

```bash
cd ./Hybrid.Caching.Web
dotnet restore
dotnet build
```

2. Run the Dotnet sample app with Dapr: 

```bash
dapr run --app-id Hybrid.Caching.Web --components-path ../components/ --app-port 7002 -- dotnet run --project .
```

3. Run the Dapr dashboard:

```bash
dapr dashboard
```
