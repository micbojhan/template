#!/bin/bash -ex

# These variables should be defined in the Builds environment variables.
# - AZURE_STORAGE_ACCOUNT (e.g.: itminds)
# - AZURE_STORAGE_ACCESS_KEY
# - AZURE_STORAGE_CONTAINERNAME (e.g.: fragtwebapp)


PUBLISH_PATH=$1
WEBDEPLOY_PATH=$2
APP_NAME=$3

BRANCH_NAME=`git rev-parse --abbrev-ref HEAD`
VERSION=${BRANCH_NAME:8}
PUBLISH_ZIP="$APP_NAME-$VERSION.zip"


# Install depdencies
curl -sL https://deb.nodesource.com/setup_6.x | bash - > /dev/null 2>&1
apt-get --yes install zip nodejs --quiet
npm install -g azure-cli --silent

# Package app to zip file
mkdir $WEBDEPLOY_PATH/Content
cp $PUBLISH_PATH $WEBDEPLOY_PATH/Content/website -R

# Hack to clean unnecessary runtimes
rm $WEBDEPLOY_PATH/Content/website/runtimes/linux-x64 -Rf
rm $WEBDEPLOY_PATH/Content/website/runtimes/osx-x64 -Rf


cd $WEBDEPLOY_PATH
zip -r ../../$PUBLISH_ZIP * # TODO Don't assume output directory for zip
cd -

# Upload file to storage container
azure telemetry --disable
azure storage blob upload $PUBLISH_ZIP $AZURE_STORAGE_CONTAINERNAME $PUBLISH_ZIP
echo Package published to https://$AZURE_STORAGE_ACCOUNT.blob.core.windows.net/$AZURE_STORAGE_CONTAINERNAME/$PUBLISH_ZIP

bash ./pipeline/hipchat.sh -v v2 -i "Package published to $AZURE_STORAGE_ACCOUNT.blob.core.windows.net/$AZURE_STORAGE_CONTAINERNAME/$PUBLISH_ZIP"