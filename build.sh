#!/usr/bin/env bash
set -e
basepath=$(cd `dirname $0`; pwd)
artifacts=${basepath}/artifacts

if [[ -d ${artifacts} ]]; then
   rm -rf ${artifacts}
fi

mkdir -p ${artifacts}

dotnet restore ${basepath}/src/Es.Throttle
dotnet restore ${basepath}/src/Es.Throttle.Mvc

dotnet build ${basepath}/src/Es.Throttle -f netstandard1.3 -c Release -o ${artifacts}/netstandard1.3
dotnet build ${basepath}/src/Es.Throttle.Mvc -f netstandard1.6 -c Release -o ${artifacts}/netstandard1.6
dotnet build ${basepath}/src/Es.Throttle.Mvc -f netstandard2.0 -c Release -o ${artifacts}/netstandard2.0

# linux not support .netframework
sed "s/netstandard1.3;netstandard2.0;net46/netstandard1.3;netstandard2.0/g"   \
-i ${basepath}/src/Es.Throttle/Es.Throttle.csproj

dotnet pack -c release ${basepath}/src/Es.Throttle -o ${basepath}/artifacts
dotnet pack -c release ${basepath}/src/Es.Throttle.Mvc -o ${basepath}/artifacts