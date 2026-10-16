Add-Type -AssemblyName System.Drawing
$size = 256
$bitmap = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$graphics.Clear([System.Drawing.Color]::Transparent)
$background = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 23, 18, 27))
$accent = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 185, 75, 255), 12)
$white = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$graphics.FillEllipse($background, 12, 12, 232, 232)
$graphics.DrawEllipse($accent, 12, 12, 232, 232)
$font = New-Object System.Drawing.Font("Segoe UI", 145, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$format = New-Object System.Drawing.StringFormat
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center
$graphics.DrawString("N", $font, $white, (New-Object System.Drawing.RectangleF(18, 18, 220, 220)), $format)
$pngStream = New-Object System.IO.MemoryStream
$bitmap.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
$png = $pngStream.ToArray()
$iconStream = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($iconStream)
$writer.Write([UInt16]0); $writer.Write([UInt16]1); $writer.Write([UInt16]1)
$writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([byte]0)
$writer.Write([UInt16]1); $writer.Write([UInt16]32); $writer.Write([UInt32]$png.Length); $writer.Write([UInt32]22)
$writer.Write($png)
[System.IO.File]::WriteAllBytes((Join-Path $PSScriptRoot "noxy.ico"), $iconStream.ToArray())
$writer.Dispose(); $iconStream.Dispose(); $pngStream.Dispose(); $font.Dispose(); $white.Dispose(); $accent.Dispose(); $background.Dispose(); $graphics.Dispose(); $bitmap.Dispose()

