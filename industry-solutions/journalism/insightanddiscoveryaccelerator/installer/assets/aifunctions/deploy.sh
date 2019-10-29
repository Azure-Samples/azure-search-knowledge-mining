#!/bin/bash

# ----------------------
# KUDU Deployment Script
# Version: 1.0.17
# ----------------------

# Helpers
# -------

exitWithMessageOnError () {
  if [ ! $? -eq 0 ]; then
    echo "An error has occurred during web site deployment."
    echo $1
    exit 1
  fi
}

# Prerequisites
# -------------

# Verify node.js installed
hash node 2>/dev/null
exitWithMessageOnError "Missing node.js executable, please install node.js, if already installed make sure it can be reached from current environment."

# Setup
# -----

SCRIPT_DIR="${BASH_SOURCE[0]%\\*}"
SCRIPT_DIR="${SCRIPT_DIR%/*}"
ARTIFACTS=$SCRIPT_DIR/../artifacts
KUDU_SYNC_CMD=${KUDU_SYNC_CMD//\"}

if [[ ! -n "$DEPLOYMENT_SOURCE" ]]; then
  DEPLOYMENT_SOURCE=$SCRIPT_DIR
fi

if [[ ! -n "$NEXT_MANIFEST_PATH" ]]; then
  NEXT_MANIFEST_PATH=$ARTIFACTS/manifest

  if [[ ! -n "$PREVIOUS_MANIFEST_PATH" ]]; then
    PREVIOUS_MANIFEST_PATH=$NEXT_MANIFEST_PATH
  fi
fi

if [[ ! -n "$DEPLOYMENT_TARGET" ]]; then
  DEPLOYMENT_TARGET=$ARTIFACTS/wwwroot
else
  KUDU_SERVICE=true
fi

if [[ ! -n "$KUDU_SYNC_CMD" ]]; then
  # Install kudu sync
  echo Installing Kudu Sync
  npm install kudusync -g --silent
  exitWithMessageOnError "npm failed"

  if [[ ! -n "$KUDU_SERVICE" ]]; then
    # In case we are running locally this is the correct location of kuduSync
    KUDU_SYNC_CMD=kuduSync
  else
    # In case we are running on kudu service this is the correct location of kuduSync
    KUDU_SYNC_CMD=$APPDATA/npm/node_modules/kuduSync/bin/kuduSync
  fi
fi

# Npm helper
# ------------

npmPath=`which npm 2> /dev/null`
if [ -z $npmPath ]
then
  NPM_CMD="node \"$NPM_JS_PATH\"" # on windows server there's only npm.cmd
else
  NPM_CMD=npm
fi

##################################################################################################################################
# Deployment
# ----------

echo Handling function App deployment.

RunNpm() {
    echo Restoring npm packages in $1

    pushd $1 > /dev/null
    eval $NPM_CMD install --production 2>&1
    exitWithMessageOnError "Npm Install Failed"
    popd > /dev/null
}

RestoreNpmPackages() {

    local lookup="package.json"
    if [ -e "$1/$lookup" ]
    then
      RunNpm $1
    fi

    for subDirectory in "$1"/*
    do
      if [ -d $subDirectory ] && [ -e "$subDirectory/$lookup" ]
      then
        RunNpm $subDirectory
      fi
    done
}

InstallFunctionExtensions() {
    echo Installing azure function extensions from nuget

    local lookup="extensions.csproj"
    if [ -e "$1/$lookup" ]
    then
      pushd $1 > /dev/null
      dotnet build -o bin
      exitWithMessageOnError "Function extensions installation failed"
      popd > /dev/null
    fi
}

DeployWithoutFuncPack() {
    echo Not using funcpack because SCM_USE_FUNCPACK is not set to 1
	echo $IN_PLACE_DEPLOYMENT

    # 1. Install function extensions
    InstallFunctionExtensions "$DEPLOYMENT_SOURCE"
	
    # 2. KuduSync
    if [[ "$IN_PLACE_DEPLOYMENT" -ne "1" ]]; then
      "$KUDU_SYNC_CMD" -v 50 -f "$DEPLOYMENT_SOURCE" -t "$DEPLOYMENT_TARGET" -n "$NEXT_MANIFEST_PATH" -p "$PREVIOUS_MANIFEST_PATH" -i ".git;.hg;.deployment;deploy.sh;obj"
      exitWithMessageOnError "Kudu Sync failed"
    fi
	
    # 3. Restore npm
    RestoreNpmPackages "$DEPLOYMENT_TARGET"	
}

DeployWithoutFuncPack
# TODO funcpack is not installed on linux machine

##################################################################################################################################
echo "Finished successfully."
