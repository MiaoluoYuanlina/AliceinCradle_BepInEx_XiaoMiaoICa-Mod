# 获取进程信息
$process = Get-Process -Name "AliceInCradle" -ErrorAction SilentlyContinue

if ($process) {
    # 获取进程路径
    $processPath = $process.Path
    Write-Output "进程路径: $processPath"

    # 结束进程
    Stop-Process -Id $process.Id -Force
    Write-Output "进程已结束"

    # 重启进程
    Start-Process -FilePath $processPath
    Write-Output "进程已重启"
} else {
    Write-Output "未找到名为 'AliceInCradle.exe' 的进程"
}
 