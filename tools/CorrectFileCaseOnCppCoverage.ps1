Param(
  [string]$xmlFile
)
	
function Get-PathCanonicalCase {
    param($path)

    $newPath = (Resolve-Path $path).Path
    $parent = Split-Path $newPath

    if($parent) {
        $leaf = Split-Path $newPath -Leaf

        (Get-ChildItem $parent| Where-Object{$_.Name -eq $leaf}).FullName
    } else {
        (Get-PSDrive ($newPath -split ':')[0]).Root
    }
}

[xml]$doc = gc $xmlFile 

$nodes = $doc.SelectNodes("//class")

foreach ($node in $nodes) {
  $path = Get-PathCanonicalCase("/$($node.attributes['filename'].value)") 
  $node.attributes['filename'].value = $path.Split(':').get(1)
}

$doc.Save($xmlFile)