# ��ȡ������Ϣ
$process = Get-Process -Name "AliceInCradle" -ErrorAction SilentlyContinue

if ($process) {
    # ��ȡ����·��
    $processPath = $process.Path
    Write-Output "����·��: $processPath"

    # ��������
    Stop-Process -Id $process.Id -Force
    Write-Output "�����ѽ���"

    # ��������
    Start-Process -FilePath $processPath
    Write-Output "����������"
} else {
    Write-Output "δ�ҵ���Ϊ 'AliceInCradle.exe' �Ľ���"
}
 