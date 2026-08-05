param(
    [string]$Dir = $PSScriptRoot
)

Add-Type -AssemblyName System.Drawing

function New-MagnifierIcon([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $blue = [System.Drawing.Color]::FromArgb(255, 0, 120, 212)
    $white = [System.Drawing.Color]::White

    $lens = $size * 0.625
    $lensX = $size * 0.188
    $lensY = $size * 0.125
    $brush = New-Object System.Drawing.SolidBrush($blue)
    $g.FillEllipse($brush, $lensX, $lensY, $lens, $lens)

    $pen = New-Object System.Drawing.Pen($blue, [float]($size * 0.109))
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($pen, $lensX + $lens * 0.54, $lensY + $lens * 0.54, $size * 0.875, $size * 0.812)

    $fontSize = [float]($size * 0.36)
    $font = New-Object System.Drawing.Font('Segoe UI', $fontSize, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $textBrush = New-Object System.Drawing.SolidBrush($white)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    $rx = [double]$lensX
    $ry = [double]($lensY + $size * 0.02)
    $rw = [double]$lens
    $rh = [double]($lens * 0.92)
    $rect = New-Object System.Drawing.RectangleF -ArgumentList @($rx, $ry, $rw, $rh)
    $g.DrawString('Aa', $font, $textBrush, $rect, $format)

    $format.Dispose()
    $textBrush.Dispose()
    $font.Dispose()
    $pen.Dispose()
    $brush.Dispose()
    $g.Dispose()
    return $bmp
}

function New-WizardImage {
    $w = 164; $h = 314
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

    $top = [System.Drawing.Color]::FromArgb(255, 222, 239, 252)
    $bottom = [System.Drawing.Color]::White
    $grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Point(0, 0)),
        (New-Object System.Drawing.Point(0, $h)),
        $top, $bottom)
    $g.FillRectangle($grad, 0, 0, $w, $h)

    $icon = New-MagnifierIcon 128
    $g.DrawImage($icon, ($w - 128) / 2, ($h - 128) / 2, 128, 128)

    $grad.Dispose()
    $icon.Dispose()
    $g.Dispose()
    return $bmp
}

function New-WizardSmallImage {
    $w = 55; $h = 58
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::White)

    $icon = New-MagnifierIcon 42
    $g.DrawImage($icon, ($w - 42) / 2, ($h - 42) / 2, 42, 42)
    $icon.Dispose()
    $g.Dispose()
    return $bmp
}

$wiz = New-WizardImage
$wiz.Save((Join-Path $Dir 'WizardImage.bmp'), [System.Drawing.Imaging.ImageFormat]::Bmp)
$wiz.Dispose()

$small = New-WizardSmallImage
$small.Save((Join-Path $Dir 'WizardSmallImage.bmp'), [System.Drawing.Imaging.ImageFormat]::Bmp)
$small.Dispose()

Write-Output "Wrote WizardImage.bmp and WizardSmallImage.bmp in $Dir"
