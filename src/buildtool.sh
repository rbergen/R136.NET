#/bin/bash

BUILDTOOL_PATH=R136.BuildTool/bin/Debug/net5.0/R136.BuildTool
START_DIRECTORY=$(pwd)

cd $(dirname "${BASH_SOURCE[0]}")

if [[ -x ${BUILDTOOL_PATH} ]]; then
        ${BUILDTOOL_PATH} "$@" ./conversions.json
else
        echo "BuildTool not available at ${BUILDTOOL_PATH}, skipping JSON conversion"
fi

cd ${START_DIRECTORY}