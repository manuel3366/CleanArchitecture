---
name: dotnet-feature-implementation
description: Implementa historias y funcionalidades .NET/C# de este repositorio, desde los requisitos hasta las pruebas y la validación.
---

# Implementación de funcionalidades .NET/C#

Implementa la solicitud completa más pequeña que satisfaga los requisitos. Trabaja en el checkout actual y conserva cambios preexistentes. No crees ramas ni worktrees, no hagas commits o push y no actualices herramientas externas salvo que el usuario lo solicite explícitamente. No inicies agentes adicionales para esta tarea.

## Requisitos

1. Lee los requisitos y criterios de aceptación de la solicitud. Si se proporciona una clave de Jira, usa Jira MCP `jira_get_issue` antes de planificar y solicita como mínimo `summary`, `description`, `issuetype`, `priority`, `status` y etiquetas. Usa la descripción del issue y sus criterios de aceptación explícitos como fuente de verdad. No modifiques el issue ni consultes issues no relacionados salvo autorización explícita.
2. Antes de editar, confirma que se conoce el comportamiento observable esperado, los resultados o criterios de aceptación y las restricciones relevantes de error, autorización y compatibilidad. Inspecciona el código para resolver decisiones técnicas que ya estén cubiertas por patrones del repositorio.
3. Si falta información funcional necesaria, pregunta únicamente por los datos que faltan y espera respuesta antes de editar. No inventes criterios de aceptación ni conviertas detalles técnicos descubribles en preguntas al usuario.

## Implementación

1. Lee y aplica `.github/NEW_FEATURE_CHECKLIST.md`, `.github/steering/dotnet-development-standards.md` y `.github/steering/dotnet-security-standards.md`.
2. Inspecciona el código pertinente y reutiliza sus patrones, abstracciones, convenciones de errores y pruebas. Mantén el cambio en la capa apropiada.
3. Haz un plan interno breve; no crees un documento de planificación.
4. Implementa y agrega o actualiza pruebas enfocadas para los cambios, incluidas rutas relevantes de validación, autorización y error.
5. Revisa el diff final frente a los criterios de aceptación y verifica referencias anulables, contratos, persistencia, seguridad y cambios no deseados.

Esta revisión interna del cambio no es una revisión de seguridad independiente. Si el usuario solicita explícitamente una revisión de seguridad, sigue el flujo de revisión de seguridad aplicable en vez de tratarla como una solicitud de implementación.

## Selección de pruebas y validación

Ejecuta primero las pruebas del proyecto existente más cercano al cambio. Usa esta correspondencia como guía; si no existe un proyecto de pruebas específico, elige el proyecto de pruebas existente más cercano y no crees uno nuevo solo para seguir la tabla.

| Área afectada | Pruebas enfocadas |
| --- | --- |
| Dominio | `dotnet test .\CleanArchitecture.Domain.Tests\CleanArchitecture.Domain.Tests.csproj` |
| Aplicación | `dotnet test .\CleanArchitecture.Application.Tests\CleanArchitecture.Application.Tests.csproj` |
| Infraestructura | `dotnet test .\CleanArchitecture.Infrastructure.Tests\CleanArchitecture.Infrastructure.Tests.csproj` |
| gRPC | `dotnet test .\CleanArchitecture.gRPC.Tests\CleanArchitecture.gRPC.Tests.csproj` |
| API, integración o comportamiento entre capas | `dotnet test .\CleanArchitecture.IntegrationTests\CleanArchitecture.IntegrationTests.csproj` |

Ejecuta `dotnet build .\CleanArchitecture.sln --no-restore` cuando el cambio afecte varios proyectos, contratos públicos o composición e inyección de dependencias, o cuando la compilación enfocada no cubra las dependencias modificadas. No inicies hosts, contenedores ni servicios externos salvo que la solicitud lo requiera. Reporta las validaciones no ejecutadas y la razón concreta.

## Respuesta

Resume qué cambió y qué proyectos fueron afectados, las pruebas y validaciones ejecutadas con su resultado, y las limitaciones o supuestos pendientes.
