$file = "src\components\admin\management\PoliciesTab.tsx"
$content = Get-Content $file -Raw

# Insert helper function before PoliciesTab component
$helper = @"
function formatFee(fee?: number) {
  if (!fee || fee === 0) return "Free";
  return `${fee.toLocaleString("vi-VN")} VNĐ`;
}

export default function PoliciesTab() {
"@
$content = $content -replace "export default function PoliciesTab\(\) \{", $helper

# Replace hardcoded formats
$content = $content -replace '\{item\.earlyCheckInFee\?\.toLocaleString\("vi-VN"\)\} VND', '{formatFee(item.earlyCheckInFee)}'
$content = $content -replace '\{item\.lateCheckOutFee\?\.toLocaleString\("vi-VN"\)\} VND', '{formatFee(item.lateCheckOutFee)}'
$content = $content -replace '\{item\.extraBedFee\?\.toLocaleString\("vi-VN"\)\} VND', '{formatFee(item.extraBedFee)}'
$content = $content -replace '\{item\.petFee\?\.toLocaleString\("vi-VN"\)\} VND', '{formatFee(item.petFee)}'

# Change VND to VNĐ in Labels
$content = $content -replace '\(VND\)', '(VNĐ)'

Set-Content $file -Value $content -Encoding UTF8
