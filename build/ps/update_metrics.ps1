param (
    [string]$endpoint = 'http://localhost:8888/metrics/sample_metric/value',
    [string]$password = '',
    [string]$value = $(throw "-value is required.")
)
$username = 'user'

$url = new-object System.Uri($endpoint)
$web = [System.Net.WebRequest]::Create($url)
$web.Method = "POST"

$auth = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($username+":"+$password ))
$web.Headers.Add('Authorization', "Basic $auth" )

$command = '{"value": "' + $value + '" }'
$bytes = [System.Text.Encoding]::ASCII.GetBytes($command)
$web.ContentLength = $bytes.Length
$web.ContentType = "application/json"

$stream = $web.GetRequestStream()
$stream.Write($bytes,0,$bytes.Length)
$stream.close()

try
{
  $reader = New-Object System.IO.Streamreader -ArgumentList $web.GetResponse().GetResponseStream()
  $reader.ReadToEnd()
  $reader.Close()
}
catch [System.Exception]
{
  Write-Host "An error occurred: $($_.Exception.Message)"
}