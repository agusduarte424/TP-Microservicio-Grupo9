# Preguntas para la defensa

## F1 - Esqueleto
1. ¿Por qué cada servicio tiene su propio puerto?
   Porque los 5 corren al mismo tiempo en la misma compu, y dos programas no pueden escuchar en el mismo puerto.
2. ¿Por qué cada servicio tiene su propia base de datos?
   Para que sean independientes: si uno se cae o cambia su tabla, no rompe a los demás.
3. ¿Para qué sirve el .gitignore?
   Para no subir archivos que se generan solos (bin, obj), configuraciones personales del editor, las bases locales ni los logs.