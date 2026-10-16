# Lista de Verificación para Desarrollo de Nuevas Funcionalidades

Aplica los elementos pertinentes a cada funcionalidad nueva o cambio. Esta lista es deliberadamente ligera y no requiere crear ramas, hacer commits, pushes, worktrees ni actualizar estados externos.

## Requisitos y Diseño

- [ ] Lee la solicitud o el ticket de Jira e identifica los criterios de aceptación explícitos.
- [ ] Busca código existente que ya proporcione el comportamiento o patrón necesario.
- [ ] Mantén el cambio dentro de la capa apropiada de Clean Architecture.
- [ ] Identifica los impactos en contratos, persistencia, autorización y manejo de errores antes de editar.

## Implementación

- [ ] Conserva la corrección de referencias anulables y los patrones existentes de inyección de dependencias.
- [ ] Valida la entrada externa en el límite de la aplicación.
- [ ] Mantén las reglas de negocio fuera de controladores, código de persistencia y servicios específicos de transporte.
- [ ] Actualiza los contratos de API o gRPC cuando cambie el comportamiento observable externamente.
- [ ] Usa acceso a datos parametrizado y evita consultas o llamadas remotas dentro de ciclos.

## Pruebas y Validación

- [ ] Agrega o actualiza pruebas xUnit enfocadas en el proyecto `*.Tests` correspondiente.
- [ ] Cubre el comportamiento exitoso y las rutas relevantes de validación, autorización o error.
- [ ] Ejecuta el comando `dotnet test` aplicable más acotado.
- [ ] Compila el proyecto afectado o la solución cuando cambien contratos o varios proyectos.
- [ ] Revisa el diff final contra los criterios de aceptación y los estándares de seguridad.
