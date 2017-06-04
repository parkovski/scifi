if (-not (Test-Path "Assets/ThirdParty/FacebookSDK")) {
    Write-Output "Restoring Facebook SDK..."
    Expand-Archive "SDKs/FacebookSDK.zip" -DestinationPath "Assets/ThirdParty/"
}

if (-not (Test-Path "Assets/ThirdParty/WebSocketSharp")) {
    Write-Output "Restoring WebSocketSharp..."
    Expand-Archive "SDKs/WebSocketSharp.zip" -DestinationPath "Assets/ThirdParty/"
}