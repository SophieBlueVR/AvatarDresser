#!/usr/bin/bash
#
# Generate a meta file

UUID=$1
if [ -z "$UUID" ]; then
    UUID=$(cat /proc/sys/kernel/random/uuid | sed 's/-//g')
fi

cat <<END
fileFormatVersion: 2
guid: ${UUID}
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
END
