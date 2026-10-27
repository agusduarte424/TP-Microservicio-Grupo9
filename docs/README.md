# TP Microservicios - Grupo 9

E-commerce armado con 5 microservicios en .NET 10.

| Servicio | Puerto |
|---|---|
| Products.API | 5001 |
| Users.API | 5002 |
| Orders.API | 5003 |
| Cart.API | 5004 |
| Notifications.API | 5005 |

## Cómo correrlo
1. Instalar el SDK de .NET 10.
2. En la raíz del repo: `dotnet build ECommerce.slnx`
3. Cada servicio se levanta con `dotnet run --project src/<Servicio>.API`

## Integrantes
- Agus: Products
- Pili: Users
- Rama: Orders