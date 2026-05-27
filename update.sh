#!/bin/bash
set -e

ConnectionStrings__Default="User ID=postgres;Password=postgres;Host=192.168.65.4;Port=5432;Database=knowledge_hub;Pooling=true;" \
  dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Api
