#!/bin/bash
set -e

dotnet ef migrations add "$1" \
--project src/Infrastructure \
--startup-project src/Api
