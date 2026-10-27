# Arquitectura

Cada microservicio es un proyecto independiente, con su propia base SQLite.
Se comunican por HTTP usando las direcciones de la sección "Services" de su appsettings.json.

## Quién llama a quién
- Products -> Orders (antes de borrar un producto, revisa que no tenga pedidos)
- Orders -> Users y Products
- Cart -> Products
- Notifications -> Users
- Users no llama a nadie