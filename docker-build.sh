#!/bin/sh
set -e

basepath=$(cd `dirname $0`; pwd)

docker run --rm \
-v ${basepath}:/dotnet-build \
eswork/dotnet-build \
sh -c "chmod 755 /dotnet-build/build.sh && /dotnet-build/build.sh"

