#!/usr/bin/env bash
set -e
basepath=$(cd `dirname $0`; pwd)
artifacts=${basepath}/artifacts

if [[ -d ${artifacts} ]]; then
   rm -rf ${artifacts}
fi

mkdir -p ${artifacts}

dotnet pack -c release ${basepath}/src/Es.Throttle -o ${basepath}/artifacts
dotnet pack -c release ${basepath}/src/Es.Throttle.Mvc -o ${basepath}/artifacts