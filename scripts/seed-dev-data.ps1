$baseUrl = "http://localhost:5070"
$password = "Password1"

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null,
        [string]$Token = $null
    )
    $headers = @{ "Content-Type" = "application/json" }
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }
    $params = @{
        Uri = "$baseUrl$Path"
        Method = $Method
        Headers = $headers
    }
    if ($Body) { $params.Body = ($Body | ConvertTo-Json -Depth 5) }
    return Invoke-RestMethod @params
}

Write-Host "Registering host@gmail.com ..."
try {
    Invoke-Api -Method POST -Path "/auth/register" -Body @{
        email = "host@gmail.com"
        password = $password
        role = "Host"
    } | Out-Null
    Write-Host "Host registered."
} catch {
    Write-Host "Host may already exist, continuing."
}

Write-Host "Registering client@gmail.com ..."
try {
    Invoke-Api -Method POST -Path "/auth/register" -Body @{
        email = "client@gmail.com"
        password = $password
        role = "Client"
    } | Out-Null
    Write-Host "Client registered."
} catch {
    Write-Host "Client may already exist, continuing."
}

$hostAuth = Invoke-Api -Method POST -Path "/auth/login" -Body @{
    email = "host@gmail.com"
    password = $password
}
$hostToken = $hostAuth.accessToken

$apartments = @(
    @{
        name = "Downtown Loft"
        description = "Bright loft with city views near the center."
        pricePerNight = 120
        guestCount = 2
        amenities = @("LargeBed", "Shower", "Microwave")
        metadata = @{ channel = "direct"; tier = "premium" }
    },
    @{
        name = "Garden Studio"
        description = "Quiet studio with patio access."
        pricePerNight = 85
        guestCount = 2
        amenities = @("LargeBed", "Bath")
        metadata = @{ channel = "direct" }
    },
    @{
        name = "Family Apartment"
        description = "Spacious flat for families, two bedrooms."
        pricePerNight = 150
        guestCount = 4
        amenities = @("LargeBed", "Bath", "Shower", "Microwave")
        metadata = @{ listingCode = "FAM-01" }
    }
)

$apartmentIds = @()
foreach ($apt in $apartments) {
    $created = Invoke-Api -Method POST -Path "/host/apartments" -Body $apt -Token $hostToken
    $apartmentIds += $created.id
    Write-Host "Created apartment: $($created.name)"
}

$clientAuth = Invoke-Api -Method POST -Path "/auth/login" -Body @{
    email = "client@gmail.com"
    password = $password
}
$clientToken = $clientAuth.accessToken

$start = (Get-Date).ToUniversalTime().AddDays(14).ToString("o")
$end = (Get-Date).ToUniversalTime().AddDays(16).ToString("o")
$booking = Invoke-Api -Method POST -Path "/bookings" -Body @{
    apartmentId = $apartmentIds[0]
    start = $start
    end = $end
} -Token $clientToken
Write-Host "Created booking on Downtown Loft: $($booking.id)"

$start2 = (Get-Date).ToUniversalTime().AddDays(20).ToString("o")
$end2 = (Get-Date).ToUniversalTime().AddDays(22).ToString("o")
$booking2 = Invoke-Api -Method POST -Path "/bookings" -Body @{
    apartmentId = $apartmentIds[1]
    start = $start2
    end = $end2
} -Token $clientToken
Write-Host "Created booking on Garden Studio: $($booking2.id)"

Write-Host ""
Write-Host "Seed complete."
Write-Host "  Host:   host@gmail.com / $password"
Write-Host "  Client: client@gmail.com / $password"
Write-Host "  Admin:  admin@bookingsystem.local / Admin123!"
