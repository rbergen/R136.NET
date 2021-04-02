#/bin/bash

BUILDTOOL_PUBLISH_PATH=R136.BuildTool/bin/Debug/net5.0/publish/R136.BuildTool
BUILDTOOL_BUILD_PATH=R136.BuildTool/bin/Debug/net5.0/R136.BuildTool
START_DIRECTORY=$(pwd)

cd $(dirname "${BASH_SOURCE[0]}")

unset BUILDTOOL_PATH

if [[ -x ${BUILDTOOL_PUBLISH_PATH} ]]; then
  echo "BuildTool found at publish path"
  BUILDTOOL_PATH = ${BUILDTOOL_PUBLISH_PATH}
elif [[ -x ${BUILDTOOL_BUILD_PATH} ]]; then
  echo "BuildTool found at build path"
  BUILDTOOL_PATH = ${BUILDTOOL_BUILD_PATH}
fi

if [[ -n ${BUILDTOOL_PATH} ]]; then
  ${BUILDTOOL_PATH} "$@" ./conversions.json
else
  echo "BuildTool not available at ${BUILDTOOL_PUBLISH_PATH} or ${BUILDTOOL_BUILD_PATH}, skipping JSON conversion"
fi

cd ${START_DIRECTORY}