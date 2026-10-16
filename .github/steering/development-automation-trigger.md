---
title: "Trigger de Automatización de Desarrollo"
description: "Enrutamiento de solicitudes de implementación .NET/C# al agente del repositorio"
version: "1.2.0"
lastUpdated: "2026-10-15"
inclusion: "always"
---

# Trigger de Automatización de Desarrollo

## Frases de Activación

Activa este flujo cuando el usuario solicite implementar o desarrollar una historia, ticket, funcionalidad o requisito, incluidas frases como:

- "desarrolla la historia"
- "implementa la historia"
- "desarrolla esta funcionalidad"
- "implementa este ticket"
- "develop story"
- "implement story"
- "implement this feature"

## Enrutamiento

Cuando se active, usa el agente personalizado `.github/agents/dotnet-feature-implementation.agent.md` para llevar la tarea desde la obtención de requisitos hasta la validación. No ejecutes en paralelo otro agente para esta misma tarea. El agente trabaja en el checkout actual; no crees, cambies ni elimines worktrees de Git.

Si la solicitud contiene una clave de Jira (por ejemplo, `PROJ-123`), el agente debe obtener el issue con Jira MCP `jira_get_issue` antes de planificar. Solicita al menos `summary`, `description`, `issuetype`, `priority`, `status` y etiquetas. La descripción del issue y sus criterios de aceptación explícitos son la fuente de verdad. No modifiques el issue ni consultes issues no relacionados, salvo solicitud explícita del usuario.

Si falta información necesaria para implementar, el agente debe preguntar únicamente por esa información y esperar respuesta antes de editar. Se consideran requisitos suficientes cuando describen el comportamiento observable esperado, los resultados o criterios de aceptación y, cuando sea pertinente, las restricciones de error, autorización o compatibilidad. No preguntes por decisiones técnicas que puedan resolverse inspeccionando los patrones y el código existentes.

El agente debe leer y aplicar `.github/NEW_FEATURE_CHECKLIST.md`, `.github/steering/dotnet-development-standards.md` y `.github/steering/dotnet-security-standards.md`. Una revisión interna de seguridad como parte de la implementación no sustituye una solicitud explícita de revisión de seguridad; para esa solicitud, sigue el flujo de revisión de seguridad aplicable en lugar de tratarla como una tarea de implementación.

No hagas commits, push, crees ramas ni actualices herramientas externas salvo que el usuario lo solicite explícitamente. Conserva los cambios no relacionados que ya estén presentes en el árbol de trabajo.
