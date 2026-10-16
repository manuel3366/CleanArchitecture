---
# Instrucciones de Copilot

Antes de actuar, comprueba si el usuario solicita implementar o desarrollar una historia, ticket, funcionalidad o requisito, según las frases y el alcance de `.github/steering/development-automation-trigger.md`. La mera mención o análisis de una historia/ticket, así como solicitudes de explicación o revisión, no activa por sí sola el flujo de implementación.

Para una solicitud de implementación que coincida:

1. Sigue el trigger y enruta la implementación completa al agente `.github/agents/dotnet-feature-implementation.agent.md`. No inicies otro agente en paralelo para la misma tarea ni uses worktrees; trabaja en el checkout actual.
2. El agente gestiona la consulta de Jira, los requisitos faltantes, la checklist, los estándares, la implementación, las pruebas y la validación tal como se detalla en sus instrucciones y en el trigger. No dupliques aquí esos pasos.
3. Esta ruta no reemplaza procedimientos especializados. Si se solicita explícitamente una revisión de seguridad, sigue el flujo de seguridad aplicable; no la trates como implementación ni la derives al agente de funcionalidades.

Para solicitudes fuera de ese trigger, atiende directamente lo solicitado. Aplica los estándares de desarrollo y seguridad de .NET/C# cuando correspondan, sin asumir que se requiere implementar código.
