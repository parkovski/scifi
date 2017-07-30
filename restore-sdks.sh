#!/bin/bash

if [[ ! -e "Assets/ThirdParty/FacebookSDK" ]]
then
  echo "Restoring Facebook SDK..."
  unzip SDKs/FacebookSDK.zip -d Assets/ThirdParty/ >/dev/null
fi

if [[ ! -e "Assets/ThirdParty/WebSocketSharp" ]]
then
  echo "Restoring WebSocketSharp..."
  unzip SDKs/WebSocketSharp.zip -d Assets/ThirdParty/ >/dev/null
fi