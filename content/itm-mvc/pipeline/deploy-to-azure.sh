#!/bin/bash -ex

# These variables should be defined in the Builds environment variables.
# Retrieve them from an Azure Web App's Publishing Profile
# (.publishsettings).
# - AZURE_DEPLOY_URL (e.g.: https://webappname.scm.azurewebsites.net/)
# - AZURE_DEPLOY_USERNAME (e.g.: username)
# - AZURE_DEPLOY_PASSWORD

apt-get update --quiet
apt-get --yes install zip curl --quiet

PUBLISH_PATH=$1

PUBLISH_ZIP="publish.zip"

# Hack to clean unnecessary runtimes
rm $PUBLISH_PATH/runtimes/linux-x64 -Rf
rm $PUBLISH_PATH/runtimes/osx-x64 -Rf

cd $PUBLISH_PATH
zip -r ../$PUBLISH_ZIP .
cd ..

curl -H "Content-Type: application/json" -X POST -d '{"command":"rm site/wwwroot/web.config", "path":""}' "$AZURE_DEPLOY_URL/api/command" --user $AZURE_DEPLOY_USERNAME:$AZURE_DEPLOY_PASSWORD

curl -X PUT --data-binary @$PUBLISH_ZIP "$AZURE_DEPLOY_URL/api/zip/site/wwwroot/" --user $AZURE_DEPLOY_USERNAME:$AZURE_DEPLOY_PASSWORD

HIPCHAT_URL=`echo $AZURE_DEPLOY_URL | cut -c9-`
echo "Deployed to $HIPCHAT_URL"