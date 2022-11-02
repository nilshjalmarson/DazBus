Import-Module ~/Dev/Me/DazBus/DazBus.Powershell/bin/Debug/net5.0/publish/DazBus.Powershell.dll -Force
$result = Get-DazBusDlqCount -ConnectionString 'Endpoint=sb://nh-sb-sample.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=iYSnMspVj5oQiTxSxFMWO7kvNTRwZL7VDNljM/NDVdI=' -TopicName 'basictopic' -SubscriptionName 'Subscription1'
Write-Output "1. Dead letter queue message count: $result"
#$result = Get-DazBusDlqCount -Namespace 'riksbyggengraphservicebusat.servicebus.windows.net' -TopicName 'boardeaser-callback-received' -SubscriptionName 'callbacks'
#Write-Output "2. Dead letter queue message count: $result"