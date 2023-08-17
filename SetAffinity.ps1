$processName = $args[0]
$coreCount = [long] ($args[1])
$affinity = [long] (1 * ([Math]::Pow(2, $coreCount) - 1))
$ps = Get-Process
foreach ($p in $ps) {
    if ($p.ProcessName -eq $processName) {
        echo "Setting CPU affinity for $($p.ProcessName) to $([Convert]::ToString($affinity,2))..."
        $p.PriorityClass = "High"
        $p.ProcessorAffinity = $affinity
        # $p | Format-List -Property Id,ProcessName,CPU,PriorityClass,ProcessorAffinity
    }
}
